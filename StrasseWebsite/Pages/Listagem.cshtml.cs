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

        // Propriedade que armazena o tipo de operação atual
        public string Operacao { get; set; } = "tudo";

        // Lista dinâmica preenchida pelo banco de dados
        public List<Imovel> Imoveis { get; set; } = new();

        public async Task OnGetAsync(string operacao)
        {
            if (!string.IsNullOrEmpty(operacao))
            {
                Operacao = operacao.ToLower();
            }

            // Agora o Include vai funcionar perfeitamente porque mapeamos a tabela e coluna reais
            var query = _context.Imoveis
                .Include(i => i.ImagensSecundarias)
                .Where(i => i.Status == "Disponivel");

            if (Operacao == "comprar" || Operacao == "lancamentos")
            {
                query = query.Where(i => i.Transacao == TipoTransacao.Compra);
            }
            else if (Operacao == "construir")
            {
                query = query.Where(i => i.Transacao == TipoTransacao.Construcao);
            }

            Imoveis = await query.OrderByDescending(i => i.DataCadastro).ToListAsync();
        }
    }
}