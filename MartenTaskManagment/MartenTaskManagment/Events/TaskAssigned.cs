namespace MartenTaskManagment.Events
{
    public class TaskAssigned
    {
        public Guid TaskId { get; set; }
        public string AssignedUser { get; set; }
        public DateTime AssignedDate { get; set; }
    }
}
