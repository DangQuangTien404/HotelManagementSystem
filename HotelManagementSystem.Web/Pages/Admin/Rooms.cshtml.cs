using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class RoomsModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        public RoomsModel(HotelManagementDbContext context) => _context = context;

        public IList<Room> Rooms { get; set; } = default!;

        [BindProperty]
        public Room NewRoom { get; set; } = new();

        public async Task OnGetAsync()
        {
            Rooms = await _context.Rooms.OrderBy(r => r.RoomNumber).ToListAsync();
        }

        // Thêm hoặc Cập nhật phòng
        public async Task<IActionResult> OnPostSaveRoomAsync()
        {
            if (NewRoom.Id == 0)
            {
                NewRoom.Status = "Available"; // Mặc định phòng mới là Trống
                _context.Rooms.Add(NewRoom);
            }
            else
            {
                _context.Attach(NewRoom).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}