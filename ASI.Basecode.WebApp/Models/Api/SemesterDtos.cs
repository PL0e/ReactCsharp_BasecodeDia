using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Models.Api
{
    public class SemesterResponse
    {
        public int SemesterId { get; set; }
        public string Name { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
