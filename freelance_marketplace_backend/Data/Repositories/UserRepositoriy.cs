using freelance_marketplace_backend.Models.Dtos;
using freelance_marketplace_backend.Models.Entities;

namespace freelance_marketplace_backend.Data.Repositories
{
    public class UserRepository
    {
        private readonly FreelancingPlatformContext _context;

        public UserRepository(FreelancingPlatformContext context) 
        {
            _context = context;
        }

        public void CreateUser(CreateUserDto user)
        {
            List<UsersSkill> userSkills = new List<UsersSkill>();

            foreach (var skill in user.Skills)
            {
                userSkills.Add(new UsersSkill
                {
                    SkillId = skill.SkillId,
                    Usersid = user.UserId
                });
            }

            var newUser = new Models.Entities.User
            {
                Usersid = user.UserId,
                Name = user.Name,
                AboutMe = user.AboutMe,
                phone = user.Phone,
                Email = user.Email,
            };

            _context.Users.Add(newUser);
            _context.UsersSkills.AddRange(userSkills);

            _context.SaveChanges();
        }
    }
}
