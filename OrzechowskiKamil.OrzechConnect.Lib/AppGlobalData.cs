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
using System.Collections.Generic;
using OrzechowskiKamil.OrzechConnect.Lib;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;
using OrzechowskiKamil.OrzechConnect.Lib.InPackets;
using OrzechowskiKamil.OrzechConnect.Lib.Processess;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
using OrzechowskiKamil.OrzechConnect.Lib.DataStorage;



namespace OrzechowskiKamil.OrzechConnect.Lib
{
	/// <summary>
	/// Globalny stan aplikacji, czyli "serwer"
	/// </summary>
	public class AppGlobalData
	{

		private Func<GG_RECV_MSG80, bool> _onRecvMsg;
		private static AppGlobalData current;
		private UserProfile currentLoggedProfile;
		public Action Disconnected;
		private bool isBlackThemeSelected;
		private bool isLastTombstoneUnhandled;
		private StatusChangeReceived lastStatusChangeHandler;
		private DateTime lastTombstoneEnd;
		private DateTime lastTombstoneStart;
		public const int MaxAllowedTimeForLoginProcess = Config.MaxAllowedTimeForLoginProcess;
		public Action<int, string> MessageSended;
		private MainApplicationSettings settings;
		private bool wasThemeCalculated;
		// w foreground sa tylko do odczytu
		private BackgroundWorkerData bgWorkerData;
		private BackgroundWorkerData BgWorkerData
		{
			get
			{
				if (this.bgWorkerData == null)
				{
					this.bgWorkerData = new BackgroundWorkerData();
					this.bgWorkerData.Load();
				}
				return this.bgWorkerData;
			}
		}

		private WorkerData WorkerData
		{
			get
			{
				return this.BgWorkerData.Data;
			}
		}

		public int BytesReceivedByAgent
		{
			get
			{
				return this.WorkerData.BytesReceivedByAgent;
			}
		}

		public int BytesSendedByAgent
		{
			get { return this.WorkerData.BytesSendedByAgent; }
		}

		public enum LoginPhases
		{
			Connecting = 0,
			Logging = 1,
			VerifingContactsActuality = 2,
			DownloadingContactsStatuses = 3
		}



		public AppGlobalData()
		{
			this.ListOfContactStatuses = new List<NotifyReply>();
			this.CurrentContactsList = new ContactBook();
#if DEBUG
			this.Helper.Diagnose( () => this.Helper.DiagnosticMessage( "Właśnie rozpocząłem odczyt ustawien." ) );
#endif
			this.loadMainSettings();
			this.TryRegisterAgent();
		}
		private void TryRegisterAgent()
		{
		}
		private DiagnosticHelper helper;
		protected DiagnosticHelper Helper
		{
			get
			{
				if (this.helper == null)
				{
					this.helper = new DiagnosticHelper( true, "AppGlobalData", "!!!!!!!" );
				}
				return this.helper;
			}
		}



		public MainAppSettingsPack AdditionalSettings
		{
			get
			{
				return this.settings.AdditionalAppSettings;
			}
		}


		public int BytesReceivedCounter
		{
			get { return this.settings.InternetBytesIncoming; }
			set { this.settings.InternetBytesIncoming = value; }
		}

		public int BytesSendCounter
		{
			get { return this.settings.InternetBytesOutcoming; }
			set { this.settings.InternetBytesOutcoming = value; }
		}

		public ContactBook CurrentContactsList { get; set; }
		/// <summary>
		/// jezeli true to znaczy ze wlasnie przeskoczylismy ze strony dodaj kontakt na tą stronę.
		/// </summary>
		public bool ComingFromAddContact { get; set; }
		public static AppGlobalData Current
		{
			get
			{
				if (current == null)
				{
					current = new AppGlobalData();
				}
				return current;
			}
		}

		/// <summary>
		/// Zwraca nazwe obecnej kompozycji obrazków itd
		/// </summary>
		public string CurrentCompositionName
		{
			get
			{
				return "Metro";
			}
		}

		public string CurrentDescription { get; protected set; }

		public UserProfile CurrentLoggedProfile
		{
			get { return this.currentLoggedProfile; }
			set { this.currentLoggedProfile = value; }
		}

		public GGStatus CurrentStatus { get; protected set; }

		public int DefaultProfileNumber
		{
			get { return this.settings.DefaultProfileGGNumber; }
			set { this.settings.DefaultProfileGGNumber = value; }
		}

		public Engine Engine { get; set; }

		public bool IsBlackThemeSelected
		{
			get
			{
				if (this.wasThemeCalculated == false)
				{
					this.isBlackThemeSelected = (( Color ) Application.Current.Resources["PhoneForegroundColor"])
						== Colors.White;
					this.wasThemeCalculated = true;
				}
				return this.isBlackThemeSelected;
			}
		}

		/// <summary>
		/// Oznacza czy to aplikacja czy agent
		/// </summary>
		public bool IsForegroundProcess { get; set; }

		//public DateTime LastResetOfBytesCounter
		//{
		//    get { return this.settings.AdditionalAppSettings.DateOfLastResetCounter; }
		//    set { this.settings.AdditionalAppSettings.DateOfLastResetCounter = value; }
		//}

		public List<NotifyReply> ListOfContactStatuses { get; set; }

		private Func<GG_RECV_MSG80, bool> onReceiveMessage
		{
			get
			{
				return this._onRecvMsg;
			}
			set
			{
				this._onRecvMsg = value;
				// powiadamiamy engina ze juz mozemy odbierac wiadomosci i ma nam przekazac te zakolejkowane
				this.Engine.FlushMessagesQueue();
			}
		}

