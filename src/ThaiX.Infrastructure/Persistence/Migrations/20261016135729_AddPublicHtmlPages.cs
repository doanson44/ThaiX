using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThaiX.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicHtmlPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublicHtmlPages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Title = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    StorageKey = table.Column<string>(type: "varchar(1024)", maxLength: 1024, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    IsPublished = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastViewedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Slug = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)"),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "char(36)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicHtmlPages", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PublicHtmlPages_IsPublished",
                table: "PublicHtmlPages",
                column: "IsPublished");

            migrationBuilder.CreateIndex(
                name: "IX_PublicHtmlPages_LastViewedAtUtc",
                table: "PublicHtmlPages",
                column: "LastViewedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PublicHtmlPages_Slug",
                table: "PublicHtmlPages",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PublicHtmlPages");
        }
    }
}
