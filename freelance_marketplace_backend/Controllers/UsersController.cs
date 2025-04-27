using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("create")]
        public IActionResult CreateUser(CreateUserDto user)
        {
            _userService.CreateUser(user); 
            return Ok();
        }
    }
}
