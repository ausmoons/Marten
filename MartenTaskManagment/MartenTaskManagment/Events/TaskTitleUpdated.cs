namespace MartenTaskManagment.Events
{
    public class TaskTitleUpdated
    {
        public Guid TaskId { get; set; }
        public string NewTitle { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
