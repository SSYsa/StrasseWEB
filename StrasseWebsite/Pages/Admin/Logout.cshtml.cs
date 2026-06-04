using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StrasseWebsite.Pages.Admin
{
    [AllowAnonymous] // <--- CRÍTICO: Permite que você acesse essa página para deslogar
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            // 1. Limpa o cookie de autenticação do navegador
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Redireciona de forma limpa para a Home pública do site
            return RedirectToPage("/Index");
        }
    }
}