using System.IO;
using OrzechowskiKamil.OrzechConnect.Lib.Connection;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using System;

namespace OrzechowskiKamil.OrzechConnect.Lib.InPackets
{
	public interface IInTcpPacket : ITcpStruct
	{
	}

	public interface IUnserializableFromGGByteArray
	{
		void UnserializeFromByteArray( NullTerminatedBinaryReader reader, uint packetContentLength );
	}
	
	public interface IInTcpPacketUsingGlobalFactory : IUnserializableFromGGByteArray, IInTcpPacket { }

	/// <summary>
	/// Baza dla wszelkich pakietów przychodzących do tego klienta.
	/// </summary>
	abstract public class InTcpPacket : TcpStruct, IInTcpPacketUsingGlobalFactory
	{
		public const int HeaderSize = 8;
		/// <summary>
		/// Tworzy nową instancje pakietu w zaleznosci od jego sygnatury (kodu)
		/// </summary>
		/// <param name="code"></param>
		/// <returns></returns>
		private static IInTcpPacketUsingGlobalFactory getPacketInstanceFromCode( uint code )
		{

			switch (( InPacketsCodes ) code)
			{
				case InPacketsCodes.GG_LOGIN_OK80: return ( IInTcpPacketUsingGlobalFactory ) new GG_LOGIN_OK80();
				case InPacketsCodes.GG_LOGIN_FAIL_2:
				case InPacketsCodes.GG_LOGIN_FAIL: return ( IInTcpPacketUsingGlobalFactory ) new GG_LOGIN_FAIL();
				case InPacketsCodes.GG_WELCOME: return ( IInTcpPacketUsingGlobalFactory ) new GG_WELCOME();
				case InPacketsCodes.GG_RECV_MSG80: return ( IInTcpPacketUsingGlobalFactory ) new GG_RECV_MSG80();
				case InPacketsCodes.GG_USER_DATA: return ( IInTcpPacketUsingGlobalFactory ) new GG_USER_DATA();
				case InPacketsCodes.GG_NOTIFY_REPLY: return ( IInTcpPacketUsingGlobalFactory ) new GG_NOTIFY_REPLY_80();
				case InPacketsCodes.GG_SEND_MSG_ACK: return ( IInTcpPacketUsingGlobalFactory ) new GG_SEND_MSG_ACK();
				case InPacketsCodes.GG_TYPING_NOTIFY: return ( IInTcpPacketUsingGlobalFactory ) new GG_TYPING_NOTIFY();
				case InPacketsCodes.GG_PING: return ( IInTcpPacketUsingGlobalFactory ) new GG_PING();
				case InPacketsCodes.GG_OWN_MESSAGE: return ( IInTcpPacketUsingGlobalFactory ) new GG_OWN_MESSAGE();
				case InPacketsCodes.GG_DISCONNECTING: return ( IInTcpPacketUsingGlobalFactory ) new GG_DISCONNECTING();
				case InPacketsCodes.GG_USERLIST100_REPLY: return ( IInTcpPacketUsingGlobalFactory ) new GG_USERLIST100_REPLY();
				case InPacketsCodes.GG_STATUS80: return ( IInTcpPacketUsingGlobalFactory ) new GG_STATUS80();
				// nieuzywane, ale musza byc rozpoznawane aby program byl odporniejszy na bledy w transferze danych
				case InPacketsCodes.GG_DCC7_ABORTED:
				case InPacketsCodes.GG_DCC7_ACCEPT:
				case InPacketsCodes.GG_DCC7_ID_REPLY:
				case InPacketsCodes.GG_DCC7_INFO:
				case InPacketsCodes.GG_DCC7_NEW:
				case InPacketsCodes.GG_DCC7_REJECT:
				case InPacketsCodes.GG_DISCONNECT_ACK:
				case InPacketsCodes.GG_NEED_EMAIL:
				case InPacketsCodes.GG_OWN_RESOURCE_INFO:
				case InPacketsCodes.GG_PUBDIR50_REPLY:
				case InPacketsCodes.GG_USERLIST100_VERSION:
				case InPacketsCodes.GG_XML_ACTION:
				case InPacketsCodes.GG_XML_EVENT:
				return ( IInTcpPacketUsingGlobalFactory ) new GG_UNUSED_PACKET();

			}

			return null;
		}
		/// <summary>
		/// Factory Method: tworzy nowy obiekt pakietu z danych odebranych z socketu
		/// </summary>
		/// <param name="data">Tablica bajtow z ktorej pakiet ma byc utworzony</param>
		/// <param name="isUnfinished">jezeli zostanie ustawione na true to znaczy ze paczka danych jest 
		/// niekompletna by utworzyc pakiet i nalezy do niej dokleic nastepna paczke</param>
		/// <param name="gluedData"></param>
		public static IInTcpPacketUsingGlobalFactory CreatePacket( byte[] data, out bool isUnfinished, out byte[] gluedData )
		{
			isUnfinished = false;
			gluedData = null;
			var bytesTransferred = (data != null) ? data.Length : 0;
			var stream = new MemoryStream( data );
			var reader = new NullTerminatedBinaryReader( stream );
			var packetCode = ( uint ) reader.ReadUInt32();
			var packetContentLength = ( uint ) reader.ReadUInt32();
			// stworz instancje obiektu pakietu
			var newPacketObject = getPacketInstanceFromCode( packetCode );
			if (newPacketObject == null)
			{
				// jezeli nasz socket nie rozpoznaje w ogole takiego typu pakietu, to bardzo prawdopodobne
				// ze cos po drodze sie urwało, zostało przemielone i tu trafilo a pod packtCode i co gorsza,
				// pod packetContentLenght znajduja sie kompletnie przypadkowe wartosci. 
				// Istnieje spore ryzyko że pod packetLength bedzie np 9 milionow i wszystkie pakiety
				// beda doklejane do niego dopoki nie przekroczy tego rozmiaru skutkiem czego ta instancja juz sie nigdy
				// z niczym nie połączy.
				// Odrzucam taką chujową porcje danych.
				return null;
			}
			var declaredByPacketByteSize = packetContentLength + InTcpPacket.HeaderSize;
			var isThisPacket = data[0] == 55;
			if (declaredByPacketByteSize > bytesTransferred)
			{
				isUnfinished = true;
				return null;
			}
			// jezeli zadeklarowany pakiet jest mniejszy niz ilosc przeslanych danych to znaczy ze pakiety sie s
			// skleiły
			else if (declaredByPacketByteSize < bytesTransferred)
			{
				// pierdolone sockety czasami potrafia skleic 2 pakiety w jeden! trzeba je recznie rozdzielac!
				// zwracam je na zewnatrz za pomocą parametru glued data
				var gluedDataLength = bytesTransferred - ( int ) declaredByPacketByteSize;
				var gluedDataStart = ( int ) declaredByPacketByteSize;
				gluedData = new byte[gluedDataLength];
				Array.Copy( data, gluedDataStart, gluedData, 0, gluedDataLength );
				// funkcja wyzej sobie te glued data jakos tam obsuzy
			}

			if (newPacketObject != null)
			{
				newPacketObject.UnserializeFromByteArray( reader, packetContentLength );
			}
			return newPacketObject;
		}

