using MartenTaskManagment.Events;

namespace MartenTaskManagment.Models
{
    public class TaskModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string AssignedUser { get; set; }
        public DateTime CreatedAt { get; set; }


        public static TaskModel Create(TaskCreated @event)
        {
            return new TaskModel
            {
                Id = @event.TaskId,
                Title = @event.Title,
                CreatedAt = @event.CreatedAt
            };
        }

        public void Apply(TaskAssigned @event)
        {
            AssignedUser = @event.AssignedUser;
        }

        public void Apply(TaskStatusUpdated @event)
        {
            Status = @event.NewStatus;
        }

        public void Apply(TaskDueDateUpdated @event)
        {
            DueDate = @event.NewDueDate;
        }

        public void Apply(TaskDescriptionUpdated @event)
        {
            Description = @event.NewDescription;
        }
    }
}