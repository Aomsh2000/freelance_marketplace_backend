using freelance_marketplace_backend.Models.Dtos;

namespace AdvancedAjax.Models.Dtos
{
    public class ProfileProjectDto
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = null!;
        public string ProjectOverview { get; set; } = null!;
        public string RequiredTasks { get; set; } = null!;
        public string AdditionalNotes { get; set; } = null!;
        public decimal Budget { get; set; }
        public DateOnly Deadline { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        public List<SkillDto> Skills { get; set; } = new List<SkillDto>();
    }
}
