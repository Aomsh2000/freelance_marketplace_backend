using System;
using System.Collections.Generic;

namespace freelance_marketplace_backend.Models.Entities;

public partial class ProjectSkill
{
    public int ProjectSkillId { get; set; }

    public int ProjectId { get; set; }

    public int SkillId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual Project Project { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
