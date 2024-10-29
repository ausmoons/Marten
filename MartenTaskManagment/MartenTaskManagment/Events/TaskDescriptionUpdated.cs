namespace MartenTaskManagment.Events
{
    public class TaskDescriptionUpdated
    {
        public Guid TaskId { get; set; }
        public string NewDescription { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
