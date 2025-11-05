using Microsoft.AspNetCore.SignalR;

namespace DentalCareManagmentSystem.Web.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly List<PatientQueueItem> _patientQueue = new();

        // انضمام الدكتور
        public async Task JoinDoctorRoom(string doctorId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"doctor-{doctorId}");
            // إرسال الـ Queue الحالي للدكتور فور انضمامه
            await Clients.Caller.SendAsync("UpdateQueue", _patientQueue);
            Console.WriteLine($"Doctor {doctorId} joined room - Queue sent: {_patientQueue.Count} patients");
        }

        // انضمام الريسيبشن
        public async Task JoinReceptionRoom()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "reception");
            // إرسال الـ Queue الحالي للريسيبشن فور انضمامها
            await Clients.Caller.SendAsync("UpdateQueue", _patientQueue);
            Console.WriteLine("Reception joined room - Queue sent: " + _patientQueue.Count);
        }

        // إضافة مريض جديد للطابور
        public async Task AddPatientToQueue(string patientName, Guid appointmentId, string appointmentTime)
        {
            var queueItem = new PatientQueueItem
            {
                PatientName = patientName,
                AppointmentId = appointmentId,
                AppointmentTime = appointmentTime,
                ArrivalTime = DateTime.Now.ToString("HH:mm"),
                Status = "Waiting",
                QueueNumber = _patientQueue.Count + 1
            };

            _patientQueue.Add(queueItem);

            // 🔥 إرسال التحديث للجميع (الريسيبشن + جميع الأطباء)
            await Clients.All.SendAsync("UpdateQueue", _patientQueue);
            await Clients.All.SendAsync("ReceiveNewPatient", patientName, appointmentId, appointmentTime, queueItem.QueueNumber);

            Console.WriteLine($"✅ Patient {patientName} added to queue. Total in queue: {_patientQueue.Count}");
        }

        // 🔥 دالة Call Next Patient الجديدة
        public async Task CallNextPatient()
        {
            Console.WriteLine(" CallNextPatient method called");

            // المريض الحالي اللي status = "In Progress"
            var currentPatient = _patientQueue.FirstOrDefault(p => p.Status == "In Progress");
            if (currentPatient != null)
            {
                // إكمال المريض الحالي
                currentPatient.Status = "Completed";
                currentPatient.CompletedTime = DateTime.Now.ToString("HH:mm");
                Console.WriteLine($" Completed patient: {currentPatient.PatientName}");
            }

            // جلب أول مريض في حالة Waiting
            var nextPatient = _patientQueue.FirstOrDefault(p => p.Status == "Waiting");
            if (nextPatient != null)
            {
                // تحويل status لـ "In Progress"
                nextPatient.Status = "In Progress";
                Console.WriteLine($"🎯 Next patient called: {nextPatient.PatientName}");

                // إرسال إشعار للمريض التالي
                await Clients.All.SendAsync("NextPatientCalled", nextPatient.PatientName, nextPatient.AppointmentId, nextPatient.QueueNumber);
            }
            else
            {
                Console.WriteLine("📭 No more patients in queue");
                await Clients.All.SendAsync("QueueEmpty");
            }

            // 🔥 إرسال تحديث الـ Queue للجميع
            await Clients.All.SendAsync("UpdateQueue", _patientQueue);

            Console.WriteLine($"📊 Queue updated. Total: {_patientQueue.Count}, Waiting: {_patientQueue.Count(p => p.Status == "Waiting")}");
        }

        // إزالة مريض من الطابور
        public async Task RemoveFromQueue(Guid appointmentId)
        {
            var patient = _patientQueue.FirstOrDefault(p => p.AppointmentId == appointmentId);
            if (patient != null)
            {
                _patientQueue.Remove(patient);

                // تحديث أرقام الطابور
                UpdateQueueNumbers();

                await Clients.All.SendAsync("UpdateQueue", _patientQueue);
                await Clients.All.SendAsync("PatientRemovedFromQueue", patient.PatientName);

                Console.WriteLine($"🗑️ Patient {patient.PatientName} removed from queue");
            }
        }

        // تحديث أرقام الطابور
        private void UpdateQueueNumbers()
        {
            var waitingPatients = _patientQueue.Where(p => p.Status == "Waiting").OrderBy(p => p.QueueNumber).ToList();
            var inProgressPatient = _patientQueue.FirstOrDefault(p => p.Status == "In Progress");
            var completedPatients = _patientQueue.Where(p => p.Status == "Completed").ToList();

            // إعادة ترقيم المرضى في الانتظار
            for (int i = 0; i < waitingPatients.Count; i++)
            {
                waitingPatients[i].QueueNumber = i + 1;
            }

            // ترقيم المريض الحالي
            if (inProgressPatient != null)
            {
                inProgressPatient.QueueNumber = 0; // أو رقم خاص
            }
        }

        // الحصول على الـ Queue الحالي
        public async Task GetCurrentQueue()
        {
            await Clients.Caller.SendAsync("UpdateQueue", _patientQueue);
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($" Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
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