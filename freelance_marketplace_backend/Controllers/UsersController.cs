//using freelance_marketplace_backend.Interfaces;
//using freelance_marketplace_backend.Models.Dtos;
//using Microsoft.AspNetCore.Mvc;

//namespace freelance_marketplace_backend.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class UsersController : ControllerBase
//    {
//        private readonly IUserService _userService;

//        public UsersController(IUserService userService)
//        {
//            _userService = userService;
//        }

//        [HttpPost("create")]
//        public IActionResult CreateUser(CreateUserDto user)
//        {
//            _userService.CreateUser(user); 
//            return Ok();
//        }
//    }
//}

using freelance_marketplace_backend.Data.Repositories;
using freelance_marketplace_backend.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;

namespace freelance_marketplace_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _userRepository;

        public UsersController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
        {
            try
            {
                await _userRepository.CreateUserAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(CreateUser), new { id = dto.UserId }, dto);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating the user: {ex.Message}");
            }
        }
    }
}