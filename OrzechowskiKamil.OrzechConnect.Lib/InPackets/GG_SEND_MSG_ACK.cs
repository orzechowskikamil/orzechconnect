
namespace OrzechowskiKamil.OrzechConnect.Lib.InPackets
{
	public enum ReceiverReceivedMessageAckStatuses
	{
		/// <summary>
		/// Wiadomości nie przesłano (zdarza się przy wiadomościach zawierających adresy internetowe blokowanych przez 
		/// serwer GG gdy odbiorca nie ma nas na liście)
		/// </summary>
		Blocked = 0x0001,
		/// <summary>
		/// Wiadomość dostarczono
		/// </summary>
		Delivered = 0x0002,
		/// <summary>
		/// Wiadomość zakolejkowano
		/// </summary>
		Queued = 0x000,
		/// <summary>
		/// Wiadomości nie dostarczono. Skrzynka odbiorcza na serwerze jest pełna (20 wiadomości maks). 
		/// Występuje tylko w trybie offline
		/// </summary>
		MboxFull = 0x0004,
		/// <summary>
		/// Wiadomości nie dostarczono. Odpowiedź ta występuje tylko w przypadku wiadomości klasy GG_CLASS_CTCP
		/// </summary>
		NotDelivered = 0x0006
	}
	/// <summary>
	/// GG_SEND_MSG_ACK.
	/// Pakiet odsyłany gdy odbiorca odbierze wiadomosc lub zostanie zakolejkowana na serwerze.
	/// </summary>
	public class GG_SEND_MSG_ACK : InTcpPacket
	{
		/// <summary>
		/// Stan wiadomosci
		/// </summary>
		public ReceiverReceivedMessageAckStatuses status;
		/// <summary>
		/// Numer odbiorcy
		/// </summary>
		public uint recipient;
		/// <summary>
		/// Numer sekwencyjny wiadomosci
		/// </summary>
		public uint seq;

		protected override void readFromBytesArray( Internals.NullTerminatedBinaryReader reader, uint packetContentLength )
		{
			this.status = (ReceiverReceivedMessageAckStatuses) reader.ReadUInt32();
			this.recipient = reader.ReadUInt32();
			this.seq = reader.ReadUInt32();
		}
	}
}
