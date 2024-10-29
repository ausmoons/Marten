using Marten;
using MartenTaskManagment.Events;
using MartenTaskManagment.Models;

namespace MartenTaskManagment.Services
{
    public class TaskModelService
    {
        private readonly IDocumentSession _session;

        public TaskModelService(IDocumentSession session)
        {
            _session = session;
        }

        public async Task<TaskModel> GetTaskModelById(Guid taskId)
        {
            var events = await _session.Events.FetchStreamAsync(taskId);
            if (!events.Any()) return null;

            TaskModel task = null;

            foreach (var @event in events)
            {
                switch (@event.Data)
                {
                    case TaskCreated taskCreated:
                        task = new TaskModel
                        {
                            Id = taskCreated.TaskId,
                            Title = taskCreated.Title,
                            Description = taskCreated.Description,
                            DueDate = taskCreated.DueDate,
                            Status = taskCreated.Status,
                            AssignedUser = taskCreated.AssignedUser,
                            CreatedAt = taskCreated.CreatedAt
                        };
                        break;

                    case TaskAssigned taskAssigned when task != null:
                        task.AssignedUser = taskAssigned.AssignedUser;
                        break;

                    case TaskStatusUpdated taskStatusUpdated when task != null:
                        task.Status = taskStatusUpdated.NewStatus;
                        break;
                }
            }

            return task;
        }
    }
}