		abstract protected void readFromBytesArray( NullTerminatedBinaryReader reader, uint packetContentLength );



		public void UnserializeFromByteArray( NullTerminatedBinaryReader reader, uint packetContentLength )
		{
			this.readFromBytesArray( reader, packetContentLength );
			reader.Close();
		}
	}



	/// <summary>
	/// Sygnatury typów pakietów 
	/// </summary>
	public enum InPacketsCodes
	{
		GG_DCC7_INFO = 0x001f,
		GG_DCC7_NEW = 0x0020,
		GG_DCC7_ACCEPT = 0x0021,
		GG_DCC7_REJECT = 0x0022,
		GG_DCC7_ID_REPLY = 0x0023,
		GG_DCC7_ABORTED = 0x0025,
		GG_XML_EVENT = 0x0027,
		GG_DISCONNECT_ACK = 0x000d,
		GG_PUBDIR50_REPLY = 0x000e,
		/// <summary>
		/// Logowanie sie powiodlo ale powinnismy uzupelnic adres email w katalogu publicznym
		/// </summary>
		GG_NEED_EMAIL = 0x0014,
		GG_XML_ACTION = 0x002c,
		GG_WELCOME = 0x0001,
		GG_LOGIN_OK80 = 0x0035,
		/// <summary>
		/// Login fail
		/// </summary>
		GG_LOGIN_FAIL = 0x0043,
		/// <summary>
		/// Login fail (nie wiadomo ktory jest dobry)
		/// </summary>
		GG_LOGIN_FAIL_2 = 0x0009,
		/// <summary>
		/// Odebrana wiadomosc
		/// </summary>
		GG_RECV_MSG80 = 0x2e,
		GG_NOTIFY_REPLY = 0x0037,
		/// <summary>
		/// Dodatkowe dane o uzytkowniku
		/// </summary>
		GG_USER_DATA = 0x0044,
		GG_SEND_MSG_ACK = 0x0005,
		/// <summary>
		/// Informacja (pisak)
		/// </summary>
		GG_TYPING_NOTIFY = 0x0059,
		/// <summary>
		/// Wiadomosc ktora wyslales na innym kliencie z tego numeru
		/// </summary>
		GG_OWN_MESSAGE = 0x005A,
		GG_PING = 0x0008,
		GG_DISCONNECTING = 0x000b,
		GG_USERLIST100_REPLY = 0x0041,
		GG_STATUS80 = 0x0036,
		/// <summary>
		/// Informacja o nowej wersji listy kontaktów
		/// </summary>
		GG_USERLIST100_VERSION = 0x005C,
		/// <summary>
		/// Informacja o innych polaczeniach  na ten numer
		/// </summary>
		GG_OWN_RESOURCE_INFO = 0x005B,
	}


}
