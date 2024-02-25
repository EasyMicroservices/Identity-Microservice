using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyMicroservices.IdentityMicroservice.Migrations
{
    /// <inheritdoc />
    public partial class AddedResetPasswordTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResetPasswordToken",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificationDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UniqueIdentity = table.Column<string>(type: "nvarchar(450)", nullable: true, collation: "SQL_Latin1_General_CP1_CS_AS"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HasConsumed = table.Column<bool>(type: "bit", nullable: false),
                    ExpirationDateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResetPasswordToken", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswordToken_CreationDateTime",
                table: "ResetPasswordToken",
                column: "CreationDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswordToken_DeletedDateTime",
                table: "ResetPasswordToken",
                column: "DeletedDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswordToken_IsDeleted",
                table: "ResetPasswordToken",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswordToken_ModificationDateTime",
                table: "ResetPasswordToken",
                column: "ModificationDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswordToken_UniqueIdentity",
                table: "ResetPasswordToken",
                column: "UniqueIdentity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResetPasswordToken");
        }
    }
}
