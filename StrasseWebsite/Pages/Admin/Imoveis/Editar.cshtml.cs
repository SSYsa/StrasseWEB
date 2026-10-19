using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StrasseWebsite.Data;
using StrasseWebsite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace StrasseWebsite.Pages.Admin.Imoveis
{
    public class EditarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public EditarModel(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [BindProperty]
        public Imovel Imovel { get; set; } = default!;


        // ==========================================================
        // NOVAS IMAGENS
        // ==========================================================

        [BindProperty]
        public List<IFormFile> NovasImagens { get; set; } = new();


        // ==========================================================
        // IMAGEM PRINCIPAL ESCOLHIDA
        // ==========================================================

        [BindProperty]
        public string? ImagemPrincipalSelecionada { get; set; }


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
            ModelState.Remove("Imovel.UrlImagemPrincipal");


            // ======================================================
            // VALIDAÇÃO DAS NOVAS IMAGENS
            // ======================================================

            const long tamanhoMaximo =
                10 * 1024 * 1024;


            var tiposPermitidos =
                new[]
                {
                    "image/jpeg",
                    "image/png",
                    "image/webp"
                };


            if (NovasImagens != null &&
                NovasImagens.Count > 0)
            {
                foreach (var imagem in NovasImagens)
                {
                    if (imagem == null ||
                        imagem.Length == 0)
                    {
                        ModelState.AddModelError(
                            "NovasImagens",
                            "Uma das imagens selecionadas está vazia.");

                        continue;
                    }


                    if (imagem.Length > tamanhoMaximo)
                    {
                        ModelState.AddModelError(
                            "NovasImagens",
                            $"A imagem '{imagem.FileName}' deve ter no máximo 10 MB.");
                    }


                    if (!tiposPermitidos.Contains(
                        imagem.ContentType,
                        StringComparer.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(
                            "NovasImagens",
                            $"A imagem '{imagem.FileName}' possui formato inválido. Utilize JPG, PNG ou WEBP.");
                    }
                }
            }


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
            // TIPO
            // ======================================================

            imovelBanco.Tipo =
                Imovel.Tipo;


            // ======================================================
            // TRANSAÇÃO
            // ======================================================

            imovelBanco.Transacao =
                Imovel.Transacao;


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
            // UPLOAD DAS NOVAS IMAGENS
            // ======================================================

            var urlsNovasImagens =
                new List<string>();


            if (NovasImagens != null &&
                NovasImagens.Count > 0)
            {
                try
                {
                    var supabaseUrl =
                        _configuration["Supabase:Url"];

                    var supabaseServiceKey =
                        _configuration["Supabase:ServiceKey"];


                    if (string.IsNullOrWhiteSpace(supabaseUrl) ||
                        string.IsNullOrWhiteSpace(supabaseServiceKey))
                    {
                        ModelState.AddModelError(
                            "NovasImagens",
                            "A configuração do Supabase não foi encontrada.");

                        await CarregarListasAsync();

                        return Page();
                    }


                    const string nomeBucket =
                        "StrasseImages";


                    using var httpClient =
                        new HttpClient();


                    foreach (var imagem in NovasImagens)
                    {
                        var extensao =
                            Path.GetExtension(
                                imagem.FileName)
                            .ToLowerInvariant();


                        var nomeArquivo =
                            $"{Guid.NewGuid():N}{extensao}";


                        var caminhoStorage =
                            $"{imovelBanco.CodigoImovel}/{nomeArquivo}";


                        var uploadUrl =
                            $"{supabaseUrl.TrimEnd('/')}" +
                            $"/storage/v1/object/{nomeBucket}/{caminhoStorage}";


                        using var stream =
                            imagem.OpenReadStream();


                        using var content =
                            new StreamContent(stream);


                        content.Headers.ContentType =
                            new MediaTypeHeaderValue(
                                imagem.ContentType);


                        using var request =
                            new HttpRequestMessage(
                                HttpMethod.Post,
                                uploadUrl);


                        request.Headers.Add(
                            "Authorization",
                            $"Bearer {supabaseServiceKey}");


                        request.Headers.Add(
                            "apikey",
                            supabaseServiceKey);


                        request.Headers.Add(
                            "x-upsert",
                            "false");


                        request.Content =
                            content;


                        var response =
                            await httpClient.SendAsync(
                                request);


                        if (!response.IsSuccessStatusCode)
                        {
                            var erro =
                                await response.Content.ReadAsStringAsync();


                            ModelState.AddModelError(
                                "NovasImagens",
                                $"Não foi possível enviar a imagem '{imagem.FileName}' para o Storage. {erro}");

                            await CarregarListasAsync();

                            return Page();
                        }


                        var urlPublica =
                            $"{supabaseUrl.TrimEnd('/')}" +
                            $"/storage/v1/object/public/{nomeBucket}/{caminhoStorage}";


                        urlsNovasImagens.Add(
                            urlPublica);
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError(
                        "NovasImagens",
                        "Ocorreu um erro ao enviar as imagens para o Storage.");

                    await CarregarListasAsync();

                    return Page();
                }
            }


            // ======================================================
            // DEFINIR IMAGEM PRINCIPAL
            // ======================================================

            if (!string.IsNullOrWhiteSpace(
                ImagemPrincipalSelecionada))
            {
                // --------------------------------------------------
                // UMA IMAGEM NOVA FOI ESCOLHIDA COMO PRINCIPAL
                // --------------------------------------------------

                if (ImagemPrincipalSelecionada.StartsWith(
                    "__NOVA__"))
                {
                    var textoIndice =
                        ImagemPrincipalSelecionada
                            .Replace(
                                "__NOVA__",
                                "");

                    if (int.TryParse(
                        textoIndice,
                        out int indice) &&
                        indice >= 0 &&
                        indice < urlsNovasImagens.Count)
                    {
                        var novaPrincipal =
                            urlsNovasImagens[indice];


                        // A antiga principal vira imagem secundária
                        if (!string.IsNullOrWhiteSpace(
                            imovelBanco.UrlImagemPrincipal) &&
                            imovelBanco.UrlImagemPrincipal !=
                            novaPrincipal)
                        {
                            imovelBanco
                                .ImagensSecundarias
                                .Add(
                                    new ImagemImovel
                                    {
                                        UrlImagem =
                                            imovelBanco.UrlImagemPrincipal
                                    });
                        }


                        imovelBanco.UrlImagemPrincipal =
                            novaPrincipal;
                    }
                }
                else
                {
                    // --------------------------------------------------
                    // UMA IMAGEM EXISTENTE FOI ESCOLHIDA COMO PRINCIPAL
                    // --------------------------------------------------

                    var novaPrincipal =
                        ImagemPrincipalSelecionada;


                    // A antiga principal vai para a galeria
                    if (!string.IsNullOrWhiteSpace(
                        imovelBanco.UrlImagemPrincipal) &&
                        imovelBanco.UrlImagemPrincipal !=
                        novaPrincipal)
                    {
                        var antigaPrincipalJaNaGaleria =
                            imovelBanco
                                .ImagensSecundarias
                                .Any(i =>
                                    i.UrlImagem ==
                                    imovelBanco.UrlImagemPrincipal);


                        if (!antigaPrincipalJaNaGaleria)
                        {
                            imovelBanco
                                .ImagensSecundarias
                                .Add(
                                    new ImagemImovel
                                    {
                                        UrlImagem =
                                            imovelBanco.UrlImagemPrincipal
                                    });
                        }
                    }


                    // Remove a nova principal da galeria
                    var imagemDaGaleria =
                        imovelBanco
                            .ImagensSecundarias
                            .FirstOrDefault(i =>
                                i.UrlImagem ==
                                novaPrincipal);


                    if (imagemDaGaleria != null)
                    {
                        imovelBanco
                            .ImagensSecundarias
                            .Remove(
                                imagemDaGaleria);
                    }


                    imovelBanco.UrlImagemPrincipal =
                        novaPrincipal;
                }
            }


            // ======================================================
            // NOVAS IMAGENS QUE NÃO SÃO A PRINCIPAL
            // ======================================================

            for (
                int i = 0;
                i < urlsNovasImagens.Count;
                i++)
            {
                var url =
                    urlsNovasImagens[i];


                var foiEscolhidaComoPrincipal =
                    ImagemPrincipalSelecionada ==
                    "__NOVA__" + i;


                if (!foiEscolhidaComoPrincipal)
                {
                    imovelBanco
                        .ImagensSecundarias
                        .Add(
                            new ImagemImovel
                            {
                                UrlImagem =
                                    url
                            });
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
        // EXCLUIR IMAGEM DA GALERIA
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