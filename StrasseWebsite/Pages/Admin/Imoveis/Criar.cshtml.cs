using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
    public class CriarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CriarModel(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [BindProperty]
        public Imovel Imovel { get; set; } = new();

        [BindProperty]
        public string? NovoProprietario { get; set; }

        // ==========================================================
        // IMAGENS
        // ==========================================================

        [BindProperty]
        public List<IFormFile> Imagens { get; set; } = new();

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

            // A URL será gerada automaticamente.
            ModelState.Remove("Imovel.UrlImagemPrincipal");

            if (!string.IsNullOrWhiteSpace(NovoProprietario))
            {
                ModelState.Remove("Imovel.ProprietarioId");
            }

            // ==========================================================
            // VALIDAÇÃO DAS IMAGENS
            // ==========================================================

            if (Imagens == null || Imagens.Count == 0)
            {
                ModelState.AddModelError(
                    "Imagens",
                    "Selecione pelo menos uma imagem para o imóvel.");
            }
            else
            {
                const long tamanhoMaximo = 10 * 1024 * 1024;

                var tiposPermitidos = new[]
                {
                    "image/jpeg",
                    "image/png",
                    "image/webp"
                };

                foreach (var imagem in Imagens)
                {
                    if (imagem == null || imagem.Length == 0)
                    {
                        ModelState.AddModelError(
                            "Imagens",
                            "Uma das imagens selecionadas está vazia.");

                        continue;
                    }

                    if (imagem.Length > tamanhoMaximo)
                    {
                        ModelState.AddModelError(
                            "Imagens",
                            $"A imagem '{imagem.FileName}' deve ter no máximo 10 MB.");
                    }

                    if (!tiposPermitidos.Contains(
                        imagem.ContentType,
                        StringComparer.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError(
                            "Imagens",
                            $"A imagem '{imagem.FileName}' possui formato inválido. Utilize JPG, PNG ou WEBP.");
                    }
                }
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
            // UPLOAD DAS IMAGENS
            // ==========================================================

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
                        "Imagens",
                        "A configuração do Supabase não foi encontrada.");

                    await CarregarListasAsync();

                    return Page();
                }

                const string nomeBucket = "StrasseImages";

                using var httpClient = new HttpClient();

                var urlsImagens = new List<string>();

                foreach (var imagem in Imagens)
                {
                    var extensao =
                        Path.GetExtension(
                            imagem.FileName)
                        .ToLowerInvariant();

                    var nomeArquivo =
                        $"{Guid.NewGuid():N}{extensao}";

                    var caminhoStorage =
                        $"{Imovel.CodigoImovel}/{nomeArquivo}";

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

                    request.Content = content;

                    var response =
                        await httpClient.SendAsync(request);

                    if (!response.IsSuccessStatusCode)
                    {
                        var erro =
                            await response.Content.ReadAsStringAsync();

                        ModelState.AddModelError(
                            "Imagens",
                            $"Não foi possível enviar a imagem '{imagem.FileName}' para o Storage. {erro}");

                        await CarregarListasAsync();

                        return Page();
                    }

                    var urlPublica =
                        $"{supabaseUrl.TrimEnd('/')}" +
                        $"/storage/v1/object/public/{nomeBucket}/{caminhoStorage}";

                    urlsImagens.Add(urlPublica);
                }


                // ======================================================
                // PRIMEIRA IMAGEM = PRINCIPAL
                // ======================================================

                Imovel.UrlImagemPrincipal =
                    urlsImagens.First();


                // ======================================================
                // DEMAIS IMAGENS = GALERIA
                // ======================================================

                for (int i = 1; i < urlsImagens.Count; i++)
                {
                    Imovel.ImagensSecundarias.Add(
                        new ImagemImovel
                        {
                            UrlImagem = urlsImagens[i]
                        });
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "Imagens",
                    "Ocorreu um erro ao enviar as imagens para o Storage.");

                await CarregarListasAsync();

                return Page();
            }


            // ==========================================================
            // SALVA O IMÓVEL + IMAGENS SECUNDÁRIAS
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