(() => {
    const menu = document.querySelector(".offcanvas-menu-wrapper");
    const overlay = document.querySelector(".offcanvas-menu-overlay");
    const openButton = document.querySelector(".canvas-open");

    if (!menu || !overlay) {
        return;
    }

    function closeMenu() {
        menu.classList.remove("show-offcanvas-menu-wrapper");
        overlay.classList.remove("active");
    }

    menu.addEventListener("click", event => {
        if (!(event.target instanceof Element)) {
            return;
        }

        const link = event.target.closest("a[href]");

        if (!link) {
            return;
        }

        const href = link.getAttribute("href");

        if (href && href !== "#") {
            closeMenu();
        }
    });

    document.addEventListener("keydown", event => {
        if (event.key === "Escape" &&
            menu.classList.contains("show-offcanvas-menu-wrapper")) {
            closeMenu();
            openButton?.focus();
        }
    });
})();