using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionOrdinateurs.API.Migrations
{
    /// <inheritdoc />
    public partial class Config : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reparations_Ordinateurs_UnOrdinateurId",
                table: "Reparations");

            migrationBuilder.RenameColumn(
                name: "UnOrdinateurId",
                table: "Reparations",
                newName: "OrdinateurId");

            migrationBuilder.RenameIndex(
                name: "IX_Reparations_UnOrdinateurId",
                table: "Reparations",
                newName: "IX_Reparations_OrdinateurId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reparations_Ordinateurs_OrdinateurId",
                table: "Reparations",
                column: "OrdinateurId",
                principalTable: "Ordinateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reparations_Ordinateurs_OrdinateurId",
                table: "Reparations");

            migrationBuilder.RenameColumn(
                name: "OrdinateurId",
                table: "Reparations",
                newName: "UnOrdinateurId");

            migrationBuilder.RenameIndex(
                name: "IX_Reparations_OrdinateurId",
                table: "Reparations",
                newName: "IX_Reparations_UnOrdinateurId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reparations_Ordinateurs_UnOrdinateurId",
                table: "Reparations",
                column: "UnOrdinateurId",
                principalTable: "Ordinateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
