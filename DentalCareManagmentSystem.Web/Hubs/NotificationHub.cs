using Microsoft.AspNetCore.SignalR;

namespace DentalCareManagmentSystem.Web.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly List<PatientQueueItem> _patientQueue = new();
        private static string _currentDoctorStatus = "available";

        public async Task JoinDoctorRoom(string doctorId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"doctor-{doctorId}");
            // إرسال حالة الطابور الحالية للدكتور
            await Clients.Group($"doctor-{doctorId}").SendAsync("UpdateQueue", _patientQueue);
            Console.WriteLine($"Doctor {doctorId} joined room");
        }

        public async Task JoinReceptionRoom()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "reception");
            // إرسال حالة الطابور الحالية للريسيبشن
            await Clients.Group("reception").SendAsync("UpdateQueue", _patientQueue);
            Console.WriteLine("Reception joined room");
        }

        public async Task AddPatientToQueue(string patientName, Guid appointmentId, string appointmentTime)
        {
            var queueItem = new PatientQueueItem
            {
                PatientName = patientName,
                AppointmentId = appointmentId,
                AppointmentTime = appointmentTime,
                ArrivalTime = DateTime.Now.ToString("hh:mm tt"),
                Status = "Waiting",
                QueueNumber = _patientQueue.Count + 1
            };

            _patientQueue.Add(queueItem);

            // إرسال التحديث للجميع
            await Clients.Group("reception").SendAsync("UpdateQueue", _patientQueue);
            await Clients.All.SendAsync("ReceiveNewPatient", patientName, appointmentId, appointmentTime, queueItem.QueueNumber);

            Console.WriteLine($"Patient {patientName} added to queue");
        }

        public async Task PatientCompleted(Guid appointmentId)
        {
            var completedPatient = _patientQueue.FirstOrDefault(p => p.AppointmentId == appointmentId);
            if (completedPatient != null)
            {
                completedPatient.Status = "Completed";
                completedPatient.CompletedTime = DateTime.Now.ToString("hh:mm tt");

                // إرسال التحديث للريسيبشن
                await Clients.Group("reception").SendAsync("UpdateQueue", _patientQueue);
                await Clients.Group("reception").SendAsync("PatientCompleted", completedPatient.PatientName);

                // إرسال للمريض التالي إذا وجد
                var nextPatient = _patientQueue.FirstOrDefault(p => p.Status == "Waiting");
                if (nextPatient != null)
                {
                    await Clients.All.SendAsync("NextPatientReady", nextPatient.PatientName, nextPatient.AppointmentId, nextPatient.QueueNumber);
                }

                Console.WriteLine($"Patient {completedPatient.PatientName} marked as completed");
            }
        }

        public async Task CallNextPatient()
        {
            var nextPatient = _patientQueue.FirstOrDefault(p => p.Status == "Waiting");
            if (nextPatient != null)
            {
                nextPatient.Status = "In Progress";

                await Clients.All.SendAsync("NextPatientCalled", nextPatient.PatientName, nextPatient.AppointmentId);
                await Clients.Group("reception").SendAsync("UpdateQueue", _patientQueue);

                Console.WriteLine($"Next patient called: {nextPatient.PatientName}");
            }
        }

        public async Task RemoveFromQueue(Guid appointmentId)
        {
            var patient = _patientQueue.FirstOrDefault(p => p.AppointmentId == appointmentId);
            if (patient != null)
            {
                _patientQueue.Remove(patient);

                await Clients.Group("reception").SendAsync("UpdateQueue", _patientQueue);
                await Clients.All.SendAsync("PatientRemovedFromQueue", patient.PatientName);

                Console.WriteLine($"Patient {patient.PatientName} removed from queue");
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }

    public class PatientQueueItem
    {
        public string PatientName { get; set; }
        public Guid AppointmentId { get; set; }
        public string AppointmentTime { get; set; }
        public string ArrivalTime { get; set; }
        public string Status { get; set; } // Waiting, In Progress, Completed
        public int QueueNumber { get; set; }
        public string CompletedTime { get; set; }
    }
}