using System.Collections.Generic;
using OrzechowskiKamil.OrzechConnect.Lib.Connection;
using OrzechowskiKamil.OrzechConnect.Lib.InPackets;
using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;
using System;
using System.Diagnostics;

namespace OrzechowskiKamil.OrzechConnect.Lib.Processess
{
	/// <summary>
	/// Proces odpowiada za przeslanie naszej listy kontaktów do serwera podczas logowania,
	/// z informacją dla kogo mamy byc widoczni, kto jest zablokowany itd.
	/// </summary>
	public class GetOurContactStatusesProcess : Process
	{

		PacketManager manager;
		/// <summary>
		/// Maksymalna ilosc pakietow GGNotify w pakiecie notifyFirst/Last
		/// </summary>
		public const int MaxAllowedNotifiesPackage = 400;


		public void Start()
		{
			this.manager.RegisterProcess( this );
		}

		public GetOurContactStatusesProcess( PacketManager manager )
		{
			this.manager = manager;
		}

		/// <summary>
		/// Event wykonywany gdy nadejdą statusy od serwera. Nastepnie handler jest kasowany.
		/// </summary>
		public Action<List<NotifyReply>> OnStatusesReceived;




		public override bool OnPacketReceived( InPackets.IInTcpPacket packet )
		{
			if (packet is GG_STATUS80)
			{
				this.onReceivedNotifyAboutStatusChange( ( GG_STATUS80 ) packet );
				return true;
			}
			else if (packet is GG_NOTIFY_REPLY_80)
			{
				this.onReceievedNotifyReplies( ( GG_NOTIFY_REPLY_80 ) packet );
				return true;
			}
			return false;
		}


		private void onReceivedNotifyAboutStatusChange( GG_STATUS80 packet )
		{
			if (this.onStatusChanged != null)
			{
				this.onStatusChanged( packet.ContactsMembers[0] );
			}
		}

		private List<GG_NOTIFY> removeDuplicates( List<GG_NOTIFY> listOfContactsStatuses )
		{
			// na wszelki wypadek trzeba usunac duplikaty zeby pozniej porownanie dobrze wyszło.
			var notifiesFound = new Dictionary<int, bool>();
			var listWithoutDuplicates = new List<GG_NOTIFY>();
			listOfContactsStatuses.ForEach( notify =>
			{
				var number = ( int ) notify.Number;
				if (notifiesFound.ContainsKey( number ) == false)
				{
					notifiesFound[number] = true;
					listWithoutDuplicates.Add( notify );
				}
			} );
			return listWithoutDuplicates;
		}

		public void SendContactsListToServer( List<GG_NOTIFY> listOfContactStatuses )
		{
			listOfContactStatuses = this.removeDuplicates( listOfContactStatuses );
			this.lastListUsedToPrepareNotifyRequest = listOfContactStatuses;
			this.clearAllRepliesList();
			Debug.WriteLine( String.Format( "Wysyłam GG_NOTIFY_REQUEST o {0} kontaktów.", listOfContactStatuses.Count ) );
			this.manager.RegisterProcess( this );
			var contactsToSendLeft = listOfContactStatuses.Count;
			var currentIndex = 0;
			if (contactsToSendLeft == 0)
			{
				this.manager.SendPacket( new GG_LIST_EMPTY() );
				return;
			}
			while (contactsToSendLeft > 0)
			{
				GG_NOTIFY_FIRST packet;
				int contactsAmountToCopyNow;
				if (contactsToSendLeft > MaxAllowedNotifiesPackage)
				{
					packet = new GG_NOTIFY_FIRST();
					contactsAmountToCopyNow = MaxAllowedNotifiesPackage;
				}
				else
				{
					packet = new GG_NOTIFY_LAST();
					contactsAmountToCopyNow = contactsToSendLeft;
				}
				contactsToSendLeft -= contactsAmountToCopyNow;
				packet.GGNotifies = listOfContactStatuses.GetRange( currentIndex, contactsAmountToCopyNow );
				currentIndex += contactsAmountToCopyNow;
				this.manager.SendPacket( packet );
			}
		}
		private List<GG_NOTIFY> lastListUsedToPrepareNotifyRequest;
		private List<NotifyReply> allReplies;

