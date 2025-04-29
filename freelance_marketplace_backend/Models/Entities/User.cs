using System;
using System.Collections.Generic;

namespace freelance_marketplace_backend.Models.Entities;

public partial class User
{
    public string Usersid { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public string? AboutMe { get; set; }

    public decimal Rating { get; set; }

    public decimal Balance { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<Chat> ChatClients { get; set; } = new List<Chat>();

    public virtual ICollection<Chat> ChatFreelancers { get; set; } = new List<Chat>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Payment> PaymentClients { get; set; } = new List<Payment>();

    public virtual ICollection<Payment> PaymentFreelancers { get; set; } = new List<Payment>();

    public virtual ICollection<Project> ProjectFreelancers { get; set; } = new List<Project>();

    public virtual ICollection<Project> ProjectPostedByNavigations { get; set; } = new List<Project>();

    public virtual ICollection<Proposal> Proposals { get; set; } = new List<Proposal>();

    public virtual ICollection<Review> ReviewFromUsers { get; set; } = new List<Review>();

    public virtual ICollection<Review> ReviewToUsers { get; set; } = new List<Review>();

    public virtual ICollection<UsersSkill> UsersSkills { get; set; } = new List<UsersSkill>();
}
