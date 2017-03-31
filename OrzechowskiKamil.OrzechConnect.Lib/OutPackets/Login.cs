using System;
using System.Security.Cryptography;
using System.Text;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;

namespace OrzechowskiKamil.OrzechConnect.Lib.OutPackets
{

	public enum LoginFeature
	{
		/// <summary>
		/// Klient obsługuje statusy graficzne i GG_STATUS_DESCR_MASK (patrz Zmiana stanu)
		/// </summary>
		GraphicStatesEnabled = 0x20,
		/// <summary>
		/// Klient obsługuje dodatkowe informacje o liście kontaktów
		/// </summary>
		AdditionalDataAboutContactsEnabled = 0x200,
		/// <summary>
		/// Klient zobowiazuje sie wysylac potwierdzenia otrzymania wiadomosci
		/// </summary>
		ReceiveConfirmationSendingEnable = 0x400,
		/// <summary>
		/// Klient obsługuje powiadomienia o pisaniu
		/// </summary>
		SendingInformationAboutWritingEnable = 0x2000,
		/// <summary>
		/// Klient obsługuje multilogowanie
		/// </summary>
		MultiLoggingEnabled = 0x00004000
	}

	enum LoginFeatureReadOnly
	{
		/// <summary>
		/// Rodzaj pakietu informującego o zmianie stanu kontaktów (patrz bit 2)
		/// 0 — GG_STATUS77, GG_NOTIFY_REPLY77
		/// 1 — GG_STATUS80BETA, GG_NOTIFY_REPLY80BETA
		/// </summary>
		ChangeContactStatePacketNewTypeEnabled = 0x1,
		/// <summary>
		/// Rodzaj pakietu z otrzymają wiadomością
		/// 0 — GG_RECV_MSG
		/// 1 — GG_RECV_MSG80
		/// </summary>
		MessageReplyNewTypeEnabled = 0x2,
		/// <summary>
		/// Rodzaj pakietu informującego o zmianie stanu kontaktów (patrz bit 0)
		/// 0 — wybrany przez bit 0
		/// 1 — GG_STATUS80, GG_NOTIFY_REPLY80
		/// </summary>
		ChangeContactStateTypeNewEnabled2 = 0x4,
		/// <summary>
		/// Klient obsługuje statusy "nie przeszkadzać" i "poGGadaj ze mną"
		/// </summary>
		DontDisturbAndPleaseGGWithMeEnabled = 0x10,
		/// <summary>
		/// Znaczenie nie jest znane, ale klient otrzyma w przypadku błędnego hasła pakiet GG_LOGIN80_FAILED zamiast
		/// GG_LOGIN_FAILED
		/// </summary>
		ErrorPacket80Enabled = 0x40,
		/// <summary>
		/// Znaczenie nie jest znane ale jest uzywany przez nowe klienty
		/// </summary>
		Unknown1 = 0x100,

	}
	/// <summary>
	/// Wyjsciowy pakiet pozwalajacy sie zalogować. GG 8.0
	/// </summary>
	public class Login : OutTcpPacket
	{
		public Login()
		{
			// Ustawienie stałych features ktore nie powinny sie zmieniać, i są cechami klienta
			this.SetValue( LoginFeatureReadOnly.ChangeContactStatePacketNewTypeEnabled, true );
			this.SetValue( LoginFeatureReadOnly.ChangeContactStateTypeNewEnabled2, true );
			this.SetValue( LoginFeatureReadOnly.DontDisturbAndPleaseGGWithMeEnabled, true );
			this.SetValue( LoginFeatureReadOnly.ErrorPacket80Enabled, true );
			this.SetValue( LoginFeatureReadOnly.MessageReplyNewTypeEnabled, true );
			this.SetValue( LoginFeatureReadOnly.Unknown1, true );
		}

		#region stałe

		/// <summary>
		/// Oznacza że wybrano sposob kodowania SHA
		/// </summary>
		private const byte HashSHA1 = 0x02;
		private const string LanguagePl = "pl";
		private const string Version10 = "Gadu-Gadu Client build 10.0.0.10450";
		private const string Version8 = "Gadu-Gadu Client build 8.0.0.7669";
		private const int MinimalFeatures = 0x7;
		private const int NormalFeatures = 0x00000367;

		#endregion

		#region impementacja abstraktów
		protected override uint packetCode
		{
			get { return ( uint ) OutPacketsCodes.Login; }
		}


		protected override void writeToBytesArray( NullTerminatedBinaryWriter writer )
		{
			// Gdzieniegdzie dla czytelnosci są rzutowania ktore są zbedne, ale mogą pomóc uniknąć pomyłek.
			writer.Write( ( uint ) GGNumber ); //int
			writer.Write( Language ); // byte[2]
			writer.Write( ( byte ) HashType ); // byte
			writer.Write( hashValue ); // byte[64] 
			writer.Write( ( uint ) InitialStatus ); // int
			writer.Write( ( uint ) flags ); // int
			writer.Write( ( uint ) features ); // int
			writer.Write( ( uint ) 0 ); // 
			writer.Write( ( ushort ) 0 );// te tutaj to pozostalosci po starym protokole.
			writer.Write( ( uint ) 0 );//
			writer.Write( ( ushort ) 0 );//
			writer.Write( ( byte ) ImageSizeInKB );
			writer.Write( ( byte ) 0x64 ); // unknown
			writer.Write( ( uint ) VersionLength );
			writer.Write( Version );
			writer.Write( ( uint ) InitialStatusDescriptionLength );
			writer.Write( InitialStatusDescription );
		}


