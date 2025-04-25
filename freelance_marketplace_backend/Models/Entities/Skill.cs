using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace freelance_marketplace_backend.Models.Entities;

public partial class Skill
{
    public int SkillId { get; set; }

    public string Skill1 { get; set; } = null!;

    public string Category { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }
    [JsonIgnore]
    public virtual ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();
    [JsonIgnore]
    public virtual ICollection<UsersSkill> UsersSkills { get; set; } = new List<UsersSkill>();
}
