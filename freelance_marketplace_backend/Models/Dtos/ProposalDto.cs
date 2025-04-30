namespace freelance_marketplace_backend.Models.Dtos
{
    public class ProposalDto
    {
        public int ProposalId { get; set; }
        public int ProjectId { get; set; }
        public string FreelancerId { get; set; } = string.Empty;
        public string FreelancerName { get; set; } = string.Empty;
        public decimal ProposedAmount { get; set; }
        public DateOnly Deadline { get; set; }
        public string CoverLetter { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime? CreatedAt { get; set; }
    }
}
