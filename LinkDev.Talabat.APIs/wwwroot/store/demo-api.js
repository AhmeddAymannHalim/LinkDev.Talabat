// Demo mode: answers the storefront's /api/* calls inside the browser, so the whole shop
// (register, sign in, basket, checkout, orders) can be tried with no server.
//
// It is active only on github.io or when the page is opened with ?demo=1; otherwise this
// file does nothing and the storefront talks to the real API. Everything created in demo
// mode is stored in this browser's localStorage and never leaves it.
(() => {
  "use strict";

  const demo = /\.github\.io$/.test(location.hostname) || new URLSearchParams(location.search).has("demo");
  if (!demo) return;
  window.TALABAT_DEMO = true;

  const realFetch = window.fetch.bind(window);
  const DB_KEY = "talabat.demo.db";

  // Same rule as the API's RegisterDto: 6-10 characters, upper, lower, digit and a symbol, no spaces.
  const PASSWORD_RULE = /(?=^.{6,10}$)(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#%^&()_+}{":;'?>.<,])(?!.*\s).*$/;
  const PASSWORD_MESSAGE = "Password must have 1 UpperCase, 1 LowerCase, 1 number , 1 non alphanumberic and at least 6 characters ";

  /* ---------- data and storage ---------- */
  let seed;
  const loadSeed = () => (seed ??= realFetch("demo-data.json").then((r) => r.json()));

  function loadDb() {
    try { return JSON.parse(localStorage.getItem(DB_KEY)) ?? emptyDb(); } catch { return emptyDb(); }
  }
  const emptyDb = () => ({ users: [], baskets: {}, orders: [], nextOrderId: 1 });
  const saveDb = (db) => { try { localStorage.setItem(DB_KEY, JSON.stringify(db)); } catch { /* storage unavailable */ } };

  /* ---------- responses ---------- */
  const reply = (status, body) => new Response(body === undefined ? null : JSON.stringify(body), {
    status, headers: body === undefined ? {} : { "Content-Type": "application/json" },
  });
  const fail = (status, message) => reply(status, { statusCode: status, message });
  const invalid = (errors) => reply(400, { errors, statusCode: 400, message: "Badrequest, you have made" });
  const unauthorized = () => reply(401);

  /* ---------- auth (the demo token is NOT a real JWT) ---------- */
  const makeToken = (user) => `demo.${btoa(user.email)}.${Date.now() + 36e5}`;

  function userFrom(db, headers) {
    const auth = headers.get("Authorization") || "";
    const parts = auth.replace(/^Bearer /i, "").split(".");
    if (parts[0] !== "demo" || parts.length !== 3) return null;
    try {
      if (Number(parts[2]) < Date.now()) return null;
      const email = atob(parts[1]);
      return db.users.find((u) => u.email.toLowerCase() === email.toLowerCase()) ?? null;
    } catch { return null; }
  }

  const publicUser = (u) => ({ id: u.id, displayName: u.displayName, email: u.email, token: makeToken(u) });

  /* ---------- catalog ---------- */
  function listProducts(products, q) {
    const search = (q.get("search") || "").toUpperCase();
    const brandId = Number(q.get("brandId")) || null;
    const categoryId = Number(q.get("categoryId")) || null;
    const sort = q.get("sort");
    const pageSize = Math.min(Math.max(Number(q.get("pageSize")) || 5, 1), 10);
    const pageIndex = Math.max(Number(q.get("pageIndex")) || 1, 1);

    let items = products.filter((p) =>
      (!search || p.name.toUpperCase().includes(search)) &&
      (!brandId || p.brandId === brandId) && (!categoryId || p.categoryId === categoryId));

    items = [...items].sort((a, b) =>
      sort === "priceAsc" ? a.price - b.price :
      sort === "priceDesc" ? b.price - a.price :
      a.name.localeCompare(b.name));

    return {
      pageIndex, pageSize, count: items.length,
      data: items.slice((pageIndex - 1) * pageSize, pageIndex * pageSize),
    };
  }

  /* ---------- account ---------- */
  function register(db, body) {
    const labels = { displayName: "DisplayName", userName: "UserName", email: "Email", phone: "Phone", password: "Password" };
    const errors = [];
    for (const [key, label] of Object.entries(labels)) {
      if (!String(body[key] ?? "").trim()) errors.push({ fields: label, errors: [`The ${label} field is required.`] });
    }
    if (body.email && !/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(body.email))
      errors.push({ fields: "Email", errors: ["The Email field is not a valid e-mail address."] });
    if (body.password && !PASSWORD_RULE.test(body.password))
      errors.push({ fields: "Password", errors: [PASSWORD_MESSAGE] });
    if (errors.length) return invalid(errors);

    const taken = [];
    if (db.users.some((u) => u.userName.toLowerCase() === body.userName.toLowerCase()))
      taken.push(`Username '${body.userName}' is already taken.`);
    if (db.users.some((u) => u.email.toLowerCase() === body.email.toLowerCase()))
      taken.push(`Email '${body.email}' is already taken.`);
    if (taken.length) return fail(400, taken.join(" "));

    const user = {
      id: crypto.randomUUID ? crypto.randomUUID() : String(Date.now()),
      displayName: body.displayName.trim(), userName: body.userName.trim(), email: body.email.trim(),
      phone: body.phone.trim(), password: body.password, address: null,
    };
    db.users.push(user);
    saveDb(db);
    return reply(200, publicUser(user));
  }

  function login(db, body) {
    const user = db.users.find((u) => u.email.toLowerCase() === String(body.email ?? "").toLowerCase());
    if (!user || user.password !== body.password)
      return fail(401, "Invalid email or password. Please try again.");
    return reply(200, publicUser(user));
  }

  /* ---------- basket ---------- */
  function saveBasket(db, body) {
    const errors = [];
    (body.items ?? []).forEach((item, i) => {
      if (!(item.quantity >= 1)) errors.push({ fields: `Items[${i}].Quantity`, errors: ["Quantity must be at least one item"] });
      if (!(item.price >= 0.1)) errors.push({ fields: `Items[${i}].Price`, errors: ["Price must be greater than Zero!"] });
    });
    if (!body.id) errors.push({ fields: "Id", errors: ["The Id field is required."] });
    if (errors.length) return invalid(errors);

    db.baskets[body.id] = { id: body.id, items: body.items ?? [] };
    saveDb(db);
    return reply(200, db.baskets[body.id]);
  }

  /* ---------- orders ---------- */
  function createOrder(db, user, body, data) {
    const basket = db.baskets[body.basketId];
    if (!basket) return fail(404, `CustomerBasketDto with (${body.basketId}) is not found`);
    if (basket.items.length === 0) return fail(400, "Can't create an order from an empty basket.");

    const delivery = data.deliveryMethods.find((m) => m.id === Number(body.deliveryMethodId));
    if (!delivery) return fail(404, `DeliveryMethod with (${body.deliveryMethodId}) is not found`);

    // Prices always come from the catalog, never from the basket the client sent.
    const items = [];
    for (const line of basket.items) {
      const product = data.products.find((p) => p.id === line.id);
      if (!product) return fail(404, `Product with (${line.id}) is not found`);
      items.push({
        id: items.length + 1, productId: product.id, productName: product.name,
        pictureUrl: product.pictureUrl, price: product.price, quantity: line.quantity,
      });
    }

    const subTotal = items.reduce((sum, i) => sum + i.price * i.quantity, 0);
    const order = {
      id: db.nextOrderId++, buyerEmail: user.email, orderDate: new Date().toISOString(), status: "Pending",
      shippingAddress: body.shippingAddress, deliveryMethodId: delivery.id, deliveryMethod: delivery.shortName,
      items, subTotal, total: subTotal + delivery.cost,
    };
    db.orders.push(order);
    saveDb(db);
    return reply(200, order);
  }

  /* ---------- router ---------- */
  async function handle(method, path, query, init, headers) {
    const data = await loadSeed();
    const db = loadDb();
    let m;
    const body = () => { try { return JSON.parse(init.body); } catch { return {}; } };

    // public endpoints
    if (method === "GET" && path === "/api/products") return reply(200, listProducts(data.products, query));
    if (method === "GET" && path === "/api/products/brands") return reply(200, data.brands);
    if (method === "GET" && path === "/api/products/categories") return reply(200, data.categories);
    if (method === "GET" && (m = path.match(/^\/api\/products\/(\d+)$/))) {
      const product = data.products.find((p) => p.id === Number(m[1]));
      return product ? reply(200, product) : fail(404, `product with (${m[1]}) is not found`);
    }
    if (method === "POST" && path === "/api/account/register") return register(db, body());
    if (method === "POST" && path === "/api/account/login") return login(db, body());
    if (method === "GET" && path === "/api/account/emailexists")
      return reply(200, db.users.some((u) => u.email.toLowerCase() === (query.get("email") || "").toLowerCase()));

    if (path === "/api/basket") {
      if (method === "POST") return saveBasket(db, body());
      const id = query.get("id");
      if (method === "GET") return db.baskets[id] ? reply(200, db.baskets[id]) : fail(404, `CustomerBasketDto with (${id}) is not found`);
      if (method === "DELETE") {
        const existed = id in db.baskets;
        delete db.baskets[id];
        saveDb(db);
        return existed ? reply(200, true) : fail(400, "unable to delete this basket.");
      }
    }

    // endpoints that need a signed-in user
    const user = userFrom(db, headers);
    if (!user) return unauthorized();

    if (method === "GET" && path === "/api/account") return reply(200, publicUser(user));
    if (path === "/api/account/address") {
      if (method === "GET") return user.address ? reply(200, user.address) : reply(204);
      if (method === "PUT") { user.address = body(); saveDb(db); return reply(200, user.address); }
    }
    if (method === "GET" && path === "/api/orders/deliveryMethods") return reply(200, data.deliveryMethods);
    if (method === "POST" && path === "/api/orders") return createOrder(db, user, body(), data);
    if (method === "GET" && path === "/api/orders") return reply(200, db.orders.filter((o) => o.buyerEmail === user.email));
    if (method === "GET" && (m = path.match(/^\/api\/orders\/(\d+)$/))) {
      const order = db.orders.find((o) => o.id === Number(m[1]) && o.buyerEmail === user.email);
      return order ? reply(200, order) : fail(404, `order with (${m[1]}) is not found`);
    }

    return fail(404, "Resource is not found ");
  }

  window.fetch = async (input, init = {}) => {
    const url = new URL(typeof input === "string" ? input : input.url, location.href);
    const at = url.pathname.indexOf("/api/");
    if (at === -1) return realFetch(input, init);

    await new Promise((resolve) => setTimeout(resolve, 120));   // feel like a network call
    return handle((init.method || "GET").toUpperCase(), url.pathname.slice(at), url.searchParams, init, new Headers(init.headers));
  };
})();
