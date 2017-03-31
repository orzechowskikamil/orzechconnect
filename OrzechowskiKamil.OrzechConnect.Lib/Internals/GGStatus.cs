
namespace OrzechowskiKamil.OrzechConnect.Lib.Internals
{
	/// <summary>
	/// Są tu możliwe statusy.
	/// </summary>
	public enum GGStatus
	{
		NotAvailable = 0x0001, NotAvailableDesc = 0x0015,
		PleaseGGWithMe = 0x0017, PleaseGGWithMeDesc = 0x0018,
		Available = 0x0002, AvailableDesc = 0x0004,
		Brb = 0x0003, BrbDesc = 0x0005,
		DontDisturb = 0x0021, DontDisturbDesc = 0x022,
		Invisible = 0x0014, InvisibleDesc = 0x0016,
		Blocked = 0x0006,
		NoSetting=0
	}

	public class GGStatusHelp
	{
		/// <summary>
		/// Metoda zamienia dowolny gg status na jego odpowiednik bez Desc dzieki czemu nie trzeba
		/// wszystkiego pisac 2 razy.
		/// </summary>
		/// <param name="statusToConvert"></param>
		/// <returns></returns>
		public static GGStatus GGStatusToGGStatusWithoutDesc( GGStatus statusToConvert )
		{
			switch (statusToConvert)
			{
				case GGStatus.InvisibleDesc: return GGStatus.Invisible;
				case GGStatus.AvailableDesc: return GGStatus.Available;
				case GGStatus.BrbDesc: return GGStatus.Brb;
				case GGStatus.DontDisturbDesc: return GGStatus.DontDisturb;
				case GGStatus.NotAvailableDesc: return GGStatus.NotAvailable;
				case GGStatus.PleaseGGWithMeDesc: return GGStatus.PleaseGGWithMe;
				default: return statusToConvert;
			}
		}
	}
	/// <summary>
	/// Maski nakladane na statusy.
	/// </summary>
	public enum GGStatusMasks
	{
		/// <summary>
		/// Maska bitowa oznaczająca ustawiony opis graficzny (tylko odbierane)
		/// </summary>
		ImageStatusMask = 0x0100,
		/// <summary>
		/// Maska bitowa informująca serwer, że jeśli istnieje już inne połączenie na tym numerze to nasze ma 
		/// przyjać jego stan (podany przez nas zostanie zignorowany). Jeśli połączenia innego nie ma, to 
		/// ustawiany jest stan podany przez nas.
		/// </summary>
		AdaptToAlreadySettedStatusByOtherClientMask = 0x0400,
		/// <summary>
		/// 	Maska bitowa oznaczająca tryb tylko dla przyjaciół
		/// </summary>
		OnlyForFriendsMask = 0x8000,
		/// <summary>
		/// Jeśli klient obsługuje statusy graficzne, to statusy opisowe będą dodatkowo określane przez dodanie tej flagi.
		/// Dotyczy to zarówno statusów wysyłanych, jak i odbieranych z serwera. 
		/// Musisz to dodac zeby sie wyswietlil opis tekstowy.
		/// </summary>
		StatusDescriptionIsSetMask = 0x4000
	}
}
