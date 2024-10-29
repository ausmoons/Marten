namespace MartenTaskManagment.Events
{
    public class TaskDeleted
    {
        public Guid TaskId { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}
