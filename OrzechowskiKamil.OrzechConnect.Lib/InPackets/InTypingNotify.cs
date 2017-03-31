
namespace OrzechowskiKamil.OrzechConnect.Lib.InPackets
{
	/// <summary>
	/// Pakiet informujacy o stanie pisaka obcej osoby.
	/// </summary>
	public class GG_TYPING_NOTIFY : InTcpPacket
	{
		/// <summary>
		/// Ile znaków ma ziomek wysylajacy nam ten pakiet w typing boxie
		/// </summary>
		public ushort HowManyCharsSenderHaveInTypingBox;

		/// <summary>
		/// Numer GG wysyłającego
		/// </summary>
		public uint SenderNumber;
		protected override void readFromBytesArray( Internals.NullTerminatedBinaryReader reader, uint packetContentLength )
		{
			this.HowManyCharsSenderHaveInTypingBox = reader.ReadUInt16();
			this.SenderNumber = reader.ReadUInt32();
		}
	}
}
