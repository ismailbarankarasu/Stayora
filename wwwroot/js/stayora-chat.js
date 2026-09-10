(() => {
    const root = document.getElementById("stayora-chat");

    if (!root) return;

    const toggle = document.getElementById("stayora-chat-toggle");
    const panel = document.getElementById("stayora-chat-panel");
    const close = document.getElementById("stayora-chat-close");
    const newButton = document.getElementById("stayora-chat-new");
    const form = document.getElementById("stayora-chat-form");
    const input = document.getElementById("stayora-chat-input");
    const send = document.getElementById("stayora-chat-send");
    const messages = document.getElementById("stayora-chat-messages");
    const status = document.getElementById("stayora-chat-status");

    let token = "";
    let storageKey = "";
    let entries = [];
    let busy = false;

    const welcome =
        "Merhaba! Nereye, hangi tarihlerde gitmek istersin? " +
        "Yetişkin ve oda sayısını da yazabilirsin.";

    function setBusy(value) {
        busy = value;
        send.disabled = value;
        newButton.disabled = value;
        input.readOnly = value;
        form.setAttribute("aria-busy", String(value));
    }

    function scrollToEnd() {
        messages.scrollTop = messages.scrollHeight;
    }

    function save() {
        try {
            sessionStorage.setItem(storageKey, JSON.stringify(entries));
        } catch {
            // Depolama kullanılamasa da sohbet çalışmaya devam eder.
        }
    }

    function addText(role, text) {
        const element = document.createElement("div");
        element.className = "stayora-chat-message";

        if (role === "user") {
            element.classList.add("is-user");
        }

        element.textContent = text;
        messages.append(element);
    }

    function money(value, currency) {
        if (typeof value !== "number" || !Number.isFinite(value)) {
            return "Fiyat bilgisi bulunamadı";
        }

        const code = currency === "TRY" ? "TL" : (currency || "");

        return new Intl.NumberFormat("tr-TR", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(value) + " " + code;
    }

    function addHotel(hotel) {
        const card = document.createElement("article");
        card.className = "stayora-chat-hotel";

        if (hotel.photoUrl) {
            try {
                const imageUrl = new URL(hotel.photoUrl);

                if (imageUrl.protocol === "https:") {
                    const image = document.createElement("img");
                    image.src = imageUrl.href;
                    image.alt = hotel.name;
                    image.loading = "lazy";
                    image.addEventListener("error", () => image.remove());
                    card.append(image);
                }
            } catch {
                // Geçersiz fotoğraf bağlantısını gösterme.
            }
        }

        const body = document.createElement("div");
        body.className = "stayora-chat-hotel-body";

        const title = document.createElement("h4");
        title.textContent = hotel.name;
        body.append(title);

        function paragraph(text) {
            const element = document.createElement("p");
            element.textContent = text;
            body.append(element);
        }

        if (hotel.reviewScore != null) {
            paragraph(
                `${hotel.reviewScore} / 10` +
                (hotel.reviewCount != null
                    ? ` · ${hotel.reviewCount} değerlendirme`
                    : "")
            );
        }

        paragraph(money(hotel.price, hotel.currency));

        if (hotel.price != null) {
            paragraph("Seçilen konaklama için gösterilen fiyat");
        }

        if (hotel.excludedPrice > 0) {
            paragraph(
                "Ek vergi ve ücretler: " +
                money(hotel.excludedPrice, hotel.excludedPriceCurrency)
            );
        }

        for (const condition of hotel.priceConditions || []) {
            paragraph(condition);
        }

        try {
            if (!hotel.detailsUrl) throw new Error("Eksik bağlantı");

            const url = new URL(hotel.detailsUrl, location.origin);

            if (url.origin === location.origin) {
                const link = document.createElement("a");
                link.href = url.href;
                link.textContent = "Oteli İncele";
                body.append(link);
            }
        } catch {
            // Geçersiz bağlantıyı gösterme.
        }

        card.append(body);
        messages.append(card);
    }

    function render() {
        messages.replaceChildren();
        addText("assistant", welcome);

        for (const entry of entries) {
            addText(entry.role, entry.text);

            for (const hotel of entry.hotels || []) {
                addHotel(hotel);
            }
        }

        scrollToEnd();
    }

    async function readResponse(response) {
        const contentType = response.headers.get("content-type") || "";

        if (!contentType.includes("application/json")) {
            throw new Error("Yanıt alınamadı. Sayfayı yenileyip tekrar deneyin.");
        }

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || "İşlem tamamlanamadı.");
        }

        return data;
    }

    async function bootstrap() {
        const response = await fetch(root.dataset.bootstrapUrl, {
            credentials: "same-origin",
            cache: "no-store"
        });

        const data = await readResponse(response);

        token = data.requestToken;
        storageKey = "stayora-chat:" + data.conversationId;

        try {
            const stored = JSON.parse(
                sessionStorage.getItem(storageKey) || "[]"
            );

            entries = Array.isArray(stored) ? stored.slice(-40) : [];
        } catch {
            entries = [];
        }

        render();
    }

    async function post(url, body) {
        body.set("__RequestVerificationToken", token);

        return readResponse(await fetch(url, {
            method: "POST",
            body,
            credentials: "same-origin"
        }));
    }

    async function openPanel() {
        panel.hidden = false;
        toggle.setAttribute("aria-expanded", "true");

        if (!token) {
            setBusy(true);
            status.textContent = "Sohbet hazırlanıyor…";

            try {
                await bootstrap();
                status.textContent = "";
            } catch (error) {
                status.textContent = error.message;
            } finally {
                setBusy(false);
            }
        }

        input.focus();
        scrollToEnd();
    }

    function closePanel() {
        panel.hidden = true;
        toggle.setAttribute("aria-expanded", "false");
        toggle.focus();
    }

    toggle.addEventListener("click", () => {
        if (panel.hidden) {
            openPanel();
        } else {
            closePanel();
        }
    });

    close.addEventListener("click", closePanel);

    root.addEventListener("keydown", event => {
        if (event.key === "Escape" && !panel.hidden) {
            closePanel();
        }
    });

    form.addEventListener("submit", async event => {
        event.preventDefault();

        if (busy || !form.reportValidity()) return;

        const message = input.value.trim();

        if (!message) return;

        if (!token) {
            status.textContent =
                "Sohbet bağlantısı hazır değil. Pencereyi kapatıp tekrar açın.";
            return;
        }

        setBusy(true);
        status.textContent = "Asistan yanıt hazırlıyor…";

        try {
            const body = new FormData();
            body.set("Message", message);

            const data = await post(root.dataset.sendUrl, body);

            entries.push({ role: "user", text: message });
            entries.push({
                role: "assistant",
                text: data.message,
                hotels: data.hotels || []
            });

            entries = entries.slice(-40);
            save();
            render();

            input.value = "";
            status.textContent = "";
        } catch (error) {
            status.textContent = error.message;
        } finally {
            setBusy(false);

            if (!panel.hidden) input.focus();
        }
    });

    newButton.addEventListener("click", async () => {
        if (busy || !token) return;

        setBusy(true);

        try {
            await post(root.dataset.newUrl, new FormData());

            try {
                sessionStorage.removeItem(storageKey);
            } catch {
                // Depolama zorunlu değil.
            }

            entries = [];
            token = "";

            await bootstrap();
            status.textContent = "Yeni sohbet başlatıldı.";
        } catch (error) {
            status.textContent = error.message;
        } finally {
            setBusy(false);
            input.focus();
        }
    });
})();