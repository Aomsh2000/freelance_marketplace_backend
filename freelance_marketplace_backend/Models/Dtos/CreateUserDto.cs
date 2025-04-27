namespace freelance_marketplace_backend.Models.Dtos
{
    public class CreateUserDto
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AboutMe { get; set; }
        public List<SkillDto> Skills { get; set; }

    }
}
