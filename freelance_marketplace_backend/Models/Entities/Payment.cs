using System;
using System.Collections.Generic;

namespace freelance_marketplace_backend.Models.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int ProjectId { get; set; }

    public string ClientId { get; set; } = null!;

    public string FreelancerId { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual User Client { get; set; } = null!;

    public virtual User Freelancer { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
