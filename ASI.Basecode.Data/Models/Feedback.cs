using NUlid;
using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class Feedback
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public int? Rating { get; set; }

    public int SubmittedBy { get; set; }

    public int TicketId { get; set; }

    public DateTime CreatedAt { get; set; }
}
