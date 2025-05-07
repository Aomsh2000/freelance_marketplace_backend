using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using freelance_marketplace_backend.Models.Entities;

namespace freelance_marketplace_backend.Data;

public partial class FreelancingPlatformContext : DbContext
{
    public FreelancingPlatformContext()
    {
    }

    public FreelancingPlatformContext(DbContextOptions<FreelancingPlatformContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectSkill> ProjectSkills { get; set; }

    public virtual DbSet<Proposal> Proposals { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UsersSkill> UsersSkills { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=freelancemarketplace.database.windows.net;Database=Freelancer;User Id=manager;Password=y93@34t@BYyRj5g;Encrypt=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("PK__CHAT__8263854D9027036E");

            entity.ToTable("CHAT");

            entity.HasIndex(e => new { e.ClientId, e.FreelancerId }, "UC_Chat").IsUnique();

            entity.Property(e => e.ChatId).HasColumnName("chatID");
            entity.Property(e => e.ClientId)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("clientID");
            entity.Property(e => e.FreelancerId)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("freelancerID");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("isDeleted");
            entity.Property(e => e.StartedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("startedAt");

            entity.HasOne(d => d.Client).WithMany(p => p.ChatClients)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHAT__clientID__4D5F7D71");

            entity.HasOne(d => d.Freelancer).WithMany(p => p.ChatFreelancers)
                .HasForeignKey(d => d.FreelancerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHAT__freelancer__4E53A1AA");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__MESSAGE__4808B87327CAA109");

            entity.ToTable("MESSAGE");

            entity.Property(e => e.MessageId).HasColumnName("messageID");
            entity.Property(e => e.ChatId).HasColumnName("chatID");
            entity.Property(e => e.Content)
                .IsUnicode(false)
                .HasColumnName("content");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("isDeleted");
            entity.Property(e => e.SenderId)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("senderID");
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("sentAt");

            entity.HasOne(d => d.Chat).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MESSAGE__chatID__531856C7");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MESSAGE__senderI__540C7B00");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__PAYMENT__A0D9EFA6F0ECBBBE");

            entity.ToTable("PAYMENT");

            entity.Property(e => e.PaymentId).HasColumnName("paymentID");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.ClientId)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("clientID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.FreelancerId)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("freelancerID");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("isDeleted");
            entity.Property(e => e.PaymentDate)
                .HasColumnType("datetime")
                .HasColumnName("paymentDate");
            entity.Property(e => e.ProjectId).HasColumnName("projectID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");

            entity.HasOne(d => d.Client).WithMany(p => p.PaymentClients)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PAYMENT__clientI__46B27FE2");

            entity.HasOne(d => d.Freelancer).WithMany(p => p.PaymentFreelancers)
                .HasForeignKey(d => d.FreelancerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PAYMENT__freelan__47A6A41B");

            entity.HasOne(d => d.Project).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PAYMENT__project__45BE5BA9");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__PROJECT__11F14D8560A34DFB");

            entity.ToTable("PROJECT");

            entity.Property(e => e.ProjectId).HasColumnName("projectID");
            entity.Property(e => e.AdditionalNotes)
                .IsUnicode(false)
                .HasColumnName("additionalNotes");
            entity.Property(e => e.Budget)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("budget");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.FreelancerId)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("freelancerID");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("isDeleted");
            entity.Property(e => e.Overview)
                .IsUnicode(false)
                .HasColumnName("overview");
            entity.Property(e => e.PostedBy)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("postedBy");
            entity.Property(e => e.RequiredTasks)
                .IsUnicode(false)
                .HasColumnName("requiredTasks");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("title");

            entity.HasOne(d => d.Freelancer).WithMany(p => p.ProjectFreelancers)
                .HasForeignKey(d => d.FreelancerId)
                .HasConstraintName("FK__PROJECT__freelan__2BFE89A6");

            entity.HasOne(d => d.PostedByNavigation).WithMany(p => p.ProjectPostedByNavigations)
                .HasForeignKey(d => d.PostedBy)
                .HasConstraintName("FK__PROJECT__postedB__607251E5");
        });

