using Moriyama.AzureSearch.Umbraco.Application.Interfaces;

namespace Moriyama.AzureSearch.Umbraco.Application.Helper
{
    /// <summary>
    ///  extensions to the ISearchContent item we get back 
    /// </summary>
    public static class SearchContentExtensions
    {

        public static string GetPropertyValue(this ISearchContent item, string property)
        {
            return GetPropertyValue<string>(item, property, default(string));
        }

        public static string GetPropertyValue(this ISearchContent item, string property, string defaultValue)
        {
            return GetPropertyValue<string>(item, property, defaultValue);
        }

        public static T GetPropertyValue<T>(this ISearchContent item, string property)
        {
            return GetPropertyValue(item, property, default(T));
        }

        public static T GetPropertyValue<T>(this ISearchContent item, string property, T defaultValue)
        {
            if (item.Properties.ContainsKey(property))
            {
                return (T)item.Properties[property];
            }

            return defaultValue;
        }
    }
}