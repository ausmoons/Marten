using Dapper;
using Marten;
using MartenTaskManagment.DTOs;
using MartenTaskManagment.Events;
using MartenTaskManagment.Models;
using MartenTaskManagment.Services;
using Microsoft.AspNetCore.Mvc;

namespace MartenTaskManagment.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly IDocumentSession _session;
        private readonly TaskModelService _taskService;

        public TasksController(IDocumentSession session, TaskModelService taskService)
        {
            _session = session;
            _taskService = taskService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreationDTO taskDto)
        {
            var task = new TaskModel
            {
                Id = Guid.NewGuid(),
                Title = taskDto.Title,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                Status = taskDto.Status,
                AssignedUser = taskDto.AssignedUser,
                CreatedAt = DateTime.UtcNow
            };

            var taskCreatedEvent = new TaskCreated
            {
                TaskId = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Status = task.Status,
                AssignedUser = task.AssignedUser,
                CreatedAt = task.CreatedAt
            };

            _session.Events.StartStream(task.Id, taskCreatedEvent);
            await _session.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(Guid id)
        {
            var task = await _taskService.GetTaskModelById(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] TaskUpdateDTO updatedTask)
        {
            var existingTask = await _taskService.GetTaskModelById(id);
            if (existingTask == null) return NotFound();

            if (existingTask.Title != updatedTask.Title)
            {
                _session.Events.Append(id, new TaskTitleUpdated
                {
                    TaskId = id,
                    NewTitle = updatedTask.Title,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (existingTask.Description != updatedTask.Description)
            {
                _session.Events.Append(id, new TaskDescriptionUpdated
                {
                    TaskId = id,
                    NewDescription = updatedTask.Description,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (updatedTask.DueDate.HasValue && existingTask.DueDate != updatedTask.DueDate.Value)
            {
                _session.Events.Append(id, new TaskDueDateUpdated
                {
                    TaskId = id,
                    NewDueDate = updatedTask.DueDate.Value,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await _session.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            _session.Events.Append(id, new TaskDeleted
            {
                TaskId = id,
                DeletedAt = DateTime.UtcNow
            });
            await _session.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignTask(Guid id, [FromBody] string assignedUser)
        {
            _session.Events.Append(id, new TaskAssigned
            {
                TaskId = id,
                AssignedUser = assignedUser,
                AssignedDate = DateTime.UtcNow
            });
            await _session.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("count-by-status")]
        public async Task<IActionResult> CountTasksByStatus([FromQuery] string status)
        {
            var tasks = await _session.Query<TaskModel>().Where(t => t.Status == status).ToListAsync();
            return Ok(new { Status = status, Count = tasks.Count });
        }

        [HttpGet("average-completion-time")]
        public async Task<IActionResult> GetAverageCompletionTime()
        {
            var completedTasks = await _session.Query<TaskModel>()
                                               .Where(t => t.Status == "Completed")
                                               .ToListAsync();
            if (!completedTasks.Any()) return Ok(0);

            var averageTime = completedTasks.Average(t => (t.DueDate - t.CreatedAt).TotalDays);
            return Ok(new { AverageCompletionTime = averageTime });
        }

        [HttpGet("tasks-per-user")]
        public async Task<IActionResult> GetTasksPerUser()
        {
            var sql = @"
                SELECT data->>'AssignedUser' AS User, COUNT(*) AS TaskCount
                FROM mt_events
                WHERE data->>'AssignedUser' IS NOT NULL
                GROUP BY data->>'AssignedUser'
            ";

            var tasksPerUser = await _session.Connection
                .QueryAsync<UserTaskCount>(sql);

            return Ok(tasksPerUser);
        }
    }
}
