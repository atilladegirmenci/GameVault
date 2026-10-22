using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDBWebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeStampsToUserList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "UserLists",
                newName: "ListTitle");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "UserLists",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserLists",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "UserLists");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserLists");

            migrationBuilder.RenameColumn(
                name: "ListTitle",
                table: "UserLists",
                newName: "Name");
        }
    }
}
