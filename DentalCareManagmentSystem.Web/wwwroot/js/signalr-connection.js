console.log("signalr-connection.js was loaded.");

(function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    async function start() {
        try {
            await connection.start();
            console.log("SignalR Connected.");
            return connection;
        } catch (err) {
            console.error("SignalR Connection Failed: ", err);
            return null;
        }
    }

    window.appConnectionPromise = start();
})();