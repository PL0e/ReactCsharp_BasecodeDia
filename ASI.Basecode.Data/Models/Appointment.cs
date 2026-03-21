using System;

namespace ASI.Basecode.Data.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int AdviserId { get; set; }
        public int SemesterId { get; set; }
        public string AppointmentType { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public TimeSpan? AppointmentTime { get; set; }
        public string Status { get; set; }
        public string CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
