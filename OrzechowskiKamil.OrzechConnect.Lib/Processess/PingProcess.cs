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
using OrzechowskiKamil.OrzechConnect.Lib.Connection;
using OrzechowskiKami.MetroGaduLib.Internals;
using System.Windows.Threading;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;

namespace OrzechowskiKamil.OrzechConnect.Lib.Processess
{
	/// <summary>
	/// Proces odpowiada za odpowiadanie na pingi serwera (oraz wysylanie ich co 4 minuty niezaleznie
	/// od tego czy dostajemy pingi czy nie)
	/// </summary>
	public class PingProcess : Process
	{
		private PacketManager manager;
		// jesli sie ustawi za niski ping interval to serwer gadu gadu rozłącza po kilku razach. ok 5 minut ma byc
		private const int PongIntervalInSeconds = 260;
		private Timer timer;
		public PingProcess( PacketManager manager )
		{
			this.manager = manager;

		}

		public void Start()
		{
			this.manager.RegisterProcess( this );
			this.timer = new Timer( PongIntervalInSeconds, () => { this.sendPongPacket(); } ).Start();
		}

		public override bool OnPacketReceived( InPackets.IInTcpPacket packet )
		{
			if (packet is GG_PING)
			{
				this.onPingReceived();
				return true;
			}
			return false;
		}

		private void onPingReceived()
		{
			this.sendPongPacket();
		}

		private void sendPongPacket()
		{
			var packet = new GG_PONG();
			if (this.manager.SendPacket( packet ) == false)
			{
				if (timer != null)
				{
					timer.Stop();
					timer = null;
				}
			}
		}
	}
}
