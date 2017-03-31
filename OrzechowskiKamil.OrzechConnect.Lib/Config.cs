using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;

namespace OrzechowskiKamil.OrzechConnect.Lib
{
	public static class Config
	{
		/// <summary>
		/// Czekanie na reszte pakietu (maxymalny czas)
		/// </summary>
		public const int WaitingForUncompletedPacketChunksInterval = 20000;
		/// <summary>
		/// Długosc wyswietlania annoucementu o nowej wiadomosci
		/// </summary>
		public const int NewMessageAnnoucementDurationSeconds = 2;
		/// <summary>
		/// Dlugosc wyswietlania annoucementu o zmianie statusu
		/// </summary>
		public const int StatusChangeAnnoucementDurationSeconds = 1;
		/// <summary>
		/// Domyslne ustawienie for friends only przy tworzeniu nowego profilu
		/// </summary>
		public const bool ForFriendsOnlyDefaultSet = false;
		/// <summary>
		/// Domyslne ustawienie mobileclient przy tworzeniu nowego profilu
		/// </summary>
		public const bool MobileClientDefaultSet = false;
		/// <summary>
		/// Domyslna wielkosc dla obrazka po tworzeniu nowego profilu
		/// </summary>
		public const int MaxImageSizeDefaultSet = 255;
		/// <summary>
		/// Domyslne dla nowego profilu revceieve links from unknowns
		/// </summary>
		public const bool ReceiveLinksFromUnknownsDefaultSet = true;
		/// <summary>
		/// Domyslny status po zalogowaniu dla nowego profilu
		/// </summary>
		public const GGStatus StatusAfterLoginDefaultSet = GGStatus.AvailableDesc;
		/// <summary>
		/// Domyslny opis po zalogowaniu dla nowego profilu
		/// </summary>
		public const string StatusDescriptionAfterLoginDefaultSet = "Korzystam z Orzech Connect na WP7!";
		/// <summary>
		/// Dozwolona ilosc prob logowania zanim da sobie spokoj
		/// </summary>
		public const int MaxTriesForLogin = 3;
		/// <summary>
		/// Dozwolony czas na logowanie w milisekundach dłuzej juz sie chyba nie da.
		/// </summary>
		public const int MaxAllowedTimeForLoginProcess = 13000;

		/// <summary>
		/// Po tylu sekundach odzaprzestania pisania lista kontaktow bedzie przefiltrowana takim filtrem 
		/// </summary>
		public const int FindContactsAfterStopWritingInSeconds = 2;
		///// <summary>
		///// Maksymalny dozwolony czas jaki jest przeznaczony na pobranie statusow kontaktow pakietem GG_NOTIFY_REPLY_80
		///// 
		///// </summary>
		//public const int MaxAllowedTimeForDownloadingContactsStatusesAfterLogin = 0;

		/// <summary>
		/// Przerwa z jaka uruchamiane sa handlery eventow OnTap (zeby mial czas sie zaznaczyc item na controlce) 
		/// (milisekundy)
		/// </summary>
		public const int DelayAfterSelectionChangedAndEventExecution = 30;

		/// <summary>
		/// Jezeli true, to app nie pozwoli otworzyc popupa z wiadomoscia jezeli popup ze statusem jest otwarty.
		/// </summary>
		public const bool HidePushPopupNotificationsWhileStatusPopupOpened = true;
		/// <summary>
		/// Nie pozwala pokazywac popupow gdy piszemy wiadomosc
		/// </summary>
		public const bool HidePushPopupNotificationsWhileWriting = true;

		/// <summary>
		/// Jezeli czas tombstoningu byl krotki nie pobiera statusów aby zaoszczedzic transfer na przypadkowych
		/// kliknieciach w softkeye (a malo prawdopodobne by sie komus zmienil wtedy opis)
		/// 
		/// Okazalo sie ze to musi byc false bo inaczej GG nie chce przysylac nam zmian statusu tych osób a wiec fiasko.
		/// </summary>
		public const bool IfTombstoningTimeIsShortDontRequestUserStatuses = false;
		/// <summary>
		/// Do tylu sekund nie bedzie pobieralo statusow jezeli powyzsza opcja jest aktywna
		/// </summary>
		public const int HowMuchSecondsOfTombstoningIsShortTime = 10;

		/// <summary>
		/// Dlugosc wibracji przy otrzymaniu nowej wiadomosci
		/// </summary>
		public const int VibrationLengthOnNewMessage = 80;
		/// <suC:\Users\Orzech\Documents\Visual Studio 2010\Projects\Metro Gadu(7)\Models\Config.csmmary>
		/// Jak duzo ciemny akcent jest ciemniejszy od jasnego akcentu
		/// </summary>
		public const int HowMuchDarkerAccentIsDarker = 60;
	}

