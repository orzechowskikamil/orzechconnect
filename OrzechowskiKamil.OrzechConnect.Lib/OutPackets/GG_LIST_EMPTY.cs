
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
namespace OrzechowskiKamil.OrzechConnect.Lib.OutPackets
{

	/// <summary>
	/// Pakiet ten jest wysylany do serwera po zalogowaniu jesli nie mamy zadnych kontaktów na liscie.
	/// </summary>
	public class GG_LIST_EMPTY : OutTcpPacket
	{

		protected override uint packetCode
		{
			get { return (uint)OutPacketsCodes.ListEmpty; }
		}

		protected override void writeToBytesArray( NullTerminatedBinaryWriter writer )
		{
			//pakiet zerowej dlugosci
		}
	}
}
