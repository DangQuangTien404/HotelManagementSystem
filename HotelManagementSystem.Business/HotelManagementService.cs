using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Business;

public class HotelManagementService
{
    private readonly HotelManagementDbContext _context;

    public HotelManagementService(HotelManagementDbContext context)
    {
        _context = context;
    }

    public async Task EnsureSeedDataAsync()
    {
        if (!await _context.Users.AnyAsync(u => u.Username == "admin"))
        {
            _context.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = "admin123",
                FullName = "Quản trị viên",
                Role = "Admin",
                Email = "admin@luxuryhotel.com"
            });
            await _context.SaveChangesAsync();
        }

        var userA = await _context.Users.FirstOrDefaultAsync(u => u.Username == "a");
        if (userA == null)
        {
            userA = new User
            {
                Username = "a",
                PasswordHash = "123",
                FullName = "Nhân viên A",
                Role = "Staff",
                Email = "staff_a@luxuryhotel.com"
            };
            _context.Users.Add(userA);
            await _context.SaveChangesAsync();
        }

        var isStaffExist = await _context.Staffs.AnyAsync(s => s.UserId == userA.Id);
        if (!isStaffExist)
        {
            _context.Staffs.Add(new Staff { UserId = userA.Id, Position = "Dọn dẹp", Shift = "Ca làm việc mặc định" });
            await _context.SaveChangesAsync();
        }
    }

    public Task<List<HotelService>> GetActiveServicesAsync() => _context.HotelServices.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
    public Task<List<HotelService>> GetAllHotelServicesAsync() => _context.HotelServices.ToListAsync();
    public async Task AddHotelServiceAsync(HotelService service) { _context.HotelServices.Add(service); await _context.SaveChangesAsync(); }

    public Task<User?> GetUserByIdAsync(int userId) => _context.Users.FindAsync(userId).AsTask();
    public async Task UpdateUserProfileAsync(User user) { _context.Users.Update(user); await _context.SaveChangesAsync(); }

    public async Task RegisterCustomerAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
    }

    public Task<List<Reservation>> GetReservationsByCustomerAsync(int customerId) =>
        _context.Reservations.Include(r => r.Room).Where(r => r.CustomerId == customerId).OrderByDescending(r => r.CreatedAt).ToListAsync();

    public Task<Reservation?> GetReservationDetailAsync(int reservationId) =>
        _context.Reservations.Include(r => r.Room).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Id == reservationId);

    public Task<List<ReservationService>> GetReservationServicesAsync(int reservationId) =>
        _context.ReservationServices.Include(s => s.HotelService).Where(s => s.ReservationId == reservationId).OrderByDescending(s => s.AddedAt).ToListAsync();

    public async Task<bool> AddServiceToReservationAsync(int reservationId, int serviceId, int quantity)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        var service = await _context.HotelServices.FirstOrDefaultAsync(s => s.Id == serviceId && s.IsActive);
        if (reservation == null || service == null) return false;

        var existing = await _context.ReservationServices.FirstOrDefaultAsync(x => x.ReservationId == reservationId && x.HotelServiceId == serviceId && x.UnitPrice == service.Price);
        if (existing != null) existing.Quantity += quantity;
        else
        {
            _context.ReservationServices.Add(new ReservationService
            {
                ReservationId = reservationId,
                HotelServiceId = serviceId,
                Quantity = quantity,
                UnitPrice = service.Price,
                AddedAt = DateTime.Now
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task RemoveReservationServiceAsync(int reservationId, int reservationServiceId)
    {
        var item = await _context.ReservationServices.FirstOrDefaultAsync(x => x.Id == reservationServiceId && x.ReservationId == reservationId);
        if (item != null)
        {
            _context.ReservationServices.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public Task<List<Room>> GetAllRoomsAsync() => _context.Rooms.OrderBy(r => r.RoomNumber).ToListAsync();
    public async Task SaveRoomAsync(Room room)
    {
        if (room.Id == 0)
        {
            room.Status = "Available";
            _context.Rooms.Add(room);
        }
        else _context.Attach(room).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public Task<List<Customer>> GetCustomersAsync() => _context.Customers.ToListAsync();
    public Task<Room?> GetRoomAsync(int roomId) => _context.Rooms.FindAsync(roomId).AsTask();

    public async Task CreateReservationAsync(Reservation reservation, List<int> selectedServiceIds)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        if (selectedServiceIds.Any())
        {
            var services = await _context.HotelServices.Where(s => selectedServiceIds.Contains(s.Id) && s.IsActive).ToListAsync();
            foreach (var service in services)
            {
                _context.ReservationServices.Add(new ReservationService
                {
                    ReservationId = reservation.Id,
                    HotelServiceId = service.Id,
                    Quantity = 1,
                    UnitPrice = service.Price,
                    AddedAt = DateTime.Now
                });
            }
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Reservation?> GetCheckoutReservationAsync(int id)
    {
        return await _context.Reservations.Include(r => r.Room).Include(r => r.Customer).FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<decimal> GetServiceTotalPriceAsync(int reservationId)
    {
        return await _context.ReservationServices.Where(rs => rs.ReservationId == reservationId).SumAsync(rs => rs.UnitPrice * rs.Quantity);
    }

    public async Task<bool> CheckoutAsync(int id)
    {
        var res = await _context.Reservations.Include(r => r.Room).FirstOrDefaultAsync(r => r.Id == id);
        if (res == null) return false;
        res.Status = "CheckedOut";
        if (res.Room != null) res.Room.Status = "Available";
        await _context.SaveChangesAsync();
        return true;
    }

    
    public Task<List<Reservation>> GetAdminActiveReservationsAsync() =>
        _context.Reservations.Include(r => r.Room).Include(r => r.Customer)
            .Where(r => r.Status == "Confirmed" || r.Status == "CheckedIn")
            .OrderBy(r => r.CheckInDate).ToListAsync();
public Task<List<Reservation>> GetCompletedReservationsInRangeAsync(DateTime start, DateTime end) =>
        _context.Reservations.Include(r => r.Room).Include(r => r.Customer)
            .Where(r => r.Status == "CheckedOut" && r.CheckOutDate >= start && r.CheckOutDate <= end)
            .ToListAsync();

    public Task<decimal> GetServiceRevenueInRangeAsync(DateTime start, DateTime end) =>
        _context.ReservationServices.Where(rs => rs.Reservation != null && rs.Reservation.CheckOutDate >= start && rs.Reservation.CheckOutDate <= end)
            .SumAsync(rs => rs.UnitPrice * rs.Quantity);

    public Task<List<MaintenanceTask>> GetPendingMaintenanceTasksAsync() =>
        _context.MaintenanceTasks.Include(t => t.Room).Include(t => t.AssignedStaff).ThenInclude(s => s!.User)
            .Where(t => t.Status != "Completed").OrderByDescending(t => t.CreatedAt).ToListAsync();

    public async Task<bool> CompleteMaintenanceTaskAsync(int taskId)
    {
        var task = await _context.MaintenanceTasks.Include(t => t.Room).FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null) return false;
        task.Status = "Completed";
        if (task.Room != null) task.Room.Status = "Available";
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<List<MaintenanceTask>> GetMaintenanceTasksByUserAsync(int userId) =>
        _context.MaintenanceTasks.Include(t => t.Room).Where(t => t.AssignedTo == userId).OrderByDescending(t => t.CreatedAt).ToListAsync();

    public async Task<bool> UpdateMaintenanceTaskStatusAsync(int taskId, int userId)
    {
        var task = await _context.MaintenanceTasks.Include(t => t.Room).FirstOrDefaultAsync(t => t.Id == taskId && t.AssignedTo == userId);
        if (task == null) return false;
        task.Status = "Completed";
        if (task.Room != null) task.Room.Status = "Available";
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<List<RoomCleaning>> GetCleaningTasksByStaffAsync(int staffId) =>
        _context.RoomCleanings.Include(c => c.Room).Where(c => c.StaffId == staffId).OrderByDescending(c => c.CreatedAt).ToListAsync();

    public async Task<bool> CompleteCleaningTaskAsync(int taskId, int staffId)
    {
        var task = await _context.RoomCleanings.Include(c => c.Room).FirstOrDefaultAsync(c => c.Id == taskId && c.StaffId == staffId);
        if (task == null) return false;
        task.Status = "Done";
        if (task.Room != null) task.Room.Status = "Available";
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<List<Staff>> GetStaffListAsync() => _context.Staffs.Include(s => s.User).OrderBy(s => s.Id).ToListAsync();
    public async Task DeleteStaffAsync(int id)
    {
        var staff = await _context.Staffs.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
        if (staff == null) return;
        _context.Staffs.Remove(staff);
        if (staff.User != null) _context.Users.Remove(staff.User);
        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<int, int>> GetActiveReservationByRoomAsync()
    {
        return await _context.Reservations.Where(r => r.Status == "Booked" || r.Status == "CheckedIn")
            .ToDictionaryAsync(r => r.RoomId, r => r.Id);
    }

    public async Task<(int total, int available, int maintenance, int occupied)> GetRoomStatsAsync()
    {
        var allRooms = await _context.Rooms.ToListAsync();
        return (allRooms.Count, allRooms.Count(r => r.Status == "Available"), allRooms.Count(r => r.Status == "Maintenance"), allRooms.Count(r => r.Status == "Occupied"));
    }

    public async Task<bool> AssignCleaningAsync(int roomId, int staffUserId)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room == null) return false;
        room.Status = "Cleaning";
        _context.RoomCleanings.Add(new RoomCleaning { RoomId = roomId, CleanedBy = staffUserId, CleaningDate = DateTime.Now, Status = "In Progress" });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignMaintenanceAsync(MaintenanceTask task)
    {
        task.CreatedAt = DateTime.Now;
        task.Status = "In Progress";
        _context.MaintenanceTasks.Add(task);
        var room = await _context.Rooms.FindAsync(task.RoomId);
        if (room != null) room.Status = "Maintenance";
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<List<User>> GetStaffUsersAsync() => _context.Users.Where(u => u.Role == "Staff" || u.Role == "Admin").ToListAsync();
}
