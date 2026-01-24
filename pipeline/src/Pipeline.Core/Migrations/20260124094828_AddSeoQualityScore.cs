using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pipeline.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddSeoQualityScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SeoQualityScore",
                schema: "content",
                table: "neighborhood_prose",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeoQualityScore",
                schema: "content",
                table: "neighborhood_prose");
        }
    }
}
