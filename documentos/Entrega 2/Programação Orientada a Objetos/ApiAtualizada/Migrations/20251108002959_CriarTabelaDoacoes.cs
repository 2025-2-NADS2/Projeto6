using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Servidor_PI.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaDoacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Doacao",
                columns: table => new
                {
                    doacao_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    doacao_valor = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Anonima = table.Column<bool>(type: "INTEGER", nullable: false),
                    doacao_status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MercadoPagoPaymentId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CheckoutLink = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    doacao_data_criacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    doacao_data_pagamento = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doacao", x => x.doacao_id);
                    table.ForeignKey(
                        name: "FK_Doacao_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Doacao_UsuarioId",
                table: "Doacao",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Doacao");
        }
    }
}