        modelBuilder.Entity<ProjectSkill>(entity =>
        {
            entity.HasKey(e => e.ProjectSkillId).HasName("PK__PROJECT___0FA3C543CB6B105F");

            entity.ToTable("PROJECT_SKILL");

            entity.HasIndex(e => new { e.ProjectId, e.SkillId }, "UC_ProjectSkill").IsUnique();

            entity.Property(e => e.ProjectSkillId).HasColumnName("projectSkillID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("isDeleted");
            entity.Property(e => e.ProjectId).HasColumnName("projectID");
            entity.Property(e => e.SkillId).HasColumnName("skillID");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectSkills)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PROJECT_S__proje__31B762FC");

            entity.HasOne(d => d.Skill).WithMany(p => p.ProjectSkills)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PROJECT_S__skill__32AB8735");
        });

        modelBuilder.Entity<Proposal>(entity =>
        {
            entity.HasKey(e => e.ProposalId).HasName("PK__PROPOSAL__3EB9E8746D64B70A");

            entity.ToTable("PROPOSAL");

            entity.Property(e => e.ProposalId).HasColumnName("proposalID");
            entity.Property(e => e.CoverLetter)
                .IsUnicode(false)
                .HasColumnName("coverLetter");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Deadline).HasColumnName("deadline");
            entity.Property(e => e.FreelancerId)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("freelancerID");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("isDeleted");
            entity.Property(e => e.ProjectId).HasColumnName("projectID");
            entity.Property(e => e.ProposedAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("proposedAmount");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("status");

            entity.HasOne(d => d.Freelancer).WithMany(p => p.Proposals)
                .HasForeignKey(d => d.FreelancerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PROPOSAL__freela__395884C4");

            entity.HasOne(d => d.Project).WithMany(p => p.Proposals)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PROPOSAL__projec__3864608B");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__REVIEW__2ECD6E24BEA509B4");

            entity.ToTable("REVIEW");

            entity.Property(e => e.ReviewId).HasColumnName("reviewID");
            entity.Property(e => e.Comment)
                .IsUnicode(false)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.FromUsersid)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("fromUSERSID");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("isDeleted");
            entity.Property(e => e.ProjectId).HasColumnName("projectID");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ToUsersid)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("toUSERSID");

            entity.HasOne(d => d.FromUsers).WithMany(p => p.ReviewFromUsers)
                .HasForeignKey(d => d.FromUsersid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REVIEW__fromUSER__3F115E1A");

            entity.HasOne(d => d.Project).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REVIEW__projectI__3E1D39E1");

            entity.HasOne(d => d.ToUsers).WithMany(p => p.ReviewToUsers)
                .HasForeignKey(d => d.ToUsersid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__REVIEW__toUSERSI__40058253");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("PK__SKILL__AE6A6BDF38F40BFD");

            entity.ToTable("SKILL");

            entity.Property(e => e.SkillId).HasColumnName("skillID");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("isDeleted");
            entity.Property(e => e.Skill1)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("skill");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Usersid).HasName("PK__USERS__F88716D8C630D2E6");

            entity.ToTable("USERS");

            entity.HasIndex(e => e.Email, "UQ_USERS_Email").IsUnique();

            entity.Property(e => e.Usersid)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("USERSID");
            entity.Property(e => e.AboutMe)
                .IsUnicode(false)
                .HasColumnName("aboutMe");
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("imageUrl");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("isDeleted");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Rating)
                .HasColumnType("decimal(2, 1)")
                .HasColumnName("rating");
        });

        modelBuilder.Entity<UsersSkill>(entity =>
        {
            entity.HasKey(e => e.UsersskillId).HasName("PK__USERS_SK__7D40E8B1D1BEEAD6");

            entity.ToTable("USERS_SKILL");

            entity.HasIndex(e => new { e.Usersid, e.SkillId }, "UC_USERSSkill").IsUnique();

            entity.Property(e => e.UsersskillId).HasColumnName("USERSSkillID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdAt");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("isDeleted");
            entity.Property(e => e.SkillId).HasColumnName("skillID");
            entity.Property(e => e.Usersid)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("USERSID");

            entity.HasOne(d => d.Skill).WithMany(p => p.UsersSkills)
                .HasForeignKey(d => d.SkillId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__USERS_SKI__skill__2645B050");

            entity.HasOne(d => d.Users).WithMany(p => p.UsersSkills)
                .HasForeignKey(d => d.Usersid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__USERS_SKI__USERS__25518C17");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
