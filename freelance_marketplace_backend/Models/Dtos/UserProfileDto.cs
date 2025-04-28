using AdvancedAjax.Models.Dtos;

namespace freelance_marketplace_backend.Models.Dtos
{
    public class UserProfileDto
    {
        public string UserId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public string? AboutMe { get; set; }
        public decimal Rating { get; set; }
        public decimal Balance { get; set; }

        public List<SkillDto> Skills { get; set; } = new List<SkillDto>();
        public List<ProfileProjectDto> Projects { get; set; } = new List<ProfileProjectDto>();
    }
}
