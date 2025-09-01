using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Signix.Entities.Migrations
{
    /// <inheritdoc />
    public partial class addSignTagsInSigningRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sign_tags",
                table: "signing_rooms",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sign_tags",
                table: "signing_rooms");
        }
    }
}
