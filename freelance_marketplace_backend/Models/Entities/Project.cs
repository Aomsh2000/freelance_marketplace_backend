using System;
using System.Collections.Generic;

namespace freelance_marketplace_backend.Models.Entities;

public partial class Project
{
    public int ProjectId { get; set; }

    public string Title { get; set; } = null!;

    public string Overview { get; set; } = null!;

    public string RequiredTasks { get; set; } = null!;

    public string AdditionalNotes { get; set; } = null!;

    public decimal Budget { get; set; }

    public DateOnly Deadline { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public string? FreelancerId { get; set; }
    public string? PostedBy { get; set; }

    public virtual User? Freelancer { get; set; }
    public virtual User? Client { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();

    public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
