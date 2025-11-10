using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Servidor_PI.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Transparencias",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Ouvidorias",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Eventos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuariosId",
                table: "Doacao",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Atividades",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Transparencias_UsuarioId",
                table: "Transparencias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Ouvidorias_UsuarioId",
                table: "Ouvidorias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Eventos_UsuarioId",
                table: "Eventos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Doacao_UsuariosId",
                table: "Doacao",
                column: "UsuariosId");

            migrationBuilder.CreateIndex(
                name: "IX_Atividades_UsuarioId",
                table: "Atividades",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Atividades_Usuarios_UsuarioId",
                table: "Atividades",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Doacao_Usuarios_UsuariosId",
                table: "Doacao",
                column: "UsuariosId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Eventos_Usuarios_UsuarioId",
                table: "Eventos",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ouvidorias_Usuarios_UsuarioId",
                table: "Ouvidorias",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transparencias_Usuarios_UsuarioId",
                table: "Transparencias",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Atividades_Usuarios_UsuarioId",
                table: "Atividades");

            migrationBuilder.DropForeignKey(
                name: "FK_Doacao_Usuarios_UsuariosId",
                table: "Doacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Eventos_Usuarios_UsuarioId",
                table: "Eventos");

            migrationBuilder.DropForeignKey(
                name: "FK_Ouvidorias_Usuarios_UsuarioId",
                table: "Ouvidorias");

            migrationBuilder.DropForeignKey(
                name: "FK_Transparencias_Usuarios_UsuarioId",
                table: "Transparencias");

            migrationBuilder.DropIndex(
                name: "IX_Transparencias_UsuarioId",
                table: "Transparencias");

            migrationBuilder.DropIndex(
                name: "IX_Ouvidorias_UsuarioId",
                table: "Ouvidorias");

            migrationBuilder.DropIndex(
                name: "IX_Eventos_UsuarioId",
                table: "Eventos");

            migrationBuilder.DropIndex(
                name: "IX_Doacao_UsuariosId",
                table: "Doacao");

            migrationBuilder.DropIndex(
                name: "IX_Atividades_UsuarioId",
                table: "Atividades");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Transparencias");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Ouvidorias");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Eventos");

            migrationBuilder.DropColumn(
                name: "UsuariosId",
                table: "Doacao");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Atividades");
        }
    }
}
