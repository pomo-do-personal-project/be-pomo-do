using System.Security.Claims;
using be_pomo_do.Application.DTOs.Tasks;
using be_pomo_do.Application.Services;
using be_pomo_do.Presentation.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be_pomo_do.Presentation.Controllers;

/// <summary>Endpoints for managing tasks, subtasks, tags association, and Eisenhower priority matrix.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    /// <summary>Initializes a new instance of <see cref="TasksController"/> with traditional constructor dependency injection.</summary>
    /// <param name="taskService">The task application service instance.</param>
    public TasksController(ITaskService taskService)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
    }

    /// <summary>Retrieves filtered and sorted tasks for the current user.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TaskDto>>>> GetTasks([FromQuery] TaskFilterParams filterParams)
    {
        var userId = GetCurrentUserId();
        var tasks = await _taskService.GetTasksAsync(userId, filterParams);
        return Ok(ApiResponse<List<TaskDto>>.Ok(tasks, "Lấy danh sách công việc thành công."));
    }

    /// <summary>Retrieves specific task details by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> GetTaskById([FromRoute] Guid id)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.GetTaskByIdAsync(id, userId);
        return Ok(ApiResponse<TaskDto>.Ok(task, "Lấy chi tiết công việc thành công."));
    }

    /// <summary>Creates a new task item with optional tags and initial subtasks.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskDto>>> CreateTask([FromBody] CreateTaskRequest request)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.CreateTaskAsync(userId, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TaskDto>.Ok(task, "Tạo công việc thành công."));
    }

    /// <summary>Updates an existing task item.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> UpdateTask(
        [FromRoute] Guid id,
        [FromBody] UpdateTaskRequest request)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.UpdateTaskAsync(id, userId, request);
        return Ok(ApiResponse<TaskDto>.Ok(task, "Cập nhật công việc thành công."));
    }

    /// <summary>Updates the status of a task item (e.g. Todo, InProgress, Completed, Cancelled).</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> UpdateTaskStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateTaskStatusRequest request)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.UpdateTaskStatusAsync(id, userId, request.Status);
        return Ok(ApiResponse<TaskDto>.Ok(task, "Cập nhật trạng thái công việc thành công."));
    }

    /// <summary>Soft deletes a task item (moves to trash bin).</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteTask([FromRoute] Guid id)
    {
        var userId = GetCurrentUserId();
        await _taskService.SoftDeleteTaskAsync(id, userId);
        return Ok(ApiResponse<string>.Ok("Đã chuyển công việc vào thùng rác thành công."));
    }

    /// <summary>Permanently deletes a task item from database (hard delete).</summary>
    [HttpDelete("{id:guid}/hard")]
    public async Task<ActionResult<ApiResponse<string>>> HardDeleteTask([FromRoute] Guid id)
    {
        var userId = GetCurrentUserId();
        await _taskService.HardDeleteTaskAsync(id, userId);
        return Ok(ApiResponse<string>.Ok("Đã xóa vĩnh viễn công việc thành công."));
    }

    /// <summary>Restores a soft-deleted task item from trash bin.</summary>
    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> RestoreTask([FromRoute] Guid id)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.RestoreTaskAsync(id, userId);
        return Ok(ApiResponse<TaskDto>.Ok(task, "Khôi phục công việc thành công."));
    }

    /// <summary>Adds a new subtask to an existing task.</summary>
    [HttpPost("{taskId:guid}/subtasks")]
    public async Task<ActionResult<ApiResponse<SubtaskDto>>> AddSubtask(
        [FromRoute] Guid taskId,
        [FromBody] CreateSubtaskRequest request)
    {
        var userId = GetCurrentUserId();
        var subtask = await _taskService.AddSubtaskAsync(taskId, userId, request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SubtaskDto>.Ok(subtask, "Thêm subtask thành công."));
    }

    /// <summary>Toggles completion status of a subtask.</summary>
    [HttpPatch("{taskId:guid}/subtasks/{subtaskId:guid}/toggle")]
    public async Task<ActionResult<ApiResponse<SubtaskDto>>> ToggleSubtask(
        [FromRoute] Guid taskId,
        [FromRoute] Guid subtaskId)
    {
        var userId = GetCurrentUserId();
        var subtask = await _taskService.ToggleSubtaskAsync(taskId, subtaskId, userId);
        return Ok(ApiResponse<SubtaskDto>.Ok(subtask, "Cập nhật trạng thái subtask thành công."));
    }

    /// <summary>Updates title or order of a subtask.</summary>
    [HttpPut("{taskId:guid}/subtasks/{subtaskId:guid}")]
    public async Task<ActionResult<ApiResponse<SubtaskDto>>> UpdateSubtask(
        [FromRoute] Guid taskId,
        [FromRoute] Guid subtaskId,
        [FromBody] UpdateSubtaskRequest request)
    {
        var userId = GetCurrentUserId();
        var subtask = await _taskService.UpdateSubtaskAsync(taskId, subtaskId, userId, request);
        return Ok(ApiResponse<SubtaskDto>.Ok(subtask, "Cập nhật subtask thành công."));
    }

    /// <summary>Permanently deletes a subtask.</summary>
    [HttpDelete("{taskId:guid}/subtasks/{subtaskId:guid}")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteSubtask(
        [FromRoute] Guid taskId,
        [FromRoute] Guid subtaskId)
    {
        var userId = GetCurrentUserId();
        await _taskService.DeleteSubtaskAsync(taskId, subtaskId, userId);
        return Ok(ApiResponse<string>.Ok("Đã xóa subtask thành công."));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");
        }

        return userId;
    }
}
