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

        public IList<User> Users { get; set; } = new List<User>();

        public async Task OnGetAsync()
        {
            Users = await _context.Users.AsNoTracking().ToListAsync();
        }
    }
}
