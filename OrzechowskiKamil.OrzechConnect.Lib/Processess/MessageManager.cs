using System;
using OrzechowskiKamil.OrzechConnect.Lib.Connection;
using OrzechowskiKamil.OrzechConnect.Lib.InPackets;
using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;

namespace OrzechowskiKamil.OrzechConnect.Lib.Processess
{
	public class SendMessageParameters
	{
		public string Message;
		public int ReceiverNumber;
	}

	/// <summary>
	/// Proces odpowiada za wysylanie i odbieranie wiadomosci
	/// </summary>
	public class MessageManager : Process
	{
		PacketManager manager;
		private Action<GG_RECV_MSG80> onReceiveMessage;
		public MessageManager( PacketManager manager, Action<GG_RECV_MSG80> onReceiveMessage )
		{
			this.onReceiveMessage = onReceiveMessage;
			this.manager = manager;
		}
		public void Start()
		{
			this.manager.RegisterProcess( this );
		}
		public void SendMessage( SendMessageParameters parameters )
		{
			var sendMessagePacket = new SendMessage()
			{
				HtmlMessage = parameters.Message,
				ReceiverNumber = ( uint ) parameters.ReceiverNumber
			};
			this.manager.SendPacket( sendMessagePacket );
		}
		public override bool OnPacketReceived( IInTcpPacket packet )
		{
			if (packet is GG_RECV_MSG80)
			{
				this.OnMessage( ( GG_RECV_MSG80 ) packet );
				return true;
			}
			return false;
		}

		private void OnMessage( GG_RECV_MSG80 packet )
		{
			if (this.onReceiveMessage != null)
			{
				this.onReceiveMessage( packet );
			}
		}
	}
}
