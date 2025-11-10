using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Servidor_PI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Atividades",
                columns: table => new
                {
                    atv_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    atv_titulo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    atv_descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    atv_data_publicacao = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETDATE()"),
                    atv_link_imagem = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atividades", x => x.atv_id);
                });

            migrationBuilder.CreateTable(
                name: "Eventos",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    event_titulo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    event_descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    event_data_publicacao = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETDATE()"),
                    event_data_evento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    event_local_evento = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    event_link_imagem = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eventos", x => x.event_id);
                });

            migrationBuilder.CreateTable(
                name: "Ouvidorias",
                columns: table => new
                {
                    ouv_Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ouv_titulo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ouv_descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ouv_data_publicacao = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ouvidorias", x => x.ouv_Id);
                });

            migrationBuilder.CreateTable(
                name: "Transparencias",
                columns: table => new
                {
                    trans_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    trans_titulo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    trans_descricao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    trans_link_download = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    trans_nome_original = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    trans_data_publicacao = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transparencias", x => x.trans_id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    user_nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    user_email = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    user_senha = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    user_data_nasc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    user_endereco = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    user_isAdmin = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Atividades");

            migrationBuilder.DropTable(
                name: "Eventos");

            migrationBuilder.DropTable(
                name: "Ouvidorias");

            migrationBuilder.DropTable(
                name: "Transparencias");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
