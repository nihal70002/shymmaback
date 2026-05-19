using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientEcommerce.API.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantSnapshotsAndDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassSnapshot",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorSnapshot",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialSnapshot",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductCodeSnapshot",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductNameSnapshot",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SizeSnapshot",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StyleSnapshot",
                table: "OrderItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassSnapshot",
                table: "CartItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorSnapshot",
                table: "CartItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialSnapshot",
                table: "CartItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductCodeSnapshot",
                table: "CartItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductNameSnapshot",
                table: "CartItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SizeSnapshot",
                table: "CartItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StyleSnapshot",
                table: "CartItems",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "ProductVariants"
                SET "Style" = CASE
                    WHEN lower(coalesce("Style", '')) = 'right' THEN 'Right'
                    ELSE 'Left'
                END
                WHERE "Style" IS NULL
                   OR btrim("Style") = ''
                   OR lower("Style") NOT IN ('left', 'right');

                UPDATE "ProductVariants"
                SET "Material" = CASE
                    WHEN lower(coalesce("Material", '')) IN ('stainless steel', 'stainlesssteel', 'steel') THEN 'Stainless Steel'
                    ELSE 'Titanium'
                END
                WHERE "Material" IS NULL
                   OR btrim("Material") = ''
                   OR lower("Material") NOT IN ('titanium', 'stainless steel');

                WITH required("Style", "Material") AS (
                    VALUES ('Left', 'Titanium'),
                           ('Left', 'Stainless Steel'),
                           ('Right', 'Titanium'),
                           ('Right', 'Stainless Steel')
                ),
                base_variants AS (
                    SELECT DISTINCT ON ("ProductId", coalesce("Size", ''), coalesce("Class", ''), coalesce("Color", ''))
                        "Id", "ProductId", "Class", "Color", coalesce("Size", 'Default') AS "Size",
                        "Price", "Stock", "LowStockThreshold"
                    FROM "ProductVariants"
                    ORDER BY "ProductId", coalesce("Size", ''), coalesce("Class", ''), coalesce("Color", ''), "Id"
                )
                INSERT INTO "ProductVariants"
                    ("ProductId", "Class", "Style", "Material", "Color", "LowStockThreshold", "ProductCode", "Size", "Stock", "Price")
                SELECT
                    b."ProductId",
                    b."Class",
                    r."Style",
                    r."Material",
                    b."Color",
                    b."LowStockThreshold",
                    'AUTO-' || b."Id" || '-' || r."Style" || '-' || replace(r."Material", ' ', ''),
                    b."Size",
                    0,
                    b."Price"
                FROM base_variants b
                CROSS JOIN required r
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "ProductVariants" existing
                    WHERE existing."ProductId" = b."ProductId"
                      AND coalesce(existing."Size", '') = coalesce(b."Size", '')
                      AND coalesce(existing."Class", '') = coalesce(b."Class", '')
                      AND coalesce(existing."Color", '') = coalesce(b."Color", '')
                      AND existing."Style" = r."Style"
                      AND existing."Material" = r."Material"
                );

                WITH required("Style", "Material") AS (
                    VALUES ('Left', 'Titanium'),
                           ('Left', 'Stainless Steel'),
                           ('Right', 'Titanium'),
                           ('Right', 'Stainless Steel')
                )
                INSERT INTO "ProductVariants"
                    ("ProductId", "Class", "Style", "Material", "Color", "LowStockThreshold", "ProductCode", "Size", "Stock", "Price")
                SELECT
                    p."Id",
                    NULL,
                    r."Style",
                    r."Material",
                    NULL,
                    10,
                    'AUTO-P' || p."Id" || '-' || r."Style" || '-' || replace(r."Material", ' ', ''),
                    'Default',
                    0,
                    0
                FROM "Products" p
                CROSS JOIN required r
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "ProductVariants" existing
                    WHERE existing."ProductId" = p."Id"
                );

                UPDATE "CartItems" ci
                SET
                    "ProductNameSnapshot" = p."Name",
                    "SizeSnapshot" = pv."Size",
                    "StyleSnapshot" = pv."Style",
                    "MaterialSnapshot" = pv."Material",
                    "ColorSnapshot" = pv."Color",
                    "ClassSnapshot" = pv."Class",
                    "ProductCodeSnapshot" = pv."ProductCode"
                FROM "ProductVariants" pv
                JOIN "Products" p ON p."Id" = pv."ProductId"
                WHERE ci."ProductVariantId" = pv."Id";

                UPDATE "OrderItems" oi
                SET
                    "ProductNameSnapshot" = p."Name",
                    "SizeSnapshot" = pv."Size",
                    "StyleSnapshot" = pv."Style",
                    "MaterialSnapshot" = pv."Material",
                    "ColorSnapshot" = pv."Color",
                    "ClassSnapshot" = pv."Class",
                    "ProductCodeSnapshot" = pv."ProductCode"
                FROM "ProductVariants" pv
                JOIN "Products" p ON p."Id" = pv."ProductId"
                WHERE oi."ProductVariantId" = pv."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "ProductVariants"
                WHERE "ProductCode" LIKE 'AUTO-%';
                """);

            migrationBuilder.DropColumn(
                name: "ClassSnapshot",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ColorSnapshot",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "MaterialSnapshot",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductCodeSnapshot",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductNameSnapshot",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "SizeSnapshot",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "StyleSnapshot",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ClassSnapshot",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ColorSnapshot",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "MaterialSnapshot",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ProductCodeSnapshot",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ProductNameSnapshot",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "SizeSnapshot",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "StyleSnapshot",
                table: "CartItems");
        }
    }
}