		public List<UserProfile> UserProfilesList
		{
			get
			{
				return this.settings.UserProfiles;
			}
			set
			{
				this.settings.UserProfiles = value;
			}
		}




		public delegate void StatusChangedEventHandler( GGStatus status, string description );
		public delegate void StatusChangeReceived( NotifyReply statusChanged );

		/// <summary>
		/// uruchamiany gdy aplikacja budzi sie z tombstona
		/// </summary>
		public event Action ApplicationActivatedFromTombstone;

		public event Action<object> SendMessageTextBoxGotFocus;

		public event Action SendMessageTextBoxLostFocus;

		/// <summary>
		/// ten event z tego co pamietam sluzy do obslugi tego ze user (ten co jest zalogowany) zmienil status
		/// </summary>
		public event StatusChangedEventHandler StatusChanged;

		/// <summary>
		/// ten sluzyl do odebrania wiadomosci o zmianie statusu przez kogos
		/// </summary>
		public event StatusChangeReceived StatusChangeReceivedEvent;




		public void AddContact( AddContactParams addContactParams, Action<uint> onSuccess, Action onFail )
		{
			//var contact = new Contact()
			//{
			//    GGNumber = addContactParams.Number,
			//    ShowName = addContactParams.Name,
			//    Guid = Guid.Parse( "0b345af6-0002-0000-0000-00000000000b" )
			//};
			//contact.GroupIds.Add( Guid.Empty );
			//	this.ContactsListForCurrentLoggedUser.Contacts.Add( contact );
			var normal = true;
			var buddy = false;
			var friend = true;
			var ignored = false;
			var contact = new Contact()
			{
				GGNumber = addContactParams.Number,
				ShowName = addContactParams.Name,
				Avatars = new List<string> { "avatar-empty.gif" },
				FlagNormal = normal,
				FlagFriend = friend,
				FlagBuddy = buddy,
				FlagIgnored = ignored,
				Groups = new List<Guid> { Guid.Empty },
				Guid = Guid.NewGuid()

			};
			var contactZ = this.GetContactByNumber( contact.GGNumber );
			if (contactZ == null)
			{
				Contact[] copyOfContacts = new Contact[this.CurrentContactsList.Contacts.Count];
				this.CurrentContactsList.Contacts.CopyTo( copyOfContacts );
				this.CurrentContactsList.Contacts.Add( contact );
				this.sendContactsToServer( ( versionNumber ) =>
				{
					//success
					this.ListOfContactStatuses.Add( new NotifyReply()
					{
						Number = ( uint ) contact.GGNumber,
						Status = GGStatus.NotAvailable
					} );
					// zostaja nowe kontakty w liscie
					this.Engine.AddNewUserNotifyDuringConversation( addContactParams.Number, UserType.Buddy );
					this.Engine.AddNewUserNotifyDuringConversation( addContactParams.Number, UserType.Friend );
					onSuccess( versionNumber );
				}
					, () =>
					{
						// rollback changes
						this.CurrentContactsList.Contacts = new List<Contact>();
						foreach (var contactX in copyOfContacts)
						{
							this.CurrentContactsList.Contacts.Add( contactX );

						}
						// kopiuje z powrotem stare kontakty do listy.
						onFail();
					} );
			}
			else
			{
				throw new Exception( String.Format( Strings.Taki_kontakt_juz_istnieje, contactZ.ShowName ) );
			}
		}
		public int BytesDownloadedAtThisSession { get; set; }
		public int BytesSendedAtThisSession { get; set; }
		/// <summary>
		/// dodaje nowy profil uzytkownika, dane te co w parametrach, reszte domyslnymi.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="numberGG"></param>
		/// <param name="password"></param>
		public void AddNewUserProfile( string name, int numberGG, string password )
		{
			if (AppGlobalData.Current.GetUserProfileByNumber( numberGG ) != null)
			{
				throw new Exception( Strings.Profil_już_istnieje );
			}
			else
			{
				this.UserProfilesList.Add( new UserProfile
				{
					AccountName = name,
					AccountNumber = numberGG,
					AccountPassword = password,
					ContactsListVersion = 0,
					ForFriendsOnly = Config.ForFriendsOnlyDefaultSet,
					StatusAfterLogin = Config.StatusAfterLoginDefaultSet,
					StatusDescriptionAfterLogin = Config.StatusDescriptionAfterLoginDefaultSet

				} );
			}
		}

		//public void ApplyStatusFilter( string filter )
		//{
		//    this.CurrentStatusFilter = filter;

		//    //zmusi to listboxa do odswiezenia danych
		//    this.StatusChanged( 0, null );

		//}
		public void CallMessageSended( int number, string message )
		{
			if (this.MessageSended != null)
			{
				Deployment.Current.Dispatcher.BeginInvoke( () => { this.MessageSended( number, message ); } );
			}
		}

		public void DeleteContact( int number, Action<uint> onSucces, Action onFail )
		{
			var contact = this.GetContactByNumber( number );
			if (contact != null)
			{
				this.CurrentContactsList.Contacts.Remove( contact );
				foreach (var item in this.ListOfContactStatuses)
				{
					if (item.Number == number)
					{
						this.ListOfContactStatuses.Remove( item );
						this.StatusChangeReceivedEvent( null );
						this.sendContactsToServer( onSucces, onFail );
						break;
					}
				}
			}

		}

