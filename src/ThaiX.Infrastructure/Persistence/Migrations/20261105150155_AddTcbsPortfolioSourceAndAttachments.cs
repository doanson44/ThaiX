using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThaiX.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTcbsPortfolioSourceAndAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ChangeType",
                table: "TcbsTop10Tickers",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "SourceContentId",
                table: "TcbsTop10Portfolios",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "TcbsTop10Portfolios",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "IWealthClub");

            migrationBuilder.AlterColumn<string>(
                name: "ImageType",
                table: "TcbsTop10Images",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "FileGuid",
                table: "TcbsTop10Images",
                type: "char(36)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "char(36)");

            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "TcbsTop10Images",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Image");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "TcbsTop10Images",
                type: "varchar(512)",
                maxLength: 512,
                nullable: true);

            // Enum columns are stored as names from now on; rewrite the existing numeric values.
            migrationBuilder.Sql("""
                UPDATE `TcbsTop10Tickers`
                SET `ChangeType` = CASE `ChangeType`
                    WHEN '0' THEN 'Added'
                    WHEN '1' THEN 'Removed'
                    ELSE `ChangeType` END;

                UPDATE `TcbsTop10Images`
                SET `ImageType` = CASE `ImageType`
                    WHEN '0' THEN 'PortfolioDetail'
                    WHEN '1' THEN 'PeriodPerformance'
                    WHEN '2' THEN 'YtdPerformance'
                    ELSE `ImageType` END;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_TcbsTop10Portfolios_Source_PostedAt",
                table: "TcbsTop10Portfolios",
                columns: new[] { "Source", "PostedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TcbsTop10Portfolios_Source_PostedAt",
                table: "TcbsTop10Portfolios");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "TcbsTop10Portfolios");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "TcbsTop10Images");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "TcbsTop10Images");

            migrationBuilder.Sql("""
                UPDATE `TcbsTop10Tickers`
                SET `ChangeType` = CASE `ChangeType`
                    WHEN 'Added' THEN '0'
                    WHEN 'Removed' THEN '1'
                    ELSE `ChangeType` END;

                UPDATE `TcbsTop10Images`
                SET `ImageType` = CASE `ImageType`
                    WHEN 'PortfolioDetail' THEN '0'
                    WHEN 'PeriodPerformance' THEN '1'
                    WHEN 'YtdPerformance' THEN '2'
                    ELSE `ImageType` END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ChangeType",
                table: "TcbsTop10Tickers",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AlterColumn<long>(
                name: "SourceContentId",
                table: "TcbsTop10Portfolios",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ImageType",
                table: "TcbsTop10Images",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FileGuid",
                table: "TcbsTop10Images",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true);
        }
    }
}
