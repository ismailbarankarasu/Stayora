(() => {
    const form = document.getElementById("chat-test-form");

    if (!form) {
        return;
    }

    const input = document.getElementById("chat-message");
    const sendButton = document.getElementById("chat-send-button");
    const newButton = document.getElementById("chat-new-button");
    const status = document.getElementById("chat-status");
    const history = document.getElementById("chat-test-history");

    let busy = false;

    function setBusy(value) {
        busy = value;
        sendButton.disabled = value;
        newButton.disabled = value;
        input.readOnly = value;
        form.setAttribute("aria-busy", String(value));
    }

    function appendEntry(title, text) {
        const block = document.createElement("div");
        block.className = "border rounded p-3 mb-3";

        const heading = document.createElement("strong");
        heading.textContent = title;

        const body = document.createElement("pre");
        body.className = "mb-0 mt-2";
        body.style.whiteSpace = "pre-wrap";
        body.style.overflowWrap = "anywhere";
        body.textContent = text;

        block.append(heading, body);
        history.append(block);
    }

    async function post(url, body) {
        const response = await fetch(url, {
            method: "POST",
            body,
            credentials: "same-origin"
        });

        const contentType = response.headers.get("content-type") || "";

        if (!contentType.includes("application/json")) {
            throw new Error(
                "Beklenen yanıt alınamadı. Sayfayı yenileyip tekrar deneyin."
            );
        }

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || "İşlem başarısız oldu.");
        }

        return data;
    }

    form.addEventListener("submit", async event => {
        event.preventDefault();

        if (busy || !form.reportValidity()) {
            return;
        }

        const message = input.value.trim();

        if (!message) {
            status.textContent = "Lütfen bir mesaj yazın.";
            return;
        }

        const body = new FormData(form);
        body.set("Message", message);

        setBusy(true);
        status.textContent = "Asistan yanıt hazırlıyor…";

        try {
            const data = await post(form.action, body);

            appendEntry("Siz", message);
            appendEntry("Stayora AI", data.message);

            if (data.hotels?.length) {
                appendEntry(
                    "Gerçek otel sonuçları",
                    JSON.stringify(data.hotels, null, 2)
                );
            }

            input.value = "";
            status.textContent = "";
        } catch (error) {
            status.textContent = error.message;
        } finally {
            setBusy(false);
            input.focus();
        }
    });

    newButton.addEventListener("click", async () => {
        if (busy) {
            return;
        }

        setBusy(true);

        try {
            const body = new FormData();

            body.set(
                "__RequestVerificationToken",
                form.querySelector(
                    'input[name="__RequestVerificationToken"]'
                ).value
            );

            const data = await post(newButton.dataset.url, body);

            history.replaceChildren();
            input.value = "";
            status.textContent = data.message;
        } catch (error) {
            status.textContent = error.message;
        } finally {
            setBusy(false);
            input.focus();
        }
    });
})();