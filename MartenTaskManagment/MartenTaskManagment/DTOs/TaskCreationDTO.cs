namespace MartenTaskManagment.DTOs
{
    public class TaskCreationDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string AssignedUser { get; set; }
    }
}
