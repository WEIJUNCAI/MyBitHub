using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace BitHub.Data.Migrations
{
    public partial class AddRepoModelProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RootPath",
                table: "Repositories",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Owner",
                table: "Repositories",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "ForkCount",
                table: "Repositories",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StarCount",
                table: "Repositories",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WatchCount",
                table: "Repositories",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RepoTagModel",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TagName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepoTagModel", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RepoTagmentModel",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RepoID = table.Column<int>(nullable: false),
                    TagID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepoTagmentModel", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RepoTagmentModel_Repositories_RepoID",
                        column: x => x.RepoID,
                        principalTable: "Repositories",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepoTagmentModel_RepoTagModel_TagID",
                        column: x => x.TagID,
                        principalTable: "RepoTagModel",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepoTagmentModel_RepoID",
                table: "RepoTagmentModel",
                column: "RepoID");

            migrationBuilder.CreateIndex(
                name: "IX_RepoTagmentModel_TagID",
                table: "RepoTagmentModel",
                column: "TagID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepoTagmentModel");

            migrationBuilder.DropTable(
                name: "RepoTagModel");

            migrationBuilder.DropColumn(
                name: "ForkCount",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "StarCount",
                table: "Repositories");

            migrationBuilder.DropColumn(
                name: "WatchCount",
                table: "Repositories");

            migrationBuilder.AlterColumn<string>(
                name: "RootPath",
                table: "Repositories",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Owner",
                table: "Repositories",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
