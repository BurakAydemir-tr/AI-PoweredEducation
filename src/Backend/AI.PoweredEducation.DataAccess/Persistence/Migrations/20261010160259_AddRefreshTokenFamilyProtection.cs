using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI.PoweredEducation.DataAccess.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenFamilyProtection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FamilyId",
                table: "RefreshTokens",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FamilyRevokedAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""
                WITH RECURSIVE token_families AS (
                    SELECT token."Id", token."ReplacedByTokenHash", token."Id" AS "RootId"
                    FROM "RefreshTokens" AS token
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM "RefreshTokens" AS parent
                        WHERE parent."ReplacedByTokenHash" = token."TokenHash")

                    UNION ALL

                    SELECT child."Id", child."ReplacedByTokenHash", family."RootId"
                    FROM token_families AS family
                    JOIN "RefreshTokens" AS child
                        ON child."TokenHash" = family."ReplacedByTokenHash"
                )
                UPDATE "RefreshTokens" AS token
                SET "FamilyId" = family."RootId"
                FROM token_families AS family
                WHERE token."Id" = family."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_FamilyId_RevokedAt",
                table: "RefreshTokens",
                columns: new[] { "FamilyId", "RevokedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_FamilyId_RevokedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "FamilyRevokedAt",
                table: "RefreshTokens");
        }
    }
}
