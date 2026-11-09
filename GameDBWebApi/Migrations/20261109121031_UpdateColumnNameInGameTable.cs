using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDBWebApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnNameInGameTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RawgId",
                table: "Games",
                newName: "CatalogId");

            migrationBuilder.RenameIndex(
                name: "IX_Games_RawgId",
                table: "Games",
                newName: "IX_Games_CatalogId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CatalogId",
                table: "Games",
                newName: "RawgId");

            migrationBuilder.RenameIndex(
                name: "IX_Games_CatalogId",
                table: "Games",
                newName: "IX_Games_RawgId");
        }
    }
}