		/// <summary>
		/// Uruchomienie tego powoduje zaaktualizowanie listy statusów dla obecnie zalogowanego profilu.
		/// Sprawdzane są osoby z listy kontaktow obecnie zalogowanego profilu.
		/// </summary>
		public void DownloadStatusesForPeopleOnCurrentContactList( Action onSuccess, Action onTimeout )
		{
			var contacts = this.CurrentContactsList.Contacts;
			var listOfNotify = new List<GG_NOTIFY>();
			contacts.ForEach( contact =>
			{
				listOfNotify.Add( new GG_NOTIFY
				{
					Number = ( uint ) contact.GGNumber,
					UserTypeMask = ( byte ) UserNotifyTypeMask.UserBuddy
				} );
			} );
			if (listOfNotify.Count == 0)
			{
				// jeżeli na liscie jest zero kontaktów, to nie wysylam drugi raz GG_NOTIFY_EMPTY bo 
				// serwer zawiesza polaczenie.
				onSuccess();

			}
			else
			{
				this.Engine.GetOurContactsStatuses( listOfNotify, ( listOfReplies ) =>
					{
						this.actualizeStatusInStatusesList( listOfReplies,
							this.ListOfContactStatuses );
						if (onSuccess != null)
						{
							onSuccess();
						}
					}, () => { if (onTimeout != null) { onTimeout(); } } );
			}
		}

		public void ForceDisconnect()
		{
			if (this.Engine != null)
			{
				this.Engine.Disconnect();
			}
		}

		public Contact GetContactByNumber( int ggNumber )
		{
			//TODO do zoptymalizowania
			var contacts = this.CurrentContactsList.Contacts;
			foreach (var contact in contacts)
			{
				if (contact.GGNumber == ggNumber)
				{
					return contact;
				}
			}
			return null;
		}

		/// <summary>
		/// Jezeli zwroci -1 to znaczy ze nie bylo jeszcze zadnego
		/// </summary>
		/// <returns></returns>
		public int GetLastTombstoneDurationInSeconds()
		{
			if (this.isLastTombstoneUnhandled)
			{
				var span = this.lastTombstoneEnd.Subtract( this.lastTombstoneStart );
				return span.Seconds;
			}
			else
			{
				return -1;
			}
		}

		public UserProfile GetUserProfileByNumber( int accountNumber )
		{
			UserProfile userProfile = null;
			foreach (var profile in this.UserProfilesList)
			{
				if (profile.AccountNumber == accountNumber)
				{
					userProfile = profile;
					break;
				}
			}
			return userProfile;
		}

		/// <summary>
		/// Sprawdza czy danych profil jest domyślny
		/// </summary>
		public bool IsProfileDefault( UserProfile profile )
		{
			return profile.AccountNumber == this.DefaultProfileNumber;
		}

		public void OnSendMessageTextBoxGotFocus( object textBox )
		{
			if (this.SendMessageTextBoxGotFocus != null)
			{
				this.SendMessageTextBoxGotFocus( textBox );
			}
		}

		public void OnSendMessageTextBoxLostFocus()
		{
			if (this.SendMessageTextBoxLostFocus != null)
			{
				this.SendMessageTextBoxLostFocus();
			}
		}

		public void OnTombstone()
		{
			try
			{
				this.SetStatusOnTombstoning();
				System.Threading.Thread.Sleep( 200 );
				this.lastTombstoneStart = DateTime.Now;
				this.isLastTombstoneUnhandled = true;
				// socket i tak sie rozlaczy wiec lepiej posprzatac po nim zeby nie napierdalal wyjątkami 
				// i usunąc go od razu.

				this.ForceDisconnect();
			}
			catch (Exception) { }
		}

		public void OnUntombstone()
		{
			try
			{
				this.lastTombstoneEnd = DateTime.Now;

				// posprzątaj
				this.ForceDisconnect();
				if (this.ApplicationActivatedFromTombstone != null)
				{
					this.ApplicationActivatedFromTombstone();

				}
				this.SetStatusOnUntombstoning();
				this.isLastTombstoneUnhandled = false;
			}
			catch (Exception) { }
		}

		/// <summary>
		/// Usuwa profil z listy profili o danym numerze konta
		/// </summary>
		public void RemoveUserProfileByNumber( int accountNumber )
		{
			UserProfile profileToDelete = this.GetUserProfileByNumber( accountNumber );
			if (profileToDelete != null)
			{
				this.UserProfilesList.Remove( profileToDelete );
				new ContactsListDocument( profileToDelete.AccountNumber ).Delete();
			}
		}

		/// <summary>
		/// Metoda zapisuje kontakty obecnie zalogowanego uzytkownika do pliku.
		/// </summary>
		public void saveContactsListForCurrentInFile()
		{
			var document = new ContactsListDocument( this.currentLoggedProfile.AccountNumber );
			document.ContactsList = this.CurrentContactsList;
			document.Save();
		}

		/// <summary>
		/// Metoda zapisuje dane przy wyjsciu z aplikacji.
		/// </summary>
		public void SaveData()
		{
			this.settings.Save();
		}

		public void SendEmptyNotifyRequest( Action<List<NotifyReply>> onSuccessCallback, Action timeoutCallback )
		{
			this.Engine.GetOurContactsStatuses( new List<GG_NOTIFY>(), onSuccessCallback, timeoutCallback );
		}




