using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Partlyx.Infrastructure.Migrations.PartlyxDB
{
    /// <inheritdoc />
    public partial class RecipesIndependanceMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_Resources_ResourceUid",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_ResourceUid",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "CraftAmount",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "ResourceUid",
                table: "Recipes");

            migrationBuilder.AddColumn<bool>(
                name: "IsReversible",
                table: "Recipes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOutput",
                table: "RecipeComponents",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReversible",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "IsOutput",
                table: "RecipeComponents");

            migrationBuilder.AddColumn<double>(
                name: "CraftAmount",
                table: "Recipes",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "ResourceUid",
                table: "Recipes",
                type: "BLOB",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_ResourceUid",
                table: "Recipes",
                column: "ResourceUid");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_Resources_ResourceUid",
                table: "Recipes",
                column: "ResourceUid",
                principalTable: "Resources",
                principalColumn: "Uid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
