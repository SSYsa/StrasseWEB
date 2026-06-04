using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StrasseWebsite.Models;

public enum TipoPessoa { Fisica, Juridica }

public enum TipoPerfil
{
    Cliente,
    Proprietario,
    Corretor,
    Construtora,
    ArquitetaDesigner,
    EngenheiraCivil,
    Admin
}

public class Pessoa
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public TipoPessoa Tipo { get; set; }

    [Required]
    [StringLength(50)]
    public string Documento { get; set; } = string.Empty;

    [Required]
    public DateTime DataNascimento { get; set; }

    [Required]
    [StringLength(20)]
    public string Telefone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public TipoPerfil Perfil { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    // Relacionamento de Navegação
    public UsuarioAcesso? Acesso { get; set; }
    public List<Imovel> ImoveisComoProprietario { get; set; } = new();
    public List<Imovel> ImoveisComoCorretor { get; set; } = new();
}

// NOVA TABELA: Acessos / Login
public class UsuarioAcesso
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Usuario { get; set; } = string.Empty; // Nome de usuário para login

    [Required]
    [StringLength(255)]
    public string Senha { get; set; } = string.Empty; // Senha (criptografada)

    // Relacionamento 1 para 1 com Pessoa
    [Required]
    public int PessoaId { get; set; }

    [ForeignKey("PessoaId")]
    public Pessoa Pessoa { get; set; } = null!;
}