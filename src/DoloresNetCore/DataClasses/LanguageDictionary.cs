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
            OrganizationalMessage,
            OrganizationalMessageFullPart1,
            OrganizationalMessageFullPart2,
            MissingAddresses,
            FilledAddresses,
            Pictures,
            Missing,
            UserFilledAddressResponse,
            UserSentPicturesResponse,
            Yes,
            No,
            ExchangeRolled,
            GifterMessagePart1,
            GifterMessagePart2,
            GifterMessagePart3,
            GifterMessagePart4,
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
                        { Language.PL, "Niczego mi to nie przypomina" },
                        { Language.EN, "Doesn't look like anything to me" },
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
                { LangString.OrganizationalMessage, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Wiadomość organizacyjna" },
                        { Language.EN, "Organizational message" },
                    }
                },
                { LangString.OrganizationalMessageFullPart1, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Wiadomość organizacyjna, jeśli widniejesz w polu `brakujące adresy` wyślij proszę swój adres do bota w prywatnej wiadomości " +
                                       "formatując ją w następujący sposób: " },
                        { Language.EN, "Organizational message, if you're in a field `missing addresses` then please send your shipping address to bot in private message " +
                                       "format your message in following way: "},
                    }
                },
                { LangString.OrganizationalMessageFullPart2, new Dictionary<Language, string>()
                    {
                        { Language.PL, " tutaj_wpisz_swój_adres` w przeciwnym wypadku nie będziesz brać udziału w zabawie, " +
                                       "w wiadomości zwróć uwagę na poprawne wpisanie numeru z tej wiadomości. " +
                                       "Po otrzymaniu prezentu, zachęcam do wrzucenia zdjęć na imgur.com a następnie udostępnienie ich przy pomocy prywatnej komendy skierowanej do bota: "},
                        { Language.EN, " here_write_your_address` or you'll not take part in gift exchange, " +
                                       "in a message note to write given number from this message properly. " +
                                       "After getting your gift, I encourage you to put pictures on imgur.com and then share them with us by sending message to bot formatted like this: "},
                    }
                },
                { LangString.MissingAddresses, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Brakujące adresy" },
                        { Language.EN, "Missing addresses" },
                    }
                },
                { LangString.FilledAddresses, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Wypełnione adresy" },
                        { Language.EN, "Filled addresses" },
                    }
                },
                { LangString.Pictures, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Zdjęcia" },
                        { Language.EN, "Pictures" },
                    }
                },
                { LangString.Missing, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Brak" },
                        { Language.EN, "Missing" },
                    }
                },
                { LangString.UserFilledAddressResponse, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Dzięki za wypełnienie swojego adresu, sprawdź proszę w wiadomości organizacyjnej czy wszystko się zaktualizowało, w razie problemów napisz do autora bota" },
                        { Language.EN, "Thanks for filling your address for gift exchange, please check organizational message to see if everything updated, in case of problems reach out to bot author" },
                    }
                },
                { LangString.UserSentPicturesResponse, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Dzięki za udostępnienie swoich zdjęć, mam nadzieję, że zabawa była wspaniała, możesz sprawdzić swoje oraz inne zdjęcia w wiadomości organizacyjnej zabawy" },
                        { Language.EN, "Thanks for sharing your pictures, I hope that was fun, you can check yours and others pictures in organizational message of this exchange" },
                    }
                },
                { LangString.Yes, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Tak" },
                        { Language.EN, "Yes" },
                    }
                },
                { LangString.No, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Nie" },
                        { Language.EN, "No" },
                    }
                },
                { LangString.ExchangeRolled, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Wymiana rozlosowana" },
                        { Language.EN, "Exchange rolled" },
                    }
                },
                { LangString.GifterMessagePart1, new Dictionary<Language, string>()
                    {
                        { Language.PL, "Hej, właśnie wylosowałam pary do wymiany prezentów z serwera: " },
                        { Language.EN, "Hi, I just drew pairs for gift exchange on server: " },
                    }
                },
                { LangString.GifterMessagePart2, new Dictionary<Language, string>()
                    {
                        { Language.PL, " tobie został przydzielony użytkownik: " },
                        { Language.EN, " you've been assigned user: " },
                    }
                },
                { LangString.GifterMessagePart3, new Dictionary<Language, string>()
                    {
                        { Language.PL, " jego/jej adres to: " },
                        { Language.EN, " his/her address is: " },
                    }
                },
                { LangString.GifterMessagePart4, new Dictionary<Language, string>()
                    {
                        { Language.PL, " więcej informacji z czasem do kiedy trzeba wysłać prezent znajduje się w wiadomości o wymianie do której się zapisałeś/aś. W przypadku braku adresu w tej wiadomości lub jeśli jest on nieprawidłowy skontaktuj się z administratorem wspomnianego serwera lub z autorem tego bota." },
                        { Language.EN, " more information about when is a deadline for sending package you can find in a message you've signed in with. In case of address not showing up in this message or in case of incorrect address, please contect mentioned server administrator or bot owner." },
                    }
                },
            };
        }
    }
}
