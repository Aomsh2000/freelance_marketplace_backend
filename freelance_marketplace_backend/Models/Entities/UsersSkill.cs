using System;
using System.Collections.Generic;

namespace freelance_marketplace_backend.Models.Entities;

public partial class UsersSkill
{
    public int UsersskillId { get; set; }

    public string Usersid { get; set; } = null!;

    public int SkillId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Skill Skill { get; set; } = null!;

    public virtual User Users { get; set; } = null!;
}
