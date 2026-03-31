using System;

namespace ASI.Basecode.Data.Models
{
    public class YearLevel
    {
        public int Id { get; set; }
        public string YearName { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string DeleteName { get; set; }
    }
}
