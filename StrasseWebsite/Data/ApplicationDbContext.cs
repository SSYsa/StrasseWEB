using Microsoft.EntityFrameworkCore;
using StrasseWebsite.Models;

namespace StrasseWebsite.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Mapeamento das tabelas no banco de dados
    public DbSet<Imovel> Imoveis { get; set; }
    public DbSet<Pessoa> Pessoas { get; set; }
    public DbSet<ImagemImovel> ImagensImoveis { get; set; } // Adicionado para gerenciar as fotos da galeria
    public DbSet<UsuarioAcesso> Acessos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Precisão para o Preço do Imóvel
        modelBuilder.Entity<Imovel>()
            .Property(i => i.Preco)
            .HasColumnType("decimal(18,2)");

        // 2. Precisão para as metragens de tamanho (Terreno e Construído)
        modelBuilder.Entity<Imovel>()
            .Property(i => i.TamanhoTerreno)
            .HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Imovel>()
            .Property(i => i.TamanhoConstruido)
            .HasColumnType("decimal(10,2)");

        // 3. Configuração do relacionamento: Imóvel -> Proprietário (Pessoa)
        modelBuilder.Entity<Imovel>()
            .HasOne(i => i.Proprietario)
            .WithMany(p => p.ImoveisComoProprietario)
            .HasForeignKey(i => i.ProprietarioId)
            .OnDelete(DeleteBehavior.Restrict); // Evita exclusão acidentais em cascata dupla

        // 4. Configuração do relacionamento: Imóvel -> Corretor (Pessoa)
        modelBuilder.Entity<Imovel>()
            .HasOne(i => i.Corretor)
            .WithMany(p => p.ImoveisComoCorretor)
            .HasForeignKey(i => i.CorretorId)
            .OnDelete(DeleteBehavior.Restrict); // Evita exclusão acidentais em cascata dupla
    }
}