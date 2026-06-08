using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // Caso use Entity Framework
using StrasseWebsite.Models;
using StrasseWebsite.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context; // Substitua pelo seu contexto real do banco

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    // Lista unificada que será lida pelo HTML
    public List<Imovel> ImoveisHome { get; set; } = new List<Imovel>();

    public async Task OnGetAsync()
    {
        // 1. Busca todos os imóveis ativos do banco (para fazer a paginação em memória com segurança)
        var todosImoveis = await _context.Imoveis
            .Where(i => i.Status.Equals("Disponivel")) // Exemplo de filtro de segurança
            .ToListAsync();

        // 2. Separa as duas categorias (Ajuste as propriedades 'Status' ou 'Tipo' conforme seu banco)
        // Exemplo: "Usado" para prontos/usados e "Construcao" ou "Lancamento" para os novos
        var usados = todosImoveis.Where(i => i.Transacao.ToString() == "Compra").ToList();
        var emConstrucao = todosImoveis.Where(i => i.Transacao.ToString() == "Construcao" || i.Transacao.ToString() == "Lancamento").ToList();

        // 3. Pega a primeira tentativa (3 de cada)
        var primeirosUsados = usados.Take(3).ToList();
        var primeirosEmConstrucao = emConstrucao.Take(3).ToList();

        // 4. Junta os resultados iniciais
        ImoveisHome.AddRange(primeirosUsados);
        ImoveisHome.AddRange(primeirosEmConstrucao);

        // 5. REGRA DE FALLBACK: Se não completou 6 itens, busca os que sobraram indiferente do filtro
        if (ImoveisHome.Count < 6)
        {
            // Descobre quais IDs já estão na lista para não duplicar na tela
            var idsJaInclusos = ImoveisHome.Select(i => i.Id).ToList();

            var restantes = todosImoveis
                .Where(i => !idsJaInclusos.Contains(i.Id))
                .Take(6 - ImoveisHome.Count)
                .ToList();

            ImoveisHome.AddRange(restantes);
        }
    }
}