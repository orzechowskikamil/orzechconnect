using System;
using System.Collections.Generic;
using System.Windows;
using OrzechowskiKamil.OrzechConnect.Lib.Connection;
using OrzechowskiKamil.OrzechConnect.Lib.InPackets;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;
using OrzechowskiKamil.OrzechConnect.Lib.Processess;
using System.Diagnostics;

namespace OrzechowskiKamil.OrzechConnect.Lib
{
	public class ConnectSettings
	{

		public GGCredentials Credentials;
		public GGStatus InitialStatus;
		public string InitialStatusDescription;
		public byte MaxImageSizeInKb;
	}

	public class GetContactStatusesArgs
	{

		public List<GG_NOTIFY> ListOfContactsStatuses;
	}

	public class Engine
	{

		private PacketManager manager;
		private MessageManager messageManager;

		public event Action Disconnected;

		public Engine()
		{
			this.unhandledMessagesQueue = new Queue<GG_RECV_MSG80>();
			this.manager = new PacketManager();
			// eventy mowiace o przeslaniu ilus tam bajtow albo odebraniu ich

			this.manager.BytesReceived += ( amount ) =>
			{

				this.BytesReceived( amount );
#if DEBUG
				if (this.BytesReceived != null)
				{
					Debug.WriteLine( "odebrał pakiet wielkosci: " + amount );
				}
#endif
			};

			this.manager.BytesSended += ( amount ) =>
			{
				this.BytesSended( amount );
#if DEBUG
				
				if (this.BytesSended != null)
				{
				
					Debug.WriteLine( "wysłał pakiet wielkosci: " + amount );
				}
#endif
			};
			this.manager.Disconnected += () =>
				{
					if (this.Disconnected != null)
					{
						this.callActionByDispatcher( () =>
						{
							this.Disconnected();
						} );
					}
				};
		}

		public event Action<int> BytesSended;
		public event Action<int> BytesReceived;

		private GetContactsListProcess getContactListProcess { get; set; }

		public bool IsLogged { get; private set; }

		/// <summary>
		/// Akcja uruchamiana przy otrzymaniu nowej wiadomosci
		/// </summary>
		protected Func<GG_RECV_MSG80, bool> OnReceiveMessage { get; set; }

		public void StartReceivingMessages( Func<GG_RECV_MSG80, bool> onReceiveMessageCallback )
		{
			this.OnReceiveMessage = onReceiveMessageCallback;
			this.FlushMessagesQueue();
		}

		/// <summary>
		/// Umożliwia podłączenie sie do serwera. 
		/// </summary
		/// <param name="settings"></param>
		public void Connect( ConnectSettings settings, Action onLoginSuccessCallback,
			Action<Exception> onLoginFailCallback )
		{
			var loginProcess = new LoginProcess( this.manager, new LoginProcessArgs
			{
				Credentials = settings.Credentials,
				InitialStatus = settings.InitialStatus,
				InitialStatusDescription = settings.InitialStatusDescription,
				MaxImageSizeInKB = settings.MaxImageSizeInKb,
				OnLoginError = ( exception ) =>
					{
						IsLogged = false;
						if (onLoginFailCallback != null)
						{
							this.callActionByDispatcher( () =>
								{
									onLoginFailCallback( exception );
								} );
						}
					},
				OnLoginFail = () =>
				{
					IsLogged = false;
					if (onLoginFailCallback != null)
					{
						this.callActionByDispatcher( () =>
						{
							onLoginFailCallback( new LoginOrPasswordIncorrectException( "Numer GG lub hasło jest niepoprawne." ) );
						} );
					}
				},
				OnLoginSuccess = () =>
				{
					IsLogged = true;
					this.setStatusProcess = new SetStatusProcess( this.manager );
					this.setStatusProcess.Start();
					this.messageManager = new MessageManager( this.manager, this.doOnReceiveMessage );
					this.messageManager.Start();
					this.getContactListProcess = new GetContactsListProcess( this.manager );
					this.getContactListProcess.Start();
					this.getOurContactStatusesProcess = new GetOurContactStatusesProcess( this.manager );
					this.getOurContactStatusesProcess.Start();
					this.getOurContactStatusesProcess.StartReceivingContactsStatusChanges( ( status ) =>
					{
						this.callActionByDispatcher( () =>
						{
							this.statusChanged( status );
						} );
					} );
					(new PingProcess( this.manager )).Start();
					if (onLoginSuccessCallback != null)
					{
						this.callActionByDispatcher( () =>
						{
							onLoginSuccessCallback();
						} );
					}
				},

			} );
			loginProcess.Start();
		}

		/// <summary>
		/// Zwraca wersje listy kontaktow na serwerze.
		/// </summary>
		/// <param name="onSuccessCallback">callback zwracajacy wersje listy kontaktow</param>
		public void GetVersionOfContactsListOnServer( Action<uint> onSuccessCallback )
		{
			this.getContactListProcess.OnContactsListVersionReceived = ( version ) =>
				{
					this.callActionByDispatcher( () =>
						{
							if (onSuccessCallback != null)
							{
								onSuccessCallback( version );
							}
						} );
				};
			this.getContactListProcess.SendContactListVersionRequest();
		}

		/// <summary>
		/// Rozłącza połączenie
		/// </summary>
		public void Disconnect()
		{
			this.manager.Disconnect();
		}

