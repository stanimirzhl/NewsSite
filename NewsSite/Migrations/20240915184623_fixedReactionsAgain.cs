using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsSite.Migrations
{
    /// <inheritdoc />
    public partial class fixedReactionsAgain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Reactions",
                table: "News",
                newName: "UserIds");

            migrationBuilder.AddColumn<string>(
                name: "IsLiked",
                table: "News",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLiked",
                table: "News");

            migrationBuilder.RenameColumn(
                name: "UserIds",
                table: "News",
                newName: "Reactions");
        }
    }
}
