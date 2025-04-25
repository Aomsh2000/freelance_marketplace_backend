using System;
using System.Collections.Generic;

namespace freelance_marketplace_backend.Models.Entities;

public partial class Review
{
    public int ReviewId { get; set; }

    public int ProjectId { get; set; }

    public string FromUsersid { get; set; } = null!;

    public string ToUsersid { get; set; } = null!;

    public int Rating { get; set; }

    public string Comment { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual User FromUsers { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual User ToUsers { get; set; } = null!;
}
