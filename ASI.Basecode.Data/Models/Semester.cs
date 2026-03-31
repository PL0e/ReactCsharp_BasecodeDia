using System;

namespace ASI.Basecode.Data.Models
{
    public class Semester
    {
        public int Id { get; set; }
        public string SemesterName { get; set; }
        public string SchoolYear { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string DeleteName { get; set; }
    }
}
