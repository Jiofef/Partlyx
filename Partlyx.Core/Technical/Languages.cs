using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Core.Technical
{
    public static class Languages
    {
        private static readonly ConcurrentDictionary<string, LanguageInfo> _availableLanguages = new();

        static Languages()
        {
            _availableLanguages.TryAdd("en-US", new LanguageInfo("English", "en-US", "English"));
            _availableLanguages.TryAdd("ru-RU", new LanguageInfo("Russian", "ru-RU", "Русский"));
        }

        public static IReadOnlyDictionary<string, LanguageInfo> AvailableLanguages
            => new ReadOnlyDictionary<string, LanguageInfo>(_availableLanguages);
        public static IReadOnlyList<LanguageInfo> AvailableLanguagesList => AvailableLanguages.Values.ToList();

        public static bool TryAddLanguage(LanguageInfo language)
        {
            return _availableLanguages.TryAdd(language.Code, language);
        }

        public static bool TryRemoveLanguage(string languageCode)
        {
            return _availableLanguages.TryRemove(languageCode, out _);
        }

        public static LanguageInfo? GetLanguage(string code)
        {
            return _availableLanguages.TryGetValue(code, out var language)
                ? language
                : null;
        }
    }

    public record LanguageInfo(string Name, string Code, string NativeName);
}
