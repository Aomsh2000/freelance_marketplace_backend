using System.Text.Json.Serialization;
namespace freelance_marketplace_backend.Models.Dtos
{
	public class AssignProjectDto
	{
		public string FreelancerId { get; set; }  //Freelancer ID
		public int ProposalId { get; set; }  //Proposal ID being accepted
        public string FreelancerPhoneNumber { get; set; }
        [JsonIgnore]
		public decimal ClientBalance { get; set; }  //client balance after modification
	}
}
