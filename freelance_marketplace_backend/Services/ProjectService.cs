using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Data;
using freelance_marketplace_backend.Models;
using freelance_marketplace_backend.Models.Entities;
using freelance_marketplace_backend.Models.Dtos;
using Microsoft.EntityFrameworkCore;


using System;
using Microsoft.AspNetCore.Mvc;

namespace freelance_marketplace_backend.Services
{
	// Implementation of the IProjectService
	public class ProjectService : IProjectService
	{
		private readonly FreelancingPlatformContext _context;

		public ProjectService(FreelancingPlatformContext context)
		{
			_context = context;
		}

		// Get detailed project information including skills and proposals
		public async Task<ProjectDetailsDto> GetProjectDetailsAsync(int projectId)
		{
			var project = await _context.Projects
				.AsNoTracking()
				.Where(p => p.ProjectId == projectId)
				.Select(p => new ProjectDetailsDto
				{
					ProjectId = p.ProjectId,
					Title = p.Title,
					Overview = p.Overview,
					RequiredTasks = p.RequiredTasks,
					AdditionalNotes = p.AdditionalNotes,
					Budget = p.Budget,
					Deadline = p.Deadline,
					ClientId = p.PostedBy, // ID of the client
					ClientName = p.PostedByNavigation.Name, // Name of the client who posted the project
					Skills = p.ProjectSkills.Select(ps => ps.Skill.Skill1).ToList(),
					Proposals = p.Proposals.Select(pr => new ProposalDto
					{
						ProposalId = pr.ProposalId,
						ProjectId = pr.ProjectId,
						FreelancerId = pr.FreelancerId,
						FreelancerName = pr.Freelancer.Name,
						ProposedAmount = pr.ProposedAmount,
						Deadline = pr.Deadline,
						CoverLetter = pr.CoverLetter,
						Status = pr.Status,
						CreatedAt = pr.CreatedAt ?? DateTime.MinValue
					}).ToList()
				})
				.FirstOrDefaultAsync();

			if (project == null)
				throw new KeyNotFoundException("Project not found.");

			return project;
		}

		// This method assigns a freelancer to a project, deducts the proposal amount from the client's balance, 
		// and changes the project status to "Approved" if all conditions are met.

		public async Task<AssignProjectDto> AssignProjectToFreelancer(int projectId, AssignProjectDto model, string uid)
		{
			// Retrieve the project with related details
			var project = await _context.Projects
				.Include(p => p.PostedByNavigation)
				.Include(p => p.Proposals)
				.FirstOrDefaultAsync(p => p.ProjectId == projectId);

			if (project == null)
			{
				return null; // Project not found
			}

			// Ensure that only the project owner can assign a freelancer
			if (project.PostedBy != uid)
			{
				throw new UnauthorizedAccessException("You are not authorized to assign a freelancer to this project.");
			}

			// Check if the project already has an assigned freelancer
			//if (project.FreelancerId != null)
			//{
			//	throw new InvalidOperationException("This project already has a freelancer assigned and cannot be changed.");
			//}

			// Find the proposal submitted by the specified freelancer
			var freelancerProposal = project.Proposals
				.FirstOrDefault(p => p.ProposalId == model.ProposalId && p.FreelancerId == model.FreelancerId);

			if (freelancerProposal == null)
			{
				return null; // Proposal not found
			}

			// Verify that the client has sufficient balance
			var clientBalance = project.PostedByNavigation.Balance;
			if (clientBalance < freelancerProposal.ProposedAmount)
			{
				throw new InvalidOperationException("Insufficient client balance.");
			}

			// Deduct the amount from the client's balance and update the project and Proposal status
			project.PostedByNavigation.Balance -= freelancerProposal.ProposedAmount;
			project.FreelancerId = model.FreelancerId;
			project.Status = "In Progress";
			freelancerProposal.Status = "Accepted";

			// Save changes
			_context.Projects.Update(project);
			await _context.SaveChangesAsync();

			// Return updated information
			return new AssignProjectDto
			{
				FreelancerId = project.FreelancerId,
				ClientBalance = project.PostedByNavigation.Balance
			};
		}

	}
}
