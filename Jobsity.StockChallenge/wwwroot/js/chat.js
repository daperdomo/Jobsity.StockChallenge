(function () {
    const messagesList = document.getElementById("messagesList");
    const messageForm = document.getElementById("messageForm");
    const messageInput = document.getElementById("messageInput");

    function appendMessage(sender, text, timestamp) {
        const item = document.createElement("article");
        item.className = sender === "Stock Bot" ? "chat-message bot-message" : "chat-message";

        const meta = document.createElement("div");
        meta.className = "chat-message-meta";

        const senderElement = document.createElement("strong");
        senderElement.textContent = sender;

        const timeElement = document.createElement("time");
        timeElement.textContent = new Date(timestamp).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });

        const body = document.createElement("p");
        body.textContent = text;

        meta.appendChild(senderElement);
        meta.appendChild(timeElement);
        item.appendChild(meta);
        item.appendChild(body);
        messagesList.appendChild(item);
        messagesList.scrollTop = messagesList.scrollHeight;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .withAutomaticReconnect()
        .build();

    connection.on("LoadMessages", function (messages) {
        messagesList.innerHTML = "";
        messages.forEach(message => {
            const sender = message.sender || message.senderUserName || message.SenderUserName || "";
            const text = message.text || message.message || message.Message || "";
            const timestamp = message.timestamp || message.Timestamp || new Date();
            appendMessage(sender, text, timestamp);
        });
    });

    connection.on("ReceiveMessage", appendMessage);

    messageForm.addEventListener("submit", async function (event) {
        event.preventDefault();
        const message = messageInput.value.trim();

        if (!message) {
            return;
        }

        messageInput.value = "";
        await connection.invoke("SendMessage", message);
    });

    connection.start().catch(function (error) {
        appendMessage("System", "The chat connection could not be started.", new Date());
        console.error(error);
    });
})();
