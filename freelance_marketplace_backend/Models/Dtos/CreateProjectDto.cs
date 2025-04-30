namespace freelance_marketplace_backend.Models.Dtos
{
    public class CreateProjectDto
    {
        public string Title { get; set; }
        public string ProjectOverview { get; set; }
        public string RequiredTasks { get; set; }
        public string AdditionalNotes { get; set; }
        public decimal Budget { get; set; }
        public DateOnly Deadline { get; set; }
        public List<SkillDto> Skills { get; set; }
    }
}
