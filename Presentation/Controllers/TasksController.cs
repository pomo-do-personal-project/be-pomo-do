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
public class TasksController(ITaskService taskService) : ControllerBase
{
    private readonly ITaskService _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));

    /// <summary>Retrieves filtered and sorted tasks for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TaskDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<TaskDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<TaskDto>>>> GetTasks(
        [FromQuery] TaskFilterParams filterParams,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var tasks = await _taskService.GetTasksAsync(userId, filterParams, cancellationToken);
        return Ok(ApiResponse<List<TaskDto>>.Ok(tasks, "Lấy danh sách công việc thành công."));
    }

    /// <summary>Retrieves specific task details by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> GetTaskById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.GetTaskByIdAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<TaskDto>.Ok(task, "Lấy chi tiết công việc thành công."));
    }

    /// <summary>Creates a new task item with optional tags and initial subtasks.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> CreateTask(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.CreateTaskAsync(userId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TaskDto>.Ok(task, "Tạo công việc thành công."));
    }

    /// <summary>Updates an existing task item.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> UpdateTask(
        [FromRoute] Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.UpdateTaskAsync(id, userId, request, cancellationToken);
        return Ok(ApiResponse<TaskDto>.Ok(task, "Cập nhật công việc thành công."));
    }

    /// <summary>Updates the status of a task item (e.g. Todo, InProgress, Completed, Cancelled).</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> UpdateTaskStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.UpdateTaskStatusAsync(id, userId, request.Status, cancellationToken);
        return Ok(ApiResponse<TaskDto>.Ok(task, "Cập nhật trạng thái công việc thành công."));
    }

    /// <summary>Soft deletes a task item (moves to trash bin).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteTask(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _taskService.SoftDeleteTaskAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Đã chuyển công việc vào thùng rác thành công."));
    }

    /// <summary>Restores a soft-deleted task item from trash bin.</summary>
    [HttpPost("{id:guid}/restore")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<TaskDto>>> RestoreTask(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var task = await _taskService.RestoreTaskAsync(id, userId, cancellationToken);
        return Ok(ApiResponse<TaskDto>.Ok(task, "Khôi phục công việc thành công."));
    }

    /// <summary>Adds a new subtask to an existing task.</summary>
    [HttpPost("{taskId:guid}/subtasks")]
    [ProducesResponseType(typeof(ApiResponse<SubtaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SubtaskDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SubtaskDto>>> AddSubtask(
        [FromRoute] Guid taskId,
        [FromBody] CreateSubtaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var subtask = await _taskService.AddSubtaskAsync(taskId, userId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<SubtaskDto>.Ok(subtask, "Thêm subtask thành công."));
    }

    /// <summary>Toggles completion status of a subtask.</summary>
    [HttpPatch("{taskId:guid}/subtasks/{subtaskId:guid}/toggle")]
    [ProducesResponseType(typeof(ApiResponse<SubtaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SubtaskDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SubtaskDto>>> ToggleSubtask(
        [FromRoute] Guid taskId,
        [FromRoute] Guid subtaskId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var subtask = await _taskService.ToggleSubtaskAsync(taskId, subtaskId, userId, cancellationToken);
        return Ok(ApiResponse<SubtaskDto>.Ok(subtask, "Cập nhật trạng thái subtask thành công."));
    }

    /// <summary>Updates title or order of a subtask.</summary>
    [HttpPut("{taskId:guid}/subtasks/{subtaskId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SubtaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<SubtaskDto>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SubtaskDto>>> UpdateSubtask(
        [FromRoute] Guid taskId,
        [FromRoute] Guid subtaskId,
        [FromBody] UpdateSubtaskRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var subtask = await _taskService.UpdateSubtaskAsync(taskId, subtaskId, userId, request, cancellationToken);
        return Ok(ApiResponse<SubtaskDto>.Ok(subtask, "Cập nhật subtask thành công."));
    }

    /// <summary>Permanently deletes a subtask.</summary>
    [HttpDelete("{taskId:guid}/subtasks/{subtaskId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteSubtask(
        [FromRoute] Guid taskId,
        [FromRoute] Guid subtaskId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        await _taskService.DeleteSubtaskAsync(taskId, subtaskId, userId, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Đã xóa subtask thành công."));
    }

    // Helper: Safely extracts current user ID from token claims
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
