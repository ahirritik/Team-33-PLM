using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PLM.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RevertAttachmentFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProposedAttachments",
                table: "ECOs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProposedAttachments",
                table: "ECOs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
