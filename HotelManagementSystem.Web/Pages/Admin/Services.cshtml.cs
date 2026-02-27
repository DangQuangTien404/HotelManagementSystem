using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Data.Context;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ServicesModel : PageModel
    {
        private readonly HotelManagementDbContext _context;
        public ServicesModel(HotelManagementDbContext context) => _context = context;

        public IList<HotelService> HotelServices { get; set; } = default!;

        [BindProperty]
        public HotelService NewService { get; set; } = new();

        public async Task OnGetAsync()
        {
            HotelServices = await _context.HotelServices.ToListAsync();
        }

        public async Task<IActionResult> OnPostAddServiceAsync()
        {
            _context.HotelServices.Add(NewService);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}
