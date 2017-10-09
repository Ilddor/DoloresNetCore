using System;
using System.Collections.Generic;
using System.Text;

namespace Dolores.DataClasses
{
    public static class LanguageDictionary
    {
        public enum Language
        {
            PL,
            EN,
        }
        public enum LangString
        {
            NoTranslation,
            AvailableCommands,
            TimeOnline,
            Version,
        }

        private static Dictionary<LangString, Dictionary<Language, string>> m_Dict = new Dictionary<LangString, Dictionary<Language, string>>();

        public static string GetString(LangString langString, Language lang)
        {
            if (m_Dict.ContainsKey(langString))
                return m_Dict[langString][lang];
            else
                return m_Dict[LangString.NoTranslation][lang];
        }

        static LanguageDictionary()
        {
            m_Dict = new Dictionary<LangString, Dictionary<Language, string>>()
            {
                { LangString.NoTranslation, new Dictionary<Language, string>()
                    {
                        { Language.PL, "<brak tłumaczenia>" },
                        { Language.EN, "<no translation>" },
                    }
                },
                { LangString.AvailableCommands, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Dostępne komendy" },
                        { Language.EN, "Available commands" },
                    }
                },
                { LangString.TimeOnline, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Czas online(bota)" },
                        { Language.EN, "Time Online(bot)" },
                    }
                },
                { LangString.Version, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Wersja" },
                        { Language.EN, "Version" },
                    }
                },
            };
        }
    }
}
