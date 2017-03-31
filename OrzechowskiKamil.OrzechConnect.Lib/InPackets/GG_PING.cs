
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
namespace OrzechowskiKamil.OrzechConnect.Lib.InPackets
{
	/// <summary>
	/// 0x0008	GG_PING
	/// Wysyłany przez niektóre wersje serwera w celu odpytania czy klient jest połączony
	/// </summary>
	public class GG_PING : InTcpPacket
	{

		protected override void readFromBytesArray( NullTerminatedBinaryReader reader,
			uint packetContentLength )
		{  

		}
	}
}
