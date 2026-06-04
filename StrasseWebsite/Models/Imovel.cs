using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // Certifique-se de ter esse using
using System.ComponentModel.DataAnnotations;

namespace StrasseWebsite.Models;

public enum TipoTransacao
{
    Compra,
    Construcao
}

public enum TipoImovel
{
    Apartamento,
    Casa,
    Terreno
}

public enum FormaPagamento
{
    Pix,
    Dinheiro,
    Cartao,
    Financiamento,
    Consorcio,
    Permuta
}

public class Imovel
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string CodigoImovel { get; set; } = string.Empty; // Ex: STR-0001

    [Required]
    [StringLength(100)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    public string Descricao { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Preco { get; set; }

    // Endereço e Localização
    [Required]
    public string Endereco { get; set; } = string.Empty;

    [Required]
    public string CidadeEstado { get; set; } = string.Empty; // Ex: Blumenau/SC

    [Required]
    public string Bairro { get; set; } = string.Empty;

    // Medidas
    [Column(TypeName = "decimal(10,2)")]
    public decimal TamanhoTerreno { get; set; } // Área total

    [Column(TypeName = "decimal(10,2)")]
    public decimal TamanhoConstruido { get; set; }

    // Filtros e Características
    public int QuantidadeQuartos { get; set; }
    public int QuantidadeBanheiros { get; set; }
    public int QuantidadeSalas { get; set; }

    // Características Booleanas
    public bool TemPiscina { get; set; }
    public bool TemAcademia { get; set; }
    public bool TemGaragem { get; set; }
    public int QuantidadeVagasGaragem { get; set; }

    // Dados da Obra
    public string? ConstrutoraResponsavel { get; set; }

    // Imagens (Usa string externa/redirecionamento)
    [Required]
    public string UrlImagemPrincipal { get; set; } = string.Empty;
    public List<ImagemImovel> ImagensSecundarias { get; set; } = new();

    [Required]
    public string Status { get; set; } = "Disponivel";

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    // Enums locais
    public TipoTransacao Transacao { get; set; }
    public TipoImovel Tipo { get; set; }
    public FormaPagamento PagamentoDisponivel { get; set; }

    // ==========================================
    // RELACIONAMENTOS (Vínculo com a tabela Pessoa)
    // ==========================================

    [Required]
    public int ProprietarioId { get; set; }
    [ForeignKey("ProprietarioId")]
    public Pessoa Proprietario { get; set; } = null!;

    [Required]
    public int CorretorId { get; set; }
    [ForeignKey("CorretorId")]
    public Pessoa Corretor { get; set; } = null!;
}

[Table("ImagensImoveis")] // Força o EF Core a buscar na tabela correta do Postgres
public class ImagemImovel
{
    public int Id { get; set; }

    public int ImovelId { get; set; }

    [Column("Url")] // Força o EF Core a ler a coluna física 'Url' do banco
    public string UrlImagem { get; set; } = string.Empty;

    // Relacionamento do EF Core
    public Imovel Imovel { get; set; } = null!;
}