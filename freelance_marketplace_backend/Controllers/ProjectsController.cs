using freelance_marketplace_backend.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        // each controller has its own service implementing an interface and a repository,
        // if one of them is not there please create it and don`t use the controller for everything to ensure that all controllers are having the same berhaviour
        // and to implement coding best practices   
        // no database access from controllers
        // you should use this controller to access <Name>Service and use <Name>Repository from there (<Name>Service)
        FreelancingPlatformContext context = new FreelancingPlatformContext();

        [HttpGet]
        public IActionResult getSkills() 
        { 
            return Ok(context.Skills.ToList());
        }
    }
}
