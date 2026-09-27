using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MimeKit;

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

            try
            {
                // ==========================================
                // CONFIGURAÇÕES
                // ==========================================

                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(
                    _configuration["EmailSettings:SmtpPort"] ?? "587"
                );

                var senderEmail =
                    _configuration["EmailSettings:SenderEmail"];

                var senderName =
                    _configuration["EmailSettings:SenderName"];

                var username =
                    _configuration["EmailSettings:Username"];

                var password =
                    _configuration["EmailSettings:Password"];


                // ==========================================
                // VALIDAÇÃO BÁSICA
                // ==========================================

                if (string.IsNullOrWhiteSpace(email))
                {
                    ModelState.AddModelError(
                        "",
                        "O e-mail informado é obrigatório."
                    );

                    return Page();
                }

                if (string.IsNullOrWhiteSpace(smtpServer) ||
                    string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(password))
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


                // ==========================================
                // CRIA A MENSAGEM
                // ==========================================

                var message = new MimeMessage();

                message.From.Add(
                    new MailboxAddress(
                        senderName,
                        senderEmail
                    )
                );

                // DESTINATÁRIOS
                message.To.Add(
                    MailboxAddress.Parse(
                        "strasse.imoveis@gmail.com"
                    )
                );

                message.Cc.Add(
                    MailboxAddress.Parse(
                        "in.ysabelle@gmail.com"
                    )
                );

                message.ReplyTo.Add(
                    MailboxAddress.Parse(email)
                );

                // ==========================================
                // FORMULÁRIO DE CARREIRA
                // ==========================================

                if (tipo == "carreira")
                {
                    message.Subject =
                        $"Novo Currículo - {nome}";

                    var body = new BodyBuilder();

                    body.HtmlBody = $@"
                        <h2>Novo currículo recebido</h2>

                        <p><strong>Nome:</strong> {nome}</p>

                        <p><strong>Telefone:</strong> {telefone}</p>

                        <p><strong>E-mail:</strong> {email}</p>

                        <p><strong>Pretensão salarial:</strong> {pretensao}</p>

                        <hr>

                        <p>
                            Currículo enviado através do site
                            <strong>Strasse Imóveis</strong>.
                        </p>
                    ";

                    // ==========================================
                    // ANEXA O CURRÍCULO
                    // ==========================================

                    if (curriculo != null && curriculo.Length > 0)
                    {
                        using var memoryStream = new MemoryStream();

                        await curriculo.CopyToAsync(memoryStream);

                        memoryStream.Position = 0;

                        body.Attachments.Add(
                            curriculo.FileName,
                            memoryStream.ToArray(),
                            ContentType.Parse(
                                curriculo.ContentType
                            )
                        );
                    }

                    message.Body = body.ToMessageBody();
                }


                // ==========================================
                // FORMULÁRIO DE PARCERIA
                // ==========================================

                else
                {
                    message.Subject =
                        $"Nova Solicitação de Parceria - {nomeParceiro}";

                    var body = new BodyBuilder();

                    body.HtmlBody = $@"
                        <h2>Nova solicitação de parceria</h2>

                        <p>
                            <strong>Nome / Razão Social:</strong>
                            {nomeParceiro}
                        </p>

                        <p>
                            <strong>Telefone:</strong>
                            {telefone}
                        </p>

                        <p>
                            <strong>E-mail:</strong>
                            {email}
                        </p>

                        <p>
                            <strong>Tipo de atuação:</strong>
                            {atuacao}
                        </p>

                        <hr>

                        <h3>Mensagem / Proposta</h3>

                        <p>
                            {mensagem}
                        </p>

                        <hr>

                        <p>
                            Solicitação enviada através do site
                            <strong>Strasse Imóveis</strong>.
                        </p>
                    ";

                    message.Body = body.ToMessageBody();
                }


                // ==========================================
                // ENVIO SMTP
                // ==========================================

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


                // ==========================================
                // LOG DE SUCESSO
                // ==========================================

                _logger.LogInformation(
                    "E-mail enviado com sucesso. Tipo: {Tipo}, Remetente: {Email}",
                    tipo,
                    email
                );


                // ==========================================
                // RETORNO
                // ==========================================

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
                // IMPORTANTE:
                // registra o erro real no log
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