using System;

namespace ASI.Basecode.Data.Models
{
    public class AdviserAssignmentHistory
    {
        public int Id { get; set; }
        public int? AdviserId { get; set; }
        public int? YearLevelId { get; set; }
        public int? AssignedBy { get; set; }
        public string Action { get; set; }
        public DateTime ActionDate { get; set; }
    }
}
