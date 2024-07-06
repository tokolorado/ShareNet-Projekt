using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ShareNet.Areas.Identity.Pages.Account
{
    [Authorize]
    public class IndexLoggedInModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
