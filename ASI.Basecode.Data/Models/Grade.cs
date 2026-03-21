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
        public DateTime CreatedAt { get; set; }
    }
}
