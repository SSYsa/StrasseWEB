using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;
using MimeKit.Utils;

namespace StrasseWebsite.Pages
{
    public class ContatoModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ContatoModel> _logger;

        public ContatoModel(
            IConfiguration configuration,
            ILogger<ContatoModel> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(
            string? nome,
            string? nomeParceiro,
            string? telefone,
            string? email,
            string? pretensao,
            string? atuacao,
            string? mensagem,
            IFormFile? curriculo)
        {
            // Descobre se é carreira ou parceria através da URL
            string tipo = Request.Query["tipo"].ToString().ToLower();

            if (tipo != "carreira" && tipo != "parceria")
            {
                tipo = "parceria";
            }

            try
            {
                // =====================================================
                // CONFIGURAÇÕES SMTP
                // As credenciais NÃO ficam neste arquivo.
                // =====================================================

                var smtpServer =
                    _configuration["EmailSettings:SmtpServer"];

                var smtpPortValue =
                    _configuration["EmailSettings:SmtpPort"];

                var senderEmail =
                    _configuration["EmailSettings:SenderEmail"];

                var senderName =
                    _configuration["EmailSettings:SenderName"];

                var username =
                    _configuration["EmailSettings:Username"];

                var password =
                    _configuration["EmailSettings:Password"];


                if (!int.TryParse(smtpPortValue, out int smtpPort))
                {
                    smtpPort = 587;
                }


                // =====================================================
                // VALIDAÇÃO
                // =====================================================

                if (string.IsNullOrWhiteSpace(email))
                {
                    ModelState.AddModelError(
                        "",
                        "O e-mail informado é obrigatório."
                    );

                    return Page();
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    try
                    {
                        _ = new MailboxAddress(
                            "",
                            email
                        );
                    }
                    catch
                    {
                        ModelState.AddModelError(
                            "",
                            "Informe um endereço de e-mail válido."
                        );

                        return Page();
                    }
                }

                if (string.IsNullOrWhiteSpace(smtpServer) ||
                    string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(password) ||
                    string.IsNullOrWhiteSpace(senderEmail))
                {
                    _logger.LogError(
                        "Configuração SMTP incompleta."
                    );

                    ModelState.AddModelError(
                        "",
                        "O serviço de e-mail não está configurado corretamente."
                    );

                    return Page();
                }


                // =====================================================
                // CRIA A MENSAGEM
                // =====================================================

                var message = new MimeMessage();

                message.From.Add(
                    new MailboxAddress(
                        senderName ?? "Strasse Imóveis",
                        senderEmail
                    )
                );

                // DESTINATÁRIO PRINCIPAL
                message.To.Add(
                    MailboxAddress.Parse(
                        "strasse.imoveis@gmail.com"
                    )
                );

                // CÓPIA
                message.Cc.Add(
                    MailboxAddress.Parse(
                        "in.ysabelle@gmail.com"
                    )
                );

                // Ao clicar em "Responder", a resposta vai para
                // a pessoa que preencheu o formulário.
                message.ReplyTo.Add(
                    MailboxAddress.Parse(email)
                );


                // =====================================================
                // FORMULÁRIO DE CARREIRA
                // =====================================================

                if (tipo == "carreira")
                {
                    message.Subject =
                        $"Novo Currículo - {nome ?? "Candidato"}";

                    var body = new BodyBuilder();

                    body.HtmlBody = $@"
                        <h2>Novo currículo recebido</h2>

                        <p>
                            <strong>Nome:</strong>
                            {System.Net.WebUtility.HtmlEncode(nome)}
                        </p>

                        <p>
                            <strong>Telefone:</strong>
                            {System.Net.WebUtility.HtmlEncode(telefone)}
                        </p>

                        <p>
                            <strong>E-mail:</strong>
                            {System.Net.WebUtility.HtmlEncode(email)}
                        </p>

                        <p>
                            <strong>Pretensão salarial:</strong>
                            {System.Net.WebUtility.HtmlEncode(pretensao)}
                        </p>

                        <hr>

                        <p>
                            Currículo enviado através do site
                            <strong>Strasse Imóveis</strong>.
                        </p>
                    ";


                    // =================================================
                    // ANEXO DO CURRÍCULO
                    // =================================================

                    if (curriculo != null && curriculo.Length > 0)
                    {
                        // Limite de segurança: 10 MB
                        const long tamanhoMaximo = 10 * 1024 * 1024;

                        if (curriculo.Length > tamanhoMaximo)
                        {
                            ModelState.AddModelError(
                                "",
                                "O currículo não pode ter mais de 10 MB."
                            );

                            return Page();
                        }

                        using var memoryStream = new MemoryStream();

                        await curriculo.CopyToAsync(memoryStream);

                        memoryStream.Position = 0;

                        var contentType =
                            string.IsNullOrWhiteSpace(curriculo.ContentType)
                                ? "application/octet-stream"
                                : curriculo.ContentType;

                        body.Attachments.Add(
                            curriculo.FileName,
                            memoryStream.ToArray(),
                            ContentType.Parse(contentType)
                        );
                    }

                    message.Body = body.ToMessageBody();
                }


                // =====================================================
                // FORMULÁRIO DE PARCERIA
                // =====================================================

                else
                {
                    message.Subject =
                        $"Nova Solicitação de Parceria - {nomeParceiro ?? "Contato"}";

                    var body = new BodyBuilder();

                    body.HtmlBody = $@"
                        <h2>Nova solicitação de parceria</h2>

                        <p>
                            <strong>Nome / Razão Social:</strong>
                            {System.Net.WebUtility.HtmlEncode(nomeParceiro)}
                        </p>

                        <p>
                            <strong>Telefone:</strong>
                            {System.Net.WebUtility.HtmlEncode(telefone)}
                        </p>

                        <p>
                            <strong>E-mail:</strong>
                            {System.Net.WebUtility.HtmlEncode(email)}
                        </p>

                        <p>
                            <strong>Tipo de atuação:</strong>
                            {System.Net.WebUtility.HtmlEncode(atuacao)}
                        </p>

                        <hr>

                        <h3>Mensagem / Proposta</h3>

                        <p>
                            {System.Net.WebUtility.HtmlEncode(mensagem)}
                        </p>

                        <hr>

                        <p>
                            Solicitação enviada através do site
                            <strong>Strasse Imóveis</strong>.
                        </p>
                    ";

                    message.Body = body.ToMessageBody();
                }


                // =====================================================
                // ENVIO SMTP
                // =====================================================

                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(
                    smtpServer,
                    smtpPort,
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    username,
                    password
                );

                await smtp.SendAsync(message);

                await smtp.DisconnectAsync(true);


                // =====================================================
                // LOG
                // =====================================================

                _logger.LogInformation(
                    "E-mail enviado com sucesso. Tipo: {Tipo}, Remetente: {Email}",
                    tipo,
                    email
                );


                // =====================================================
                // RETORNO
                // =====================================================

                TempData["MensagemSucesso"] =
                    tipo == "carreira"
                        ? "Seu currículo foi enviado com sucesso! Entraremos em contato caso seu perfil seja selecionado."
                        : "Sua solicitação de parceria foi enviada com sucesso! Em breve entraremos em contato.";

                return RedirectToPage(
                    "/Contato",
                    new { tipo = tipo }
                );
            }
            catch (Exception ex)
            {
                // O erro técnico fica somente no log do servidor.
                // Não mostramos detalhes ao visitante.

                _logger.LogError(
                    ex,
                    "Erro ao enviar e-mail através do formulário de contato."
                );

                ModelState.AddModelError(
                    "",
                    "Não foi possível enviar sua solicitação neste momento. Tente novamente."
                );

                return Page();
            }
        }
    }
}