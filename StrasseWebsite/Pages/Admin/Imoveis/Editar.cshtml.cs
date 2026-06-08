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
            // 1. Remove validações de objetos de navegação que o formulário não preenche
            ModelState.Remove("Imovel.Proprietario");
            ModelState.Remove("Imovel.Corretor");
            ModelState.Remove("Imovel.ImagensSecundarias"); // Adicione se houver essa lista no Model

            if (!ModelState.IsValid)
            {
                // Se cair aqui, as listas recarregam e a página é exibida com os erros
                var pessoas = await _context.Pessoas.ToListAsync();
                ProprietariosList = new SelectList(pessoas, "Id", "Nome");
                CorretoresList = new SelectList(pessoas, "Id", "Nome");
                return Page();
            }

            // 2. Busca o imóvel original do banco para evitar perda de dados omitidos
            var imovelBanco = await _context.Imoveis
                .Include(i => i.ImagensSecundarias)
                .FirstOrDefaultAsync(i => i.Id == Imovel.Id);

            if (imovelBanco == null)
            {
                return NotFound();
            }

            // 3. Atualiza apenas os campos permitidos da tela
            imovelBanco.Titulo = Imovel.Titulo;
            imovelBanco.Descricao = Imovel.Descricao;
            imovelBanco.Preco = Imovel.Preco;
            imovelBanco.CidadeEstado = Imovel.CidadeEstado;
            imovelBanco.Bairro = Imovel.Bairro;
            imovelBanco.Endereco = Imovel.Endereco;
            imovelBanco.QuantidadeQuartos = Imovel.QuantidadeQuartos;
            imovelBanco.QuantidadeBanheiros = Imovel.QuantidadeBanheiros;
            imovelBanco.QuantidadeVagasGaragem = Imovel.QuantidadeVagasGaragem;
            imovelBanco.ProprietarioId = Imovel.ProprietarioId;
            imovelBanco.CorretorId = Imovel.CorretorId;
            imovelBanco.Tipo = Imovel.Tipo;
            imovelBanco.Transacao = Imovel.Transacao;
            imovelBanco.UrlImagemPrincipal = Imovel.UrlImagemPrincipal;
            imovelBanco.Status = Imovel.Status;
            imovelBanco.ConstrutoraResponsavel = Imovel.ConstrutoraResponsavel;

            // 4. Processa os links múltiplos de imagem
            if (!string.IsNullOrEmpty(NovasImagensLinks))
            {
                var links = NovasImagensLinks.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var link in links)
                {
                    _context.ImagensImoveis.Add(new ImagemImovel
                    {
                        ImovelId = imovelBanco.Id,
                        UrlImagem = link.Trim()
                    });
                }
            }

            // 5. Salva as alterações rastreadas
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = $"Imóvel {imovelBanco.CodigoImovel} atualizado com sucesso!";
            return RedirectToPage("/Admin/Dashboard");
        }
    }
}