		#endregion

		#region Todo

		//// TODO dokonczyc Bity features. Na razie features są zahardkodowane.
		//// 0 - Rodzaj pakietu informującego o zmianie stanu kontaktów (patrz bit 2)
		//// 0 — GG_STATUS77, GG_NOTIFY_REPLY77
		//// 1 — GG_STATUS80BETA, GG_NOTIFY_REPLY80BETA
		//private bool contactChangeOption0 { get { return true; } }
		//// 1 - Rodzaj pakietu z otrzymają wiadomością
		//// 0 — GG_RECV_MSG
		//// 1 — GG_RECV_MSG80
		//private bool receivedMessageOption1 { get { return true; } }
		//// 2 - Rodzaj pakietu informującego o zmianie stanu kontaktów (patrz bit 0)
		//// 0 — wybrany przez bit 0
		//// 1 — GG_STATUS80, GG_NOTIFY_REPLY80
		//private bool contactChangeOption2 { get { return true; } }
		//// 4- Klient obsługuje statusy "nie przeszkadzać" i "poGGadaj ze mną"
		//private bool newStatusesAccepted4 { get { return true; } }
		//// 5 - Klient obsługuje statusy graficzne i GG_STATUS_DESCR_MASK (patrz Zmiana stanu)
		////6	0x00000040	Znaczenie nie jest znane, ale klient otrzyma w przypadku błędnego hasła pakiet GG_LOGIN80_FAILED 
		//// zamiast GG_LOGIN_FAILED
		////7	0x00000100	Znaczenie nie jest znane, ale jest używane przez nowe klienty
		////9	0x00000200	Klient obsługuje dodatkowe informacje o liście kontaktów
		////10	0x00000400	Klient wysyła potwierdzenia odebrania wiadomości
		////13	0x00002000	Klient obsługuje powiadomienia o pisaniu
		////13	0x00004000	Klient obsługuje multilogowanie
		#endregion

		#region inne pola i procedury

		/// <summary>
		/// Oblicza hash potrzebny do zalogowania sie na serwer z hasla i z seedu
		/// </summary>
		/// <param name="password"></param>
		/// <param name="seed"></param>
		/// <returns></returns>
		private byte[] getHash( string password, uint seed )
		{
			var passwordBytes = Encoding.UTF8.GetBytes( password );
			var seedBytes = BitConverter.GetBytes( seed );
			var bytes = new byte[passwordBytes.Length + seedBytes.Length];
			passwordBytes.CopyTo( bytes, 0 );
			seedBytes.CopyTo( bytes, passwordBytes.Length );
			var sha1Hash = new SHA1Managed();
			var hashInBytes = sha1Hash.ComputeHash( bytes );
			var result = new byte[64];
			hashInBytes.CopyTo( result, 0 );
			return result;
		}

		/// <summary>
		/// Hasło do zalogwowania się na GG
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Wartosc Seed otrzymana w pakiecie Welcome
		/// </summary>
		public uint Seed { get; set; }

		public void SetValue( LoginFeature feature, bool value )
		{
			this.features = this.setMask( this.features, ( uint ) feature, value );
		}
		private void SetValue( LoginFeatureReadOnly feature, bool value )
		{
			this.features = this.setMask( this.features, ( uint ) feature, value );
		}
		#endregion

		#region Pola w pakiecie

		/// <summary>
		/// Numer GG sluzacy do zalogowania sie
		/// </summary>
		public int GGNumber { get; set; }
		private string Language { get { return LanguagePl; } }
		private byte HashType { get { return HashSHA1; } }
		private byte[] hashValue
		{
			get { return getHash( this.Password, this.Seed ); }
		}
		/// <summary>
		/// Co obsluguje nasz klient
		/// </summary>
		//private int features { get { return 0x00000367; } }
		private uint features { get; set; }

		private int flags { get { return 0; } }

		/// <summary>
		/// Wielkosc obrazka w KB na jakie sie zgadzamy
		/// </summary>
		public byte ImageSizeInKB { get; set; }

		/// <summary>
		/// Kod statusu początkowego po zalogowaniu
		/// </summary>
		public GGStatus InitialStatus { get; set; }

		/// <summary>
		/// Opis po zalogowaniu
		/// </summary>
		public string InitialStatusDescription { get; set; }
		private string Version { get { return Version10; } }
		private int InitialStatusDescriptionLength
		{
			get { return (this.InitialStatusDescription != null) ? this.InitialStatusDescription.Length : 0; }
		}
		private int VersionLength { get { return this.Version.Length; } }

		#endregion
	}
}
