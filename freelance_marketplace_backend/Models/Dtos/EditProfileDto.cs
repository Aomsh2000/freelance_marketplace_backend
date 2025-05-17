using freelance_marketplace_backend.Models.Dtos;
using AdvancedAjax.Models.Dtos;

namespace freelance_marketplace_backend.Models.Dtos
{
    public class EditProfileDto
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string? ImageUrl { get; set; }
        public string AboutMe { get; set; }
        public List<SkillDto> Skills { get; set; } 
    }
}