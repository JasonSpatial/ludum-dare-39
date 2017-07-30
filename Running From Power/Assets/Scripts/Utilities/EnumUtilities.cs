namespace Assets.Scripts.Utilities
{
    using System;

    /// <summary>
    ///     Utilities for enums.
    /// </summary>
    public static class EnumUtilities
    {
        /// <summary>
        ///     Parses a string value and converts it into an enum value.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The enum value.</returns>
        public static bool TryParse<TEnum>(string value, out TEnum result)
        {
            try
            {
                result = (TEnum)Enum.Parse(typeof(TEnum), value, true);
                return true;
            }
            catch (ArgumentException)
            {
                result = (TEnum)Activator.CreateInstance(typeof(TEnum));
                return false;
            }
        }
    }
}