using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace LinkDev.Talabat.Tests.Integration
{
    // Response shapes used by the tests (extra JSON fields are ignored).
    public record ProductItem(int Id, string Name, decimal Price);
    public record ProductPage(int Count, int PageIndex, int PageSize, List<ProductItem> Data);
    public record DeliveryItem(int Id, string ShortName, decimal Cost);
    public record AuthResult(string Email, string Token);
    public record OrderLine(int ProductId, string ProductName, decimal Price, int Quantity);
    public record OrderResult(int Id, string BuyerEmail, decimal SubTotal, decimal Total, List<OrderLine> Items);

    /// <summary>End-to-end tests against the real API, SQL Server and Redis.</summary>
    [Trait("Category", "Integration")]
    public class ApiFlowTests : IClassFixture<TalabatApiFactory>
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
        private const string Password = "P@ssw0rd!";

        private readonly TalabatApiFactory _factory;
        private readonly HttpClient _client;

        public ApiFlowTests(TalabatApiFactory factory)
        {
            _factory = factory;
            _client = factory.Ready ? factory.CreateClient() : null!;
        }

        private void RequireServers() => Skip.IfNot(_factory.Ready, _factory.SkipReason);

        // ---------- helpers ----------
        private async Task<T> Get<T>(string url, string? token = null)
        {
            var response = await Send(HttpMethod.Get, url, null, token);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<T>(Json))!;
        }

        private Task<HttpResponseMessage> Send(HttpMethod method, string url, object? body = null, string? token = null)
        {
            var request = new HttpRequestMessage(method, url);
            if (body is not null) request.Content = JsonContent.Create(body, options: Json);
            if (token is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return _client.SendAsync(request);
        }

        private async Task<(string Email, string Token)> RegisterUser()
        {
            var unique = Guid.NewGuid().ToString("N")[..10];
            var email = $"it{unique}@test.com";
            var response = await Send(HttpMethod.Post, "/api/account/register", new
            {
                displayName = "IT User", userName = $"u{unique}", email, phone = "01000000000", password = Password,
            });
            response.EnsureSuccessStatusCode();
            var auth = (await response.Content.ReadFromJsonAsync<AuthResult>(Json))!;
            return (email, auth.Token);
        }

        private async Task<List<ProductItem>> Products(int count = 2) =>
            (await Get<ProductPage>($"/api/products?pageSize={count}")).Data;

        private static object Address => new { firstName = "A", lastName = "B", street = "1 Nile St", city = "Cairo", country = "Egypt" };

        private async Task<string> CreateBasket(params (int productId, int quantity, decimal clientPrice)[] items)
        {
            var id = Guid.NewGuid().ToString();
            var response = await Send(HttpMethod.Post, "/api/basket", new
            {
                id,
                items = items.Select(i => new { id = i.productId, productName = "x", price = i.clientPrice, quantity = i.quantity }),
            });
            response.EnsureSuccessStatusCode();
            return id;
        }

        // ---------- catalog ----------
        [SkippableFact]
        public async Task Products_Search_FindsSeededCakes()
        {
            RequireServers();
            var page = await Get<ProductPage>("/api/products?search=cake");

            Assert.NotEmpty(page.Data);
            Assert.All(page.Data, p => Assert.Contains("cake", p.Name, StringComparison.OrdinalIgnoreCase));
        }

        [SkippableFact]
        public async Task Products_UnknownId_Returns404()
        {
            RequireServers();
            Assert.Equal(HttpStatusCode.NotFound, (await Send(HttpMethod.Get, "/api/products/99999")).StatusCode);
        }

        // ---------- accounts ----------
        [SkippableFact]
        public async Task Account_RegisterThenLogin_ReturnsWorkingToken()
        {
            RequireServers();
            var (email, _) = await RegisterUser();

            var login = await Send(HttpMethod.Post, "/api/account/login", new { email, password = Password });
            login.EnsureSuccessStatusCode();
            var auth = (await login.Content.ReadFromJsonAsync<AuthResult>(Json))!;

            var me = await Get<AuthResult>("/api/account", auth.Token);
            Assert.Equal(email, me.Email);
        }

        [SkippableFact]
        public async Task Account_DuplicateRegistration_Returns400()
        {
            RequireServers();
            var unique = Guid.NewGuid().ToString("N")[..10];
            var body = new { displayName = "Dup", userName = $"d{unique}", email = $"dup{unique}@test.com", phone = "01000000000", password = Password };

            Assert.Equal(HttpStatusCode.OK, (await Send(HttpMethod.Post, "/api/account/register", body)).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await Send(HttpMethod.Post, "/api/account/register", body)).StatusCode);
        }

        [SkippableFact]
        public async Task Account_WrongPassword_Returns401()
        {
            RequireServers();
            var (email, _) = await RegisterUser();

            var response = await Send(HttpMethod.Post, "/api/account/login", new { email, password = "Wr0ng!pw" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [SkippableFact]
        public async Task Account_Address_IsSavedAndUpdated()
        {
            RequireServers();
            var (_, token) = await RegisterUser();

            (await Send(HttpMethod.Put, "/api/account/address", Address, token)).EnsureSuccessStatusCode();
            var saved = await Get<JsonElement>("/api/account/address", token);
            Assert.Equal("1 Nile St", saved.GetProperty("street").GetString());

            (await Send(HttpMethod.Put, "/api/account/address",
                new { firstName = "A", lastName = "B", street = "2 New St", city = "Cairo", country = "Egypt" }, token)).EnsureSuccessStatusCode();
            var updated = await Get<JsonElement>("/api/account/address", token);
            Assert.Equal("2 New St", updated.GetProperty("street").GetString());
        }

        // ---------- basket (Redis) ----------
        [SkippableFact]
        public async Task Basket_CanBeSavedReadAndDeleted()
        {
            RequireServers();
            var products = await Products(1);
            var id = await CreateBasket((products[0].Id, 2, products[0].Price));

            var basket = await Get<JsonElement>($"/api/basket?id={id}");
            Assert.Equal(id, basket.GetProperty("id").GetString());
            Assert.Equal(1, basket.GetProperty("items").GetArrayLength());

            Assert.Equal(HttpStatusCode.OK, (await Send(HttpMethod.Delete, $"/api/basket?id={id}")).StatusCode);
        }

        [SkippableFact]
        public async Task Basket_InvalidQuantity_Returns400()
        {
            RequireServers();
            var response = await Send(HttpMethod.Post, "/api/basket", new
            {
                id = Guid.NewGuid().ToString(),
                items = new[] { new { id = 1, productName = "x", price = 10, quantity = 0 } },
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ---------- orders ----------
        [SkippableFact]
        public async Task Orders_WithoutToken_Returns401()
        {
            RequireServers();
            Assert.Equal(HttpStatusCode.Unauthorized, (await Send(HttpMethod.Get, "/api/orders")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await Send(HttpMethod.Post, "/api/orders", new { })).StatusCode);
        }

        [SkippableFact]
        public async Task Orders_FullFlow_UsesDatabasePricesAndKeepsOrdersPrivate()
        {
            RequireServers();
            var (email, token) = await RegisterUser();
            var products = await Products(2);
            var delivery = (await Get<List<DeliveryItem>>("/api/orders/deliveryMethods", token)).First(d => d.Cost > 0);

            // The client lies about prices (0.10); the order must still use the database prices.
            var basketId = await CreateBasket(
                (products[0].Id, 2, 0.10m),
                (products[1].Id, 3, 0.10m));

            var create = await Send(HttpMethod.Post, "/api/orders",
                new { basketId, deliveryMethodId = delivery.Id, shippingAddress = Address }, token);
            create.EnsureSuccessStatusCode();
            var order = (await create.Content.ReadFromJsonAsync<OrderResult>(Json))!;

            var expectedSubTotal = products[0].Price * 2 + products[1].Price * 3;
            Assert.Equal(email, order.BuyerEmail);
            Assert.Equal(2, order.Items.Count);
            Assert.Equal(expectedSubTotal, order.SubTotal);
            Assert.Equal(expectedSubTotal + delivery.Cost, order.Total);

            var mine = await Get<List<OrderResult>>("/api/orders", token);
            Assert.Contains(mine, o => o.Id == order.Id);
            Assert.Equal(order.Id, (await Get<OrderResult>($"/api/orders/{order.Id}", token)).Id);

            // Another customer cannot read this order.
            var (_, otherToken) = await RegisterUser();
            Assert.Equal(HttpStatusCode.NotFound, (await Send(HttpMethod.Get, $"/api/orders/{order.Id}", null, otherToken)).StatusCode);
            Assert.Empty(await Get<List<OrderResult>>("/api/orders", otherToken));
        }

        [SkippableFact]
        public async Task Orders_EmptyBasket_Returns400()
        {
            RequireServers();
            var (_, token) = await RegisterUser();
            var delivery = (await Get<List<DeliveryItem>>("/api/orders/deliveryMethods", token)).First();
            var basketId = await CreateBasket();

            var response = await Send(HttpMethod.Post, "/api/orders",
                new { basketId, deliveryMethodId = delivery.Id, shippingAddress = Address }, token);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [SkippableFact]
        public async Task Orders_UnknownDeliveryMethod_Returns404()
        {
            RequireServers();
            var (_, token) = await RegisterUser();
            var product = (await Products(1))[0];
            var basketId = await CreateBasket((product.Id, 1, product.Price));

            var response = await Send(HttpMethod.Post, "/api/orders",
                new { basketId, deliveryMethodId = 99999, shippingAddress = Address }, token);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [SkippableFact]
        public async Task Orders_UnknownBasket_Returns404()
        {
            RequireServers();
            var (_, token) = await RegisterUser();
            var delivery = (await Get<List<DeliveryItem>>("/api/orders/deliveryMethods", token)).First();

            var response = await Send(HttpMethod.Post, "/api/orders",
                new { basketId = Guid.NewGuid().ToString(), deliveryMethodId = delivery.Id, shippingAddress = Address }, token);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
