using OrzechowskiKamil.OrzechConnect.Lib.Internals;
namespace OrzechowskiKamil.OrzechConnect.Lib.InPackets
{
	/// <summary>
	/// Pakiet oznaczajacy niepoprawne zalogowanie sie
	/// </summary>
	public class GG_LOGIN_FAIL : InTcpPacket
	{
		/// <summary>
		/// Tu powinno byc wieksze od zera
		/// </summary>
		public uint Value;
		protected override void readFromBytesArray( NullTerminatedBinaryReader reader, uint packetContentLength )
		{
			this.Value = reader.ReadUInt32();
		}
	}
}