		/// <summary>
		/// Pozwala wstrzyknac handler dla zmian statusów
		/// </summary>
		/// <param name="statusChangeHandler"></param>
		public void StartListenForStatusChanges( StatusChangeReceived statusChangeHandler )
		{
			this.StatusChangeReceivedEvent += statusChangeHandler;
		}



		/// <summary>
		/// Rozpoczyna proces logowania do serwera.
		/// </summary>
		/// <param name="profileToLogin"></param>
		/// <param name="onSuccessCallback">ten event obsluguje logowanie zakonczone sukcesem</param>
		/// <param name="onTimeoutCallback">Ten event obsluguje logowanie gdy sie przedawni</param>
		/// <param name="onErrorCallback">Ten event obsluguje spodziewane wyjątki</param>
		/// <param name="onLoginPhaseChangedCallback">Ten event obsluguje zmiane obecnej czynnosci
		/// wykonywanej przy logowaniu</param>
		/// <param name="onUnhandledException">Ten event obsluguje wyjatki ktore niespodziewanie sie pojawily
		/// podczas wykonywania akcji</param>
		/// <param name="shouldDownloadContacts">Jezeli true to sciagnie/wczyta z pliku kontakty. jEzeli nie
		/// to zostana te co sa juz teraz wczytane.</param>
		/// <param name="shouldGetUserStatuses">jezeli true to sciagnie statusy uzytkownikow, jezeli nie to nie.</param>
		public void StartLoginProcess( UserProfile profileToLogin, Action onSuccessCallback, Action onTimeoutCallback,
			Action<Exception> onErrorCallback, Action<LoginPhases> onLoginPhaseChangedCallback,
			Action onUnhandledExceptionCallback, bool shouldDownloadContacts, bool shouldGetUserStatuses, bool isReconnect )
		{
			// to na wszelki wypadek jakby zostal jakis syf.
			this.ForceDisconnect();
			this.Engine = new Engine();
			this.BytesSendedAtThisSession = 0;
			this.BytesDownloadedAtThisSession = 0;
			this.Engine.BytesReceived += ( amount ) =>
			{
				this.BytesDownloadedAtThisSession += amount;
				this.BytesReceivedCounter += amount;
			};
			this.Engine.BytesSended += ( amount ) =>
			{
				this.BytesSendedAtThisSession += amount;
				this.BytesSendCounter += amount;
			};

			this.Engine.StartReceivingStatusChanges( ( status ) =>
				{
					this.onReceiveStatusChange( status );
				} );
			this.Engine.Disconnected += () =>
				{
					if (this.Disconnected != null)
					{
						// wyzwala zewnetrzna reakcje na odłączenie sie od serwera.
						this.Disconnected();
					}
				};
			var engine = this.Engine;
			this.CurrentLoggedProfile = profileToLogin;
			this.ListOfContactStatuses = new List<NotifyReply>();
			var loginCor = new ChainOfResponsibility<LoginArgs>
			{
				// recznie uruchamiane kolejne stepy (poprzez callbacki)
				AutomaticExecution = false,

			}
				// lista metod w łańcuchu
			.Add( ( instance ) =>
			{
				instance.StaticMethods.OnPhaseChange( LoginPhases.Connecting );
				instance.StaticMethods.SetTimeoutTimer( MaxAllowedTimeForLoginProcess );
				var statuses = (isReconnect) ? this.getStatusForReconnect() : this.getStatusForAfterLogin();
				// rozpoczynamy logowanie , podlaczaamy do serwera
				engine.Connect( new ConnectSettings
				{
					Credentials = new GGCredentials
					{
						Number = profileToLogin.AccountNumber,
						Password = profileToLogin.AccountPassword
					},
					MaxImageSizeInKb = this.AdditionalSettings.MaxImageSize,
					InitialStatus = statuses.status,
					InitialStatusDescription = statuses.description
				}, () =>
				{
					instance.StaticMethods.OnNextStep( LoginPhases.Logging );
				},
				instance.StaticMethods.OnException );
			} ).Add( ( instance ) =>
			{

				// wysylamy pustą liste zeby nas zalogowało
				this.SendEmptyNotifyRequest( ( list ) =>
				{
					instance.StaticMethods.OnNextStep( LoginPhases.VerifingContactsActuality );
				}, instance.StaticMethods.OnTimeout );
			} ).Add( ( instance ) =>
			{
				//	this.setStatusAfterLogin();
				// sciagamy kontakty
				Action doAfterDownloadContacts = () =>
					{
						instance.StaticMethods.OnNextStep( LoginPhases.DownloadingContactsStatuses );
					};
				if (shouldDownloadContacts == true)
				{
					// wyczysc stare zanim sciagniesz nowe
					this.CurrentContactsList = new ContactBook();
					this.actualizeContactsList( profileToLogin, () =>
					{
						doAfterDownloadContacts();
					},
						instance.StaticMethods.OnException, instance.StaticMethods.OnTimeout );
				}
				else
				{
					doAfterDownloadContacts();
				}
			} ).Add( ( instance ) =>
			{
				Action goToNextStep = () =>
					{
						instance.StaticMethods.OnNextStep( LoginPhases.Connecting );
					};
				if (shouldGetUserStatuses)
				{
					// sciagamy statusy z serwera
					this.DownloadStatusesForPeopleOnCurrentContactList( () =>
					{
						goToNextStep();

					}, onTimeoutCallback );
				}
				else
				{
					goToNextStep();
				}
			} ).Add( ( instance ) =>
			{
				//___ SUKCES!____
				instance.StaticMethods.StopTimeoutTimer();
				//	this.listenFromLastPassedStatusChangedHandler();
				this.Engine.StartReceivingMessages( ( message ) =>
				{
					// true / false oznacza czy wiadomosc zostala dostarczona do wlasciwej klasy odbiorcy czy nie
					if (this.onReceiveMessage != null)
					{
						return this.onReceiveMessage( message );
					}
					else
					{
						return false;
					}
				} );
				instance.StaticMethods.OnSuccess();
			} );
			loginCor.StaticMethods = new LoginArgs
				{
					// metoda wykona sie w razie wystapienia wyjątku, łańcuch zostaje przerwany
					OnException = ( exception ) =>
					{
						if (loginCor.StaticMethods.IsStopped == false)
						{
							loginCor.StaticMethods.IsCancelled = true;
							if (onErrorCallback != null)
							{
								onErrorCallback( exception );
							}
						}
					},
					// metody uzywa sie by uruchomic nastepny krok w łańcuchu
					OnNextStep = ( phaseChange ) =>
						{
							if (loginCor.StaticMethods.IsStopped == false)
							{
								{
									// zmienia sie faza
									loginCor.StaticMethods.OnPhaseChange( phaseChange );
									// przejscie do kolejnej metody

									loginCor.CallNext();
								}
							}
						},
					// ta metoda umozliwia zmiane fazy łańcucha responsibilities
					OnPhaseChange = ( phaseChange ) =>
					{
						if (loginCor.StaticMethods.IsStopped == false)
						{
							if (onLoginPhaseChangedCallback != null)
							{
								// call na zewnatrz
								onLoginPhaseChangedCallback( phaseChange );
							}
						}

					},
					// ta metoda wykona sie w razoe powodzenia procesu w łańcuchu
					OnSuccess = () =>
					{
						loginCor.StaticMethods.IsSucessfullyConnected = true;
						if (onSuccessCallback != null)
						{
							onSuccessCallback();
						}
					},
					// ta metoda wykona sie jezeli któryś z procesów będzie trwał zbyt długo.
					OnTimeout = () =>
					{
						if (loginCor.StaticMethods.IsStopped == false)
						{
							loginCor.StaticMethods.IsCancelled = true;
							if (onTimeoutCallback != null)
							{
								onTimeoutCallback();
							}
						}
					},
					onUnhandledException = () =>
						{
							if (loginCor.StaticMethods.IsStopped == false)
							{
								loginCor.StaticMethods.IsCancelled = true;
								if (onUnhandledExceptionCallback != null)
								{
									onUnhandledExceptionCallback();
								}
							}
						}
				};
			// ta metoda wykonuje sie zawsze zanim wykona sie kolejny element łańcucha
			loginCor.BeforeCallNext = ( instance ) =>
			{
				if (instance.StaticMethods.IsStopped)
				{
					// przerwij wykonanie jesli łańcuch został anulowany
					instance.AbortExecution = true;
					return false;
				}

				return true;
			};
			// gdy wystapi wyjatek podczas wykonywania jakiejs akcji
			loginCor.OnExceptionOccured = ( instance, exception ) =>
				{
					// to wtedy anuluj wszystko.
					instance.AbortExecution = true;
					// podczepiam pod timeouta bo to prawie to samo.
					loginCor.StaticMethods.onUnhandledException();
				};
			loginCor.Start();
		}

