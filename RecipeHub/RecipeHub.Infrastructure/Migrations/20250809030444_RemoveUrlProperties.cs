using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUrlProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageStorage_Users_UploadedByUserId",
                table: "ImageStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_ImageStorage_ImageId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "AvatarImageId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AvatarImageId",
                table: "Users",
                column: "AvatarImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageStorage_Users_UploadedByUserId",
                table: "ImageStorage",
                column: "UploadedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_ImageStorage_ImageId",
                table: "Recipes",
                column: "ImageId",
                principalTable: "ImageStorage",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_ImageStorage_AvatarImageId",
                table: "Users",
                column: "AvatarImageId",
                principalTable: "ImageStorage",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageStorage_Users_UploadedByUserId",
                table: "ImageStorage");

            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_ImageStorage_ImageId",
                table: "Recipes");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_ImageStorage_AvatarImageId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_AvatarImageId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarImageId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageStorage_Users_UploadedByUserId",
                table: "ImageStorage",
                column: "UploadedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_ImageStorage_ImageId",
                table: "Recipes",
                column: "ImageId",
                principalTable: "ImageStorage",
                principalColumn: "Id");
        }
    }
}
