using HotelManagementSystem.Models;

namespace HotelManagementSystem.Business;

public interface IMaintenanceService
{
    Task<List<Staff>> GetTechnicalStaff();
    Task<bool> CreateMaintenanceTask(int roomId, int staffUserId, string description, string priority, int creatorId);
    Task<List<MaintenanceTask>> GetPendingTasks(int staffId);
    Task<List<MaintenanceTask>> GetOpenTasks();
    Task<MaintenanceTask?> CompleteTaskByStaff(int taskId, int staffId);
    Task<MaintenanceTask?> CompleteTaskByAdmin(int taskId);
    Task<List<Room>> GetRoomsNotInMaintenance();
    Task<List<User>> GetAssignableStaffUsers();
    Task<bool> AssignMaintenanceTask(MaintenanceTask task);
}