		private void onReceiveStatusChange( NotifyReply status )
		{
			this.actualizeStatusInStatusesList( status, this.ListOfContactStatuses );
			if (this.StatusChangeReceivedEvent != null)
			{
				this.StatusChangeReceivedEvent( status );
			}
		}

		public void StartLoginProcessForNumber( int ggNumber, Action onSuccessCallback,
			Action<LoginPhases> onLoginPhaseChangedCallback, Action onTimeoutCallback, Action<Exception> onErrorCallback,
			Action onUnhandledException, bool shouldLoadContacts, bool shouldDownloadStatuses, bool isReconnect )
		{
			var currentProfile = this.GetUserProfileByNumber( ggNumber );
			this.StartLoginProcess( currentProfile, onSuccessCallback, onTimeoutCallback, onErrorCallback,
				onLoginPhaseChangedCallback, onUnhandledException, shouldLoadContacts, shouldDownloadStatuses, isReconnect );
		}

		public void StartReceivingMessages( Func<GG_RECV_MSG80, bool> onReceiveMessage )
		{
			this.onReceiveMessage = onReceiveMessage;
		}

		public void tombstoneWasHandled()
		{
			this.isLastTombstoneUnhandled = false;
		}

		private void actualizeContactsList( UserProfile profileToActualize, Action successCallback,
			Action<Exception> onErrorCallback, Action onTimeoutCallback )
		{
			var appData = this;
			// jesli lista kontaktow jest pusta to na pewno trzeba sciagnac nową
			//if (appData.ContactsListForCurrentLoggedUser == null)
			var downloadContactsFromServer = false;
			var shouldDownloadFreshStatuses = false;
			// jesli mamy wersje zero to pobrac trzeba na bank
			if (profileToActualize.ContactsListVersion == 0) { shouldDownloadFreshStatuses = true; }
			// takie wartosci rowniez moga oznaczac powazne błędy
			if ((this.CurrentContactsList.Groups.Count < 2) || (this.CurrentContactsList.Contacts.Count < 0))
			{
				shouldDownloadFreshStatuses = true;
			}
			if (shouldDownloadFreshStatuses)
			{
				this.ImportContactsListAndSaveToCurrentProfile( profileToActualize, () =>
					{
						successCallback();
					} );
			}
			else
			{
				appData.Engine.GetVersionOfContactsListOnServer( ( version ) =>
				{
					var contactsVersion = ( int ) version;

					if (contactsVersion != profileToActualize.ContactsListVersion)
					{
						downloadContactsFromServer = true;
					}
					else
					{
						// wersja sie zgadza, idziemy do zalogowanych od razu
						this.LoadContactsListForCurrentLoggedUser();
						// jezeli kontakty nie sa puste
						if (!(this.CurrentContactsList.Contacts == null ||
							this.CurrentContactsList.Contacts.Count == 0))
						{
							successCallback();
						}
						else
						{
							downloadContactsFromServer = true;
						}
					}
					// sciagamy liste kontaktow bo tak trzeba
					if (downloadContactsFromServer)
					{
						this.ImportContactsListAndSaveToCurrentProfile( profileToActualize, () =>
						{
							successCallback();
						} );
					}
				} );
			}
		}

