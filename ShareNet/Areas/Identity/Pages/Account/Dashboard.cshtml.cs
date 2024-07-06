using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShareNet.Areas.Identity.Pages.Account
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        public IdentityUser? user1;

        public DashboardModel(UserManager<IdentityUser> userManager)
        {
            this._userManager = userManager;

        }

        public void OnGet()
        {
            var task = _userManager.GetUserAsync(User);
            task.Wait();

            user1 = task.Result; 
        }
    }
}
