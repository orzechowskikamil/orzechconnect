
namespace OrzechowskiKamil.OrzechConnect.Lib.OutPackets
{
	public class GG_NOTIFY_LAST : GG_NOTIFY_FIRST
	{
		protected override uint packetCode
		{
			get { return 0x0010; }
		}
	}
}
