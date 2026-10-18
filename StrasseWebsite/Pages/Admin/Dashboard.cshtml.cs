using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StrasseWebsite.Data;
using StrasseWebsite.Models;

namespace StrasseWebsite.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // IMÓVEIS
        // ==========================================================

        public List<Imovel> Imoveis { get; set; } = new();


        // ==========================================================
        // FILTROS DINÂMICOS
        // ==========================================================

        // Cidades/Estados realmente existentes no banco
        public List<string> CidadesEstadosDisponiveis { get; set; } = new();

        // Tipos realmente existentes nos imóveis cadastrados
        public List<TipoImovel> TiposImoveisDisponiveis { get; set; } = new();


        // ==========================================================
        // GET
        // ==========================================================

        public async Task OnGetAsync()
        {
            // ======================================================
            // CARREGA OS IMÓVEIS
            // ======================================================

            Imoveis = await _context.Imoveis
                .Include(i => i.Corretor)
                .OrderByDescending(i => i.DataCadastro)
                .ToListAsync();


            // ======================================================
            // CIDADES / ESTADOS
            //
            // Vem diretamente do campo:
            //
            // Imovel.CidadeEstado
            //
            // Não existem cidades fixadas no código.
            // ======================================================

            CidadesEstadosDisponiveis = Imoveis
                .Where(i =>
                    !string.IsNullOrWhiteSpace(i.CidadeEstado))
                .Select(i =>
                    i.CidadeEstado.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();


            // ======================================================
            // TIPOS DE IMÓVEL
            //
            // Vem diretamente do campo:
            //
            // Imovel.Tipo
            //
            // Somente tipos realmente utilizados aparecem no filtro.
            // ======================================================

            TiposImoveisDisponiveis = Imoveis
                .Select(i => i.Tipo)
                .Distinct()
                .OrderBy(t => t.ToString())
                .ToList();
        }


        // ==========================================================
        // EXCLUIR IMÓVEL
        // ==========================================================

        public async Task<IActionResult> OnPostDeletarAsync(int id)
        {
            var imovel = await _context.Imoveis
                .FirstOrDefaultAsync(i => i.Id == id);

            if (imovel != null)
            {
                _context.Imoveis.Remove(imovel);

                await _context.SaveChangesAsync();

                TempData["MensagemSucesso"] =
                    "Imóvel excluído com sucesso!";
            }

            return RedirectToPage();
        }


        // ==========================================================
        // FORMATA CIDADE / ESTADO
        //
        // Exemplo:
        //
        // Blumenau/SC
        //
        // vira:
        //
        // Blumenau - SC
        // ==========================================================

        public string FormatarCidadeEstado(string? cidadeEstado)
        {
            if (string.IsNullOrWhiteSpace(cidadeEstado))
            {
                return string.Empty;
            }

            return cidadeEstado
                .Trim()
                .Replace("/", " - ")
                .Replace(",", " - ");
        }
    }
}