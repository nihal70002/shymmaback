namespace ClientEcommerce.API.Helpers
{
    public static class ProductVariantOptions
    {
        public static readonly string[] Styles = ["Left", "Right"];
        public static readonly string[] Materials = ["Titanium", "Stainless Steel"];

        public static string? NormalizeStyle(string? value)
        {
            return Normalize(value, Styles);
        }

        public static string? NormalizeMaterial(string? value)
        {
            return Normalize(value, Materials);
        }

        public static bool IsValidStyle(string? value)
        {
            return NormalizeStyle(value) != null;
        }

        public static bool IsValidMaterial(string? value)
        {
            return NormalizeMaterial(value) != null;
        }

        private static string? Normalize(string? value, IReadOnlyCollection<string> allowedValues)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return allowedValues.FirstOrDefault(v =>
                string.Equals(v, value.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
