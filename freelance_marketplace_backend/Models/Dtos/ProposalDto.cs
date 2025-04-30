namespace freelance_marketplace_backend.Models.Dtos
{
	// DTO for displaying basic details about each proposal
	public class ProposalDto
	{
		public int ProposalId { get; set; } // ID of the proposal

		public int ProjectId { get; set; } // ID of the project associated with this proposal
		public string FreelancerId { get; set; } = null!; // ID of the freelancer who submitted the proposal
		public string FreelancerName { get; set; } = null!; // Name of the freelancer who submitted the proposal

		public decimal ProposedAmount { get; set; } // Amount proposed by the freelancer

		public DateOnly Deadline { get; set; } // Deadline suggested by the freelancer for completing the project

		public string CoverLetter { get; set; } = null!; // Cover letter or description from the freelancer

		public string Status { get; set; } = null!; // Status of the proposal (e.g., Pending, Accepted, Rejected)

		public string ProfilePictureUrl { get; set; } = null!; // URL of the freelancer's profile picture
		public DateTime CreatedAt { get; set; } // Date and time when the proposal was created
	}
}
