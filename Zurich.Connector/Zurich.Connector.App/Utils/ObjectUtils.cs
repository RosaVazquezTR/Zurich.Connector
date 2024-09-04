namespace Zurich.Connector.App.Utils
{
    /// <summary>
    /// Provides utility methods for working with objects.
    /// </summary>
    public static class ObjectUtils
    {
        /// <summary>
        /// Determines whether the specified dynamic object has a property with the given name.
        /// </summary>
        /// <param name="obj">The dynamic object to check.</param>
        /// <param name="propertyName">The name of the property to check for.</param>
        /// <returns>True if the property exists; otherwise, false.</returns>
        public static bool HasProperty(dynamic obj, string propertyName)
        {
            return obj[propertyName] != null;
        }
    }
}
