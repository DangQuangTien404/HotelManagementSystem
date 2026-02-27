using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ServicesModel : PageModel
    {
        private readonly HotelManagementService _hotelService;
        public ServicesModel(HotelManagementService hotelService) => _hotelService = hotelService;

        public IList<HotelService> HotelServices { get; set; } = default!;

        [BindProperty]
        public HotelService NewService { get; set; } = new();

        public async Task OnGetAsync() => HotelServices = await _hotelService.GetAllHotelServicesAsync();

        public async Task<IActionResult> OnPostAddServiceAsync()
        {
            await _hotelService.AddHotelServiceAsync(NewService);
            return RedirectToPage();
        }
    }
}
