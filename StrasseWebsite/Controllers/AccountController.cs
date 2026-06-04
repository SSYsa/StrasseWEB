using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StrasseWebsite.Data;
using System.Security.Claims;

namespace StrasseWebsite.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string usuario, string senha)
    {
        // Busca o usuário incluindo os dados da pessoa vinculada
        var conta = await _context.Acessos
            .Include(a => a.Pessoa)
            .FirstOrDefaultAsync(a => a.Usuario == usuario && a.Senha == senha); // Em produção, usar hash!

        if (conta == null || !conta.Pessoa.Ativo)
        {
            ViewBag.Erro = "Usuário ou senha inválidos, ou conta inativa.";
            return View();
        }

        // Criando as credenciais da sessão (Claims)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, conta.Pessoa.Nome),
            new Claim(ClaimTypes.NameIdentifier, conta.Pessoa.Id.ToString()),
            new Claim(ClaimTypes.Email, conta.Pessoa.Email),
            new Claim(ClaimTypes.Role, conta.Pessoa.Perfil.ToString()) // Define a Role (ex: Admin, Corretor)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}