(() => {
    const forms = document.querySelectorAll(
        "[data-hotel-search-form]"
    );

    function resetFormState(form) {
        form.dataset.submitting = "false";
        form.removeAttribute("aria-busy");

        const button = form.querySelector('button[type="submit"]');
        const text = form.querySelector("[data-search-button-text]");
        const spinner = form.querySelector("[data-search-spinner]");
        const status = form.querySelector("[data-search-status]");

        button.disabled = false;
        text.textContent = "Otelleri Ara";
        spinner.hidden = true;
        status.textContent = "";
    }

    forms.forEach(form => {
        form.addEventListener("submit", event => {
            if (form.dataset.submitting === "true") {
                event.preventDefault();
                return;
            }

            if (!form.checkValidity()) {
                event.preventDefault();
                form.reportValidity();
                return;
            }

            form.dataset.submitting = "true";
            form.setAttribute("aria-busy", "true");

            form.querySelector('button[type="submit"]').disabled = true;

            form.querySelector("[data-search-button-text]")
                .textContent = "Aranıyor…";

            form.querySelector("[data-search-spinner]").hidden = false;

            form.querySelector("[data-search-status]")
                .textContent = "Oteller getiriliyor, lütfen bekleyin.";
        });
    });

    window.addEventListener("pageshow", () => {
        forms.forEach(resetFormState);
    });
})();