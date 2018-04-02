using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BitHub.Data.Migrations
{
    public partial class AddRepoTagAndTagment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepoTagmentModel_Repositories_RepoID",
                table: "RepoTagmentModel");

            migrationBuilder.DropForeignKey(
                name: "FK_RepoTagmentModel_RepoTagModel_TagID",
                table: "RepoTagmentModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RepoTagModel",
                table: "RepoTagModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RepoTagmentModel",
                table: "RepoTagmentModel");

            migrationBuilder.RenameTable(
                name: "RepoTagModel",
                newName: "Tags");

            migrationBuilder.RenameTable(
                name: "RepoTagmentModel",
                newName: "Tagments");

            migrationBuilder.RenameIndex(
                name: "IX_RepoTagmentModel_TagID",
                table: "Tagments",
                newName: "IX_Tagments_TagID");

            migrationBuilder.RenameIndex(
                name: "IX_RepoTagmentModel_RepoID",
                table: "Tagments",
                newName: "IX_Tagments_RepoID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tagments",
                table: "Tagments",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Tagments_Repositories_RepoID",
                table: "Tagments",
                column: "RepoID",
                principalTable: "Repositories",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tagments_Tags_TagID",
                table: "Tagments",
                column: "TagID",
                principalTable: "Tags",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tagments_Repositories_RepoID",
                table: "Tagments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tagments_Tags_TagID",
                table: "Tagments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tagments",
                table: "Tagments");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "RepoTagModel");

            migrationBuilder.RenameTable(
                name: "Tagments",
                newName: "RepoTagmentModel");

            migrationBuilder.RenameIndex(
                name: "IX_Tagments_TagID",
                table: "RepoTagmentModel",
                newName: "IX_RepoTagmentModel_TagID");

            migrationBuilder.RenameIndex(
                name: "IX_Tagments_RepoID",
                table: "RepoTagmentModel",
                newName: "IX_RepoTagmentModel_RepoID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RepoTagModel",
                table: "RepoTagModel",
                column: "ID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RepoTagmentModel",
                table: "RepoTagmentModel",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RepoTagmentModel_Repositories_RepoID",
                table: "RepoTagmentModel",
                column: "RepoID",
                principalTable: "Repositories",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RepoTagmentModel_RepoTagModel_TagID",
                table: "RepoTagmentModel",
                column: "TagID",
                principalTable: "RepoTagModel",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
