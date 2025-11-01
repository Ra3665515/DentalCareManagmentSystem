using Microsoft.AspNetCore.SignalR;

namespace DentalCareManagmentSystem.Web.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task AddNewPatient(Guid appointmentId, string patientName)
        {
            await Clients.All.SendAsync("ReceiveNewPatient", appointmentId, patientName);
        }
        public async Task TransferToDoctor(Guid appointmentId, string patientName)
        {
            await Clients.All.SendAsync("PatientTransferred", appointmentId, patientName);
        }
        public async Task CompleteSession(Guid appointmentId, string patientName)
        {
            await Clients.All.SendAsync("PatientCompleted", appointmentId, patientName);
        }
    }
}