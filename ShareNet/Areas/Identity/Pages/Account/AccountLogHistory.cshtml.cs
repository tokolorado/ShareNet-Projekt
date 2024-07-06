using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShareNet.Data;
using System.Runtime.Intrinsics.X86;

namespace ShareNet.Areas.Identity.Pages.Account
{
    public class AccountLogHistoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public IdentityUser? user1;


        public List<Models.ShareNetLog> Logi { get; set; } = new List<Models.ShareNetLog>();
        
        public AccountLogHistoryModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            this._userManager = userManager;
            _context = context;
        }

        public void OnGet()
        {
            var task = _userManager.GetUserAsync(User);
            task.Wait();

            user1 = task.Result;

            Logi = _context.Logi.Where(x => x.UserId == user1.Id).OrderByDescending(x => x.Id).ToList();
            
        }
    }
}
