using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ThaiX.Infrastructure.Persistence;

#nullable disable

namespace ThaiX.Infrastructure.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20261104050000_AddEmailOtpLogin")]
public partial class AddEmailOtpLogin : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "EmailOtpLoginEnabled",
            table: "AspNetUsers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "EmailOtpChallenges",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "char(36)", nullable: false),
                UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                CodeHash = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                ExpiresAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                FailedAttempts = table.Column<int>(type: "int", nullable: false),
                ConsumedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                InvalidatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EmailOtpChallenges", x => x.Id);
                table.ForeignKey(
                    name: "FK_EmailOtpChallenges_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySQL:Charset", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_EmailOtpChallenges_ExpiresAtUtc",
            table: "EmailOtpChallenges",
            column: "ExpiresAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_EmailOtpChallenges_UserId",
            table: "EmailOtpChallenges",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_EmailOtpChallenges_UserId_ConsumedAtUtc_InvalidatedAtUtc",
            table: "EmailOtpChallenges",
            columns: new[] { "UserId", "ConsumedAtUtc", "InvalidatedAtUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "EmailOtpChallenges");

        migrationBuilder.DropColumn(
            name: "EmailOtpLoginEnabled",
            table: "AspNetUsers");
    }
}
