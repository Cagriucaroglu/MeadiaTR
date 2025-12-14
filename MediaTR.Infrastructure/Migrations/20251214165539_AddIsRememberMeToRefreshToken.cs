using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediaTR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRememberMeToRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRememberMe",
                table: "RefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRememberMe",
                table: "RefreshTokens");
        }
    }
}
