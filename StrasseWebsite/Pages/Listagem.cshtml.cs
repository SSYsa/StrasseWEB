using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StrasseWebsite.Data;
using StrasseWebsite.Models;

namespace StrasseWebsite.Pages
{
    public class ListagemModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ListagemModel(ApplicationDbContext context)
        {
            _context = context;
        }


        // ==========================================================
        // RESULTADOS
        // ==========================================================

        public List<Imovel> Imoveis { get; set; } = new();


        // ==========================================================
        // OPERAÇÃO
        // ==========================================================

        [BindProperty(SupportsGet = true)]
        public string Operacao { get; set; } = "comprar";


        // ==========================================================
        // FILTROS
        // ==========================================================

        [BindProperty(SupportsGet = true)]
        public string? Tipo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Construtora { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Quartos { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Banheiros { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Garagem { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? PrecoMin { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? PrecoMax { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? OrdenarPor { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Localizacao { get; set; }


        // ==========================================================
        // CONSTRUTORAS DISPONÍVEIS
        // ==========================================================

        public List<string> ConstrutorasDisponiveis { get; set; } = new();


        // ==========================================================
        // GET PRINCIPAL
        // ==========================================================

        public async Task OnGetAsync()
        {
            Operacao = NormalizarOperacao(Operacao);


            // ======================================================
            // CONSTRUTORAS
            //
            // A construtora pode existir em imóveis de COMPRA,
            // ALUGUEL ou CONSTRUÇÃO.
            //
            // Portanto, a lista é carregada independentemente
            // da operação atualmente selecionada.
            // ======================================================

            var valoresConstrutoras =
                await _context.Imoveis
                    .AsNoTracking()
                    .Where(i =>
                        i.Status == "Disponivel" &&
                        i.ConstrutoraResponsavel != null)
                    .Select(i => i.ConstrutoraResponsavel!)
                    .ToListAsync();

            ConstrutorasDisponiveis =
                valoresConstrutoras
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(c => c)
                    .ToList();


            // ======================================================
            // QUERY BASE
            // ======================================================

            var query =
                _context.Imoveis
                    .AsNoTracking()
                    .Include(i => i.ImagensSecundarias)
                    .Where(i => i.Status == "Disponivel")
                    .AsQueryable();


            // ======================================================
            // OPERAÇÃO
            // ======================================================

            switch (Operacao)
            {
                case "alugar":

                    query = query.Where(i =>
                        i.Transacao == TipoTransacao.Aluguel);

                    break;


                case "construir":

                    query = query.Where(i =>
                        i.Transacao == TipoTransacao.Construcao);

                    break;


                default:

                    query = query.Where(i =>
                        i.Transacao == TipoTransacao.Compra);

                    break;
            }


            // ======================================================
            // TIPO DE IMÓVEL
            // ======================================================

            if (!string.IsNullOrWhiteSpace(Tipo))
            {
                if (Enum.TryParse<TipoImovel>(
                    Tipo,
                    true,
                    out var tipoEnum))
                {
                    query = query.Where(i =>
                        i.Tipo == tipoEnum);
                }
            }


            // ======================================================
            // CONSTRUTORA
            //
            // O filtro funciona independentemente da operação.
            // A operação continua sendo aplicada separadamente.
            // ======================================================

            if (!string.IsNullOrWhiteSpace(Construtora))
            {
                var construtoraSelecionada =
                    Construtora.Trim();

                query = query.Where(i =>
                    i.ConstrutoraResponsavel != null &&
                    i.ConstrutoraResponsavel == construtoraSelecionada);
            }


            // ======================================================
            // QUARTOS
            // ======================================================

            if (Quartos.HasValue)
            {
                query = query.Where(i =>
                    i.QuantidadeQuartos >= Quartos.Value);
            }


            // ======================================================
            // BANHEIROS
            // ======================================================

            if (Banheiros.HasValue)
            {
                query = query.Where(i =>
                    i.QuantidadeBanheiros >= Banheiros.Value);
            }


            // ======================================================
            // GARAGEM
            // ======================================================

            if (Garagem.HasValue)
            {
                query = query.Where(i =>
                    i.QuantidadeVagasGaragem >= Garagem.Value);
            }


            // ======================================================
            // PREÇO MÍNIMO
            // ======================================================

            if (PrecoMin.HasValue)
            {
                query = query.Where(i =>
                    i.Preco >= PrecoMin.Value);
            }


            // ======================================================
            // PREÇO MÁXIMO
            // ======================================================

            if (PrecoMax.HasValue)
            {
                query = query.Where(i =>
                    i.Preco <= PrecoMax.Value);
            }


            // ======================================================
            // LOCALIZAÇÃO
            // ======================================================

            if (!string.IsNullOrWhiteSpace(Localizacao))
            {
                var termoLocalizacao =
                    Localizacao.Trim();

                query = query.Where(i =>
                    EF.Functions.ILike(
                        i.CidadeEstado,
                        $"%{termoLocalizacao}%"
                    )
                    ||
                    EF.Functions.ILike(
                        i.Bairro,
                        $"%{termoLocalizacao}%"
                    )
                    ||
                    EF.Functions.ILike(
                        i.Endereco,
                        $"%{termoLocalizacao}%"
                    ));
            }


            // ======================================================
            // ORDENAÇÃO
            // ======================================================

            switch (OrdenarPor)
            {
                case "preco-asc":

                    query = query.OrderBy(i =>
                        i.Preco);

                    break;


                case "preco-desc":

                    query = query.OrderByDescending(i =>
                        i.Preco);

                    break;


                default:

                    query = query.OrderByDescending(i =>
                        i.DataCadastro);

                    break;
            }


            // ======================================================
            // EXECUTA QUERY
            // ======================================================

            Imoveis =
                await query.ToListAsync();
        }


        // ==========================================================
        // SUGESTÕES DE LOCALIZAÇÃO
        // ==========================================================

        public async Task<JsonResult> OnGetSugerirLocalizacaoAsync(
            string? termo)
        {
            termo ??= string.Empty;

            var query =
                _context.Imoveis
                    .AsNoTracking()
                    .Where(i =>
                        i.Status == "Disponivel" &&
                        i.CidadeEstado != null &&
                        i.CidadeEstado.Trim() != "");


            if (!string.IsNullOrWhiteSpace(termo))
            {
                var termoBusca =
                    termo.Trim();

                query = query.Where(i =>
                    EF.Functions.ILike(
                        i.CidadeEstado,
                        $"%{termoBusca}%"
                    ));
            }


            var localizacoes =
                await query
                    .Select(i => i.CidadeEstado.Trim())
                    .Distinct()
                    .OrderBy(i => i)
                    .Take(20)
                    .ToListAsync();


            return new JsonResult(localizacoes);
        }


        // ==========================================================
        // NORMALIZA OPERAÇÃO
        // ==========================================================

        private string NormalizarOperacao(string? operacao)
        {
            if (string.IsNullOrWhiteSpace(operacao))
            {
                return "comprar";
            }

            return operacao.Trim().ToLowerInvariant() switch
            {
                "alugar" => "alugar",

                "aluguel" => "alugar",

                "construir" => "construir",

                "construcao" => "construir",

                "construção" => "construir",

                "comprar" => "comprar",

                _ => "comprar"
            };
        }


        // ==========================================================
        // FORMATA CIDADE / ESTADO
        // ==========================================================

        public string FormatarCidadeEstado(
            string cidadeEstado)
        {
            if (string.IsNullOrWhiteSpace(cidadeEstado))
            {
                return cidadeEstado;
            }

            return cidadeEstado
                .Trim()
                .Replace("/", " - ")
                .Replace(",", " - ");
        }
    }
}