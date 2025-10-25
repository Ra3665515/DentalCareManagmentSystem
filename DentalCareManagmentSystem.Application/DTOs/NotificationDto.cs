using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalCareManagmentSystem.Application.DTOs
{
    
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string? PatientName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Message { get; set; }
        public DateTime NotifiedAt { get; set; }
        public bool IsDone { get; set; }
    }

}
