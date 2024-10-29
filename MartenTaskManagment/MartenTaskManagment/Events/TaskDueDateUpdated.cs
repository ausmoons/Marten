namespace MartenTaskManagment.Events
{
    public class TaskDueDateUpdated
    {
        public Guid TaskId { get; set; }
        public DateTime NewDueDate { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
