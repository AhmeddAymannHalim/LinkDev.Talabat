# API guide

A step-by-step walkthrough of the Talabat API: register, sign in, browse, fill a basket and place an order. Every command below was run against the real API.

Start the API first (see the [README](../README.md#getting-started)). The examples assume it listens on `http://localhost:5086`; change `BASE` if yours is different.

```bash
BASE=http://localhost:5086
```

Prefer clicking? Open **Swagger** at `/swagger`, or import the [Postman collection](postman).

## How it works

| Idea | What to know |
|---|---|
| **Authentication** | `register` and `login` return a **JWT**. Send it as `Authorization: Bearer <token>`. Tokens last 10 minutes. |
| **Public endpoints** | The catalog (`/api/products/...`) and the basket (`/api/basket`) need no login, so guests can shop. |
| **Protected endpoints** | `/api/account` (except register, login and emailexists) and `/api/orders/...` need a token. Without one they return `401`. |
| **Basket** | Stored in **Redis**. The client chooses the basket `id` (any unique string, for example a GUID). |
| **Orders** | Created from a basket id. **Prices are always read from the database**, never from the basket, so a client can't change what it pays. |
| **Privacy** | You can only read your own orders. Someone else's order id returns `404`. |

### Password rule
6 to 10 characters, with at least one upper-case letter, one lower-case letter, one digit and one symbol (for example `P@ssw0rd!`).

### Errors
Every error has a `statusCode` and a `message`:

```json
{ "statusCode": 404, "message": "product with (99999) is not found" }
```

Validation errors (`400`) also list each field:

```json
{
  "errors": [{ "fields": "Password", "errors": ["Password must have 1 UpperCase, ..."] }],
  "statusCode": 400,
  "message": "Badrequest, you have made"
}
```

| Status | Meaning |
|---|---|
| `400` | Invalid input (weak password, duplicate email, bad quantity, empty basket) |
| `401` | Missing or invalid token, or wrong email or password |
| `404` | The product, basket, delivery method or order doesn't exist |

## 1. Register

```bash
curl -s -X POST $BASE/api/account/register \
  -H "Content-Type: application/json" \
  -d '{"displayName":"Ahmed","userName":"ahmed","email":"ahmed@example.com","phone":"01000000000","password":"P@ssw0rd!"}'
```

Returns `200` with the user and a token:

```json
{ "id": "…", "displayName": "Ahmed", "email": "ahmed@example.com", "token": "eyJhbGciOi…" }
```

Registering the same email or username again returns `400` with a message such as `Email 'ahmed@example.com' is already taken.`

## 2. Sign in

```bash
curl -s -X POST $BASE/api/account/login \
  -H "Content-Type: application/json" \
  -d '{"email":"ahmed@example.com","password":"P@ssw0rd!"}'
```

Copy the `token` from the response and keep it in a variable:

```bash
TOKEN=<paste the token here>
```

A wrong password or unknown email returns `401`.

Check the token works:

```bash
curl -s $BASE/api/account -H "Authorization: Bearer $TOKEN"
```

## 3. Browse the catalog

```bash
curl -s "$BASE/api/products?search=cake&sort=priceDesc&pageSize=5"
```

| Query | Meaning |
|---|---|
| `search` | Part of a product name |
| `sort` | `priceAsc` or `priceDesc` (default: by name) |
| `brandId`, `categoryId` | Filter by brand or category |
| `pageIndex`, `pageSize` | Paging (page size is capped at 10) |

The response lists products with `id`, `name`, `price`, `brand` and `category`, plus the total `count`. Remember a product `id` for the next step. Also available: `GET /api/products/{id}`, `/api/products/brands` and `/api/products/categories`.

## 4. Fill a basket

Choose any unique basket id and send its items:

```bash
curl -s -X POST $BASE/api/basket \
  -H "Content-Type: application/json" \
  -d '{"id":"my-basket-1","items":[{"id":9,"productName":"Blueberry Cheesecake","price":15,"quantity":2}]}'
```

Read it back with `GET /api/basket?id=my-basket-1` and remove it with `DELETE /api/basket?id=my-basket-1`. A quantity below 1 returns `400`.

## 5. Place an order

List the delivery methods (needs your token) and pick an `id`:

```bash
curl -s $BASE/api/orders/deliveryMethods -H "Authorization: Bearer $TOKEN"
```

Create the order from your basket:

```bash
curl -s -X POST $BASE/api/orders \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"basketId":"my-basket-1","deliveryMethodId":2,"shippingAddress":{"firstName":"Ahmed","lastName":"Halim","street":"1 Nile Street","city":"Cairo","country":"Egypt"}}'
```

The response includes the order `id`, the items, `subTotal` and `total` (subtotal plus the delivery cost). An empty basket returns `400`, and an unknown basket or delivery method returns `404`.

## 6. Your orders

```bash
curl -s $BASE/api/orders -H "Authorization: Bearer $TOKEN"
curl -s $BASE/api/orders/1 -H "Authorization: Bearer $TOKEN"
```

## Saved address

```bash
curl -s -X PUT $BASE/api/account/address \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"firstName":"Ahmed","lastName":"Halim","street":"1 Nile Street","city":"Cairo","country":"Egypt"}'

curl -s $BASE/api/account/address -H "Authorization: Bearer $TOKEN"
```

`GET` returns `204 No Content` until you have saved an address.
