using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models;
using freelance_marketplace_backend.Models.Entities;
using global::freelance_marketplace_backend.Models.Dtos;
using global::freelance_marketplace_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;


namespace freelance_marketplace_backend.Services
{
	

	namespace freelance_marketplace_backend.Services
	{
		// Service to manage proposal-related operations
		public class ProposalService: IProposalService
		{
			private readonly FreelancingPlatformContext _context;

			public ProposalService(FreelancingPlatformContext context)
			{
				_context = context;
			}


			// Submits a proposal for a specific project and returns the created proposal with freelancer info.
			public async Task<ProposalDto> SubmitProposalAsync(int projectId, CreateProposalDto proposalDto)
			{
				var project = await _context.Projects.FindAsync(projectId);
				if (project == null)
				{
					throw new KeyNotFoundException("Project not found.");
				}

				DateOnly deadlineDateOnly = DateOnly.FromDateTime(proposalDto.Deadline);

				// Create a new Proposal
				var proposal = new Proposal
				{
					ProjectId = projectId,
					FreelancerId = proposalDto.FreelancerId,
					ProposedAmount = proposalDto.ProposedAmount,
					Deadline = deadlineDateOnly,
					CoverLetter = proposalDto.CoverLetter,
					Status = "Pending",  // Initial status is Pending
					CreatedAt = DateTime.UtcNow
				};

				_context.Proposals.Add(proposal);
				await _context.SaveChangesAsync();

				// After saving, we fetch the offer with its associated freelancer information.
				var proposalWithFreelancer = await _context.Proposals
					.Include(p => p.Freelancer)
					.FirstOrDefaultAsync(p => p.ProposalId == proposal.ProposalId);

				var freelancerName = proposalWithFreelancer?.Freelancer?.Name ?? "Unknown";

				var proposalDtoResult = new ProposalDto
				{
					ProposalId = proposal.ProposalId,
					ProjectId = proposal.ProjectId,
					FreelancerId = proposal.FreelancerId,
					FreelancerName = freelancerName,
					ProposedAmount = proposal.ProposedAmount,
					Deadline = proposal.Deadline,
					CoverLetter = proposal.CoverLetter,
					Status = proposal.Status,
					CreatedAt = proposal.CreatedAt ?? DateTime.MinValue
				};

				return proposalDtoResult;
			}

		}
	}
}

	