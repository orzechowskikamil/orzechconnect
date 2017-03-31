using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;

namespace OrzechowskiKamil.OrzechConnect.Lib.InPackets
{
	/// <summary>
	/// GG_USERLIST100_REPLY
	/// Pakiet bedący odpowiedzia na UserListReques
	/// </summary>
	public class GG_USERLIST100_REPLY : InTcpPacket
	{
		/// <summary>
		/// Typy odpowiedzi
		/// </summary>
		public enum ReplyType
		{
			/// <summary>
			/// w odpowiedzi znajduje się aktualna lista kontaktów na serwerze
			/// </summary>
			GG_USERLIST100_REPLY_LIST = 0x00,
			/// <summary>
			/// potwierdzenie odebrania nowej wersji listy kontaktów
			/// </summary>
			GG_USERLIST100_REPLY_ACK = 0x10,
			/// <summary>
			/// odmowa przyjęcia nowej wersji listy kontaktów z powodu
			/// Niezgodności numeru wersji listy kontaktów
			/// </summary>
			GG_USERLIST100_REPLY_REJECT = 0x12
		}
		/// <summary>
		/// Rodzaj odppowiedzi.
		/// </summary>
		public ReplyType Type;
		/// <summary>
		/// numer wersji listy kontaktów aktualnie przechowywanej przez serwer
		/// </summary>
		public uint Version;
		/// <summary>
		///  rodzaj przesyłanego typu formatu listy kontaktów 
		/// </summary>
		public UserListFormatType FormatType;
		/// <summary>
		/// zawsze 0x01
		/// </summary>
		private byte unknown1;
		/// <summary>
		/// Tresc (nie musi wystąpić)
		/// </summary>
		public byte[] Reply;

		protected override void readFromBytesArray( Internals.NullTerminatedBinaryReader reader, uint packetContentLength )
		{
			this.Type = ( ReplyType ) reader.ReadByte();
			this.Version = reader.ReadUInt32();
			this.FormatType = ( UserListFormatType ) reader.ReadByte();
			this.unknown1 = reader.ReadByte();
			var replyLength = packetContentLength - 1 - 4 - 1 - 1;
			//this.Reply = reader.ReadNullTerminatedBytesArray();
			this.Reply = reader.ReadBytes( ( int ) replyLength );
		}
	}
}
