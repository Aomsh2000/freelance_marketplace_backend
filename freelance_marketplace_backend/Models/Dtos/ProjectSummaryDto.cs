namespace freelance_marketplace_backend.Models.Dtos
{
    public class ProjectSummaryDto
    {
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public decimal Budget { get; set; }
        public DateOnly Deadline { get; set; }
        public string Status { get; set; }
        public FreelancerSummaryDto? Freelancer { get; set; } 
        public List<SkillDto> Skills { get; set; }    
    }
}
