namespace MartenTaskManagment.Events
{
    public class TaskCreated
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string AssignedUser { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
