using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Pipeline.Core.Migrations
{
    /// <inheritdoc />
    public partial class SeedValueCardTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "content",
                table: "value_card_templates",
                columns: new[] { "CardType", "SortOrder", "Title" },
                values: new object[,]
                {
                    { "DogParks", 1, "Hondenparken" },
                    { "Parks", 2, "Parken" },
                    { "PetStores", 4, "Dierenwinkels" },
                    { "Supermarkets", 5, "Supermarkten" },
                    { "Transit", 6, "Openbaar vervoer" },
                    { "Vets", 3, "Dierenartsen" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "content",
                table: "value_card_templates",
                keyColumn: "CardType",
                keyValue: "DogParks");

            migrationBuilder.DeleteData(
                schema: "content",
                table: "value_card_templates",
                keyColumn: "CardType",
                keyValue: "Parks");

            migrationBuilder.DeleteData(
                schema: "content",
                table: "value_card_templates",
                keyColumn: "CardType",
                keyValue: "PetStores");

            migrationBuilder.DeleteData(
                schema: "content",
                table: "value_card_templates",
                keyColumn: "CardType",
                keyValue: "Supermarkets");

            migrationBuilder.DeleteData(
                schema: "content",
                table: "value_card_templates",
                keyColumn: "CardType",
                keyValue: "Transit");

            migrationBuilder.DeleteData(
                schema: "content",
                table: "value_card_templates",
                keyColumn: "CardType",
                keyValue: "Vets");
        }
    }
}
