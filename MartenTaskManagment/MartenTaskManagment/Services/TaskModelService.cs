using Dapper;
using Marten;
using MartenTaskManagment.DTOs;
using MartenTaskManagment.Events;
using MartenTaskManagment.Interfaces;
using MartenTaskManagment.Models;

namespace MartenTaskManagment.Services
{
    public class TaskModelService : ITaskModelService
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
                    case TaskTitleUpdated taskTitleUpdated when task != null:
                        task.Title = taskTitleUpdated.NewTitle;
                        break;

                    case TaskDescriptionUpdated taskDescriptionUpdated when task != null:
                        task.Description = taskDescriptionUpdated.NewDescription;
                        break;
                    case TaskDueDateUpdated taskDueDateUpdated when task != null:
                        task.DueDate = taskDueDateUpdated.NewDueDate;
                        break;
                }
            }

            return task;
        }

        public async Task<List<TaskModel>> GetAllTasksAsync()
        {
            var allTaskIds = await _session.Events
                .QueryRawEventDataOnly<TaskCreated>()
                .Select(e => e.TaskId)
                .Distinct()
                .ToListAsync();

            var tasks = new List<TaskModel>();
            foreach (var taskId in allTaskIds)
            {
                var task = await GetTaskModelById(taskId);
                if (task != null)
                {
                    tasks.Add(task);
                }
            }

            return tasks;
        }


        public IEnumerable<object> GetUpdateEvents(Guid taskId, TaskModel existingTask, TaskUpdateDTO updatedTask)
        {
            var events = new List<object>();

            if (existingTask.Title != updatedTask.Title && !string.IsNullOrEmpty(updatedTask.Title))
            {
                events.Add(new TaskTitleUpdated { TaskId = taskId, NewTitle = updatedTask.Title, UpdatedAt = DateTime.UtcNow });
            }

            if (existingTask.Description != updatedTask.Description && !string.IsNullOrEmpty(updatedTask.Description))
            {
                events.Add(new TaskDescriptionUpdated { TaskId = taskId, NewDescription = updatedTask.Description, UpdatedAt = DateTime.UtcNow });
            }

            if (updatedTask.DueDate.HasValue && existingTask.DueDate != updatedTask.DueDate.Value)
            {
                events.Add(new TaskDueDateUpdated { TaskId = taskId, NewDueDate = updatedTask.DueDate.Value, UpdatedAt = DateTime.UtcNow });
            }

            if (!string.IsNullOrEmpty(updatedTask.Status) && existingTask.Status != updatedTask.Status)
            {
                events.Add(new TaskStatusUpdated { TaskId = taskId, NewStatus = updatedTask.Status, UpdatedAt = DateTime.UtcNow });
            }

            return events;
        }

        public async Task<int> CountTasksByStatusAsync(string status)
        {
            return await _session.Query<TaskModel>().CountAsync(t => t.Status == status);
        }

        public async Task<double> GetAverageCompletionTimeAsync()
        {
            var completedTasks = await _session.Query<TaskModel>().Where(t => t.Status == "Completed").ToListAsync();
            return completedTasks.Any() ? completedTasks.Average(t => (t.DueDate - t.CreatedAt).TotalDays) : 0;
        }

        public async Task<IEnumerable<UserTaskCount>> GetTasksPerUserAsync()
        {
            var sql = @"
            SELECT data->>'AssignedUser' AS User, COUNT(*) AS TaskCount
            FROM mt_events
            WHERE data->>'AssignedUser' IS NOT NULL
            GROUP BY data->>'AssignedUser'
        ";

            return await _session.Connection.QueryAsync<UserTaskCount>(sql);
        }
    }
}