		/// <summary>
		/// Wkleja pierwsze statusy do drugich, albo zamienia je jezeli juz istnieją
		/// </summary>
		private void actualizeStatusInStatusesList( List<NotifyReply> incomingStatuses, List<NotifyReply> statusesList )
		{
			if (incomingStatuses != null)
			{
				incomingStatuses.ForEach( status =>
					{
						// czasami gadu nie wiedziec czemu wysyla mi statusy osob ktorych nie mam na liscie.
						// a wiec takie statusy obcinam.
						if (this.GetContactByNumber( ( int ) status.Number ) != null)
						{
							if (this.actualizeStatusInStatusesList( status, statusesList ) == false)
							{
								statusesList.Add( status );
							}
						}
					} );
			}
		}

		/// <summary>
		/// Znajduje w liscie statusów odpowiednik reply status, i podmienia go. Zwraca true jezeli znaleziono
		/// taki status, false jezeli nie znaleziono.
		/// </summary>
		private bool actualizeStatusInStatusesList( NotifyReply status, List<NotifyReply> statusesList )
		{
			if (statusesList != null)
			{
				for (int i = 0, max = statusesList.Count; i < max; i++)
				{
					var curr = statusesList[i];
					if (curr != null)
					{
						if (curr.Number == status.Number)
						{
							statusesList[i] = status;
							return true;
						}
					}
				}
			}
			return false;
		}

		private void createInitialStatusesUnavailableFromContactList( List<Contact> contacts )
		{
			this.ListOfContactStatuses = this.createStatusesListFromContactList( contacts );
		}

		private List<NotifyReply> createStatusesListFromContactList( List<Contact> contacts )
		{
			var list = new List<NotifyReply>();
			contacts.ForEach( contact =>
			{
				list.Add( new NotifyReply
				{
					Description = "",
					Number = ( uint ) contact.GGNumber,
					Status = GGStatus.NotAvailable
				} );
			} );
			return list;
		}

		private void ImportContactsListAndSaveToCurrentProfile( UserProfile currentProfile, Action successCallback )
		{
			var userprofile = currentProfile;
			this.Engine.GetContactsListFromServer( ( version, contactsList ) =>
			{
				CurrentContactsList = contactsList;
				userprofile.ContactsListVersion = version;
				var contacts = CurrentContactsList.Contacts;
				this.createInitialStatusesUnavailableFromContactList( contacts );
				if (successCallback != null) { successCallback(); }
			} );

		}

		/// <summary>
		/// rozpoczyna nasluch na zmieniajace sie statusy korzystajac z domyslnego handlera
		/// jaki zostal zaproponowany metoda startlistenforstatuschanged
		/// </summary>
		private void listenFromLastPassedStatusChangedHandler()
		{
			this.Engine.StartReceivingStatusChanges( ( status ) =>
			{
				this.actualizeStatusInStatusesList( status, this.ListOfContactStatuses );
				if (this.StatusChangeReceivedEvent != null)
				{
					this.StatusChangeReceivedEvent( status );
				}
			} );
		}

		private void loadMainSettings()
		{
			this.settings = new MainApplicationSettings();
			Exception exception = null;
			this.settings.Load( out exception );
#if DEBUG
			if (exception != null)
			{
				this.Helper.Diagnose( () => this.Helper.DiagnosticMessage( "wystapil wyjatek przy wczytywaniu ustawien. Wyjatek to " + exception.Message ) );
			}
#endif
		}


		/// <summary>
		/// wysyla liste kontaktow na serwer
		/// </summary>
		private void sendContactsToServer( Action<uint> onSuccess, Action onFail )
		{
			this.Engine.SendContactsListToServer( this.currentLoggedProfile.ContactsListVersion,
				this.CurrentContactsList, ( version ) =>
				{
					this.currentLoggedProfile.ContactsListVersion = version;
					if (onSuccess != null) { onSuccess( version ); }
				}, () =>
					{
						if (onFail != null) { onFail(); }

					} );
		}

		/// <summary>
		/// Umozliwia ustawienie statusu
		/// </summary>
		/// <param name="archivizeInClient">jezeli true to zapisuje to w kliencie i ustawia na pasku stanu</param>
		private void SetStatus( GGStatus status, string description, bool archivizeInClient )
		{
			bool receiveLinksFromUnknowns = this.AdditionalSettings.ReceiveLinksFromUnknowns;
			bool isMobileClient = this.AdditionalSettings.MobileClient;
			bool forFriendsOnly = this.currentLoggedProfile.ForFriendsOnly;
			SetStatus( status, description, receiveLinksFromUnknowns, isMobileClient, forFriendsOnly, archivizeInClient );
		}

