using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameDBWebApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusBasedUserLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserLists_UserId",
                table: "UserLists");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "UserLists",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Backfill the three status lists for verified accounts. The unique filtered
            // index below also protects against duplicates after this one-time insert.
            migrationBuilder.Sql("""
                INSERT INTO [UserLists] ([ListTitle], [UserId], [CreatedAt], [Type])
                SELECT defaults.[Title], [Users].[Id], SYSUTCDATETIME(), defaults.[Type]
                FROM [Users]
                CROSS JOIN (VALUES
                    (1, N'Plans to Play'),
                    (2, N'Currently Playing'),
                    (3, N'Finished')
                ) AS defaults([Type], [Title])
                WHERE [Users].[EmailConfirmed] = 1
                  AND NOT EXISTS (
                      SELECT 1
                      FROM [UserLists] AS existing
                      WHERE existing.[UserId] = [Users].[Id]
                        AND existing.[Type] = defaults.[Type]
                  );

                WITH RankedStatusGames AS (
                    SELECT lists.[Id] AS [UserListId], library.[GameId],
                           library.[UpdatedDate] AS [AddedDate],
                           ROW_NUMBER() OVER (
                               PARTITION BY lists.[Id]
                               ORDER BY library.[UpdatedDate] DESC, library.[GameId] ASC
                           ) - 1 AS [Position]
                    FROM [UserGames] AS library
                    INNER JOIN [UserLists] AS lists
                        ON lists.[UserId] = library.[UserId]
                       AND lists.[Type] = CONVERT(int, library.[Status])
                    WHERE lists.[Type] IN (1, 2, 3)
                )
                INSERT INTO [UserListGames] ([UserListId], [GameId], [AddedDate], [Position])
                SELECT ranked.[UserListId], ranked.[GameId], ranked.[AddedDate], ranked.[Position]
                FROM RankedStatusGames AS ranked
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [UserListGames] AS existing
                    WHERE existing.[UserListId] = ranked.[UserListId]
                      AND existing.[GameId] = ranked.[GameId]
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_UserLists_UserId_Type",
                table: "UserLists",
                columns: new[] { "UserId", "Type" },
                unique: true,
                filter: "[Type] <> 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserLists_UserId_Type",
                table: "UserLists");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "UserLists");

            migrationBuilder.CreateIndex(
                name: "IX_UserLists_UserId",
                table: "UserLists",
                column: "UserId");
        }
    }
}
