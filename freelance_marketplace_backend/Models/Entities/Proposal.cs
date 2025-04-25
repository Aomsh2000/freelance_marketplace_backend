using System;
using System.Collections.Generic;

namespace freelance_marketplace_backend.Models.Entities;

public partial class Proposal
{
    public int ProposalId { get; set; }

    public int ProjectId { get; set; }

    public string FreelancerId { get; set; } = null!;

    public decimal ProposedAmount { get; set; }

    public DateOnly Deadline { get; set; }

    public string CoverLetter { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual User Freelancer { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
