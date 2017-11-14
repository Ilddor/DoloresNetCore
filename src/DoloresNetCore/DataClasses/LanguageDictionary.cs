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
            Results,
            UnknownCommand,
            CurrentlyPlaying,
            StartingPlaying,
            SongEnd,
            StoppingPlaying,
            Moving,
            ToChannel,
            RemovingChannel,
            Optional,
            PossibleValues,
            NoGame,
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
                { LangString.Results, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Wyniki" },
                        { Language.EN, "Results" },
                    }
                },
                { LangString.UnknownCommand, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Niczego mi to nie przypomina (nie ma takiej komendy)" },
                        { Language.EN, "Doesn't look like anything to me (there's no such command)" },
                    }
                },
                { LangString.CurrentlyPlaying, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Aktualnie odtwarzana jest muzyka, kolejka nie jest jeszcze wspierana, by zmienic utwór wpisz najpierw !stopPlay" },
                        { Language.EN, "Currently playing a song, queue is not yet supported, to change song use !stopPlay before" },
                    }
                },
                { LangString.StartingPlaying, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Właczam odtwarzanie" },
                        { Language.EN, "starting playing" },
                    }
                },
                { LangString.SongEnd, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Koniec utworu" },
                        { Language.EN, "Song end" },
                    }
                },
                { LangString.StoppingPlaying, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Przerywam odtwarzanie, proces sie zakonczyl" },
                        { Language.EN, "Stopping playing, process has ended" },
                    }
                },
                { LangString.Moving, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Przenoszę" },
                        { Language.EN, "Moving" },
                    }
                },
                { LangString.ToChannel, new Dictionary<Language, string>()
                    {
                        { Language.PL, "na kanał gry" },
                        { Language.EN, "to game voice channel" },
                    }
                },
                { LangString.RemovingChannel, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Usuwam kanał gry" },
                        { Language.EN, "Removing game voice channel" },
                    }
                },
                { LangString.Optional, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Opcjnalny" },
                        { Language.EN, "Optional" },
                    }
                },
                { LangString.PossibleValues, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Możliwe wartości" },
                        { Language.EN, "Possible values" },
                    }
                },
                { LangString.NoGame, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Nie grasz aktualnie w żadną grę" },
                        { Language.EN, "You're not playing anything right now" },
                    }
                },
            };
        }
    }
}
