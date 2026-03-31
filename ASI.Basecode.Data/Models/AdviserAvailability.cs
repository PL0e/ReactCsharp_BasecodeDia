using System;

namespace ASI.Basecode.Data.Models
{
    public class AdviserAvailability
    {
        public int Id { get; set; }
        public int AdviserId { get; set; }
        public string DayOfWeek { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Location { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string DeleteName { get; set; }
    }
}
