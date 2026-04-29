const main = document.querySelector("main");

initPage();
window.addEventListener("popstate", () => {
  loadPage(window.location.href, { push: false });
});

function initPage() {
  initSmoothLinks();
  initCalculatorForm();
}

function initSmoothLinks() {
  document.querySelectorAll("[data-smooth-link]").forEach((link) => {
    if (link.dataset.smoothReady === "true") {
      return;
    }

    link.dataset.smoothReady = "true";
    link.addEventListener("click", async (event) => {
      if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
        return;
      }

      const url = new URL(link.href, window.location.href);
      if (url.origin !== window.location.origin) {
        return;
      }

      event.preventDefault();
      await loadPage(url.href, { push: true });
    });
  });
}

async function loadPage(url, options = { push: true }) {
  if (!main) {
    window.location.href = url;
    return;
  }

  main.classList.add("is-navigating");

  try {
    const response = await fetch(url, {
      headers: {
        "X-Requested-With": "fetch"
      }
    });

    if (!response.ok) {
      throw new Error("Nao foi possivel carregar a pagina.");
    }

    const html = await response.text();
    const documentFragment = new DOMParser().parseFromString(html, "text/html");
    const nextMain = documentFragment.querySelector("main");

    if (!nextMain) {
      throw new Error("Conteudo da pagina nao encontrado.");
    }

    document.title = documentFragment.title || document.title;
    main.innerHTML = nextMain.innerHTML;

    if (options.push) {
      window.history.pushState({}, document.title, url);
    }

    window.scrollTo({ top: 0, behavior: "smooth" });
    initPage();
  } catch (error) {
    window.location.href = url;
  } finally {
    main.classList.remove("is-navigating");
  }
}

function initCalculatorForm() {
  const calculatorForm = document.querySelector("[data-calculator-form]");

  if (!calculatorForm || calculatorForm.dataset.calculatorReady === "true") {
    return;
  }

  calculatorForm.dataset.calculatorReady = "true";

  const resultPanel = document.querySelector(".result-panel");
  const submitButton = calculatorForm.querySelector("button[type='submit']");
  const vehicleSelect = calculatorForm.querySelector("select[name='Input.TipoVeiculo']");
  const originalButtonText = submitButton?.textContent ?? "Calcular valor";
  const fields = {
    amount: document.querySelector("[data-result-amount]"),
    duration: document.querySelector("[data-result-duration]"),
    minutes: document.querySelector("[data-result-minutes]"),
    hours: document.querySelector("[data-result-hours]"),
    exit: document.querySelector("[data-result-exit]"),
    error: document.querySelector("[data-result-error]")
  };

  calculatorForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    await calculate();
  });

  vehicleSelect?.addEventListener("change", async () => {
    await calculate();
  });

  async function calculate() {
    setLoading(true);

    try {
      const response = await fetch(`${calculatorForm.action || window.location.pathname}?handler=Calculate`, {
        method: "POST",
        body: new FormData(calculatorForm),
        headers: {
          "X-Requested-With": "fetch"
        }
      });

      if (!response.ok) {
        throw new Error("Nao foi possivel calcular agora.");
      }

      const payload = await response.json();
      updateResult(payload);
    } catch (error) {
      showError(error.message || "Nao foi possivel calcular agora.");
    } finally {
      setLoading(false);
    }
  }

  function setLoading(isLoading) {
    resultPanel?.classList.toggle("is-updating", isLoading);

    if (submitButton) {
      submitButton.disabled = isLoading;
      submitButton.classList.toggle("is-loading", isLoading);
      submitButton.textContent = isLoading ? "Calculando..." : originalButtonText;
    }
  }

  function updateResult(payload) {
    const result = payload.result ?? {};
    setText(fields.amount, result.amount);
    setText(fields.duration, result.duration);
    setText(fields.minutes, result.minutes);
    setText(fields.hours, result.hours);
    setText(fields.exit, result.exit);

    if (payload.success) {
      hideError();
    } else {
      showError(payload.errorMessage || "Revise os dados informados.");
    }

    resultPanel?.classList.remove("is-updated");
    window.requestAnimationFrame(() => {
      resultPanel?.classList.add("is-updated");
    });
  }

  function showError(message) {
    if (!fields.error) {
      return;
    }

    fields.error.textContent = message;
    fields.error.hidden = false;
  }

  function hideError() {
    if (!fields.error) {
      return;
    }

    fields.error.textContent = "";
    fields.error.hidden = true;
  }

  function setText(element, value) {
    if (element && value !== undefined && value !== null) {
      element.textContent = value;
    }
  }
}
