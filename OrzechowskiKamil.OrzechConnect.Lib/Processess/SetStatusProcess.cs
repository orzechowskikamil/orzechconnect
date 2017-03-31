using OrzechowskiKamil.OrzechConnect.Lib.Connection;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using System;
using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;

namespace OrzechowskiKamil.OrzechConnect.Lib.Processess
{
	public class SetStatusOptions
	{
		public GGStatus Status;
		public string Description;
		public bool ForFriendsOnly;
		public bool MobileClient;
		public bool ReceiveLinksFromUnknowns;
		public bool StatusIsSet;
	}
	/// <summary>
	/// Process pozwala ustawic status na gadu gadu
	/// </summary>
	public class SetStatusProcess : Process
	{
		PacketManager manager;
		public SetStatusProcess( PacketManager manager )
		{
			this.manager = manager;
		}
		public void Start()
		{
			this.manager.RegisterProcess( this );
		}
		public void SetStatus( SetStatusOptions options )
		{
			var packet = new SetStatus()
			{
				Description = options.Description,
				Status = options.Status,
			};
			packet.SetValue( GGStatusMasks.OnlyForFriendsMask, options.ForFriendsOnly );
			packet.SetValue( SetStatusFlags.MobileClient, options.MobileClient );
			packet.SetValue( SetStatusFlags.IWantToRecieveLinksFromUnknowns, options.ReceiveLinksFromUnknowns );
			packet.SetValue( GGStatusMasks.StatusDescriptionIsSetMask, options.StatusIsSet );
			this.manager.SendPacket( packet );
		}

		public override bool OnPacketReceived( InPackets.IInTcpPacket packet )
		{
			return false;
		}
	}
}
