using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models.Api
{
    public class AdviserAvailabilityResponse
    {
        public int AvailabilityId { get; set; }
        public int AdviserId { get; set; }
        public string DayOfWeek { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Location { get; set; }
    }

    public class UpsertAdviserAvailabilityRequest
    {
        public string DayOfWeek { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Location { get; set; }
    }

    public class SaveMyAdviserAvailabilitiesRequest
    {
        public List<UpsertAdviserAvailabilityRequest> Availabilities { get; set; } = new();
    }
}
