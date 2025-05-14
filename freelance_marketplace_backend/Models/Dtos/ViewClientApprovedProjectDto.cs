namespace freelance_marketplace_backend.Models.Dtos
{
    public class ViewClientApprovedProjectDto
    {
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public decimal Budget { get; set; }
        public DateOnly Deadline { get; set; }
        public string Status { get; set; }

        public FreelancerDto Freelancer { get; set; }
    }


    // Freelancer class to represent the freelancer details
    public class FreelancerDto
    {
        public string FreelancerId { get; set; }
        public string FreelancerName { get; set; }

        public string FreelancerImage { get; set; }
    }


}
