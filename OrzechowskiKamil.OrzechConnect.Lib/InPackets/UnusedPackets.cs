using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using OrzechowskiKamil.OrzechConnect.Lib.InPackets;

namespace OrzechowskiKamil.OrzechConnect.Lib.InPackets
{
	/// <summary>
	/// klasa bedaca reprezentacja nieuzywanego pakietu przez nas ktorego nie ma sensu parsowac
	/// </summary>
	public class GG_UNUSED_PACKET : InTcpPacket
	{

		protected override void readFromBytesArray( OrzechowskiKamil.OrzechConnect.Lib.Internals.NullTerminatedBinaryReader reader, 
			uint packetContentLength )
		{
			
		}
	}
}
