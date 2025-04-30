using freelance_marketplace_backend.Models.Dtos;

namespace freelance_marketplace_backend.Interfaces
{
    public interface IClientProjectService
    {
        // View all projects approved projectt by ID
        Task<List<ViewClientApprovedProjectDto>> GetClientApprovedProjects(string userID);

        // Marke project as completed: convert statuse to completed and increase the freelancer's balance
        public Task<bool> MarkProjectAsCompleted(int projectId);
    }
}
