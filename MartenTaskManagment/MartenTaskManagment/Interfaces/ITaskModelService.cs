using MartenTaskManagment.DTOs;
using MartenTaskManagment.Events;
using MartenTaskManagment.Models;

namespace MartenTaskManagment.Interfaces
{
    public interface ITaskModelService
    {
        Task<TaskModel> GetTaskModelById(Guid taskId);

        IEnumerable<object> GetUpdateEvents(Guid taskId, TaskModel existingTask, TaskUpdateDTO updatedTask);

        Task<int> CountTasksByStatusAsync(string status);

        Task<double> GetAverageCompletionTimeAsync();

        Task<IEnumerable<UserTaskCount>> GetTasksPerUserAsync();

        Task<List<TaskModel>> GetAllTasksAsync();
    }
}
