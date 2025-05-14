using System;
using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models;
using freelance_marketplace_backend.Models.Entities;
using global::freelance_marketplace_backend.Models.Dtos;
using global::freelance_marketplace_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Pipelines.Sockets.Unofficial.Arenas;

namespace freelance_marketplace_backend.Services
{
    namespace freelance_marketplace_backend.Services
    {
        // Service to manage proposal-related operations
        public class ProposalService : IProposalService
        {
            private readonly FreelancingPlatformContext _context;
			private readonly IDistributedCache _cache;

			public ProposalService(FreelancingPlatformContext context, IDistributedCache cache)
            {
                _context = context;
				_cache = cache;
			}

			// Submits a proposal for a specific project and returns the created proposal with freelancer info.
		
			public async Task<ProposalDto> SubmitProposalAsync(int projectId, CreateProposalDto proposalDto)
			{
				var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId);

				if (project == null)
					throw new KeyNotFoundException("Project not found.");

				// Verify project status "Open"
				if (!project.Status.Equals("Open", StringComparison.OrdinalIgnoreCase))
					throw new InvalidOperationException("You can only submit proposals to projects with status 'Open'.");

				//  Check if there is a previous proposal for the same freelancer and it has not been deleted
				var existingProposal = await _context.Proposals.FirstOrDefaultAsync(p =>
					p.ProjectId == projectId &&
					p.FreelancerId == proposalDto.FreelancerId &&
					(p.IsDeleted == null || p.IsDeleted == false)
				);

				if (existingProposal != null)
					throw new InvalidOperationException("You have already submitted a proposal for this project. Please delete the existing one before submitting a new proposal.");

				// Create a new proposal
				DateOnly deadlineDateOnly = DateOnly.FromDateTime(proposalDto.Deadline);

				var proposal = new Proposal
				{
					ProjectId = projectId,
					FreelancerId = proposalDto.FreelancerId,
					ProposedAmount = proposalDto.ProposedAmount,
					Deadline = deadlineDateOnly,
					CoverLetter = proposalDto.CoverLetter,

					Status = "Pending",
					CreatedAt = DateTime.UtcNow,
				};

				_context.Proposals.Add(proposal);
				await _context.SaveChangesAsync();

				var proposalWithFreelancer = await _context
					.Proposals.Include(p => p.Freelancer)
					.FirstOrDefaultAsync(p => p.ProposalId == proposal.ProposalId);

				var freelancerName = proposalWithFreelancer?.Freelancer?.Name ?? "Unknown";

				return new ProposalDto
				{
					ProposalId = proposal.ProposalId,
					ProjectId = proposal.ProjectId,
					FreelancerId = proposal.FreelancerId,
					FreelancerName = freelancerName,
					freelancerPhoneNumber = proposal.Freelancer.Phone,
					ProposedAmount = proposal.ProposedAmount,
					Deadline = proposal.Deadline,
					CoverLetter = proposal.CoverLetter,
					Status = proposal.Status,
					ProjectTitle= proposal.Project.Title,
					ProfilePictureUrl = proposal.Freelancer.ImageUrl,
					CreatedAt = proposal.CreatedAt ?? DateTime.MinValue,
				};
			}

			public async Task<IEnumerable<ProposalDto>> GetProposalsByFreelancerIdAsync(
                string freelancerId
            )
            {
                var proposals = await _context
                    .Proposals.Include(p => p.Freelancer)
                    .Include(p => p.Project)
                    .Where(p =>
                        p.FreelancerId == freelancerId
                        && (p.IsDeleted == null || p.IsDeleted == false)
                    )
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var result = proposals.Select(p => new ProposalDto
                {
                    ProposalId = p.ProposalId,
                    ProjectId = p.ProjectId,
                    ProjectTitle = p.Project?.Title ?? "Unknown",
                    FreelancerId = p.FreelancerId,
                    FreelancerName = p.Freelancer.Name,
                    ProposedAmount = p.ProposedAmount,
                    Deadline = p.Deadline,
                    CoverLetter = p.CoverLetter,
                    Status = p.Status,
                    ProfilePictureUrl = p.Freelancer.ImageUrl ?? "",
                    CreatedAt = p.CreatedAt ?? DateTime.MinValue,
                });

                return result;
            }

            public async Task<bool> DeleteProposalAsync(int proposalId, string freelancerId)
{
    var proposal = await _context.Proposals.FirstOrDefaultAsync(p =>
        p.ProposalId == proposalId &&
        p.FreelancerId == freelancerId &&
        (p.IsDeleted == null || p.IsDeleted == false)
    );


    if (proposal == null)
        return false;

    // Prevent deletion if status is "Accepted"
    if (proposal.Status.Equals("Accepted", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Accepted proposals cannot be deleted.");

    proposal.IsDeleted = true;
    await _context.SaveChangesAsync();

    return true;
}


    }
}
}