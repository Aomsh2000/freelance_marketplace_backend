using System;
using System.Collections.Generic;

namespace freelance_marketplace_backend.Models.Entities;

public partial class Chat
{
    public int ChatId { get; set; }

    public string ClientId { get; set; } = null!;

    public string FreelancerId { get; set; } = null!;

    public DateTime? StartedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual User Client { get; set; } = null!;

    public virtual User Freelancer { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