		/// <summary>
		/// Umozliwia ustawienie statusu
		/// </summary>
		private void SetStatus( GGStatus status, string description, bool receiveLinksFromUnknowns, bool isMobileClient,
			bool forFriendsOnly, bool archivizeInClient )
		{
			this.Engine.SetStatus( new SetStatusOptions
			{
				Description = description,
				Status = status,
				StatusIsSet = true,
				ReceiveLinksFromUnknowns = receiveLinksFromUnknowns,
				MobileClient = isMobileClient,
				ForFriendsOnly = forFriendsOnly,
			} );

			if (this.StatusChanged != null)
			{
				this.StatusChanged( status, description );
			}
		}




		/// <summary>
		/// Na zewnetrzne żądanie ładuje liste kontaktow zapisana w isolatedstorage.
		/// </summary>
		internal void LoadContactsListForCurrentLoggedUser()
		{
			var document = new ContactsListDocument( this.currentLoggedProfile.AccountNumber );
			document.Load();
			if (document.ContactsList != null)
			{
				this.CurrentContactsList = document.ContactsList;
			}
		}
		private void SetStatus( StatusArgs args )
		{
			this.SetStatus( args.status, args.description, false );
		}


		public void StartReceivingQueuedMessagesFromAgent( Func<List<MessageModel>,bool> receive )
		{
			var pack = this.WorkerData.GetPackByNumber( this.currentLoggedProfile.AccountNumber );
			if (pack != null)
			{
				receive( pack.MessagesUnreaded );
				pack.MessagesUnreaded.Clear();
				try
				{
					// mus zapisac bo sie nie zaznacza jako przeczytane
					this.bgWorkerData.Save();
				}
				
				catch (Exception) { }
			}

		}

		/// <summary>
		/// Ustawia status gdy user nacisnie przycisk zmiany
		/// </summary>
		/// <param name="args"></param>
		public void SetStatusNormally( StatusArgs args )
		{
			this.currentLoggedProfile.LastDescription = args.description;
			this.currentLoggedProfile.LastStatus = args.status;
			this.SetStatus( args );
		}
		/// <summary>
		/// ustawia status przy tombstonie
		/// </summary>
		public void SetStatusOnTombstoning()
		{
			if (this.currentLoggedProfile.SetStatusOnTombstone)
			{

				this.SetStatus( new StatusArgs
				{
					description = this.currentLoggedProfile.DescriptionStatusOnTombstone,
					status = GGStatus.InvisibleDesc
				} );
			}
		}
		/// <summary>
		/// ustawia status przy untombstonieniu
		/// </summary>
		public void SetStatusOnUntombstoning()
		{
			this.SetStatusLast();
		}
		/// <summary>
		/// ustawia ostatni status ustawiony przez nas lub przy logowaniu
		/// </summary>
		public void SetStatusLast()
		{
			this.SetStatus( this.getStatusForReconnect() );
		}
		/// <summary>
		/// ustawia dobry status przy logowaniu
		/// </summary>
		public void SetStatusOnLogin()
		{
			this.SetStatusNormally( this.getStatusForAfterLogin() );
		}

		private StatusArgs getStatusForReconnect()
		{
			return new StatusArgs
			{
				description = this.currentLoggedProfile.LastDescription,
				status = this.currentLoggedProfile.LastStatus
			};
		}
		private StatusArgs getStatusForAfterLogin()
		{
			var statuses = new StatusArgs
			{
				description = this.currentLoggedProfile.LastDescription,
				status = this.currentLoggedProfile.LastStatus
			};
			if (this.currentLoggedProfile.SetLastSettedStatusAfterLogin == false)
			{
				statuses.description = this.currentLoggedProfile.StatusDescriptionAfterLogin;
				statuses.status = this.currentLoggedProfile.StatusAfterLogin;
			}
			return statuses;
		}

		public class StatusArgs
		{
			public GGStatus status;
			public string description;
		}




		class LoginArgs
		{
			#region Fields (6)

			public Action<Exception> OnException;
			/// <summary>
			/// Nastepny krok w chain of responsibility
			/// </summary>
			public Action<LoginPhases> OnNextStep;
			/// <summary>
			/// Tylko zmiana fazy bez nastepnego kroku
			/// </summary>
			public Action<LoginPhases> OnPhaseChange;
			public Action OnSuccess;
			/// <summary>
			/// Uruchamiane gdy nastąpi timeout
			/// </summary>
			public Action OnTimeout;
			private Timer timeoutTimer;

			#endregion Fields

			#region Properties (4)

			public bool IsCancelled { get; set; }

			public bool IsStopped
			{
				get { return (this.IsCancelled || this.IsSucessfullyConnected); }
			}

			public bool IsSucessfullyConnected { get; set; }

			public Action onUnhandledException { get; set; }

			#endregion Properties

			#region Methods (2)

			// Public Methods (2) 

			/// <summary>
			/// Tylko tyle czasu bedzie mial na wykonanie nastepny step zanim jego wykonanie zostanie przerwane.
			/// </summary>
			public void SetTimeoutTimer( int maxAllowedExecutionTimeInMiliseconds )
			{
				this.StopTimeoutTimer();
				this.timeoutTimer = new Timer( maxAllowedExecutionTimeInMiliseconds, () =>
					{
						this.StopTimeoutTimer();
						this.OnTimeout();
					}, true ).Start();
			}

