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

        // Nome do novo proprietário, caso ele não exista na lista
        [BindProperty]
        public string? NovoProprietario { get; set; }

        // Listas para alimentar os selects
        public SelectList ProprietariosList { get; set; } = default!;
        public SelectList CorretoresList { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            await CarregarListasAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remove validações das propriedades de navegação.
            ModelState.Remove("Imovel.Proprietario");
            ModelState.Remove("Imovel.Corretor");
            ModelState.Remove("Imovel.CodigoImovel");

            // ==========================================================
            // VERIFICA SE FOI INFORMADO UM NOVO PROPRIETÁRIO
            // ==========================================================

            if (!string.IsNullOrWhiteSpace(NovoProprietario))
            {
                // Como estamos criando uma nova Pessoa somente com o nome,
                // removemos a validação do ProprietarioId.
                // O ID será definido abaixo.
                ModelState.Remove("Imovel.ProprietarioId");
            }

            // ==========================================================
            // VALIDAÇÃO DO FORMULÁRIO
            // ==========================================================

            if (!ModelState.IsValid)
            {
                await CarregarListasAsync();

                return Page();
            }

            // ==========================================================
            // TRATA O PROPRIETÁRIO
            // ==========================================================

            if (!string.IsNullOrWhiteSpace(NovoProprietario))
            {
                var nomeProprietario = NovoProprietario.Trim();

                // Procura pessoa existente pelo nome.
                var pessoa = await _context.Pessoas
                    .FirstOrDefaultAsync(p =>
                        EF.Functions.ILike(p.Nome, nomeProprietario));

                if (pessoa == null)
                {
                    // IMPORTANTE:
                    // Sua classe Pessoa possui vários campos obrigatórios.
                    //
                    // Por isso, somente Nome NÃO é suficiente para criar
                    // uma Pessoa válida.
                    //
                    // Aqui estamos criando os valores temporários apenas
                    // para permitir o cadastro pelo formulário atual.

                    pessoa = new Pessoa
                    {
                        Nome = nomeProprietario,

                        Tipo = TipoPessoa.Fisica,

                        Documento = "PENDENTE",

                        DataNascimento = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc),

                        Telefone = "PENDENTE",

                        Email = "strasse.imoveis@gmail.com",

                        Perfil = TipoPerfil.Proprietario,

                        Ativo = true,

                        DataCadastro = DateTime.UtcNow
                    };

                    _context.Pessoas.Add(pessoa);

                    // Vincula a nova Pessoa ao imóvel.
                    // O EF Core irá cuidar do Id.
                    Imovel.Proprietario = pessoa;
                }
                else
                {
                    // Pessoa já existe.
                    Imovel.ProprietarioId = pessoa.Id;
                }
            }

            // ==========================================================
            // GERAÇÃO AUTOMÁTICA DO CÓDIGO DO IMÓVEL
            // ==========================================================

            Imovel.CodigoImovel = "STR-" + new Random().Next(1000, 9999);

            Imovel.DataCadastro = DateTime.UtcNow;

            // ==========================================================
            // SALVA O IMÓVEL
            // ==========================================================

            _context.Imoveis.Add(Imovel);

            await _context.SaveChangesAsync();

            // ==========================================================
            // MENSAGEM DE SUCESSO
            // ==========================================================

            TempData["MensagemSucesso"] =
                $"Imóvel {Imovel.CodigoImovel} cadastrado com sucesso!";

            return RedirectToPage("/Admin/Dashboard");
        }

        // ==============================================================
        // CARREGA AS LISTAS
        // ==============================================================

        private async Task CarregarListasAsync()
        {
            var pessoas = await _context.Pessoas
                .OrderBy(p => p.Nome)
                .ToListAsync();

            ProprietariosList = new SelectList(
                pessoas,
                "Id",
                "Nome",
                Imovel.ProprietarioId
            );

            CorretoresList = new SelectList(
                pessoas,
                "Id",
                "Nome",
                Imovel.CorretorId
            );
        }
    }
}