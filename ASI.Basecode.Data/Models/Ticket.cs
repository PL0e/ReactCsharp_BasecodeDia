using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class Ticket
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string Category { get; set; }

    public string Priority { get; set; }

    public string Attachment { get; set; }

    public int? AssignedTo { get; set; }

    public DateTime? DueDate { get; set; }

    public string Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public int CreatedBy { get; set; }
}
