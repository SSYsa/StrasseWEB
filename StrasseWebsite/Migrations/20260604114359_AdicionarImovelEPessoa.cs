using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StrasseWebsite.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarImovelEPessoa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UrlImagem",
                table: "Imoveis",
                newName: "UrlImagemPrincipal");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Pessoas",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Documento",
                table: "Pessoas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Pessoas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Pessoas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Perfil",
                table: "Pessoas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SenhaHash",
                table: "Pessoas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "Pessoas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Imoveis",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ConstrutoraResponsavel",
                table: "Imoveis",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Bairro",
                table: "Imoveis",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CidadeEstado",
                table: "Imoveis",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CodigoImovel",
                table: "Imoveis",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CorretorId",
                table: "Imoveis",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Imoveis",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ProprietarioId",
                table: "Imoveis",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TamanhoConstruido",
                table: "Imoveis",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TamanhoTerreno",
                table: "Imoveis",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ImagensImoveis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Url = table.Column<string>(type: "text", nullable: false),
                    ImovelId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagensImoveis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImagensImoveis_Imoveis_ImovelId",
                        column: x => x.ImovelId,
                        principalTable: "Imoveis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Imoveis_CorretorId",
                table: "Imoveis",
                column: "CorretorId");

            migrationBuilder.CreateIndex(
                name: "IX_Imoveis_ProprietarioId",
                table: "Imoveis",
                column: "ProprietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ImagensImoveis_ImovelId",
                table: "ImagensImoveis",
                column: "ImovelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Imoveis_Pessoas_CorretorId",
                table: "Imoveis",
                column: "CorretorId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Imoveis_Pessoas_ProprietarioId",
                table: "Imoveis",
                column: "ProprietarioId",
                principalTable: "Pessoas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Imoveis_Pessoas_CorretorId",
                table: "Imoveis");

            migrationBuilder.DropForeignKey(
                name: "FK_Imoveis_Pessoas_ProprietarioId",
                table: "Imoveis");

            migrationBuilder.DropTable(
                name: "ImagensImoveis");

            migrationBuilder.DropIndex(
                name: "IX_Imoveis_CorretorId",
                table: "Imoveis");

            migrationBuilder.DropIndex(
                name: "IX_Imoveis_ProprietarioId",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Pessoas");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Pessoas");

            migrationBuilder.DropColumn(
                name: "Perfil",
                table: "Pessoas");

            migrationBuilder.DropColumn(
                name: "SenhaHash",
                table: "Pessoas");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "Pessoas");

            migrationBuilder.DropColumn(
                name: "Bairro",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "CidadeEstado",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "CodigoImovel",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "CorretorId",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "ProprietarioId",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "TamanhoConstruido",
                table: "Imoveis");

            migrationBuilder.DropColumn(
                name: "TamanhoTerreno",
                table: "Imoveis");

            migrationBuilder.RenameColumn(
                name: "UrlImagemPrincipal",
                table: "Imoveis",
                newName: "UrlImagem");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Pessoas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Documento",
                table: "Pessoas",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "Imoveis",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ConstrutoraResponsavel",
                table: "Imoveis",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
