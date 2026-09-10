document.querySelectorAll("[data-destination-city]")
    .forEach(link => {
        link.addEventListener("click", event => {
            const searchArea = document.getElementById("hotel-search");

            const cityInput = searchArea?.querySelector(
                'input[name="City"]'
            );

            if (!searchArea || !cityInput) {
                return;
            }

            event.preventDefault();

            cityInput.value = link.dataset.destinationCity;
            cityInput.dispatchEvent(
                new Event("input", { bubbles: true })
            );

            const reduceMotion = window.matchMedia(
                "(prefers-reduced-motion: reduce)"
            ).matches;

            cityInput.focus({ preventScroll: true });

            searchArea.scrollIntoView({
                behavior: reduceMotion ? "auto" : "smooth",
                block: "start"
            });
        });
    });