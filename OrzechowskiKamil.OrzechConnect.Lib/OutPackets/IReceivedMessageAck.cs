
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
namespace OrzechowskiKamil.OrzechConnect.Lib.OutPackets
{
	/// <summary>
	///  GG_RECV_MSG_ACK
	/// Odsyłam jak odbiore wiadomosc.
	/// </summary>
	public class IReceivedMessageAck : OutTcpPacket
	{
		public uint SequenceNumberOfReceivedMessage;

		protected override uint packetCode
		{
			get { return 0x0046; }
		}

		protected override void writeToBytesArray( NullTerminatedBinaryWriter writer )
		{
			writer.Write( this.SequenceNumberOfReceivedMessage );
		}
	}
}
