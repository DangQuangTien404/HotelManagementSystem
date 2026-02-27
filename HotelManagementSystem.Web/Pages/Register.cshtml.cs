using HotelManagementSystem.Business;
using HotelManagementSystem.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagementSystem.Web.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly HotelManagementService _hotelService;

        public RegisterModel(HotelManagementService hotelService)
        {
            _hotelService = hotelService;
        }

        [BindProperty]
        public Customer Customer { get; set; } = new();

        public void OnGet() => ModelState.Clear();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                Customer.CreatedAt = DateTime.Now;
                if (string.IsNullOrWhiteSpace(Customer.Address)) Customer.Address = "N/A";
                if (string.IsNullOrWhiteSpace(Customer.IdentityNumber)) Customer.IdentityNumber = "N/A";
                if (string.IsNullOrWhiteSpace(Customer.Email)) Customer.Email = "none@hotel.com";

                await _hotelService.RegisterCustomerAsync(Customer);
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Lỗi: " + ex.InnerException?.Message ?? ex.Message);
                return Page();
            }
        }
    }
}