		/// <summary>
		/// Umożliwia pobranie z serwera listy kontaktów.
		/// </summary>
		/// <param name="onSuccessCallback">closure przyjmujace za parametry wersje listy oraz jej zawartosc</param>
		public void GetContactsListFromServer( Action<uint, ContactBook> onSuccessCallback )
		{
			this.getContactListProcess.OnContactListReceived = ( version, document ) =>
			{
				this.callActionByDispatcher( () =>
				{
					var contactList = new ContactBook();
					contactList.LoadFromXml( document );
					if (onSuccessCallback != null)
					{
						onSuccessCallback( version, contactList );
					}
				} );
			};
			this.getContactListProcess.SendContactListImportRequest();
		}
		public void AddNewUserNotifyDuringConversation( int number, UserType type )
		{
			this.getOurContactStatusesProcess.AddNewUserToNotifyDuringConversation( new GG_ADD_NOTIFY { Number = (uint)number, UserType= type} );
		}
		/// <summary>
		/// Wysyła na serwer liste kontaktów
		/// </summary>
		/// <param name="versionOfContacts">ostatni numerek listy kontaktow jaki przyszedl od nas z serwera</param>
		/// <param name="contactListToSend">lista kontaktow</param>
		/// <param name="onSuccessCallback">akcja wykonywana gdy wysle sie pomyslnie 
		/// (zwraca numerek versji listy kontaktow)</param>
		/// <param name="onRejectCallback">akcja wykonywana gdy wysle sie niepomyslnie - niezgodnosc wersji.</param>
		public void SendContactsListToServer( uint versionOfContacts, ContactBook contactListToSend,
			Action<uint> onSuccessCallback, Action onRejectCallback )
		{
			this.getContactListProcess.ExportContactListToServer( versionOfContacts, contactListToSend.SaveToXml(), () =>
			{
				if (onRejectCallback != null)
				{
					//poracha
					this.callActionByDispatcher( () =>
						{
							onRejectCallback();
						} );
				}
			}, ( version ) =>
				{
					// sukces
					if (onSuccessCallback != null)
					{
						this.callActionByDispatcher( () =>
							{
								onSuccessCallback( version );
							} );
					};
				} );

		}

		/// <summary>
		/// Pozwala sciagnac statusy reszty osób
		/// </summary>
		public void GetOurContactsStatuses( List<GG_NOTIFY> contactsToGet, Action<List<NotifyReply>> successCallback,
			Action requestTimedOutCallback )
		{
			if (successCallback != null)
			{
				this.getOurContactStatusesProcess.OnStatusesReceived = ( listOfReplies ) =>
					{
						this.callActionByDispatcher( () =>
						{
							successCallback( listOfReplies );
						} );
					};
			}
			this.getOurContactStatusesProcess.SendContactsListToServer( contactsToGet
				//, () =>
				//    {
				//        if (requestTimedOutCallback != null)
				//        {
				//            this.callActionByDispatcher( () =>
				//                {
				//                    requestTimedOutCallback();
				//                } );
				//        }
				//    }
				);
		}


		/// <summary>
		/// Wysyła wiadomość
		/// </summary>
		public void SendMessage( SendMessageParameters parameters )
		{
			if (this.messageManager != null)
			{
				this.messageManager.SendMessage( parameters );
			}
			else throw new OrzechowskiKamil.OrzechConnect.Lib.Exceptions.NotEstablishedConnectionException( "Brak połączenia" );
		}

		private SetStatusProcess setStatusProcess;

		/// <summary>
		/// Ustawia status
		/// </summary>
		public void SetStatus( SetStatusOptions options )
		{
			this.setStatusProcess.SetStatus( options );
		}

		private void callActionByDispatcher( Action action )
		{
			if (action != null)
			{
				Deployment.Current.Dispatcher.BeginInvoke( () =>
				{
					action();
				} );
			}
		}

		private Action<NotifyReply> statusChanged;

		public void StartReceivingStatusChanges( Action<NotifyReply> statusChanged )
		{
			this.statusChanged = statusChanged;
		}

		private GetOurContactStatusesProcess getOurContactStatusesProcess;

		private Queue<GG_RECV_MSG80> unhandledMessagesQueue;

		/// <summary>
		/// Powoduje wysłanie do handlera całej kolejki wiadomosci (to samo dzieje sie gdy zostanie otrzymana
		/// nowa wiadomosc)
		/// </summary>
		public void FlushMessagesQueue()
		{
			if (this.OnReceiveMessage == null) { return; }
			var goingOn = true;
			while (goingOn)
			{
				goingOn = false;
				if (unhandledMessagesQueue.Count > 0)
				{
					var packetToSend = unhandledMessagesQueue.Peek();
					if (packetToSend != null)
					{
						goingOn = this.OnReceiveMessage( packetToSend );
						if (goingOn == true)
						{
							unhandledMessagesQueue.Dequeue();
						}
					}
				}
			}
		}

		private void doOnReceiveMessage( GG_RECV_MSG80 message )
		{

			this.callActionByDispatcher( () =>
			{
				this.unhandledMessagesQueue.Enqueue( message );
				this.FlushMessagesQueue();
			} );

		}
	}
	public class LoginOrPasswordIncorrectException : Exception
	{
		public LoginOrPasswordIncorrectException( string message ) : base( message ) { }
	}
}