	/// <summary>
	/// stringi uzywane w apce, nie chce mi sie korzystac z resources dla jednego języka...
	/// Oczywiscie nie wszystkie stringi tu są, niektore sa zaszyte w xamlu ale przynajmniej nic nie trzeba szukac
	/// po kodzie.
	/// </summary>
	public static class Strings
	{
		//public const string             =   "          ";
		//public const string             =   "          ";
		//public const string             =   "          ";
		//public const string             =   "          ";
		//public const string             =   "          ";
		//public const string             =   "          ";
		//public const string             =   "          ";
		//public const string             =   "          ";
		//public const string             =   "          ";
		//public const string             =   "          ";
		//public const string             =   "          ";
		public const string Potwierdź_title_usunięcie_profilu = "Potwierdź";
		public const string Czy_jesteś_pewien_że_chcesz_usunąć_profil =
			"Czy jesteś pewien że chcesz usunąć dany profil użytkownika?";
		public const string Potwierdź = "Potwierdź";
		public const string Czy_jesteś_pewien_że_chcesz_usunąć_dany_kontakt = "Czy jesteś pewien że chcesz usunąć dany kontakt?";
		public const string Zmieniles_swoj_status = "Zmieniłeś swój status na {0}.";
		public const string Taki_kontakt_juz_istnieje = "Kontakt o tym numerze już istnieje na twojej liście, pod nazwą {0}.";
		public const string Błędny_login_lub_hasło = "Błędny login lub hasło";
		public const string Brak_połączenia_z_internetem = "Brak połączenia z internetem";
		public const string Profil_już_istnieje = "Profil już istnieje.";
		public const string Błąd_w_edycji_profilu = "Błąd w edycji profilu: ";
		public const string Nazwa_profilu_nie_może_być_pusta = "Nazwa profilu nie może być pusta.";
		public const string Hasło_nie_może_być_puste = "Hasło nie może być puste";
		public const string Dostępny = "Dostępny";
		public const string Zablokowany = "Zablokowany";
		public const string Zaraz_wracam = "Zaraz wracam";
		public const string Nie_przeszkadzać = "Nie przeszkadzać";
		public const string Niewidoczny = "Niewidoczny";
		public const string Niedostępny = "Niedostępny";
		public const string Pogadaj_ze_mną = "Pogadaj ze mną";
		public const string Trwa_wznawianie_połączenia_z_serwerem = "Czekaj, trwa wznawianie połączenia z serwerem";
		public const string Połączenie_zerwane_zaloguj_ponownie = "Połączenie z serwerem zostało zerwane. Zaloguj się ponownie.";
		public const string Na_pewno_chcesz_wyjść = "Czy na pewno chcesz wyjśc? Jeśli wyjdziesz, zostaniesz wylogowany.";
		public const string Potwierdzenie_tytuł = "Potwierdzenie";
		public const string Ktoś_przesyła_ci_wiadomość = "{0} przesyła ci wiadomość.";
		public const string KtosZmienilSwojStatusPowiadomienie = "{0} zmienił swój status na {1}.";
		public const string SzukajAppBarLabel = "szukaj";
		public const string KontaktyAppBarLabel = "kontakty";
		public const string WyslijAppBarLabel = "wyslij";
		public const string LinkDoGaduGadu = "https://login.gadu-gadu.pl/account/register";
		public const string TekstNaLinkuDoGaduGadu = "Naciśnij tutaj by założyć konto GG";
		public const string UtworzKontoTekst = "Do korzystania z komunikatora Orzech Connect potrzebujesz konta na jednej z obsługiwanych przez komunikator sieci Instant Messaging. Na razie obsługiwane jest jedynie GG. Konto możesz założyć używając linku poniżej. ";
		public const string MessageBoxProbaUtworzeniaDwochProfiliOTymSamymNumerze = "Nie można utworzyć dwóch profili użytkownika o takim samym numerze Gadu Gadu. Profil o numerze {0} już istnieje. Przewiń w lewo by go użyć.";
		public const string MessageBoxTitleProbaUtworzeniaDwochProfiliOTymSamymNumerze = "Błąd";
		public const string NumerGGMozeBycZapisanyTylkoZaPomocaCyfrException = "Numer GG musi byc zapisany tylko i wyłącznie za pomocą cyfr.";
		public const string NazwaKontaTextBoxLabel = "Nazwa konta";
		public const string NumerGGTextBoxLabel = "Numer GG";
		public const string HasloGGTextBoxLabel = "Hasło GG";
		public const string LogowanieKomunikatProgressBar = "Logowanie...";
		public const string SerwerNieOdpowiadaSprobojPozniej = "Serwer nie odpowiada. Spróbuj się zalogować ponownie później.";
		public const string NieMoznaZalogowacSieDoProfilu = "Nie można się zalogować do danego profilu. {0}.";
		public const string LogowanieTrwaloZbytDlugoIZostaloPrzerwane = "Logowanie trwało zbyt długo i zostało przerwane. Spróbuj ponownie.";
		public const string PoleNieMozeBycPuste = "Pole \"{0}\" nie może być puste.";
		public const string CofnijUstawienieJakoDomyslnyProfilMenu = "Cofnij ustawienie jako domyślny profil";
		public const string UstawJakoDomyslnyProfilMenu = "Ustaw jako domyślny profil";
		public const string ProfilZostalUtworzony = "Profil został utworzony.";
		//public const string NieprzeczytaneWiadomosciAnnoucementOnPivotHeader = "Nieprzeczytanych: {0}.";
	}
}
