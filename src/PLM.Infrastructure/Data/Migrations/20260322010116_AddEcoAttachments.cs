using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PLM.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEcoAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProposedAttachments",
                table: "ECOs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProposedAttachments",
                table: "ECOs");
        }
    }
}
