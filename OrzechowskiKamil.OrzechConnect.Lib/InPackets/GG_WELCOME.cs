using OrzechowskiKamil.OrzechConnect.Lib.Internals;
namespace OrzechowskiKamil.OrzechConnect.Lib.InPackets
{
	/// <summary>
	/// GG_WELCOME
	/// Pakiet wysylany przez serwer w odpowiedzi na rozpoczecie nowego polaczenia TCP.
	/// Zawiera seed uzyty do zakodowania hasła do logowania.
	/// </summary>
	public class GG_WELCOME : InTcpPacket
	{
		public uint Seed;
		protected override void readFromBytesArray( NullTerminatedBinaryReader reader, uint packetContentLength )
		{
			this.Seed = reader.ReadUInt32();
		}
	}
}
