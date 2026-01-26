using BLL.Interfaces;
using DAL.Interfaces;
using DTOs;
using DTOs.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class StaffTaskService : IStaffTaskService
    {
        private readonly IMaintenanceTaskRepository _maintenanceTaskRepository;

        public StaffTaskService(IMaintenanceTaskRepository maintenanceTaskRepository)
        {
            _maintenanceTaskRepository = maintenanceTaskRepository;
        }

        public async Task<IEnumerable<MaintenanceTaskDto>> GetTasksByStaffIdAsync(int staffUserId)
        {
            var allTasks = await _maintenanceTaskRepository.GetAllTasksWithDetailsAsync();
            
            var myTasks = allTasks.Where(t => t.AssignedTo == staffUserId);
            
            return myTasks.Select(MapToDto);
        }

        public async Task<MaintenanceTaskDto?> GetTaskByIdForStaffAsync(int taskId, int staffUserId)
        {
            var task = await _maintenanceTaskRepository.GetTaskWithDetailsByIdAsync(taskId);
            
            if (task == null || task.AssignedTo != staffUserId)
            {
                return null;
            }
            
            return MapToDto(task);
        }

        public async Task UpdateMyTaskStatusAsync(int taskId, int staffUserId, string status)
        {
            var task = await _maintenanceTaskRepository.GetByIdAsync(taskId);
            
            if (task == null)
            {
                throw new Exception("Task not found.");
            }
            
            if (task.AssignedTo != staffUserId)
            {
                throw new UnauthorizedAccessException("You are not assigned to this task.");
            }
            
            var validStatuses = new[] { "Pending", "InProgress", "Completed" };
            if (!validStatuses.Contains(status))
            {
                throw new Exception("Invalid status.");
            }
            
            task.Status = status;
            
            if (status == "Completed")
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            
            await _maintenanceTaskRepository.UpdateAsync(task);
        }

        public async Task CompleteMyTaskAsync(int taskId, int staffUserId)
        {
            await UpdateMyTaskStatusAsync(taskId, staffUserId, "Completed");
        }

        private MaintenanceTaskDto MapToDto(MaintenanceTask task)
        {
            return new MaintenanceTaskDto
            {
                Id = task.Id,
                RoomId = task.RoomId,
                RoomNumber = task.Room?.RoomNumber ?? string.Empty,
                AssignedTo = task.AssignedTo,
                AssignedStaffName = task.AssignedStaff?.FullName ?? "Unassigned",
                Priority = task.Priority,
                Deadline = task.Deadline,
                Status = task.Status,
                Description = task.Description,
                CreatedAt = task.CreatedAt,
                CompletedAt = task.CompletedAt,
                ApprovedBy = task.ApprovedBy,
                ApprovedByName = task.ApprovedByUser?.FullName ?? string.Empty
            };
        }
    }
}
