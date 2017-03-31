
using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;
namespace OrzechowskiKamil.OrzechConnect.Lib.OutPackets
{

	/// <summary>
	/// Pakiet ten usuwa flagę z danego numeru gg.
	/// </summary>
	public class GG_REMOVE_NOTIFY : GG_ADD_NOTIFY
	{
		protected override uint packetCode
		{
			get
			{
				return 0x000e;
			}
		}
	}
}
