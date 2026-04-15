using System;

namespace ASI.Basecode.WebApp.Models.Api
{
    public class AdviserAssignmentResponse
    {
        public int AdviserAssignmentId { get; set; }
        public int AdviserId { get; set; }
        public int YearLevelId { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string DeleteName { get; set; }
    }

    public class UpsertAdviserAssignmentRequest
    {
        public int AdviserId { get; set; }
        public int YearLevelId { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
