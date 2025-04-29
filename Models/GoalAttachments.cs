namespace WorkWise.Models
{
    public class GoalAttachments
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }

        public Guid GoalId { get; set; }
        public Goals Goal { get; set; }

        public string UploadedById { get; set; }
        public Users UploadedBy { get; set; }
    }
}
