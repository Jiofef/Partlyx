using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Partlyx.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Uid = table.Column<Guid>(type: "BLOB", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DefaultRecipeUid = table.Column<Guid>(type: "BLOB", nullable: true),
                    IconType = table.Column<int>(type: "INTEGER", nullable: false, defaultValue:0),
                    IconData = table.Column<string>(type: "TEXT", nullable: false, defaultValue:"{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Uid);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Uid = table.Column<Guid>(type: "BLOB", nullable: false),
                    ResourceUid = table.Column<Guid>(type: "BLOB", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CraftAmount = table.Column<double>(type: "REAL", nullable: false),
                    IconType = table.Column<int>(type: "INTEGER", nullable: false, defaultValue:0),
                    IconData = table.Column<string>(type: "TEXT", nullable: false, defaultValue:"{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Uid);
                    table.ForeignKey(
                        name: "FK_Recipes_Resources_ResourceUid",
                        column: x => x.ResourceUid,
                        principalTable: "Resources",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeComponents",
                columns: table => new
                {
                    Uid = table.Column<Guid>(type: "BLOB", nullable: false),
                    RecipeUid = table.Column<Guid>(type: "BLOB", nullable: true),
                    ComponentResourceUid = table.Column<Guid>(type: "BLOB", nullable: false),
                    Quantity = table.Column<double>(type: "REAL", nullable: false),
                    ComponentSelectedRecipeUid = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeComponents", x => x.Uid);
                    table.ForeignKey(
                        name: "FK_RecipeComponents_Recipes_RecipeUid",
                        column: x => x.RecipeUid,
                        principalTable: "Recipes",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeComponents_Resources_ComponentResourceUid",
                        column: x => x.ComponentResourceUid,
                        principalTable: "Resources",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeComponents_ComponentResourceUid",
                table: "RecipeComponents",
                column: "ComponentResourceUid");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeComponents_RecipeUid",
                table: "RecipeComponents",
                column: "RecipeUid");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_ResourceUid",
                table: "Recipes",
                column: "ResourceUid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeComponents");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "Resources");
        }
    }
}
