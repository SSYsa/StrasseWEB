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

        // Vinculação automática dos parâmetros via GET (Mantém o estado na URL)
        [BindProperty(SupportsGet = true)]
        public string Operacao { get; set; } = "comprar";

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

        public List<Imovel> Imoveis { get; set; } = new();

        public async Task OnGetAsync()
        {
            // 1. Normalização das Abas para compatibilidade com Home/Menu
            if (string.IsNullOrEmpty(Operacao))
            {
                Operacao = "comprar";
            }
            else
            {
                Operacao = Operacao.ToLower();
                // Se vier "imobiliaria" mapeia para "comprar" para unificar os links externos
                if (Operacao == "imobiliaria") Operacao = "comprar";
                if (Operacao != "comprar" && Operacao != "construir") Operacao = "comprar";
            }

            // Inicializa query base buscando apenas imóveis disponíveis
            var query = _context.Imoveis
                .Include(i => i.ImagensSecundarias)
                .Where(i => i.Status == "Disponivel");

            // 2. Filtro de Contexto da Aba Principal
            if (Operacao == "comprar")
            {
                // Mostra imóveis de compra tradicional (usados/prontos) e ignora construções exclusivas
                query = query.Where(i => i.Transacao == TipoTransacao.Compra &&
                                         (i.ConstrutoraResponsavel == null || i.ConstrutoraResponsavel == ""));
            }
            else if (Operacao == "construir")
            {
                // Mostra imóveis sob regime de construção ou vinculados a construtoras listadas
                query = query.Where(i => i.Transacao == TipoTransacao.Construcao ||
                                         i.ConstrutoraResponsavel == "FG" ||
                                         i.ConstrutoraResponsavel == "MRV" ||
                                         i.ConstrutoraResponsavel == "RNI" ||
                                         i.ConstrutoraResponsavel == "Flyer");
            }

            // ==========================================
            // APLICAÇÃO DOS FILTROS DA SIDEBAR
            // ==========================================

            // Pesquisa por texto livre de Localização (Abrangência Nacional)
            if (!string.IsNullOrEmpty(Localizacao))
            {
                string busca = Localizacao.ToLower().Trim();
                query = query.Where(i => i.CidadeEstado.ToLower().Contains(busca) ||
                                         i.Bairro.ToLower().Contains(busca) ||
                                         i.Endereco.ToLower().Contains(busca));
            }

            // Tipo de Imóvel
            if (!string.IsNullOrEmpty(Tipo))
            {
                if (Enum.TryParse<TipoImovel>(Tipo, true, out var tipoEnum))
                {
                    query = query.Where(i => i.Tipo == tipoEnum);
                }
            }

            // Construtoras Parceiras
            if (Operacao == "construir" && !string.IsNullOrEmpty(Construtora))
            {
                query = query.Where(i => i.ConstrutoraResponsavel != null &&
                                         i.ConstrutoraResponsavel.ToLower() == Construtora.ToLower());
            }

            // Dormitórios (Mínimo selecionado)
            if (Quartos.HasValue && Quartos.Value > 0)
            {
                query = query.Where(i => i.QuantidadeQuartos >= Quartos.Value);
            }

            // Banheiros (Mínimo selecionado)
            if (Banheiros.HasValue && Banheiros.Value > 0)
            {
                query = query.Where(i => i.QuantidadeBanheiros >= Banheiros.Value);
            }

            // Vagas de Garagem (Mínimo selecionado)
            if (Garagem.HasValue && Garagem.Value > 0)
            {
                query = query.Where(i => i.QuantidadeVagasGaragem >= Garagem.Value);
            }

            // Faixas de Preço/Valor
            if (PrecoMin.HasValue) query = query.Where(i => i.Preco >= PrecoMin.Value);
            if (PrecoMax.HasValue) query = query.Where(i => i.Preco <= PrecoMax.Value);

            // ==========================================
            // COMPORTAMENTO DE ORDENAÇÃO DINÂMICA
            // ==========================================
            switch (OrdenarPor)
            {
                case "preco-asc":
                    query = query.OrderBy(i => i.Preco);
                    break;
                case "preco-desc":
                    query = query.OrderByDescending(i => i.Preco);
                    break;
                case "novos":
                default:
                    query = query.OrderByDescending(i => i.DataCadastro);
                    break;
            }

            Imoveis = await query.ToListAsync();
        }
        public async Task<IActionResult> OnGetSugerirLocalizacaoAsync(string termo)
        {
            if (string.IsNullOrEmpty(termo) || termo.Length < 2)
            {
                return new JsonResult(new List<string>());
            }

            string busca = termo.ToLower().Trim();

            // Busca no banco os bairros ou cidades/estados que começam ou contêm o texto digitado
            var sugestoesBairros = await _context.Imoveis
                .Where(i => i.Status == "Disponivel" && i.Bairro.ToLower().Contains(busca))
                .Select(i => i.Bairro)
                .Distinct()
                .Take(5)
                .ToListAsync();

            var sugestoesCidades = await _context.Imoveis
                .Where(i => i.Status == "Disponivel" && i.CidadeEstado.ToLower().Contains(busca))
                .Select(i => i.CidadeEstado)
                .Distinct()
                .Take(5)
                .ToListAsync();

            // Une os resultados, remove duplicados e retorna para o HTML
            var resultadoFinal = sugestoesBairros.Concat(sugestoesCidades).Distinct().ToList();

            return new JsonResult(resultadoFinal);
        }
    }
}