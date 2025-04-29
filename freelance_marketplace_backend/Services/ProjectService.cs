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
		//public async Task<ProjectDetailsDto> GetProjectDetailsAsync(int projectId)
		//{
		//	var project = await _context.Projects
		//		.AsNoTracking()
		//		.Where(p => p.ProjectId == projectId)
		//		.Select(p => new ProjectDetailsDto
		//		{
		//			ProjectId = p.ProjectId,
		//			Title = p.Title,
		//			Overview = p.Overview,
		//			RequiredTasks = p.RequiredTasks,
		//			AdditionalNotes = p.AdditionalNotes,
		//			Budget = p.Budget,
		//			Deadline = p.Deadline,
		//			Skills = p.ProjectSkills.Select(ps => ps.Skill.Skill1).ToList(),
		//			Proposals = p.Proposals.Select(pr => new ProposalDto
		//			{
		//				ProposalId = pr.ProposalId,
		//				ProjectId = pr.ProjectId,
		//				FreelancerId = pr.FreelancerId,
		//				FreelancerName = pr.Freelancer.Name,
		//				ProposedAmount = pr.ProposedAmount,
		//				Deadline = pr.Deadline,
		//				CoverLetter = pr.CoverLetter,
		//				Status = pr.Status,
		//				CreatedAt = pr.CreatedAt ?? DateTime.MinValue

		//			}).ToList()
		//		})
		//		.FirstOrDefaultAsync();

		//	if (project == null)
		//		throw new KeyNotFoundException("Project not found.");

		//	return project;
		//}
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

		public async Task<AssignProjectDto> AssignProjectToFreelancer(int projectId, AssignProjectDto model)
		{
			// Verify project existence
			var project = await _context.Projects
				.Include(p => p.PostedByNavigation) // Include the client details
				.Include(p => p.Proposals) // Include the proposals related to the project
				.FirstOrDefaultAsync(p => p.ProjectId == projectId);

			if (project == null)
			{
				return null;  // Project not found
			}

			// Check if the proposal exists
			var freelancerProposal = project.Proposals.FirstOrDefault(o => o.ProposalId == model.ProposalId && o.FreelancerId == model.FreelancerId);
			if (freelancerProposal == null)
			{
				return null;  // Proposal not found
			}

			// Check client balance
			if (project.PostedByNavigation.Balance < freelancerProposal.ProposedAmount)
			{
				throw new InvalidOperationException("Insufficient balance.");  // More descriptive error
			}

			// Deduct the amount from client's balance
			project.PostedByNavigation.Balance -= freelancerProposal.ProposedAmount;

			// Assign freelancer to project
			project.FreelancerId = model.FreelancerId;

			// Change project status to Approved
			project.Status = "Approved";  // Automatically set the status to "Approved"

			// Update project
			_context.Projects.Update(project);
			await _context.SaveChangesAsync();

			// Return updated info
			var updatedProject = new AssignProjectDto
			{
				ProjectId = project.ProjectId,
				FreelancerId = project.FreelancerId,
				//Status = project.Status,  // Return the updated status
				ClientBalance = project.PostedByNavigation.Balance
			};

			return updatedProject;
		}
	


}
}
