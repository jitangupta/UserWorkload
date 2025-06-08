using Microsoft.AspNetCore.Mvc.RazorPages;
using UserWorkload.Models;
using Microsoft.EntityFrameworkCore;
using UserWorkload.Context;

namespace UserWorkload.Pages
{
    public class UserModel : PageModel
    {
        private readonly DemoDeckDbContext _context;

        public UserModel(DemoDeckDbContext context)
        {
            _context = context;
        }

        public bool IsAuthenticated => User.Identity.IsAuthenticated;
        public IReadOnlyList<User>? Users { get; set; }

        public async Task OnGetAsync()
        {
            if (IsAuthenticated)
            {
                Users = await _context.Users.AsNoTracking().ToListAsync();
            }
            else
            {
                Users = null;
            }
        }
    }
}
