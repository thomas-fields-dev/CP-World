﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CpWorld.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsActiveFlagToItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Item",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Item");
        }
    }
}
