using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDBWebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenFingerprintToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshToken",
                table: "Users",
                newName: "RefreshTokenFingerprint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RefreshTokenFingerprint",
                table: "Users",
                newName: "RefreshToken");
        }
    }
}
