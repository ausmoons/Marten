using MartenTaskManagment.Models;

namespace MartenTaskManagment.Interfaces
{
    public interface ITaskModelService
    {
        Task<TaskModel> GetTaskModelById(Guid taskId);
    }
}
