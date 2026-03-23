using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class _3ra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NombreArchivoFactura",
                table: "Cliente",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RutaArchivoFactura",
                table: "Cliente",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombreArchivoFactura",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "RutaArchivoFactura",
                table: "Cliente");
        }
    }
}
