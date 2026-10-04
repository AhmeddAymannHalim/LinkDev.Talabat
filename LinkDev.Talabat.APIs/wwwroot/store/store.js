(() => {
  "use strict";

  const PAGE_SIZE = 10;            // the API caps page size at 10
  const CURRENCY = "EGP";          // the API returns plain numbers; change here if needed
  const STORAGE = { lang: "talabat.lang", basket: "talabat.basket" };

  const TEXT = {
    en: {
      headline: "What can we bring you?",
      searchLabel: "Search the menu",
      searchPlaceholder: "Latte, cheesecake, matcha",
      categoryLabel: "Categories",
      brandLabel: "Brand",
      sortLabel: "Sort",
      all: "All",
      allBrands: "All brands",
      sortName: "A to Z",
      sortPriceAsc: "Price: low to high",
      sortPriceDesc: "Price: high to low",
      add: "Add",
      basket: "Basket",
      close: "Close",
      basketEmpty: "Your basket is empty. Add something from the menu.",
      subtotal: "Subtotal",
      checkout: "Checkout",
      checkoutNote: "Checkout opens once sign-in is added.",
      remove: "Remove",
      more: "Show more",
      shown: "Showing {n} of {total}",
      noResults: "Nothing matches your search. Try another word or clear the filters.",
      clear: "Clear filters",
      error: "Can't load the menu. Check that the API is running, then try again.",
      retry: "Try again",
      inc: "Add one more {name}",
      dec: "Remove one {name}",
      otherLang: "العربية",
    },
    ar: {
      headline: "ماذا نُحضر لك اليوم؟",
      searchLabel: "ابحث في القائمة",
      searchPlaceholder: "لاتيه، تشيز كيك، ماتشا",
      categoryLabel: "الأقسام",
      brandLabel: "العلامة التجارية",
      sortLabel: "الترتيب",
      all: "الكل",
      allBrands: "كل العلامات",
      sortName: "أبجديًا",
      sortPriceAsc: "السعر: الأقل أولًا",
      sortPriceDesc: "السعر: الأعلى أولًا",
      add: "أضف",
      basket: "السلة",
      close: "إغلاق",
      basketEmpty: "سلتك فارغة. أضف شيئًا من القائمة.",
      subtotal: "المجموع الفرعي",
      checkout: "إتمام الطلب",
      checkoutNote: "سيتوفر إتمام الطلب بعد إضافة تسجيل الدخول.",
      remove: "حذف",
      more: "عرض المزيد",
      shown: "عرض {n} من {total}",
      noResults: "لا توجد نتائج مطابقة. جرّب كلمة أخرى أو امسح التصفية.",
      clear: "مسح التصفية",
      error: "تعذّر تحميل القائمة. تأكد من تشغيل الخادم ثم حاول مرة أخرى.",
      retry: "حاول مرة أخرى",
      inc: "زيادة {name}",
      dec: "إنقاص {name}",
      otherLang: "English",
    },
  };

  // Category names come from the database in English; Arabic labels are mapped here.
  const CATEGORY_AR = {
    Frappuccino: "فرابتشينو", Latte: "لاتيه", Mocha: "موكا", Macchiato: "ماكياتو",
    Matcha: "ماتشا", Cake: "كيك", Donuts: "دونات", Salad: "سلطة",
  };

  const state = {
    lang: load(STORAGE.lang, "en") === "ar" ? "ar" : "en",
    q: "", categoryId: null, brandId: null, sort: "",
    page: 1, items: [], total: 0,
    categories: [], brands: [],
    basket: load(STORAGE.basket, {}),
    requestId: 0,
  };

  const $ = (id) => document.getElementById(id);
  const t = (key, vars = {}) =>
    TEXT[state.lang][key].replace(/\{(\w+)\}/g, (_, k) => vars[k]);

  function load(key, fallback) {
    try {
      const raw = localStorage.getItem(key);
      return raw === null ? fallback : JSON.parse(raw);
    } catch { return fallback; }
  }
  function save(key, value) {
    try { localStorage.setItem(key, JSON.stringify(value)); } catch { /* storage unavailable */ }
  }

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

  // pictureUrl is absolute and points at the configured API host; keep only the path.
  const imageSrc = (url) => {
    if (!url) return "";
    try { return new URL(url).pathname; } catch { return url; }
  };

  const categoryLabel = (name) => (state.lang === "ar" && CATEGORY_AR[name]) || name;

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
  }

  /* ---------- filters ---------- */
  function renderFilters() {
    const tabs = $("categories");
    tabs.replaceChildren(
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
  async function api(path) {
    const res = await fetch(path, { headers: { Accept: "application/json" } });
    if (!res.ok) throw new Error(`${path} -> ${res.status}`);
    return res.json();
  }

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
      const data = await api(`/api/products?${params}`);
      if (id !== state.requestId) return;          // a newer request replaced this one
      state.items = state.items.concat(data.data);
      state.total = data.count;
      renderGrid();
    } catch (err) {
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
    const grid = $("grid");
    const box = $("message");

    if (!state.loaded) return;                      // first load still pending

    grid.replaceChildren(...state.items.map(card));

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

  /* ---------- basket ---------- */
  function changeQty(p, delta) {
    const line = state.basket[p.id] ?? { id: p.id, name: p.name, price: p.price, pictureUrl: p.pictureUrl, qty: 0 };
    line.qty += delta;
    if (line.qty <= 0) delete state.basket[p.id]; else state.basket[p.id] = line;
    save(STORAGE.basket, state.basket);
    refreshCardAction(p.id);
    renderBasket();
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
    const lines = Object.values(state.basket);
    const count = lines.reduce((n, l) => n + l.qty, 0);
    const badge = $("count");
    badge.textContent = count;
    badge.hidden = count === 0;

    $("emptyBasket").hidden = lines.length > 0;
    $("subtotal").textContent = money(lines.reduce((sum, l) => sum + l.price * l.qty, 0));

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
    requestAnimationFrame(() => $("drawer").classList.add("open"));
    $("basketBtn").setAttribute("aria-expanded", "true");
    $("closeBtn").focus();
  }

  function closeDrawer() {
    $("drawer").classList.remove("open");
    $("drawer").inert = true;
    $("scrim").hidden = true;
    $("basketBtn").setAttribute("aria-expanded", "false");
    $("basketBtn").focus();
  }

  /* ---------- init ---------- */
  function debounce(fn, ms) {
    let timer;
    return (...args) => { clearTimeout(timer); timer = setTimeout(() => fn(...args), ms); };
  }

  async function init() {
    $("langBtn").addEventListener("click", () => {
      state.lang = state.lang === "ar" ? "en" : "ar";
      save(STORAGE.lang, state.lang);
      applyLanguage();
    });
    $("basketBtn").addEventListener("click", openDrawer);
    $("closeBtn").addEventListener("click", closeDrawer);
    $("scrim").addEventListener("click", closeDrawer);
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
        api("/api/products/categories"), api("/api/products/brands"),
      ]);
      renderFilters();
    } catch { /* the products request below reports the error */ }

    state.loaded = true;
    fetchProducts(true);
  }

  init();
})();
