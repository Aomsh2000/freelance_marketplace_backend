using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace freelance_marketplace_backend.Data.Repositories
{
    public class ProjectRepository
    {
        private readonly FreelancingPlatformContext _context;

        public ProjectRepository(FreelancingPlatformContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectSummaryDto>> GetProjectsByUserAsync(string userId)
        {
            var projects = await _context.Projects
                .Where(p => p.PostedBy == userId && p.IsDeleted == false)
                .Include(p => p.Freelancer)
                .Include(p => p.ProjectSkills)
                    .ThenInclude(ps => ps.Skill)
                .ToListAsync();

            return projects.Select(p => new ProjectSummaryDto
            {
                ProjectId = p.ProjectId,
                Title = p.Title,
                Overview = p.Overview,
                Budget = p.Budget,
                Deadline = p.Deadline,
                Status = p.Status,
                Freelancer = p.Freelancer != null ? new FreelancerSummaryDto
                {
                    FreelancerId = p.Freelancer.Usersid,
                    FreelancerName = p.Freelancer.Name // adjust as needed
                } : null,
                Skills = p.ProjectSkills.Select(ps => new SkillDto
                {
                    SkillId = ps.Skill.SkillId,
                    Skill = ps.Skill.Skill1,
                    Category = ps.Skill.Category
                }).ToList()
            }).ToList();
        }


        public async Task AddProjectAsync(Project project, List<SkillDto>? skills, CancellationToken cancellationToken)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync(cancellationToken);

            if (skills != null && skills.Any())
            {
                var projectSkills = new List<ProjectSkill>();

                foreach (var skill in skills)
                {
                    if (skill != null && skill.SkillId > 0)
                    {
                        var skillExists = await _context.Skills.AnyAsync(s => s.SkillId == skill.SkillId, cancellationToken);
                        if (skillExists)
                        {
                            projectSkills.Add(new ProjectSkill
                            {
                                SkillId = skill.SkillId,
                                ProjectId = project.ProjectId
                            });
                        }
                    }
                }

                _context.ProjectSkills.AddRange(projectSkills);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<string> MarkProjectAsDeletedAsync(int projectId, string userId, CancellationToken cancellationToken)
        {
            var existingProject = await _context.Projects.FindAsync(new object[] { projectId }, cancellationToken);
            if (existingProject == null) return "NotFound";
            if (existingProject.PostedBy != userId) return "Unauthorized";

            existingProject.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
            return "Success";
        }

        public async Task<ProjectDetailsDto?> GetProjectByIdAsync(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.PostedByNavigation) 
                .Include(p => p.ProjectSkills).ThenInclude(ps => ps.Skill)
                .Include(p => p.Proposals).ThenInclude(pr => pr.Freelancer)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId && p.IsDeleted == false);

            if (project == null) return null;

            return new ProjectDetailsDto
            {
                ProjectId = project.ProjectId,
                Title = project.Title,
                ProjectOverview = project.Overview,
                RequiredTasks = project.RequiredTasks,
                AdditionalNotes = project.AdditionalNotes ?? "",
                Budget = project.Budget,
                Deadline = project.Deadline,
                Status = project.Status,
                PostedBy = project.PostedBy,
                ClientName = project.PostedByNavigation?.Name ?? "",
                CreatedAt = project.CreatedAt,

                Proposals = project.Proposals.Select(pr => new ProposalDto
                {
                    ProposalId = pr.ProposalId,
                    ProjectId = pr.ProjectId,
                    FreelancerId = pr.FreelancerId,
                    FreelancerName = pr.Freelancer?.Name ?? "",
                    ProposedAmount = pr.ProposedAmount,
                    Deadline = pr.Deadline,
                    CoverLetter = pr.CoverLetter,
                    Status = pr.Status,
                    CreatedAt = pr.CreatedAt
                }).ToList(),

                Skills = project.ProjectSkills.Select(ps => new SkillDto
                {
                    SkillId = ps.Skill.SkillId,
                    Skill = ps.Skill.Skill1,
                    Category = ps.Skill.Category
                }).ToList()
            };
        }

    }
}