			public void StopTimeoutTimer()
			{
				if (this.timeoutTimer != null)
				{
					this.timeoutTimer.Stop();
				}
			}

			#endregion Methods
		}


		private static Regex StripTagsHtmlRegex
		{
			get
			{
				if (stripTagsRegex == null)
				{
					stripTagsRegex = new Regex( "<.*?>", RegexOptions.Compiled );
				}
				return stripTagsRegex;
			}
		}
		private static Regex stripTagsRegex;
		public static string StripHtmlTags( string input )
		{
			var messageText = StripTagsHtmlRegex.Replace( input, "" );
			messageText = HttpUtility.HtmlDecode( messageText );
			return messageText;
		}
		public static MessageModel MessagePacketToModel( GG_RECV_MSG80 message )
		{
			var date = UnixTimeStampToDateTime( message.Time );
			var msgPlainText = StripHtmlTags( message.HtmlMessage );
			var msg = new MessageModel
			{
				IsIncoming = true,
				MessageContent = msgPlainText,
				SendDate = date,
				SenderNumber=(int)message.Sender
			};
			return msg;
		}

		public static DateTime UnixTimeStampToDateTime( double unixTimeStamp )
		{
			System.DateTime dtDateTime = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
			dtDateTime = dtDateTime.AddSeconds( unixTimeStamp ).ToLocalTime();
			return dtDateTime;
		}
	}


	/// <summary>
	/// Val obj do dodawania kontaktów
	/// </summary>
	public class AddContactParams
	{

		public string Name { get; set; }

		public int Number { get; set; }
	}
	/// <summary>
	/// Wszystkie dane powiązane z profilem uzytkownika.
	/// </summary>
	public class UserProfile
	{

		private bool tombstone;



		/// <summary>
		/// Nazwa konta uzytkownika
		/// </summary>
		public string AccountName { get; set; }

		/// <summary>
		/// Numer konta uzytkownika
		/// </summary>
		public int AccountNumber { get; set; }

		/// <summary>
		/// Haslo do konta uzytkownika
		/// </summary>
		public string AccountPassword { get; set; }

		/// <summary>
		/// wersja listy kontaktów
		/// </summary>
		public uint ContactsListVersion { get; set; }

		/// <summary>
		/// Opis do ustawienia przy tombstoningu
		/// </summary>
		public string DescriptionStatusOnTombstone { get; set; }

		/// <summary>
		/// tylko dla znajomych ustawiaj status
		/// </summary>
		public bool ForFriendsOnly { get; set; }

		/// <summary>
		/// Ostatni opis
		/// </summary>
		public string LastDescription { get; set; }

		/// <summary>
		/// Ostatni ustawiony przez nas recznie status
		/// </summary>
		public GGStatus LastStatus { get; set; }

		/// <summary>
		/// Jezeli true przywraca poprzedni status po zalogowaniu
		/// </summary>
		public bool SetLastSettedStatusAfterLogin { get; set; }

		/// <summary>
		/// Czy ustawiac status przy tombstoningu 
		/// </summary>
		public bool SetStatusOnTombstone
		{
			get
			{
				return this.tombstone;
			}
			set
			{
				if (value == false)
				{
					var chuj = "break";
				} this.tombstone = value;
			}
		}

		/// <summary>
		/// Status do ustawienia po zalogowaniu
		/// </summary>
		public GGStatus StatusAfterLogin { get; set; }

		/// <summary>
		/// Opis do ustawienia po zalogowaniu
		/// </summary>
		public string StatusDescriptionAfterLogin { get; set; }
	}

	/// <summary>
	/// Helper do kasowania polskich znaków ze stringów
	/// </summary>
	public class PolishChars
	{


		public static string CutPolishChars( string input )
		{
			if (input != null)
			{
				input = input.ToLower();
				var sb = new StringBuilder( string.Empty );
				for (int i = 0, max = input.Length; i < max; i++)
				{
					var oldChar = input[i];
					switch (oldChar)
					{
						case 'ą': oldChar = 'a'; break;
						case 'ę': oldChar = 'e'; break;
						case 'ó': oldChar = 'o'; break;
						case 'ż': oldChar = 'z'; break;
						case 'ź': oldChar = 'z'; break;
						case 'ć': oldChar = 'c'; break;
						case 'ń': oldChar = 'n'; break;
						case 'ł': oldChar = 'l'; break;
						case 'ś': oldChar = 's'; break;
					}

					sb.Append( oldChar );
				}
				return sb.ToString();
			}
			return null;
		}
	}

	/// <summary>
	/// Helper do konwersji kolorow
	/// </summary>
	public class ColorHelper
	{


		public static Color addToLuminance( Color color, int howMuch )
		{
			color.R = subFromColor( color.R, howMuch );
			color.G = subFromColor( color.G, howMuch );
			color.B = subFromColor( color.B, howMuch );
			return color;
		}

		public static Color subFromLuminance( Color color, int howMuch )
		{
			return addToLuminance( color, howMuch * -1 );
		}

		private static byte subFromColor( byte val, int howMuch )
		{
			var value = val + howMuch;
			if (value < 0)
			{
				return 0;
			}
			if (value > 255)
			{
				return 255;
			}
			else return ( byte ) value;
		}
	}





}
