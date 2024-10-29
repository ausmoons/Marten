using Dapper;
using Marten;
using MartenTaskManagment.DTOs;
using MartenTaskManagment.Events;
using MartenTaskManagment.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MartenTaskManagment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly IDocumentSession _session;
        private readonly ITaskModelService _taskService;

        public TasksController(IDocumentSession session, ITaskModelService taskService)
        {
            _session = session;
            _taskService = taskService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllTasks()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreationDTO taskDto)
        {
            var taskId = Guid.NewGuid();
            var taskCreatedEvent = new TaskCreated
            {
                TaskId = taskId,
                Title = taskDto.Title,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                Status = taskDto.Status,
                AssignedUser = taskDto.AssignedUser,
                CreatedAt = DateTime.UtcNow
            };

            await AppendEvent(taskId, taskCreatedEvent);
            return CreatedAtAction(nameof(GetTaskById), new { id = taskId }, taskDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(Guid id)
        {
            var task = await _taskService.GetTaskModelById(id);
            return task != null ? Ok(task) : NotFound();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] TaskUpdateDTO updatedTask)
        {
            var existingTask = await _taskService.GetTaskModelById(id);
            if (existingTask == null) return NotFound();

            var updateEvents = _taskService.GetUpdateEvents(id, existingTask, updatedTask);
            foreach (var updateEvent in updateEvents)
            {
                await AppendEvent(id, updateEvent);
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var taskDeletedEvent = new TaskDeleted
            {
                TaskId = id,
                DeletedAt = DateTime.UtcNow
            };

            await AppendEvent(id, taskDeletedEvent);
            return NoContent();
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignTask(Guid id, [FromBody] string assignedUser)
        {
            var taskAssignedEvent = new TaskAssigned
            {
                TaskId = id,
                AssignedUser = assignedUser,
                AssignedDate = DateTime.UtcNow
            };

            await AppendEvent(id, taskAssignedEvent);
            return Ok();
        }

        [HttpGet("count-by-status")]
        public async Task<int> CountTasksByStatusFromEventsAsync(string status)
        {
            var sql = @"
                SELECT COUNT(*) 
                FROM (
                    SELECT DISTINCT ON (stream_id) stream_id, 
                        COALESCE(data->>'NewStatus', data->>'Status') AS Status
                    FROM mt_events 
                    WHERE LOWER(type) IN ('task_created', 'task_status_updated')  -- Include both creation and status update events
                      AND (data->>'NewStatus' IS NOT NULL OR data->>'Status' IS NOT NULL)  -- Ensure at least one status field is present
                    ORDER BY stream_id, seq_id DESC  -- Get the latest event per stream
                ) AS latest_status
                WHERE Status = @status;
            ";

            using var connection = _session.Connection;
            return await connection.ExecuteScalarAsync<int>(sql, new { status });
        }

        [HttpGet("average-completion-time")]
        public async Task<IActionResult> GetAverageCompletionTime()
        {
            var averageTime = await _taskService.GetAverageCompletionTimeAsync();
            return Ok(new { AverageCompletionTime = averageTime });
        }

        [HttpGet("tasks-per-user")]
        public async Task<IActionResult> GetTasksPerUser()
        {
            var tasksPerUser = await _taskService.GetTasksPerUserAsync();
            return Ok(tasksPerUser);
        }

        private async Task AppendEvent(Guid streamId, object @event)
        {
            _session.Events.Append(streamId, @event);
            await _session.SaveChangesAsync();
        }
    }
}
