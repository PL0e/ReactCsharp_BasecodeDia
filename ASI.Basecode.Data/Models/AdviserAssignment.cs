using System;

namespace ASI.Basecode.Data.Models
{
    public class AdviserAssignment
    {
        public int Id { get; set; }
        public int AdviserId { get; set; }
        public int YearLevelId { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
