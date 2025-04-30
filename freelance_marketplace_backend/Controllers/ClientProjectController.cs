using freelance_marketplace_backend.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientProjectController : ControllerBase
    {
        private readonly IClientProjectService _clientProjectService;

        public ClientProjectController(IClientProjectService clientProjectService)
        {
            _clientProjectService = clientProjectService;
        }


        // GET: api/clientproject/approved
        [HttpGet("approved/{clientId}")]
        public async Task<IActionResult> GetApprovedProjectsForClient(string clientId)
        {
            var projects = await _clientProjectService.GetClientApprovedProjects(clientId);

            if (projects == null || !projects.Any())
            {
                return NotFound("No approved projects found for this client.");
            }

            return Ok(projects);
        }


        [HttpPut("approved/{projectId}/mark-completed")]
        public async Task<IActionResult> MarkProjectAsCompleted(int projectId)
        {
            var result = await _clientProjectService.MarkProjectAsCompleted(projectId);
            if (result)
            {
                return Ok(new { message = "Project marked as completed successfully." });
            }
            return BadRequest(new { message = "Failed to mark the project as completed." });
        }


    }
}
