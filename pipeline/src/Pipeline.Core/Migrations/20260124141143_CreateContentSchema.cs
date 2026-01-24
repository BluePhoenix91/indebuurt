using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pipeline.Core.Migrations
{
    /// <inheritdoc />
    public partial class CreateContentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "label_rules",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LabelText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LabelIcon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConditionField = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConditionOperator = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConditionValue = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_label_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "neighborhood_prose",
                schema: "content",
                columns: table => new
                {
                    NisCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Intro = table.Column<string>(type: "text", nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    QualityScore = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    SeoQualityScore = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    PromptVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_neighborhood_prose", x => x.NisCode);
                });

            migrationBuilder.CreateTable(
                name: "value_card_templates",
                schema: "content",
                columns: table => new
                {
                    CardType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_value_card_templates", x => x.CardType);
                });

            migrationBuilder.CreateIndex(
                name: "IX_neighborhood_prose_Slug",
                schema: "content",
                table: "neighborhood_prose",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "label_rules",
                schema: "content");

            migrationBuilder.DropTable(
                name: "neighborhood_prose",
                schema: "content");

            migrationBuilder.DropTable(
                name: "value_card_templates",
                schema: "content");
        }
    }
}
