using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StrasseWebsite.Data;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONNECTION STRING
// A connection string deve vir da configuração/Secret,
// nunca ficar gravada diretamente neste arquivo.
// ============================================================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "A connection string 'DefaultConnection' não foi encontrada."
    );
}
Console.WriteLine("Connection String carregada.");

// ============================================================
// BANCO DE DADOS
// PostgreSQL / Supabase
// ============================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ============================================================
// RAZOR PAGES
// ============================================================

builder.Services.AddRazorPages(options =>
{
    // Toda a pasta /Admin exige autenticação
    options.Conventions.AuthorizeFolder("/Admin");
});

// ============================================================
// AUTENTICAÇÃO
// ============================================================

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        // O cookie será utilizado somente via HTTPS
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        // Proteção contra envio do cookie em requisições cross-site
        options.Cookie.SameSite = SameSiteMode.Lax;

        // Tempo de validade da sessão
        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        // Renova a sessão enquanto o usuário permanecer ativo
        options.SlidingExpiration = true;
    });

// ============================================================
// AUTORIZAÇÃO
// ============================================================

builder.Services.AddAuthorization();

// ============================================================
// CONSTRÓI A APLICAÇÃO
// ============================================================

var app = builder.Build();

// ============================================================
// PIPELINE DE PRODUÇÃO
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");

    // HSTS informa ao navegador que o site deve ser acessado
    // exclusivamente através de HTTPS.
    app.UseHsts();

    // Redireciona HTTP para HTTPS.
    app.UseHttpsRedirection();
}

// ============================================================
// ARQUIVOS ESTÁTICOS
// ============================================================

app.UseStaticFiles();

// ============================================================
// ROTEAMENTO
// ============================================================

app.UseRouting();

// ============================================================
// AUTENTICAÇÃO E AUTORIZAÇÃO
// ============================================================

app.UseAuthentication();
app.UseAuthorization();

// ============================================================
// RAZOR PAGES
// ============================================================

app.MapRazorPages();

// ============================================================
// INICIA A APLICAÇÃO
// ============================================================

app.Run();