		private bool isNotifyReplyOk( List<GG_NOTIFY> inputList, List<NotifyReply> reply )
		{

			if ((inputList != null) && (reply != null))
			{
				if ((inputList.Count != 0))
				{
					Debug.WriteLine( String.Format(
						"Żądano {0} statusów, wymagane jest {2} statusów a przyszlo juz w sumie {1}.", inputList.Count, reply.Count, inputList.Count * 0.9 ) );
				}
				// jezeli przyszla juz do nas wieksza czesc listy,to prawdopodobnie przyjdzie i reszta.
				// niestety jesli kontakty są nieprawidłowe to nie przychodza tutaj i nic z tym nie mozna zrobic.
				// male listy tez pozwalamy bo tam zawsze cos moze zginąć. Pakiety i tak przychodza paczkami po 40 wiec
				// jak przyjdzie pierwszy a na liscie bylo 30 np to mamy wszystkie.
				if (((inputList.Count * 0.7) <= reply.Count) || (inputList.Count < 30))
				{
					//// to uruchamiamy tą jakze zasobożerną procedure diffu tego co wyslalismy i tego co otrzymalismy
					//// i otrzymujemy na tacy ktorych numerow serwer nie odesłał
					//var diff = this.getDifferenceBetweenRequestAndResponse( inputList, reply );


					Debug.WriteLine( String.Format( "Sukces - otrzymano wszystkie statusy ({0}", reply.Count ) );
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// juzniepotrzebne
		/// </summary>
		/// <param name="request"></param>
		/// <param name="response"></param>
		/// <returns></returns>
		private List<int> getDifferenceBetweenRequestAndResponse( List<GG_NOTIFY> request, List<NotifyReply> response )
		{
			var requestsNotFound = new List<int>();
			foreach (var req in request)
			{
				var number = req.Number;
				var found = false;
				foreach (var resp in response)
				{
					if (number.ToString().Length > 8)
					{
						if (resp.Number == number)
						{
							found = true;
						}
					}
				}
				if (found == false)
				{
					requestsNotFound.Add( ( int ) number );
				}
			}
			return requestsNotFound;
		}

		private Action<NotifyReply> onStatusChanged;

		public void StartReceivingContactsStatusChanges( Action<NotifyReply> statusChangedCallback )
		{
			this.onStatusChanged = statusChangedCallback;
		}

		private void addToAllRepliesList( List<NotifyReply> listToAdd )
		{
			var howMuchAdded = 0;
			var howMuchStatuses = 0;
			howMuchAdded = listToAdd.Count;
			if (this.allReplies == null)
			{
				this.allReplies = new List<NotifyReply>();
			}
			else
			{
				howMuchStatuses = this.allReplies.Count;
			}
			Debug.WriteLine( String.Format( "Otrzymano {1} statusów. W liście all replies jest {0} statusów. ", howMuchStatuses, howMuchAdded ) );
			this.allReplies.AddRange( listToAdd );
		}

		private void clearAllRepliesList()
		{
			this.allReplies = null;
		}

		/// <summary>
		/// Dodaje uzytkownika do sledzonej przez nas listy userów bedziemy dostawac o nim notify od teraz.
		/// </summary>
		/// <param name="number"></param>
		/// <param name="isBuddy"></param>
		/// <param name="isFriend"></param>
		/// <param name="isBlocked"></param>
		public void AddNewUserToNotifyDuringConversation( GG_ADD_NOTIFY packet )
		{
			this.manager.SendPacket( packet );
		}

		private void onReceievedNotifyReplies( GG_NOTIFY_REPLY_80 replies )
		{
			this.addToAllRepliesList( replies.ContactsMembers );

			if (this.OnStatusesReceived != null)
			{
				var result = (this.isNotifyReplyOk( this.lastListUsedToPrepareNotifyRequest, this.allReplies ));
				// callback uruchamiam tylko jak wszystko jest w porządku i przyjda wszystkie co maja przyjsc.
				// jak nie to niech sie przedawni.
				if (result == true)
				{
					Debug.WriteLine( String.Format( "Wywołałem handler OnStatusesRecieved dla {0} statusów na liscie.",
						this.allReplies.Count ) );
					this.OnStatusesReceived( this.allReplies );
				}
				else
				{
					var vait = "wait";
				}

				//else
				//{
				//    this.SendContactsListToServer( this.lastListUsedToPrepareNotifyRequest );
				//}
			}
		}
	}
}
