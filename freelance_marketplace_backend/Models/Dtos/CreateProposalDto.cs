
using System.ComponentModel.DataAnnotations;
namespace freelance_marketplace_backend.Models.Dtos
{
	// DTO for a freelancer to submit a new proposal
	public class CreateProposalDto
	{
		public string FreelancerId { get; set; } = null!; // ID of the freelancer who submitted the proposal
		public decimal ProposedAmount { get; set; } // The amount offered by the freelancer

		//public string Deadline { get; set; } // The freelancer's suggested deadline

		// استخدام DateTime لتخزين التاريخ (بدون الوقت)
		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)] // تنسيق التاريخ كـ "yyyy-MM-dd"
		public DateTime Deadline { get; set; } // The freelancer's suggested deadline


		//[DataType(DataType.Date)]
		//public DateOnly Deadline { get; set; } // Use DateOnly for just the date part

		public string CoverLetter { get; set; } = null!; // A short message from the freelancer
	}
}
