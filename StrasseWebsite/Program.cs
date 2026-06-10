using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StrasseWebsite.Data;

var builder = WebApplication.CreateBuilder(args);

// BUSCA E VALIDA A LINHA DE CONEXÃO
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("ERRO: A chave 'DefaultConnection' não foi encontrada dentro 'ConnectionStrings' no seu appsettings.json!");
}

// Registrar o ApplicationDbContext configurado para o PostgreSQL do Supabase
builder.Services.AddDbContext<StrasseWebsite.Data.ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services to the container.
// Modifique para incluir a convenção de autorização na pasta Admin
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
}); 
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
    options.LoginPath = "/Account/Login";
});

// =================================================================
// 🚀 A PARTIR DAQUI ESTAVA FALTANDO NO SEU PROJETO:
// =================================================================

var app = builder.Build();

// Configura o pipeline de requisições HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // O HSTS só roda em produção
    app.UseHsts();

    // ATIVA O HTTPS APENAS EM PRODUÇÃO (No Render)
    // Isso protege o seu login sem quebrar o seu localhost de desenvolvimento!
    app.UseHttpsRedirection();
}

app.UseStaticFiles(); // Permite carregar o CSS, imagens e o site.js
app.UseRouting();
app.UseAuthentication(); // Habilita o sistema de autenticação (cookies)
app.UseAuthorization();

app.MapRazorPages(); // DIZ AO .NET PARA MAPEAR A PASTA PAGES (Index, SobreNos, etc)

app.Run(); // LIGA O SERVIDOR DE VERDADE!