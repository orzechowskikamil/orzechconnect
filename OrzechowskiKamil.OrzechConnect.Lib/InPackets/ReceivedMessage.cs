using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using System;
namespace OrzechowskiKamil.OrzechConnect.Lib.InPackets
{
	/// <summary>
	/// Pakiet enkapsulujący nadchodzącą wiadomosc od serwera.
	/// </summary>
	public class GG_RECV_MSG80 : InTcpPacket
	{

		public uint Sender;
		public uint SequenceNumber;
		public uint Time;
		private uint messageClass;
		public uint PlainMessageOffset;
		public uint PlainMessageAttributes;
		public string HtmlMessage;
		public string PlainMessage;
		public string Attributes;

		public bool GetValue( MessageClass flag )
		{
			return this.GetValue( this.messageClass, ( uint ) flag );
		}

		protected override void readFromBytesArray( NullTerminatedBinaryReader reader, uint packetContentLength )
		{
			this.Sender = reader.ReadUInt32();
			this.SequenceNumber = reader.ReadUInt32();
			this.Time = reader.ReadUInt32();
			this.messageClass = reader.ReadUInt32();
			this.PlainMessageOffset = reader.ReadUInt32();
			this.PlainMessageAttributes = reader.ReadUInt32();
			this.HtmlMessage = reader.ReadNullTerminatedString();
			this.PlainMessage = "";
			this.Attributes = "";
			try
			{
				this.PlainMessage = reader.ReadNullTerminatedString();
				this.Attributes = reader.ReadNullTerminatedString();
			}
			catch (Exception) { }
		}
	}
}
