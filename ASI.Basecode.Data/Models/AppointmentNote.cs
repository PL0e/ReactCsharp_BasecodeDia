using System;

namespace ASI.Basecode.Data.Models
{
    public class AppointmentNote
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
