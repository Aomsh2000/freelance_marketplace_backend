using System;
using System.Collections.Generic;

namespace freelance_marketplace_backend.Models.Entities;

public partial class Message
{
    public int MessageId { get; set; }

    public int ChatId { get; set; }

    public string SenderId { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime? SentAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Chat Chat { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
