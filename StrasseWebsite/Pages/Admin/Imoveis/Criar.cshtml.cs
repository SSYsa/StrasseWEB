using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StrasseWebsite.Data;
using StrasseWebsite.Models;

namespace StrasseWebsite.Pages.Admin.Imoveis
{
    public class CriarModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CriarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Imovel Imovel { get; set; } = new();

        // Listas para alimentar os selects da tela
        public SelectList ProprietariosList { get; set; } = default!;
        public SelectList CorretoresList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            // Carrega as pessoas do banco para os selects (Ajuste o filtro se você tiver uma propriedade 'Tipo' na classe Pessoa)
            var pessoas = await _context.Pessoas.ToListAsync();

            ProprietariosList = new SelectList(pessoas, "Id", "Nome");
            CorretoresList = new SelectList(pessoas, "Id", "Nome");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Removemos a validação de propriedades de navegação para evitar erros do ModelState
            ModelState.Remove("Imovel.Proprietario");
            ModelState.Remove("Imovel.Corretor");
            ModelState.Remove("Imovel.CodigoImovel"); // Vamos gerar o código automaticamente abaixo

            if (!ModelState.IsValid)
            {
                // Se der erro, recarrega as listas antes de devolver a página
                var pessoas = await _context.Pessoas.ToListAsync();
                ProprietariosList = new SelectList(pessoas, "Id", "Nome");
                CorretoresList = new SelectList(pessoas, "Id", "Nome");
                return Page();
            }

            // Geração automática de código simples (Ex: STR- + número aleatório ou ID se fosse possível)
            // Você pode customizar essa lógica de negócio depois
            Imovel.CodigoImovel = "STR-" + new Random().Next(1000, 9999);
            Imovel.DataCadastro = DateTime.UtcNow;

            _context.Imoveis.Add(Imovel);
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = $"Imóvel {Imovel.CodigoImovel} cadastrado com sucesso!";
            return RedirectToPage("/Admin/Dashboard"); // Ou a página de listagem que preferir
        }
    }
}