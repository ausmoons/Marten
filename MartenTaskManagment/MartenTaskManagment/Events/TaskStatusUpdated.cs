namespace MartenTaskManagment.Events
{
    public class TaskStatusUpdated
    {
        public Guid TaskId { get; set; }
        public string NewStatus { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
