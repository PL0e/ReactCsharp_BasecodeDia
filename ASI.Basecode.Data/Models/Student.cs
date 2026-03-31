using System;

namespace ASI.Basecode.Data.Models
{
    public class Student
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? YearLevelId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string DeleteName { get; set; }
    }
}
