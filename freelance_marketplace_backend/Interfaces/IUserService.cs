using freelance_marketplace_backend.Models.Dtos;

namespace freelance_marketplace_backend.Interfaces
{
    public interface IUserService
    {
        public void CreateUser(CreateUserDto user);
    }
}
