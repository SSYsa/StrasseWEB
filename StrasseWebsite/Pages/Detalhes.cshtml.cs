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

        // Força a página a ler o ID vindo da URL (?id=...) sem quebrar a rota
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        // Propriedade que guardará o imóvel encontrado para o HTML usar
        public Imovel Imovel { get; set; } = null!;

        // Mantém o controle se o cliente veio da aba comprar ou construir
        public string Operacao { get; set; } = "comprar";

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Seu código que busca no banco... ex:
            Imovel = await _context.Imoveis.FindAsync(id);

            if (Imovel == null)
            {
                // Vai cair na sua validação do HTML e exibir "Imóvel não encontrado" amigavelmente
                return Page();
            }

            return Page();
        }
    }
}