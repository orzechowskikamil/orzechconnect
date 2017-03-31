using System;
using OrzechowskiKamil.OrzechConnect.Lib.Connection;
using OrzechowskiKamil.OrzechConnect.Lib.InPackets;
using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;


namespace OrzechowskiKamil.OrzechConnect.Lib.Processess
{
	/// <summary>
	/// Process importowania i eksportowania listy kontaktow z serwera
	/// </summary>
	public class GetContactsListProcess : Process
	{

		PacketManager manager;
		/// <summary>
		/// uint to wersja listy, a string to XML-string zawierajacy dokument opisujacy liste kontaktow.
		/// Uruchamiane w momencie gdy otrzymana zostanie lista kontaktow (sukces).
		/// </summary>
		public Action<uint, string> OnContactListReceived;
		/// <summary>
		/// Akcja uruchamiana gdy nadejdzie pakiet z numerkiem wersji aktualnej listy
		/// </summary>
		public Action<uint> OnContactsListVersionReceived;
		/// <summary>
		/// Uruchamiane gdy zawiedzie eksport kontaktów
		/// </summary>
		private Action OnExportContactsRejection;
		/// <summary>
		/// Uruchamiane gdy powiedzie sie eksport kontaktów
		/// </summary>
		private Action<uint> OnExportContactsSuccess;



		public GetContactsListProcess( PacketManager manager )
		{
			this.manager = manager;
		}




		public void ExportContactListToServer( uint lastVersionOfList, string xmlDocumentWithContactsList,
			Action onRejection, Action<uint> onSuccess )
		{
			this.OnExportContactsRejection = onRejection;
			this.OnExportContactsSuccess = onSuccess;
			var content = Deflate.Compress( xmlDocumentWithContactsList );
			if (content != null)
			{
				var packet = new GG_USERLIST100_REQUEST
				{
					Content = content,
					LastVersionNumberOfUserList = lastVersionOfList,
					ListFormatType = UserListFormatType.GG_USERLIST100_FORMAT_TYPE_GG100,
					ListRequestType = GG_USERLIST100_REQUEST.RequestType.GG_USERLIST100_PUT
				};
				this.manager.SendPacket( packet );
			}
			else
			{
				throw new Exception( "contact book after serialization is null." );
			}
		}

		public override bool OnPacketReceived( InPackets.IInTcpPacket packet )
		{
			if (packet is GG_USERLIST100_REPLY)
			{
				var replyPacket = ( GG_USERLIST100_REPLY ) packet;
				if (replyPacket.Type == GG_USERLIST100_REPLY.ReplyType.GG_USERLIST100_REPLY_LIST)
				{
					if (replyPacket.Reply == null || replyPacket.Reply.Length == 0)
					{
						// pakiet bez zawartosci czyli proszono tylko o wersje
						this.onContactsListVersionReceived( replyPacket );
					}
					else
					{
						// pakiet z zawartoscia - lista kontaktów
						this.onContactsListReceived( replyPacket );
					}
				}
				else if (replyPacket.Type == GG_USERLIST100_REPLY.ReplyType.GG_USERLIST100_REPLY_ACK)
				{
					this.onSendedContactsListReceivedByServer( replyPacket );
				}
				else if (replyPacket.Type == GG_USERLIST100_REPLY.ReplyType.GG_USERLIST100_REPLY_REJECT)
				{
					this.onSendedContactsListRejectionByServer( replyPacket );
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Wysyla do serwera GG request z prosba o liste kontaktow
		/// </summary>
		public void SendContactListImportRequest()
		{
			var packet = new GG_USERLIST100_REQUEST
			{
				ListFormatType = UserListFormatType.GG_USERLIST100_FORMAT_TYPE_GG100,
				ListRequestType = GG_USERLIST100_REQUEST.RequestType.GG_USERLIST100_GET,
				LastVersionNumberOfUserList = 0
			};
			this.manager.SendPacket( packet );
		}

		/// <summary>
		/// Wysyła do serwera GG request z prosba o wersje kontaktów. 
		/// </summary>
		public void SendContactListVersionRequest()
		{
			var packet = new GG_USERLIST100_REQUEST
			{
				ListFormatType = UserListFormatType.GG_USERLIST100_FORMAT_TYPE_NONE,
				ListRequestType = GG_USERLIST100_REQUEST.RequestType.GG_USERLIST100_GET,
				LastVersionNumberOfUserList = 0
			};
			this.manager.SendPacket( packet );
		}

		/// <summary>
		/// Inicjuje działanie procesu. Użyj zanim zaczniesz korzystac z klasy.
		/// </summary>
		public void Start()
		{
			this.manager.RegisterProcess( this );
		}

		private void onContactsListReceived( GG_USERLIST100_REPLY replyPacket )
		{
			var contactsString = Deflate.Uncompress( replyPacket.Reply );
			if (this.OnContactListReceived != null)
			{
				this.OnContactListReceived( replyPacket.Version, contactsString );
				// zeby uniknac jakichs tam anomalii pozniej
				this.OnContactListReceived = null;
			}
		}

		private void onContactsListVersionReceived( GG_USERLIST100_REPLY replyPacket )
		{
			if (this.OnContactsListVersionReceived != null)
			{
				this.OnContactsListVersionReceived( replyPacket.Version );
				// zeby uniknac jakichs tam anomalii pozniej
				this.OnContactsListVersionReceived = null;
			}
		}

		private void onSendedContactsListReceivedByServer( GG_USERLIST100_REPLY replyPacket )
		{
			if (this.OnExportContactsSuccess != null)
			{
				this.OnExportContactsSuccess( replyPacket.Version );
				// zeby uniknac jakichs tam anomalii pozniej
				this.OnExportContactsSuccess = null;
			}
		}

		private void onSendedContactsListRejectionByServer( GG_USERLIST100_REPLY replyPacket )
		{
			if (this.OnExportContactsRejection != null)
			{
				this.OnExportContactsRejection();
				// zeby uniknac jakichs tam anomalii pozniej
				this.OnExportContactsRejection = null;
			}
		}
	}
}
