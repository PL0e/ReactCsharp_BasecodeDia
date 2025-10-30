using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class Preference
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public bool? ShowStats { get; set; }

    public bool? ShowSatisfaction { get; set; }

    public string CardOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
