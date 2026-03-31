using System;

namespace ASI.Basecode.Data.Models
{
    public class AppointmentNote
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public int? AdviserId { get; set; }
        public string AdviserNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string DeleteName { get; set; }
    }
}
