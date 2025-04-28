using freelance_marketplace_backend.Interfaces;
using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Data.Repositories;

namespace freelance_marketplace_backend.Services
{
    public class UserService : IUserService
    {
        private readonly UserRepository _userRepository;
        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public void CreateUser(CreateUserDto user)
        {
            _userRepository.CreateUserAsync(user); 
        }
    }
}
