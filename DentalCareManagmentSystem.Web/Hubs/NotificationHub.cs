using Microsoft.AspNetCore.SignalR;

namespace DentalCareManagmentSystem.Web.Hubs
{
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Called when a new patient is added (legacy method)
        /// </summary>
        public async Task AddNewPatient(Guid appointmentId, string patientName)
        {
            await Clients.All.SendAsync("ReceiveNewPatient", appointmentId, patientName);
        }

        /// <summary>
        /// Called when a patient is transferred to doctor
        /// </summary>
        public async Task TransferToDoctor(Guid appointmentId, string patientName)
        {
            await Clients.All.SendAsync("PatientTransferred", appointmentId, patientName);
        }

        /// <summary>
        /// Called when a doctor completes a patient session
        /// </summary>
        public async Task CompleteSession(Guid appointmentId, string patientName)
        {
            await Clients.All.SendAsync("PatientCompleted", appointmentId, patientName);
        }

        /// <summary>
        /// Override OnConnectedAsync to handle client connections
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            Console.WriteLine($"Client connected: {connectionId}");
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Override OnDisconnectedAsync to handle client disconnections
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            Console.WriteLine($"Client disconnected: {connectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}