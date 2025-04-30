namespace freelance_marketplace_backend.Models.Dtos
{
	public class AssignProjectDto
	{
		public string FreelancerId { get; set; }  //Freelancer ID
		public int ProposalId { get; set; }  //Proposal ID being accepted
		public int ProjectId { get; set; }
		//public string Status { get; set; }
		public decimal ClientBalance { get; set; }  //client balance after modification
	}
}
