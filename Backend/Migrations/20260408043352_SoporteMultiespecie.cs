using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VidaAnimal.API.Migrations
{
    /// <inheritdoc />
    public partial class SoporteMultiespecie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Especies_EspecieID",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "IX_Productos_EspecieID",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "EspecieID",
                table: "Productos");

            migrationBuilder.CreateTable(
                name: "EspecieProducto",
                columns: table => new
                {
                    EspeciesEspecieID = table.Column<int>(type: "int", nullable: false),
                    ProductoID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EspecieProducto", x => new { x.EspeciesEspecieID, x.ProductoID });
                    table.ForeignKey(
                        name: "FK_EspecieProducto_Especies_EspeciesEspecieID",
                        column: x => x.EspeciesEspecieID,
                        principalTable: "Especies",
                        principalColumn: "EspecieID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EspecieProducto_Productos_ProductoID",
                        column: x => x.ProductoID,
                        principalTable: "Productos",
                        principalColumn: "ProductoID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EspecieProducto_ProductoID",
                table: "EspecieProducto",
                column: "ProductoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EspecieProducto");

            migrationBuilder.AddColumn<int>(
                name: "EspecieID",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_EspecieID",
                table: "Productos",
                column: "EspecieID");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Especies_EspecieID",
                table: "Productos",
                column: "EspecieID",
                principalTable: "Especies",
                principalColumn: "EspecieID",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
