using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Verduleria.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cajeros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Legajo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cajeros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrecioPorKg = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    StockKg = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CantidadKg = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CajeroId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventas_Cajeros_CajeroId",
                        column: x => x.CajeroId,
                        principalTable: "Cajeros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Cajeros",
                columns: new[] { "Id", "Activo", "Legajo", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "CAJ-01", "Lucía Gómez" },
                    { 2, true, "CAJ-02", "Martín Pérez" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Nombre", "PrecioPorKg", "StockKg" },
                values: new object[,]
                {
                    { 1, "Pimiento", 2800m, 25m },
                    { 2, "Tomate", 1900m, 40m },
                    { 3, "Pera", 1600m, 30m },
                    { 4, "Cebolla", 1200m, 50m },
                    { 5, "Banana", 2100m, 35m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CajeroId",
                table: "Ventas",
                column: "CajeroId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ProductoId",
                table: "Ventas",
                column: "ProductoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "Cajeros");

            migrationBuilder.DropTable(
                name: "Productos");
        }
    }
}
