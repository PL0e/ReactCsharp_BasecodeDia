using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models;

public partial class KnowledgeBase
{
    public int Id { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public string Author { get; set; }

    public string Category { get; set; }

    public DateTime CreatedAt { get; set; }
}
