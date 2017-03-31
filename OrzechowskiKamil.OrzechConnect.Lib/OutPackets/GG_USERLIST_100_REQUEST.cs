
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
namespace OrzechowskiKamil.OrzechConnect.Lib.OutPackets
{
	/// <summary>
	/// Format listy kontaktów
	/// </summary>
	public enum UserListFormatType
	{
		/// <summary>
		/// brak treści listy kontaktów
		/// </summary>
		GG_USERLIST100_FORMAT_TYPE_NONE = 0x00,
		/// <summary>
		/// format listy kontaktów zgodny z Gadu-Gadu 7.0 
		/// </summary>
		GG_USERLIST100_FORMAT_TYPE_GG70 = 0x01,
		/// <summary>
		/// format listy kontaktów zgodny z Gadu-Gadu 10.0
		/// </summary>
		GG_USERLIST100_FORMAT_TYPE_GG100 = 0x02
	}

	/// <summary>
	/// Pakiet sluzacy do wysylania listy kontaktow na serwer lub pobierania ich z serwera
	/// TODO trzeba zrobic refaktoryzacje i wylaczyc wspolna czesc bo pakiet wejsciowy i wyjsciowy to to samo. ale juz nie dzis.
	/// </summary>
	public class GG_USERLIST100_REQUEST : OutTcpPacket
	{
		/// <summary>
		/// Co do zrobienia z listą kontaktów
		/// </summary>
		public enum RequestType
		{
			/// <summary>
			/// Eksport listy
			/// </summary>
			GG_USERLIST100_PUT = 0x00,
			/// <summary>
			/// Import listy
			/// </summary>
			GG_USERLIST100_GET = 0x02
		}

		/// <summary>
		/// Rodzaj zapytania - pobierz/ wyslij (get/put)
		/// </summary>
		public RequestType ListRequestType;
		/// <summary>
		/// Numer wersji ostatniej listy kontaktów którą otrzymalismy, bądź zero 
		/// (zapewne sluzy do tego by wyslac tylko zmiany)
		/// </summary>
		public uint LastVersionNumberOfUserList;
		/// <summary>
		/// Format listy kontaktów
		/// </summary>
		public UserListFormatType ListFormatType;
		/// <summary>
		/// zawsze 0x01
		/// </summary>
		byte unknown1 { get { return 0x01; } }
		/// <summary>
		/// Tresc (nie musi wystąpić)
		/// </summary>
		public byte[] Content;
		protected override uint packetCode
		{
			get { return 0x0040; }
		}

		protected override void writeToBytesArray( NullTerminatedBinaryWriter writer )
		{
			writer.Write( ( byte ) this.ListRequestType );
			writer.Write( ( uint ) this.LastVersionNumberOfUserList );
			writer.Write( ( byte ) this.ListFormatType );
			writer.Write( ( byte ) this.unknown1 );
			if (Content != null)
			{
				writer.Write( Content );
			}
		}
	}
}
