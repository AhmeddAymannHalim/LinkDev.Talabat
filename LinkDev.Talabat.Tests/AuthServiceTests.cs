using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using LinkDev.Talabat.Core.Application;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Auth;
using LinkDev.Talabat.Core.Application.Exceptions;
using LinkDev.Talabat.Core.Application.Services.Auth;
using LinkDev.Talabat.Core.Domain.Entities._Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace LinkDev.Talabat.Tests
{
    public class AuthServiceTests
    {
        private const string Issuer = "TalabatIdentity";
        private const string Audience = "Talabat Users";
        private const string SigningKey = "unit-test-signing-key-that-is-long-enough-for-hmac-sha256";

        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManager;
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _userManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);

            _signInManager = new Mock<SignInManager<ApplicationUser>>(
                _userManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<ApplicationUser>>());

            _userManager.Setup(m => m.GetClaimsAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<Claim>());
            _userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string> { "Admin" });

            var jwt = Options.Create(new JwtSettings
            {
                Key = SigningKey,
                Issuer = Issuer,
                Audience = Audience,
                DurationInMinutes = 10,
            });

            _sut = new AuthService(new Mock<IMapper>().Object, _userManager.Object, _signInManager.Object, jwt);
        }

        private static ApplicationUser NewUser() => new()
        {
            Id = "u1", DisplayName = "Test User", UserName = "test", Email = "test@test.com",
        };

        private static LoginDto Login(string password = "P@ssw0rd") => new() { Email = "test@test.com", Password = password };

        [Fact]
        public async Task LoginAsync_UnknownEmail_ThrowsUnauthorized()
        {
            _userManager.Setup(m => m.FindByEmailAsync("test@test.com")).ReturnsAsync((ApplicationUser?)null);

            await Assert.ThrowsAsync<UnAuthorizedException>(() => _sut.LoginAsync(Login()));
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
        {
            var user = NewUser();
            _userManager.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
            _signInManager.Setup(s => s.CheckPasswordSignInAsync(user, "bad", true)).ReturnsAsync(SignInResult.Failed);

            await Assert.ThrowsAsync<UnAuthorizedException>(() => _sut.LoginAsync(Login("bad")));
        }

        [Fact]
        public async Task LoginAsync_LockedOutAccount_ThrowsUnauthorizedWithLockedMessage()
        {
            var user = NewUser();
            _userManager.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
            _signInManager.Setup(s => s.CheckPasswordSignInAsync(user, It.IsAny<string>(), true)).ReturnsAsync(SignInResult.LockedOut);

            var ex = await Assert.ThrowsAsync<UnAuthorizedException>(() => _sut.LoginAsync(Login()));

            Assert.Contains("locked", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsUserWithSignedToken()
        {
            var user = NewUser();
            _userManager.Setup(m => m.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
            _signInManager.Setup(s => s.CheckPasswordSignInAsync(user, "P@ssw0rd", true)).ReturnsAsync(SignInResult.Success);

            var result = await _sut.LoginAsync(Login());

            Assert.Equal("u1", result.Id);
            Assert.Equal("test@test.com", result.Email);

            // Validate exactly like the API's JWT middleware does: signature, issuer, audience and lifetime.
            var principal = new JwtSecurityTokenHandler().ValidateToken(result.Token, new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = Issuer,
                ValidateAudience = true, ValidAudience = Audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            }, out var validated);

            Assert.Equal("test@test.com", principal.FindFirstValue(ClaimTypes.Email));
            Assert.True(principal.IsInRole("Admin"));
            Assert.True(validated.ValidTo > DateTime.UtcNow && validated.ValidTo <= DateTime.UtcNow.AddMinutes(11));
        }

        [Fact]
        public async Task RegisterAsync_WhenIdentityRejectsUser_ThrowsValidationWithErrors()
        {
            _userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too short" }));

            var ex = await Assert.ThrowsAsync<ValidationException>(() => _sut.RegisterAsync(NewRegister()));

            Assert.Contains("Password too short", ex.Errors);
        }

        [Fact]
        public async Task RegisterAsync_ValidUser_ReturnsTokenForNewUser()
        {
            _userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "P@ssw0rd")).ReturnsAsync(IdentityResult.Success);

            var result = await _sut.RegisterAsync(NewRegister());

            Assert.Equal("new@test.com", result.Email);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EmailExists_ReflectsWhetherUserIsFound(bool exists)
        {
            _userManager.Setup(m => m.FindByEmailAsync("x@test.com"))
                        .ReturnsAsync(exists ? NewUser() : null);

            Assert.Equal(exists, await _sut.EmailExists("x@test.com"));
        }

        private static RegisterDto NewRegister() => new()
        {
            DisplayName = "New", UserName = "new", Email = "new@test.com", Phone = "01000000000", Password = "P@ssw0rd",
        };
    }
}
