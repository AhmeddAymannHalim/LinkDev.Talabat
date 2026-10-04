// Sign in / register, account menu, checkout and order history. Builds on the core in store.js.
(() => {
  "use strict";

  const T = window.Talabat;
  const { $, t, el, money, imageSrc, api } = T;

  const ADDRESS_FIELDS = ["firstName", "lastName", "street", "city", "country"];

  /* ---------- dialog helpers ---------- */
  // Focus the first visible text field (or the title), not the Close button.
  function openDialog(dialog) {
    if (dialog.open) return;
    dialog.showModal();
    const field = [...dialog.querySelectorAll("input")].find((i) => i.offsetParent !== null && i.type !== "radio");
    const target = field || dialog.querySelector("h2");
    if (target) { target.tabIndex = -1; target.focus(); }
  }

  function setError(id, message) {
    const box = $(id);
    box.textContent = message || "";
    box.hidden = !message;
  }

  function setBusy(button, busy, idleText, busyText) {
    button.disabled = busy;
    button.textContent = busy ? busyText : idleText;
  }

  document.querySelectorAll("dialog").forEach((dialog) => {
    dialog.addEventListener("click", (e) => { if (e.target === dialog) dialog.close(); });   // backdrop click
    dialog.querySelectorAll("[data-close]").forEach((b) => b.addEventListener("click", () => dialog.close()));
  });

  /* ---------- header account button ---------- */
  function renderAccountButton() {
    const session = T.session.get();
    $("accountBtn").textContent = session ? session.displayName : t("signIn");
  }

  $("accountBtn").addEventListener("click", () => {
    if (T.session.get()) openAccountMenu(); else openAuth("login");
  });

  function openAccountMenu() {
    const session = T.session.get();
    if (!session) return openAuth("login");
    $("accountInfo").textContent = t("signedInAs", { email: session.email });
    openDialog($("accountDialog"));
  }

  $("signOutBtn").addEventListener("click", () => {
    T.session.clear();
    $("accountDialog").close();
  });
  $("accountOrdersBtn").addEventListener("click", () => {
    $("accountDialog").close();
    openOrders();
  });

  /* ---------- sign in / create account ---------- */
  let authMode = "login";
  let afterAuth = null;

  function renderAuth() {
    const register = authMode === "register";
    document.querySelectorAll("#authDialog [data-for='register']").forEach((n) => { n.hidden = !register; });
    document.querySelectorAll("#authDialog .seg .tab").forEach((b) => {
      b.setAttribute("aria-pressed", String(b.dataset.mode === authMode));
    });
    $("authTitle").textContent = t(register ? "register" : "signIn");
    $("authSubmit").textContent = t(register ? "submitRegister" : "submitLogin");
    $("fPassword").autocomplete = register ? "new-password" : "current-password";
  }

  function openAuth(mode, continuation = null) {
    authMode = mode;
    afterAuth = continuation;
    $("authForm").reset();
    setError("authError", "");
    renderAuth();
    openDialog($("authDialog"));
  }

  document.querySelectorAll("#authDialog .seg .tab").forEach((b) => {
    b.addEventListener("click", () => { authMode = b.dataset.mode; setError("authError", ""); renderAuth(); });
  });

  $("authForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    const register = authMode === "register";
    const form = new FormData($("authForm"));
    const fields = register ? ["displayName", "userName", "email", "phone", "password"] : ["email", "password"];
    const body = Object.fromEntries(fields.map((f) => [f, String(form.get(f) ?? "").trim()]));

    if (fields.some((f) => !body[f])) { setError("authError", t("fillAll")); return; }

    setError("authError", "");
    const idle = t(register ? "submitRegister" : "submitLogin");
    setBusy($("authSubmit"), true, idle, t("working"));
    try {
      const user = await api(register ? "/api/account/register" : "/api/account/login", { method: "POST", body });
      T.session.set({ id: user.id, email: user.email, displayName: user.displayName, token: user.token });
      $("authDialog").close();
      const next = afterAuth;
      afterAuth = null;
      if (next) next();
    } catch (err) {
      setError("authError", !register && err.status === 401 ? t("invalidLogin") : err.message);
    } finally {
      setBusy($("authSubmit"), false, idle, t("working"));
    }
  });

  /* ---------- checkout ---------- */
  let deliveryMethods = [];

  function selectedDelivery() {
    const checked = document.querySelector("#deliveryOptions input:checked");
    return deliveryMethods.find((m) => String(m.id) === checked?.value);
  }

  function renderSummary() {
    const subtotal = T.basketSubtotal();
    const delivery = selectedDelivery();
    const cost = delivery ? delivery.cost : 0;
    $("sumSubtotal").textContent = money(subtotal);
    $("sumDelivery").textContent = delivery ? (cost === 0 ? t("free") : money(cost)) : "–";
    $("sumTotal").textContent = money(subtotal + cost);
  }

  function renderDeliveryOptions() {
    const current = selectedDelivery()?.id ?? deliveryMethods[0]?.id;
    $("deliveryOptions").replaceChildren(...deliveryMethods.map((m) =>
      el("label", { class: "option" },
        el("input", { type: "radio", name: "delivery", value: String(m.id), ...(m.id === current ? { checked: "" } : {}) }),
        el("span", { class: "option-text" },
          el("strong", {}, m.shortName),
          el("span", { class: "muted" }, `${m.description} (${m.deliveryTime})`)),
        el("span", { class: "option-cost" }, m.cost === 0 ? t("free") : money(m.cost)))));
    document.querySelectorAll("#deliveryOptions input").forEach((r) => r.addEventListener("change", renderSummary));
    renderSummary();
  }

  async function openCheckout() {
    if (!T.session.get()) { openAuth("login", openCheckout); return; }
    if (T.basketLines().length === 0) return;

    $("checkoutForm").hidden = false;
    $("orderDone").hidden = true;
    setError("checkoutError", "");
    openDialog($("checkoutDialog"));

    try {
      const [methods, address] = await Promise.all([api("/api/orders/deliveryMethods"), api("/api/account/address")]);
      deliveryMethods = methods;
      ADDRESS_FIELDS.forEach((f) => { $("checkoutForm").elements[f].value = address?.[f] ?? ""; });
      renderDeliveryOptions();
    } catch (err) {
      $("checkoutDialog").close();
      if (err.status === 401) openAuth("login", openCheckout);
    }
  }
  T.hooks.checkout.push(openCheckout);

  $("checkoutForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    const form = $("checkoutForm");
    const address = Object.fromEntries(ADDRESS_FIELDS.map((f) => [f, form.elements[f].value.trim()]));
    const delivery = selectedDelivery();

    if (ADDRESS_FIELDS.some((f) => !address[f]) || !delivery) { setError("checkoutError", t("fillAll")); return; }

    setError("checkoutError", "");
    setBusy($("placeOrderBtn"), true, t("placeOrder"), t("placing"));
    try {
      await api("/api/account/address", { method: "PUT", body: address }).catch(() => { /* saving is optional */ });
      await T.flushBasket();
      const order = await api("/api/orders", {
        method: "POST",
        body: { basketId: T.basketId(), deliveryMethodId: delivery.id, shippingAddress: address },
      });
      await T.resetBasket();

      $("doneTitle").textContent = t("orderPlaced", { id: order.id });
      form.hidden = true;
      $("orderDone").hidden = false;
    } catch (err) {
      setError("checkoutError", err.message);
      if (err.status === 401) { $("checkoutDialog").close(); openAuth("login", openCheckout); }
    } finally {
      setBusy($("placeOrderBtn"), false, t("placeOrder"), t("placing"));
    }
  });

  $("doneOrdersBtn").addEventListener("click", () => { $("checkoutDialog").close(); openOrders(); });
  $("doneMenuBtn").addEventListener("click", () => $("checkoutDialog").close());

  /* ---------- order history ---------- */
  function orderBlock(order) {
    const date = new Intl.DateTimeFormat(T.state.lang === "ar" ? "ar-EG" : "en-GB", { dateStyle: "medium" })
      .format(new Date(order.orderDate));
    const address = order.shippingAddress;

    return el("details", { class: "order" },
      el("summary", {},
        el("span", { class: "order-id" }, t("orderNumber", { id: order.id })),
        el("span", { class: "muted" }, `${date}, ${t("status_" + order.status)}`),
        el("strong", {}, money(order.total))),
      el("ul", { class: "order-lines" }, ...order.items.map((i) =>
        el("li", {},
          el("img", { src: imageSrc(i.pictureUrl), alt: "" }),
          el("span", {}, `${i.productName} × ${i.quantity}`),
          el("span", {}, money(i.price * i.quantity))))),
      el("p", { class: "muted" },
        `${t("shippingTo")}: ${address.firstName} ${address.lastName}, ${address.street}, ${address.city}, ${address.country}`));
  }

  async function openOrders() {
    if (!T.session.get()) { openAuth("login", openOrders); return; }
    setError("ordersError", "");
    $("ordersList").replaceChildren();
    $("ordersEmpty").hidden = true;
    openDialog($("ordersDialog"));

    try {
      const orders = (await api("/api/orders")).sort((a, b) => b.id - a.id);
      $("ordersList").replaceChildren(...orders.map(orderBlock));
      $("ordersEmpty").hidden = orders.length > 0;
    } catch (err) {
      if (err.status === 401) { $("ordersDialog").close(); openAuth("login", openOrders); }
      else setError("ordersError", err.message);
    }
  }

  /* ---------- keep dynamic text in sync ---------- */
  T.hooks.session.push(renderAccountButton);
  T.hooks.language.push(() => {
    renderAccountButton();
    renderAuth();
    if ($("checkoutDialog").open && !$("checkoutForm").hidden) renderDeliveryOptions();
    if ($("ordersDialog").open) openOrders();
  });
})();
