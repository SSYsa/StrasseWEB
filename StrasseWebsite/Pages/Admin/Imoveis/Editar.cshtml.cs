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
            // 1. Remove validações que o formulário não preenche
            ModelState.Remove("Imovel.Proprietario");
            ModelState.Remove("Imovel.Corretor");
            ModelState.Remove("Imovel.CodigoImovel");
            ModelState.Remove("Imovel.ImagensSecundarias");

            if (!ModelState.IsValid)
            {
                var pessoas = await _context.Pessoas.ToListAsync();
                ProprietariosList = new SelectList(pessoas, "Id", "Nome");
                CorretoresList = new SelectList(pessoas, "Id", "Nome");
                return Page();
            }

            // 2. Busca o imóvel do banco TRAZENDO as imagens secundárias atuais (.Include)
            var imovelBanco = await _context.Imoveis
                .Include(i => i.ImagensSecundarias)
                .FirstOrDefaultAsync(i => i.Id == Imovel.Id);

            if (imovelBanco == null)
            {
                return NotFound();
            }

            // 3. Atualiza os dados base do imóvel
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

            // 4. Processa e adiciona os novos links na lista vinculada ao imóvel
            if (!string.IsNullOrEmpty(NovasImagensLinks))
            {
                // Quebra o texto por vírgula ou quebra de linha
                var links = NovasImagensLinks.Split(new[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var link in links)
                {
                    var urlLimpa = link.Trim();
                    if (!string.IsNullOrEmpty(urlLimpa))
                    {
                        // Adiciona direto na lista do imóvel rastreado pelo EF Core
                        imovelBanco.ImagensSecundarias.Add(new ImagemImovel
                        {
                            UrlImagem = urlLimpa
                        });
                    }
                }
            }

            // 5. Salva tudo de uma vez (Imóvel e Novas Imagens)
            await _context.SaveChangesAsync();

            TempData["MensagemSucesso"] = $"Imóvel {imovelBanco.CodigoImovel} atualizado com sucesso!";
            return RedirectToPage("/Admin/Dashboard");
        }
        public async Task<IActionResult> OnPostDeletarImagemAsync(int imagemId)
        {
            // Busca a imagem diretamente pelo ID dela
            var imagem = await _context.ImagensImoveis.FindAsync(imagemId);

            if (imagem == null)
            {
                return BadRequest(new { sucesso = false, mensagem = "Imagem não encontrada." });
            }

            try
            {
                _context.ImagensImoveis.Remove(imagem);
                await _context.SaveChangesAsync();
                return PackageJson(new { sucesso = true }); // Retorna sucesso para o JavaScript
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = ex.Message });
            }
        }

        // Auxiliar para retornar JSON no Razor Pages sem complicações
        private IActionResult PackageJson(object data)
        {
            return new JsonResult(data);
        }
    }
}