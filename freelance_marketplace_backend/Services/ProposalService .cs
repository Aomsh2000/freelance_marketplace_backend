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
            public async Task<ProposalDto> SubmitProposalAsync(
                int projectId,
                CreateProposalDto proposalDto
            )
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
                    Status = "Pending", // Initial status is Pending
                    CreatedAt = DateTime.UtcNow,
                };

                _context.Proposals.Add(proposal);
                await _context.SaveChangesAsync();

                // After saving, we fetch the offer with its associated freelancer information.
                var proposalWithFreelancer = await _context
                    .Proposals.Include(p => p.Freelancer)
                    .FirstOrDefaultAsync(p => p.ProposalId == proposal.ProposalId);

                var freelancerName = proposalWithFreelancer?.Freelancer?.Name ?? "Unknown";

                var proposalDtoResult = new ProposalDto
                {
                    ProposalId = proposal.ProposalId,
                    ProjectId = proposal.ProjectId,
                    FreelancerId = proposal.FreelancerId,
                    FreelancerName = freelancerName,
                    ProposedAmount = proposal.ProposedAmount,
                    freelancerPhoneNumber = proposal.Freelancer.Phone,
                    Deadline = proposal.Deadline,
                    CoverLetter = proposal.CoverLetter,
                    Status = proposal.Status,
                    CreatedAt = proposal.CreatedAt ?? DateTime.MinValue,
                };

                return proposalDtoResult;
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