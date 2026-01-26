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
    public class AdminTaskService : IAdminTaskService
    {
        private readonly IMaintenanceTaskRepository _maintenanceTaskRepository;
        private readonly IGenericRepository<Room> _roomRepository;

        public AdminTaskService(
            IMaintenanceTaskRepository maintenanceTaskRepository,
            IGenericRepository<Room> roomRepository)
        {
            _maintenanceTaskRepository = maintenanceTaskRepository;
            _roomRepository = roomRepository;
        }

        public async Task<IEnumerable<MaintenanceTaskDto>> GetAllMaintenanceTasksAsync()
        {
            var tasks = await _maintenanceTaskRepository.GetAllTasksWithDetailsAsync();
            return tasks.Select(MapToDto);
        }

        public async Task<MaintenanceTaskDto?> GetMaintenanceTaskByIdAsync(int id)
        {
            var task = await _maintenanceTaskRepository.GetTaskWithDetailsByIdAsync(id);
            return task == null ? null : MapToDto(task);
        }

        public async Task AssignMaintenanceTaskAsync(MaintenanceTaskDto taskDto)
        {
            var task = new MaintenanceTask
            {
                RoomId = taskDto.RoomId,
                AssignedTo = taskDto.AssignedTo,
                Priority = taskDto.Priority,
                Deadline = taskDto.Deadline,
                Status = "Pending",
                Description = taskDto.Description,
                CreatedAt = DateTime.UtcNow
            };

            await _maintenanceTaskRepository.AddAsync(task);

            // Update room status to Maintenance
            var room = await _roomRepository.GetByIdAsync(taskDto.RoomId);
            if (room != null)
            {
                room.Status = "Maintenance";
                await _roomRepository.UpdateAsync(room);
            }
        }

        public async Task UpdateMaintenanceTaskAsync(MaintenanceTaskDto taskDto)
        {
            var task = await _maintenanceTaskRepository.GetByIdAsync(taskDto.Id);
            if (task == null)
            {
                throw new Exception("Maintenance task not found.");
            }

            task.RoomId = taskDto.RoomId;
            task.AssignedTo = taskDto.AssignedTo;
            task.Priority = taskDto.Priority;
            task.Deadline = taskDto.Deadline;
            task.Description = taskDto.Description;

            await _maintenanceTaskRepository.UpdateAsync(task);
        }

        public async Task UpdateTaskStatusAsync(int taskId, string status)
        {
            var task = await _maintenanceTaskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new Exception("Maintenance task not found.");
            }

            task.Status = status;

            if (status == "Completed")
            {
                task.CompletedAt = DateTime.UtcNow;
            }

            await _maintenanceTaskRepository.UpdateAsync(task);
        }

        public async Task ApproveMaintenanceTaskAsync(int taskId, int adminId)
        {
            var task = await _maintenanceTaskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new Exception("Maintenance task not found.");
            }

            if (task.Status != "Completed")
            {
                throw new Exception("Only completed tasks can be approved.");
            }

            task.Status = "Approved";
            task.ApprovedBy = adminId;
            await _maintenanceTaskRepository.UpdateAsync(task);

            // Update room status to Available after approval
            var room = await _roomRepository.GetByIdAsync(task.RoomId);
            if (room != null)
            {
                room.Status = "Available";
                await _roomRepository.UpdateAsync(room);
            }
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

