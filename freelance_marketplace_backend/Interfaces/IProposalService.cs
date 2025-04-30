using freelance_marketplace_backend.Models.Dtos;

namespace freelance_marketplace_backend.Interfaces
{
	public interface IProposalService
	{
		
		// Method to allow a freelancer to submit a new proposal
		
		Task<ProposalDto> SubmitProposalAsync(int projectId, CreateProposalDto proposalDto);

	}
}
