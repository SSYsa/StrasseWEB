using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StrasseWebsite.Data;
using System.Security.Claims;

namespace StrasseWebsite.Pages.Admin
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LoginModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Usuario { get; set; } = string.Empty;

        [BindProperty]
        public string Senha { get; set; } = string.Empty;

        public string MensagemErro { get; set; } = string.Empty;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Validação básica de campos vazios
            if (string.IsNullOrEmpty(Usuario) || string.IsNullOrEmpty(Senha))
            {
                MensagemErro = "Usuário e senha são obrigatórios.";
                return Page();
            }

            // 2. Busca na tabela correta (singular: Usuario) pelo campo exato de login/usuario
            // Nota: Altere 'u.Login' ou 'u.NomeUsuario' para o nome exato da propriedade na sua classe Usuario se for diferente
            var usuarioDb = await _context.Acessos
                .FirstOrDefaultAsync(u => u.Usuario == Usuario);

            // 3. Valida se o usuário existe e se a senha está correta
            if (usuarioDb == null || usuarioDb.Senha != Senha)
            {
                MensagemErro = "Usuário ou senha incorretos.";
                return Page();
            }

            // 4. Cria as credenciais (Claims) seguras para a sessão
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuarioDb.Usuario),
                new Claim(ClaimTypes.NameIdentifier, usuarioDb.Id.ToString()),
                new Claim("PessoaId", usuarioDb.PessoaId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 5. Grava o cookie de autenticação no navegador
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // 6. Redireciona para a tela de cadastro de imóveis administrada
            return RedirectToPage("/Admin/Dashboard");
        }
    }
}