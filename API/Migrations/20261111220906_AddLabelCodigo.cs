using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddLabelCodigo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "LabelTemplates",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            // Backfill: ETI- + Id con ceros a la izquierda (mínimo 4 dígitos)
            migrationBuilder.Sql(
                "UPDATE LabelTemplates SET Codigo = 'ETI-' + FORMAT(Id, '0000') WHERE Codigo = '';");

            migrationBuilder.CreateIndex(
                name: "IX_LabelTemplates_Codigo",
                table: "LabelTemplates",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LabelTemplates_Codigo",
                table: "LabelTemplates");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "LabelTemplates");
        }
    }
}
