using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ArchitectsLibrary.API
{
    /// <summary>
    /// A class for Multi-Language localization.
    /// </summary>
    public static class LanguageSystem
    {
        internal static List<string> languagePaths = new();
        internal static Dictionary<string, string> currentLanguageStrings = new();
        internal static Dictionary<string, string> fallbackLanguageStrings = new();

        /// <summary>
        /// A read-only property of "Undefined" string literal.
        /// </summary>
        public static string Default => "Undefined";

        /// <summary>
        /// Registers a folder path as a Multi-Language folder
        /// </summary>
        /// <param name="languageFolderName">the folder name</param>
        public static void RegisterLocalization(string languageFolderName = "Localization")
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), languageFolderName);
            languagePaths.Add(path);
        }

        /// <summary>
        /// Gets the translation of the key
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the translation string</returns>
        public static string Get(string key) => currentLanguageStrings.GetOrDefault(key, Default);
        
        /// <summary>
        /// Gets a translation of a tooltip of a TechType. Same as <see cref="Get"/> but grabs a translation by the key "Tooltip_{<paramref name="key"/>}"
        /// </summary>
        /// <param name="key">the key</param>
        /// <returns>the translation string</returns>
        public static string GetTooltip(string key) => currentLanguageStrings.GetOrDefault($"Tooltip_{key}", Default);

    }
}