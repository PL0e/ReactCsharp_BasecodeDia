using System;

namespace ASI.Basecode.Data.Models
{
    public class Grade
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int SemesterId { get; set; }
        public string GradeValue { get; set; }
        public int? Units { get; set; }
        public int NumberOfTakes { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string DeleteName { get; set; }
    }
}
