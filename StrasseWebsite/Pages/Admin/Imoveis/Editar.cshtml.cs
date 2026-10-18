using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StrasseWebsite.Data;
using StrasseWebsite.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        [BindProperty]
        public string? NovasImagensLinks { get; set; }

        [BindProperty]
        public string? NovoProprietario { get; set; }

        public SelectList ProprietariosList { get; set; } = default!;

        public SelectList CorretoresList { get; set; } = default!;


        // ==========================================================
        // GET
        // ==========================================================

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var imovelBanco =
                await _context.Imoveis
                    .Include(i => i.ImagensSecundarias)
                    .FirstOrDefaultAsync(
                        m => m.Id == id);

            if (imovelBanco == null)
            {
                return NotFound();
            }

            Imovel = imovelBanco;

            await CarregarListasAsync();

            return Page();
        }


        // ==========================================================
        // POST
        // ==========================================================

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Imovel.Proprietario");
            ModelState.Remove("Imovel.Corretor");
            ModelState.Remove("Imovel.CodigoImovel");
            ModelState.Remove("Imovel.ImagensSecundarias");


            if (!ModelState.IsValid)
            {
                await CarregarListasAsync();

                return Page();
            }


            // ======================================================
            // BUSCA O IMÓVEL ORIGINAL
            // ======================================================

            var imovelBanco =
                await _context.Imoveis
                    .Include(i => i.ImagensSecundarias)
                    .FirstOrDefaultAsync(
                        i => i.Id == Imovel.Id);


            if (imovelBanco == null)
            {
                return NotFound();
            }


            // ======================================================
            // DADOS BÁSICOS
            // ======================================================

            imovelBanco.Titulo =
                Imovel.Titulo;

            imovelBanco.Descricao =
                Imovel.Descricao;

            imovelBanco.Preco =
                Imovel.Preco;

            imovelBanco.CidadeEstado =
                Imovel.CidadeEstado;

            imovelBanco.Bairro =
                Imovel.Bairro;

            imovelBanco.Endereco =
                Imovel.Endereco;


            // ======================================================
            // ESTRUTURA
            // ======================================================

            imovelBanco.QuantidadeQuartos =
                Imovel.QuantidadeQuartos;

            imovelBanco.QuantidadeBanheiros =
                Imovel.QuantidadeBanheiros;

            imovelBanco.QuantidadeVagasGaragem =
                Imovel.QuantidadeVagasGaragem;


            // ======================================================
            // PROPRIETÁRIO
            // ======================================================

            if (!string.IsNullOrWhiteSpace(NovoProprietario))
            {
                var novoProprietario =
                    new Pessoa
                    {
                        Nome =
                            NovoProprietario.Trim(),

                        Tipo =
                            TipoPessoa.Fisica,

                        Documento =
                            "PENDENTE",

                        DataNascimento =
                            new DateTime(
                                1900,
                                1,
                                1,
                                0,
                                0,
                                0,
                                DateTimeKind.Utc),

                        Telefone =
                            "PENDENTE",

                        Email =
                            "strasse.imoveis@gmail.com",

                        Perfil =
                            TipoPerfil.Cliente,

                        Ativo =
                            true,

                        DataCadastro =
                            DateTime.UtcNow
                    };


                _context.Pessoas.Add(
                    novoProprietario);

                imovelBanco.Proprietario =
                    novoProprietario;
            }
            else
            {
                imovelBanco.ProprietarioId =
                    Imovel.ProprietarioId;
            }


            // ======================================================
            // CORRETOR
            // ======================================================

            imovelBanco.CorretorId =
                Imovel.CorretorId;


            // ======================================================
            // TIPO DO IMÓVEL
            // ======================================================

            imovelBanco.Tipo =
                Imovel.Tipo;


            // ======================================================
            // TRANSAÇÃO
            //
            // Aqui entra:
            // Compra
            // Construcao
            // Aluguel
            // ======================================================

            imovelBanco.Transacao =
                Imovel.Transacao;


            // ======================================================
            // IMAGEM
            // ======================================================

            imovelBanco.UrlImagemPrincipal =
                Imovel.UrlImagemPrincipal;


            // ======================================================
            // STATUS
            // ======================================================

            imovelBanco.Status =
                Imovel.Status;


            // ======================================================
            // CONSTRUTORA
            // ======================================================

            imovelBanco.ConstrutoraResponsavel =
                Imovel.ConstrutoraResponsavel;


            // ======================================================
            // NOVAS IMAGENS
            // ======================================================

            if (!string.IsNullOrWhiteSpace(
                NovasImagensLinks))
            {
                var links =
                    NovasImagensLinks.Split(
                        new[]
                        {
                            ',',
                            '\n',
                            '\r'
                        },
                        StringSplitOptions.RemoveEmptyEntries);


                foreach (var link in links)
                {
                    var urlLimpa =
                        link.Trim();


                    if (!string.IsNullOrWhiteSpace(
                        urlLimpa))
                    {
                        imovelBanco
                            .ImagensSecundarias
                            .Add(
                                new ImagemImovel
                                {
                                    UrlImagem =
                                        urlLimpa
                                });
                    }
                }
            }


            // ======================================================
            // SALVA
            // ======================================================

            await _context.SaveChangesAsync();


            TempData["MensagemSucesso"] =
                $"Imóvel {imovelBanco.CodigoImovel} atualizado com sucesso!";


            return RedirectToPage(
                "/Admin/Dashboard");
        }


        // ==========================================================
        // EXCLUIR IMAGEM
        // ==========================================================

        public async Task<IActionResult>
            OnPostDeletarImagemAsync(
                int imagemId)
        {
            var imagem =
                await _context.ImagensImoveis
                    .FindAsync(imagemId);


            if (imagem == null)
            {
                return BadRequest(
                    new
                    {
                        sucesso = false,
                        mensagem =
                            "Imagem não encontrada."
                    });
            }


            try
            {
                _context.ImagensImoveis
                    .Remove(imagem);

                await _context
                    .SaveChangesAsync();


                return new JsonResult(
                    new
                    {
                        sucesso = true
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        sucesso = false,
                        mensagem =
                            ex.Message
                    });
            }
        }


        // ==========================================================
        // CARREGAR LISTAS
        // ==========================================================

        private async Task CarregarListasAsync()
        {
            var pessoas =
                await _context.Pessoas
                    .OrderBy(p => p.Nome)
                    .ToListAsync();


            ProprietariosList =
                new SelectList(
                    pessoas,
                    "Id",
                    "Nome",
                    Imovel.ProprietarioId);


            CorretoresList =
                new SelectList(
                    pessoas,
                    "Id",
                    "Nome",
                    Imovel.CorretorId);
        }
    }
}