using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StrasseWebsite.Data;
using StrasseWebsite.Models;

namespace StrasseWebsite.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Imovel> Imoveis { get; set; } = new();

        // Carrega a lista de imóveis ao entrar na página
        public async Task OnGetAsync()
        {
            Imoveis = await _context.Imoveis
                .Include(i => i.Corretor)
                .OrderByDescending(i => i.DataCadastro)
                .ToListAsync();
        }

        // Action disparada quando o usuário clica em Excluir
        public async Task<IActionResult> OnPostDeletarAsync(int id)
        {
            var imovel = await _context.Imoveis.FindAsync(id);

            if (imovel != null)
            {
                _context.Imoveis.Remove(imovel);
                await _context.SaveChangesAsync();
                TempData["MensagemSucesso"] = "Imóvel excluído com sucesso!";
            }

            return RedirectToPage();
        }
    }
}