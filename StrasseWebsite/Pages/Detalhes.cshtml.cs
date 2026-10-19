using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StrasseWebsite.Data;
using StrasseWebsite.Models;

namespace StrasseWebsite.Pages
{
    public class DetalhesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetalhesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Força a página a ler o ID vindo da URL (?id=...)
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        // Imóvel encontrado no banco
        public Imovel Imovel { get; set; } = null!;

        // Mantém o controle se o cliente veio da aba comprar ou construir
        public string Operacao { get; set; } = "comprar";

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            /*
             * Carrega o imóvel juntamente com:
             *
             * - ImagensSecundarias
             * - Corretor
             *
             * Dessa forma a galeria consegue utilizar
             * todas as imagens cadastradas no banco.
             */
            Imovel = await _context.Imoveis
                .Include(i => i.ImagensSecundarias)
                .Include(i => i.Corretor)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (Imovel == null)
            {
                return Page();
            }

            return Page();
        }
    }
}