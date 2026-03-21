namespace ASI.Basecode.Data.Models
{
    public class Student
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? YearLevelId { get; set; }
    }
}
