namespace freelance_marketplace_backend.Models.Dtos
{
    public class ProjectDetailsDto
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ProjectOverview { get; set; } = string.Empty;
        public string RequiredTasks { get; set; } = string.Empty;
        public string AdditionalNotes { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public DateOnly Deadline { get; set; }
        public string Status { get; set; } = "Open";
        public string PostedBy { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }

        public List<ProposalDto> Proposals { get; set; } = new();
        public List<SkillDto> Skills { get; set; } = new();
    }

}
