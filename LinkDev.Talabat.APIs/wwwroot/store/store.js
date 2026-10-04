// Storefront core: catalog, basket, language, and the pieces shared with account.js.
(() => {
  "use strict";

  const PAGE_SIZE = 10;            // the API caps page size at 10
  const CURRENCY = "EGP";          // the API returns plain numbers; change here if needed
  const STORAGE = {
    lang: "talabat.lang", basket: "talabat.basket", basketId: "talabat.basketId", session: "talabat.session",
  };

  // Category names come from the database in English; Arabic labels are mapped here.
  const CATEGORY_AR = {
    Frappuccino: "فرابتشينو", Latte: "لاتيه", Mocha: "موكا", Macchiato: "ماكياتو",
    Matcha: "ماتشا", Cake: "كيك", Donuts: "دونات", Salad: "سلطة",
  };

  function load(key, fallback) {
    try {
      const raw = localStorage.getItem(key);
      return raw === null ? fallback : JSON.parse(raw);
    } catch { return fallback; }
  }
  function save(key, value) {
    try { localStorage.setItem(key, JSON.stringify(value)); } catch { /* storage unavailable */ }
  }
  function remove(key) {
    try { localStorage.removeItem(key); } catch { /* storage unavailable */ }
  }

  const state = {
    lang: load(STORAGE.lang, "en") === "ar" ? "ar" : "en",
    q: "", categoryId: null, brandId: null, sort: "",
    page: 1, items: [], total: 0,
    categories: [], brands: [],
    basket: load(STORAGE.basket, {}),
    requestId: 0,
    loaded: false,
  };

  const T = window.Talabat = { state, hooks: { language: [], session: [], checkout: [] } };

  const $ = (id) => document.getElementById(id);
  const t = (key, vars = {}) =>
    (window.TEXT[state.lang][key] ?? key).replace(/\{(\w+)\}/g, (_, k) => vars[k]);

  function el(tag, props = {}, ...children) {
    const node = document.createElement(tag);
    for (const [k, v] of Object.entries(props)) {
      if (k === "class") node.className = v;
      else if (k.startsWith("on")) node.addEventListener(k.slice(2), v);
      else node.setAttribute(k, v);
    }
    node.append(...children);
    return node;
  }

  const money = (n) =>
    new Intl.NumberFormat(state.lang === "ar" ? "ar-EG" : "en-EG", {
      style: "currency", currency: CURRENCY, minimumFractionDigits: 0, maximumFractionDigits: 2,
    }).format(n);

  // pictureUrl may be absolute (it points at the configured API host). Keep only the path and
  // resolve it from the store page (/store/), so it works on any host or base path.
  const imageSrc = (url) => {
    if (!url) return "";
    let path = url;
    try { path = new URL(url).pathname; } catch { /* already a path */ }
    return "../" + path.replace(/^\/+/, "");
  };

  const categoryLabel = (name) => (state.lang === "ar" && CATEGORY_AR[name]) || name;

  Object.assign(T, { $, t, el, money, imageSrc, load, save });

  /* ---------- session ---------- */
  T.session = {
    get: () => load(STORAGE.session, null),
    set(user) { save(STORAGE.session, user); T.hooks.session.forEach((fn) => fn()); },
    clear() { remove(STORAGE.session); T.hooks.session.forEach((fn) => fn()); },
  };

  /* ---------- API client ---------- */
  class ApiError extends Error {
    constructor(status, message) { super(message); this.status = status; }
  }
  T.ApiError = ApiError;

  function errorMessage(status, data) {
    if (data && Array.isArray(data.errors) && data.errors.length)
      return data.errors.flatMap((e) => e.errors).join(" ");
    if (data && data.message && data.message !== "Bad Request") return data.message;
    if (status === 401) return t("unauthorized");
    if (status === 404) return t("notFound");
    return t("genericError");
  }

  T.api = async function api(path, { method = "GET", body } = {}) {
    const headers = { Accept: "application/json" };
    if (body !== undefined) headers["Content-Type"] = "application/json";
    const session = T.session.get();
    if (session && session.token) headers.Authorization = `Bearer ${session.token}`;

    let res;
    try {
      res = await fetch(path, { method, headers, body: body === undefined ? undefined : JSON.stringify(body) });
    } catch {
      throw new ApiError(0, t("networkError"));
    }

    const text = await res.text();
    let data = null;
    if (text) { try { data = JSON.parse(text); } catch { /* not JSON */ } }

    if (!res.ok) {
      if (res.status === 401 && session) T.session.clear();   // the token expired or is invalid
      throw new ApiError(res.status, errorMessage(res.status, data));
    }
    return data;
  };

  /* ---------- language ---------- */
  function applyLanguage() {
    const html = document.documentElement;
    html.lang = state.lang;
    html.dir = state.lang === "ar" ? "rtl" : "ltr";
    const name = state.lang === "ar" ? "طلبات" : "Talabat";
    document.title = name;
    $("wordmark").textContent = name;

    document.querySelectorAll("[data-i18n]").forEach((n) => { n.textContent = t(n.dataset.i18n); });
    document.querySelectorAll("[data-i18n-placeholder]").forEach((n) => { n.placeholder = t(n.dataset.i18nPlaceholder); });
    document.querySelectorAll("[data-i18n-aria]").forEach((n) => { n.setAttribute("aria-label", t(n.dataset.i18nAria)); });
    $("langBtn").textContent = t("otherLang");
    $("langBtn").lang = state.lang === "ar" ? "en" : "ar";

    renderFilters();
    renderGrid();
    renderBasket();
    T.hooks.language.forEach((fn) => fn());
  }

  /* ---------- filters ---------- */
  function renderFilters() {
    $("categories").replaceChildren(
      tabButton(t("all"), null),
      ...state.categories.map((c) => tabButton(categoryLabel(c.name), c.id)),
    );

    const brand = $("brand");
    brand.replaceChildren(
      el("option", { value: "" }, t("allBrands")),
      ...state.brands.map((b) => el("option", { value: b.id }, b.name)),
    );
    brand.value = state.brandId ?? "";

    const sort = $("sort");
    sort.replaceChildren(
      el("option", { value: "" }, t("sortName")),
      el("option", { value: "priceAsc" }, t("sortPriceAsc")),
      el("option", { value: "priceDesc" }, t("sortPriceDesc")),
    );
    sort.value = state.sort;
  }

  function tabButton(label, id) {
    return el("button", {
      type: "button", class: "tab", "aria-pressed": String(state.categoryId === id),
      onclick: () => { state.categoryId = id; refreshFilters(); },
    }, label);
  }

  function refreshFilters() {
    renderFilters();
    fetchProducts(true);
  }

  /* ---------- products ---------- */
  async function fetchProducts(reset) {
    if (reset) { state.page = 1; state.items = []; }
    const id = ++state.requestId;

    const params = new URLSearchParams({ pageIndex: state.page, pageSize: PAGE_SIZE });
    if (state.q) params.set("search", state.q);
    if (state.categoryId) params.set("categoryId", state.categoryId);
    if (state.brandId) params.set("brandId", state.brandId);
    if (state.sort) params.set("sort", state.sort);

    showLoading(reset);
    try {
      const data = await T.api(`/api/products?${params}`);
      if (id !== state.requestId) return;          // a newer request replaced this one
      state.items = state.items.concat(data.data);
      state.total = data.count;
      renderGrid();
    } catch {
      if (id !== state.requestId) return;
      showError();
    }
  }

  function showLoading(reset) {
    $("message").hidden = true;
    if (!reset) return;
    $("pager").hidden = true;
    $("grid").replaceChildren(...Array.from({ length: 8 }, () =>
      el("div", { class: "item skeleton", "aria-hidden": "true" },
        el("div", { class: "photo" }), el("div", { class: "line" }), el("div", { class: "line" }))));
  }

  function showError() {
    $("grid").replaceChildren();
    $("pager").hidden = true;
    const box = $("message");
    box.replaceChildren(el("p", {}, t("error")),
      el("button", { type: "button", class: "outline-btn", onclick: () => fetchProducts(true) }, t("retry")));
    box.hidden = false;
  }

  function renderGrid() {
    if (!state.loaded) return;                      // first load still pending
    const box = $("message");

    $("grid").replaceChildren(...state.items.map(card));

    if (state.items.length === 0) {
      box.replaceChildren(el("p", {}, t("noResults")),
        el("button", { type: "button", class: "outline-btn", onclick: clearFilters }, t("clear")));
      box.hidden = false;
      $("pager").hidden = true;
      return;
    }

    box.hidden = true;
    $("pager").hidden = false;
    $("shown").textContent = t("shown", { n: state.items.length, total: state.total });
    $("more").hidden = state.items.length >= state.total;
  }

  function card(p) {
    const img = el("img", {
      class: "photo", src: imageSrc(p.pictureUrl), alt: p.name, loading: "lazy", width: "400", height: "500",
    });
    const buy = el("div", { class: "buy" }, el("span", { class: "price" }, money(p.price)), actionFor(p));
    return el("article", { class: "item", "data-id": p.id },
      img, el("h3", {}, p.name), el("p", { class: "brand" }, p.brand || ""), buy);
  }

  function actionFor(p) {
    const line = state.basket[p.id];
    if (!line) {
      return el("button", { type: "button", class: "outline-btn", onclick: () => changeQty(p, 1) }, t("add"));
    }
    return stepper(p, line.qty);
  }

  function stepper(p, qty) {
    return el("div", { class: "stepper" },
      el("button", { type: "button", "aria-label": t("dec", { name: p.name }), onclick: () => changeQty(p, -1) }, "−"),
      el("output", { "aria-live": "polite" }, String(qty)),
      el("button", { type: "button", "aria-label": t("inc", { name: p.name }), onclick: () => changeQty(p, 1) }, "+"));
  }

  function clearFilters() {
    state.q = ""; state.categoryId = null; state.brandId = null; state.sort = "";
    $("q").value = "";
    refreshFilters();
  }

  /* ---------- basket (kept in the browser and mirrored to the API for checkout) ---------- */
  T.basketId = () => {
    let id = load(STORAGE.basketId, null);
    if (!id) {
      id = (crypto.randomUUID && crypto.randomUUID()) || `b-${Date.now()}-${Math.random().toString(16).slice(2)}`;
      save(STORAGE.basketId, id);
    }
    return id;
  };

  T.basketLines = () => Object.values(state.basket);
  T.basketSubtotal = () => T.basketLines().reduce((sum, l) => sum + l.price * l.qty, 0);

  let syncTimer;
  function scheduleSync() {
    clearTimeout(syncTimer);
    syncTimer = setTimeout(() => { syncBasket().catch(() => { /* retried on checkout */ }); }, 400);
  }

  async function syncBasket() {
    await T.api("/api/basket", {
      method: "POST",
      body: {
        id: T.basketId(),
        items: T.basketLines().map((l) => ({
          id: l.id, productName: l.name, productUrl: l.pictureUrl ?? null, price: l.price,
          quantity: l.qty, brand: l.brand ?? null, category: l.category ?? null,
        })),
      },
    });
  }

  // Called right before checkout so the server sees exactly what is on screen.
  T.flushBasket = async () => { clearTimeout(syncTimer); await syncBasket(); };

  // After an order is placed: empty the basket here and on the server, and start a new one.
  T.resetBasket = async () => {
    clearTimeout(syncTimer);
    const oldId = T.basketId();
    state.basket = {};
    save(STORAGE.basket, state.basket);
    remove(STORAGE.basketId);
    T.api(`/api/basket?id=${encodeURIComponent(oldId)}`, { method: "DELETE" }).catch(() => { /* best effort */ });
    renderGrid();
    renderBasket();
  };

  function changeQty(p, delta) {
    const line = state.basket[p.id] ?? {
      id: p.id, name: p.name, price: p.price, pictureUrl: p.pictureUrl, brand: p.brand, category: p.category, qty: 0,
    };
    line.qty += delta;
    if (line.qty <= 0) delete state.basket[p.id]; else state.basket[p.id] = line;
    save(STORAGE.basket, state.basket);
    refreshCardAction(p.id);
    renderBasket();
    scheduleSync();
  }

  function refreshCardAction(id) {
    const product = state.items.find((p) => p.id === id);
    const buy = document.querySelector(`.item[data-id="${id}"] .buy`);
    if (!product || !buy) return;
    const focusedPlus = buy.contains(document.activeElement) && document.activeElement.textContent === "+";
    buy.lastElementChild.replaceWith(actionFor(product));
    if (focusedPlus) buy.querySelector(".stepper button:last-child")?.focus();
  }

  function renderBasket() {
    const lines = T.basketLines();
    const count = lines.reduce((n, l) => n + l.qty, 0);
    const badge = $("count");
    badge.textContent = count;
    badge.hidden = count === 0;

    $("emptyBasket").hidden = lines.length > 0;
    $("subtotal").textContent = money(T.basketSubtotal());
    $("checkoutBtn").disabled = lines.length === 0;

    $("lines").replaceChildren(...lines.map((l) =>
      el("li", { class: "line-item" },
        el("img", { src: imageSrc(l.pictureUrl), alt: "" }),
        el("div", {},
          el("div", { class: "name" }, l.name),
          el("div", { class: "price" }, money(l.price * l.qty))),
        el("div", { class: "row" },
          stepper(l, l.qty),
          el("button", { type: "button", class: "remove", onclick: () => changeQty(l, -l.qty) }, t("remove"))))));
  }

  function openDrawer() {
    $("drawer").inert = false;
    $("scrim").hidden = false;
    void $("drawer").offsetWidth;                   // force a reflow so the slide-in transition runs
    $("drawer").classList.add("open");
    $("basketBtn").setAttribute("aria-expanded", "true");
    $("closeBtn").focus();
  }

  function closeDrawer(returnFocus = true) {
    $("drawer").classList.remove("open");
    $("drawer").inert = true;
    $("scrim").hidden = true;
    $("basketBtn").setAttribute("aria-expanded", "false");
    if (returnFocus) $("basketBtn").focus();
  }

  /* ---------- start ---------- */
  function debounce(fn, ms) {
    let timer;
    return (...args) => { clearTimeout(timer); timer = setTimeout(() => fn(...args), ms); };
  }

  T.start = async function start() {
    $("demoBanner").hidden = !window.TALABAT_DEMO;

    $("langBtn").addEventListener("click", () => {
      state.lang = state.lang === "ar" ? "en" : "ar";
      save(STORAGE.lang, state.lang);
      applyLanguage();
    });
    $("basketBtn").addEventListener("click", openDrawer);
    $("closeBtn").addEventListener("click", () => closeDrawer());
    $("scrim").addEventListener("click", () => closeDrawer());
    $("checkoutBtn").addEventListener("click", () => {
      closeDrawer(false);
      T.hooks.checkout.forEach((fn) => fn());
    });
    document.addEventListener("keydown", (e) => {
      if (e.key === "Escape" && $("drawer").classList.contains("open")) closeDrawer();
    });

    $("q").addEventListener("input", debounce((e) => {
      state.q = e.target.value.trim();
      fetchProducts(true);
    }, 300));
    $("brand").addEventListener("change", (e) => { state.brandId = e.target.value || null; fetchProducts(true); });
    $("sort").addEventListener("change", (e) => { state.sort = e.target.value; fetchProducts(true); });
    $("more").addEventListener("click", () => { state.page += 1; fetchProducts(false); });

    applyLanguage();
    try {
      [state.categories, state.brands] = await Promise.all([
        T.api("/api/products/categories"), T.api("/api/products/brands"),
      ]);
      renderFilters();
    } catch { /* the products request below reports the error */ }

    state.loaded = true;
    fetchProducts(true);
  };
})();
