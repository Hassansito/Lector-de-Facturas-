using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "CreatedDate", "NombreUsuario", "PasswordHash", "Rol" },
                values: new object[] { new Guid("355983f8-c69b-4427-878f-518c7b77663d"), new DateTime(2026, 3, 24, 14, 25, 9, 557, DateTimeKind.Utc).AddTicks(5608), "admin", "$2a$11$DHmJWP5NnVTiRg4beT2X1OBXF/tNvJB42X3XT3fNLWYPd09qCYbg2", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: new Guid("355983f8-c69b-4427-878f-518c7b77663d"));
        }
    }
}
