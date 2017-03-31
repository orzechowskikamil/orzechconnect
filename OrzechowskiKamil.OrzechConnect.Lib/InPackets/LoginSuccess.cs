using OrzechowskiKamil.OrzechConnect.Lib.Internals;
namespace OrzechowskiKamil.OrzechConnect.Lib.InPackets
{
	/// <summary>
	/// Pakiet oznaczajacy poprawne zalogowanie sie
	/// </summary>
	public class GG_LOGIN_OK80 : InTcpPacket
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
