
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
namespace OrzechowskiKamil.OrzechConnect.Lib.OutPackets
{
	/// <summary>
	/// Odsylam jak chce poinformowac o pisaku (ze wiadomosc w trakcie pisania)
	/// </summary>
	public class OutTypingNotify : OutTcpPacket
	{
		/// <summary>
		/// Ilosc znaków jaka znajduje sie w okienku wiadomosci. Zero - wykasowalismy wiadomosc.
		/// </summary>
		public ushort HowManyCharsInTypingWindow { get; set; }
		/// <summary>
		/// Numer GG
		/// </summary>
		public uint Number { get; set; }


		protected override uint packetCode
		{
			get { return 0x0059; }
		}

		protected override void writeToBytesArray( NullTerminatedBinaryWriter writer )
		{
			writer.Write( this.HowManyCharsInTypingWindow );
			writer.Write( this.Number );
		}
	}
}
