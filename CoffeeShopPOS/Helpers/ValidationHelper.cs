namespace CoffeeShopPOS.Helpers;

/// <summary>
/// Provides input-validation helper methods for the Coffee Shop POS application.
/// </summary>
public static class ValidationHelper
{
    /// <summary>Checks whether the text represents a valid decimal price greater than 0.</summary>
    public static bool IsValidPrice(string text)
    {
        return decimal.TryParse(text, out decimal value) && value > 0;
    }

    /// <summary>Checks whether the text represents a valid integer quantity >= 0.</summary>
    public static bool IsValidQuantity(string text)
    {
        return int.TryParse(text, out int value) && value >= 0;
    }

    /// <summary>Checks whether the text is not null, empty, or whitespace.</summary>
    public static bool IsRequired(string text)
    {
        return !string.IsNullOrWhiteSpace(text);
    }

    /// <summary>Checks whether the text represents a valid percentage between 0 and 100.</summary>
    public static bool IsValidPercentage(string text)
    {
        return decimal.TryParse(text, out decimal value) && value >= 0 && value <= 100;
    }

    /// <summary>Shows a red validation-error message on the given label.</summary>
    public static void ShowValidationError(Label lbl, string message)
    {
        lbl.Text = message;
        lbl.ForeColor = ThemeManager.Danger;
        lbl.Font = ThemeManager.SmallFont;
        lbl.Visible = true;
    }

    /// <summary>Clears a validation-error label.</summary>
    public static void ClearValidationError(Label lbl)
    {
        lbl.Text = string.Empty;
        lbl.Visible = false;
    }
}
