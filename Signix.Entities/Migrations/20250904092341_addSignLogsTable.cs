using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Signix.Entities.Migrations
{
    /// <inheritdoc />
    public partial class addSignLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_signers_designations_designation_id",
                table: "signers");

            migrationBuilder.CreateTable(
                name: "sign_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    document_id = table.Column<int>(type: "integer", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sign_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_sign_logs_documents_document_id",
                        column: x => x.document_id,
                        principalTable: "documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_sign_logs_document_id",
                table: "sign_logs",
                column: "document_id");

            migrationBuilder.AddForeignKey(
                name: "fk_signers_designations_designation_id",
                table: "signers",
                column: "designation_id",
                principalTable: "designations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_signers_designations_designation_id",
                table: "signers");

            migrationBuilder.DropTable(
                name: "sign_logs");

            migrationBuilder.AddForeignKey(
                name: "fk_signers_designations_designation_id",
                table: "signers",
                column: "designation_id",
                principalTable: "designations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
