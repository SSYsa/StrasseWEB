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

        public async Task<IActionResult> OnGetAsync()
        {
            // Validação de segurança para ID zerado ou negativo
            if (Id <= 0)
            {
                return NotFound();
            }

            // Busca o imóvel no banco trazendo junto as fotos secundárias e os dados do Corretor
            var imovelBanco = await _context.Imoveis
                .Include(i => i.ImagensSecundarias)
                .Include(i => i.Corretor)
                .FirstOrDefaultAsync(m => m.Id == Id);

            // Se o ID não existir no banco, retorna página não encontrada (404)
            if (imovelBanco == null)
            {
                return NotFound();
            }

            Imovel = imovelBanco;

            // Define dinamicamente o modo da página com base no tipo de transação salvo no banco
            Operacao = Imovel.Transacao == TipoTransacao.Construcao ? "construir" : "comprar";

            return Page();
        }
    }
}