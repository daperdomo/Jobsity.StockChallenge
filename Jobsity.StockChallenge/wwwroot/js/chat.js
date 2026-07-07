(function () {
    const messagesList = document.getElementById("messagesList");
    const messageForm = document.getElementById("messageForm");
    const messageInput = document.getElementById("messageInput");
    const roomTabs = Array.from(document.querySelectorAll(".chat-room-tab"));
    let currentRoom = roomTabs.find(tab => tab.classList.contains("active"))?.dataset.room || "General";

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

    function setActiveRoom(room) {
        currentRoom = room;
        roomTabs.forEach(tab => {
            tab.classList.toggle("active", tab.dataset.room === room);
        });
    }

    async function joinRoom(room) {
        setActiveRoom(room);
        await connection.invoke("JoinRoom", room);
    }

    connection.on("LoadMessages", function (room, messages) {
        setActiveRoom(room);
        messagesList.innerHTML = "";
        messages.forEach(message => {
            appendMessage(message.senderUserName, message.message, message.timestamp);
        });
    });

    connection.on("ReceiveMessage", appendMessage);

    roomTabs.forEach(tab => {
        tab.addEventListener("click", async function () {
            if (tab.dataset.room === currentRoom) {
                return;
            }

            await joinRoom(tab.dataset.room);
        });
    });

    messageForm.addEventListener("submit", async function (event) {
        event.preventDefault();
        const message = messageInput.value.trim();

        if (!message) {
            return;
        }

        messageInput.value = "";
        await connection.invoke("SendMessage", message, currentRoom);
    });

    connection
        .start()
        .then(() => joinRoom(currentRoom))
        .catch(function (error) {
            appendMessage("System", "The chat connection could not be started.", new Date());
            console.error(error);
        });
})();
