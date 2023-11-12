using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PasteBinASP.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "short_link_id_sequence",
                minValue: 1L,
                maxValue: 2147483647L);

            migrationBuilder.CreateTable(
                name: "short_links",
                columns: table => new
                {
                    link = table.Column<string>(type: "text", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    delete_date_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    requests_count = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_short_links", x => x.link);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "short_links");

            migrationBuilder.DropSequence(
                name: "short_link_id_sequence");
        }
    }
}
