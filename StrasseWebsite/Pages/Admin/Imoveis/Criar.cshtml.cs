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
    public class CriarModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CriarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Imovel Imovel { get; set; } = new();

        [BindProperty]
        public string? NovoProprietario { get; set; }

        public SelectList ProprietariosList { get; set; } = default!;

        public SelectList CorretoresList { get; set; } = default!;


        public async Task<IActionResult> OnGetAsync()
        {
            await CarregarListasAsync();

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Imovel.Proprietario");
            ModelState.Remove("Imovel.Corretor");
            ModelState.Remove("Imovel.CodigoImovel");

            if (!string.IsNullOrWhiteSpace(NovoProprietario))
            {
                ModelState.Remove("Imovel.ProprietarioId");
            }

            if (!ModelState.IsValid)
            {
                await CarregarListasAsync();

                return Page();
            }


            // ==========================================================
            // PROPRIETÁRIO
            // ==========================================================

            if (!string.IsNullOrWhiteSpace(NovoProprietario))
            {
                var nomeProprietario =
                    NovoProprietario.Trim();

                var pessoa =
                    await _context.Pessoas
                        .FirstOrDefaultAsync(p =>
                            EF.Functions.ILike(
                                p.Nome,
                                nomeProprietario));

                if (pessoa == null)
                {
                    pessoa = new Pessoa
                    {
                        Nome = nomeProprietario,

                        Tipo = TipoPessoa.Fisica,

                        Documento = "PENDENTE",

                        DataNascimento =
                            new DateTime(
                                1900,
                                1,
                                1,
                                0,
                                0,
                                0,
                                DateTimeKind.Utc),

                        Telefone = "PENDENTE",

                        Email = "strasse.imoveis@gmail.com",

                        Perfil = TipoPerfil.Proprietario,

                        Ativo = true,

                        DataCadastro = DateTime.UtcNow
                    };

                    _context.Pessoas.Add(pessoa);

                    Imovel.Proprietario = pessoa;
                }
                else
                {
                    Imovel.ProprietarioId = pessoa.Id;
                }
            }


            // ==========================================================
            // CÓDIGO DO IMÓVEL
            // ==========================================================

            Imovel.CodigoImovel =
                "STR-" +
                new Random().Next(1000, 9999);

            Imovel.DataCadastro =
                DateTime.UtcNow;


            // ==========================================================
            // SALVA
            // ==========================================================

            _context.Imoveis.Add(Imovel);

            await _context.SaveChangesAsync();


            TempData["MensagemSucesso"] =
                $"Imóvel {Imovel.CodigoImovel} cadastrado com sucesso!";

            return RedirectToPage("/Admin/Dashboard");
        }


        // ==============================================================
        // LISTAS
        // ==============================================================

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