using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StrasseWebsite.Data;
using StrasseWebsite.Models;

namespace StrasseWebsite.Pages.Admin.Imoveis
{
    public class EditarModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Imovel Imovel { get; set; } = default!;

        // Propriedade para receber novos links da galeria digitados no input de texto
        [BindProperty]
        public string? NovasImagensLinks { get; set; }

        public SelectList ProprietariosList { get; set; } = default!;
        public SelectList CorretoresList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var imovelBanco = await _context.Imoveis
                .Include(i => i.ImagensSecundarias)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (imovelBanco == null) return NotFound();

            Imovel = imovelBanco;

            var pessoas = await _context.Pessoas.ToListAsync();
            ProprietariosList = new SelectList(pessoas, "Id", "Nome");
            CorretoresList = new SelectList(pessoas, "Id", "Nome");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Imovel.Proprietario");
            ModelState.Remove("Imovel.Corretor");

            if (!ModelState.IsValid)
            {
                var pessoas = await _context.Pessoas.ToListAsync();
                ProprietariosList = new SelectList(pessoas, "Id", "Nome");
                CorretoresList = new SelectList(pessoas, "Id", "Nome");
                return Page();
            }

            // Atualiza os dados bases do Imóvel no EF Core
            _context.Attach(Imovel).State = EntityState.Modified;
            _context.Entry(Imovel).Property(x => x.DataCadastro).IsModified = false;
            _context.Entry(Imovel).Property(x => x.CodigoImovel).IsModified = false;

            // Processa os links múltiplos de imagem adicionados (separados por vírgula ou quebra de linha)
            if (!string.IsNullOrEmpty(NovasImagensLinks))
            {
                var links = NovasImagensLinks.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var link in links)
                {
                    _context.ImagensImoveis.Add(new ImagemImovel
                    {
                        ImovelId = Imovel.Id,
                        UrlImagem = link.Trim()
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["MensagemSucesso"] = $"Imóvel {Imovel.CodigoImovel} atualizado com sucesso!";
            return RedirectToPage("/Admin/Dashboard");
        }
    }
}