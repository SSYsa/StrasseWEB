using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StrasseWebsite.Data;
using StrasseWebsite.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }


    public List<Imovel> ImoveisHome { get; set; }
        = new List<Imovel>();


    public List<string> LocalizacoesDisponiveis { get; set; }
        = new List<string>();


    public async Task OnGetAsync()
    {
        // ==========================================
        // LOCALIZAÇÕES DISPONÍVEIS
        // Busca diretamente do banco.
        // ==========================================

        LocalizacoesDisponiveis =
            await _context.Imoveis
                .Where(i =>
                    i.Status == "Disponivel" &&
                    i.CidadeEstado != null &&
                    i.CidadeEstado != "")
                .Select(i => i.CidadeEstado)
                .Distinct()
                .OrderBy(i => i)
                .ToListAsync();


        // ==========================================
        // IMÓVEIS DISPONÍVEIS
        // ==========================================

        var todosImoveis =
            await _context.Imoveis
                .Where(i =>
                    i.Status == "Disponivel")
                .OrderByDescending(
                    i => i.DataCadastro)
                .ToListAsync();


        // ==========================================
        // IMÓVEIS USADOS / PRONTOS
        // ==========================================

        var usados =
            todosImoveis
                .Where(i =>
                    i.Transacao ==
                    TipoTransacao.Compra)
                .Take(2)
                .ToList();


        // ==========================================
        // IMÓVEIS NOVOS / LANÇAMENTOS
        // ==========================================

        var novos =
            todosImoveis
                .Where(i =>
                    i.Transacao ==
                    TipoTransacao.Construcao)
                .Take(2)
                .ToList();


        // ==========================================
        // IMÓVEIS PARA ALUGAR
        // ==========================================

        var alugueis =
            todosImoveis
                .Where(i =>
                    i.Transacao ==
                    TipoTransacao.Aluguel)
                .Take(2)
                .ToList();


        // ==========================================
        // JUNTA OS DESTAQUES
        // ==========================================

        ImoveisHome.AddRange(usados);

        ImoveisHome.AddRange(novos);

        ImoveisHome.AddRange(alugueis);


        // ==========================================
        // FALLBACK
        // ==========================================

        if (ImoveisHome.Count < 6)
        {
            var idsJaInclusos =
                ImoveisHome
                    .Select(i => i.Id)
                    .ToList();


            var restantes =
                todosImoveis
                    .Where(i =>
                        !idsJaInclusos.Contains(
                            i.Id))
                    .Take(
                        6 - ImoveisHome.Count)
                    .ToList();


            ImoveisHome.AddRange(restantes);
        }
    }


    // ==========================================
    // FORMATA CIDADE / ESTADO
    // ==========================================

    public string FormatarCidadeEstado(
        string cidadeEstado)
    {
        if (string.IsNullOrWhiteSpace(
            cidadeEstado))
        {
            return cidadeEstado;
        }


        return cidadeEstado
            .Trim()
            .Replace("/", " - ")
            .Replace(",", " - ");
    }
}