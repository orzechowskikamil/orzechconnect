using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;

namespace OrzechowskiKami.MetroGaduLib.Internals
{
	public class GG_PONG : OutTcpPacket
	{

		protected override uint packetCode
		{
			get { return 0x0007; }
		}

		protected override void writeToBytesArray( NullTerminatedBinaryWriter writer )
		{

		}
	}
}
