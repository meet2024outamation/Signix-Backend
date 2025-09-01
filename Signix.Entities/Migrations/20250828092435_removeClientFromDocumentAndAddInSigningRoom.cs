using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Signix.Entities.Migrations
{
    /// <inheritdoc />
    public partial class removeClientFromDocumentAndAddInSigningRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_documents_clients_client_id",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "ix_documents_client_id",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "client_id",
                table: "documents");

            migrationBuilder.AddColumn<int>(
                name: "client_id",
                table: "signing_rooms",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_signing_rooms_client_id",
                table: "signing_rooms",
                column: "client_id");

            migrationBuilder.AddForeignKey(
                name: "fk_signing_rooms_clients_client_id",
                table: "signing_rooms",
                column: "client_id",
                principalTable: "clients",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_signing_rooms_clients_client_id",
                table: "signing_rooms");

            migrationBuilder.DropIndex(
                name: "ix_signing_rooms_client_id",
                table: "signing_rooms");

            migrationBuilder.DropColumn(
                name: "client_id",
                table: "signing_rooms");

            migrationBuilder.AddColumn<int>(
                name: "client_id",
                table: "documents",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_documents_client_id",
                table: "documents",
                column: "client_id");

            migrationBuilder.AddForeignKey(
                name: "fk_documents_clients_client_id",
                table: "documents",
                column: "client_id",
                principalTable: "clients",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
