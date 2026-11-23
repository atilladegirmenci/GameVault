using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDBWebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserListGamePosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "UserListGames",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Preserve the list endpoint's existing newest-added-first order.
            // GameId makes entries with the same AddedDate deterministic.
            migrationBuilder.Sql("""
                WITH RankedGames AS (
                    SELECT [UserListId], [GameId],
                           ROW_NUMBER() OVER (
                               PARTITION BY [UserListId]
                               ORDER BY [AddedDate] DESC, [GameId] ASC
                           ) - 1 AS [Position]
                    FROM [UserListGames]
                )
                UPDATE ulg
                SET [Position] = ranked.[Position]
                FROM [UserListGames] AS ulg
                INNER JOIN RankedGames AS ranked
                    ON ranked.[UserListId] = ulg.[UserListId]
                    AND ranked.[GameId] = ulg.[GameId];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Position",
                table: "UserListGames");
        }
    }
}
