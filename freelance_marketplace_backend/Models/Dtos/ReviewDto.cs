namespace freelance_marketplace_backend.Models.Dtos
{
    public class ReviewDto
    {
        public int ProjectId { get; set; }
        public string FromUsersid { get; set; } = null!;
        public string ToUsersid { get; set; } = null!;
        public int Rating { get; set; }
        public string Comment { get; set; } = null!;
    }
}
