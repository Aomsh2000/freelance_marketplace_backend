namespace freelance_marketplace_backend.Models.Dtos
{
	// DTO for displaying detailed information about a specific project,
	// including its skills and proposals
	public class ProjectDetailsDto
	{
		public int ProjectId { get; set; } // ID of the project

		public string Title { get; set; } // Title of the project

		public string Overview { get; set; } // Overview/description of the project

		public string RequiredTasks { get; set; } // Tasks required for the project

		public string AdditionalNotes { get; set; } // Any additional notes or information

		public decimal Budget { get; set; } // Budget allocated for the project

		public DateOnly Deadline { get; set; } // Deadline date to complete the project
		public string ClientId { get; set; }
		public string ClientName { get; set; }


		public string Status { get; set; } // Status of the project (e.g., Open, In Progress, Closed)

		public List<string> Skills { get; set; } = new(); // List of skills required (only skill names)

		public List<ProposalDto> Proposals { get; set; } = new(); // List of proposals submitted for this project
	}

}
