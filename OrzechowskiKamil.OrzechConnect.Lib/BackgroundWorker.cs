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
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using OrzechowskiKamil.OrzechConnect.Lib.InPackets;
using OrzechowskiKamil.OrzechConnect.Lib.DataStorage;

namespace OrzechowskiKamil.OrzechConnect.Lib
{
	// interfejs definiuje klase zdolną pokazywać toasty
	public interface IToastCaller
	{
		void ShowToast( string title, string message );
	}


	public class BackgroundWorker
	{

		public WorkerData Data;
		private int defaultProfileNumber;
		private bool isCallbackAlreadyCalled;
		private bool isSuccessfullFinishedAllready;
		private Action onFail;
		private OnBackgroundWorkerSuccess onSuccess;
		private int secondsAmountOfWorking;
		private Timer timer;
		private bool dontStart;

		public IToastCaller ToastCaller { get; set; }
		private void ShowToast( string title, string message )
		{
			if (this.ToastCaller != null)
			{
				// to tylko do debuggingu
		//		this.ToastCaller.ShowToast( title, message );
			}
		}

		public BackgroundWorker()
		{
#if DEBUG
			this.helper = new DiagnosticHelper( BackgroundWorker.IsDiagnosticHelperEnabled,
				"BackgroundWorker: ", "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" );
#endif
			this.defaultProfileNumber = AppGlobalData.Current.DefaultProfileNumber;
			if (this.defaultProfileNumber == 0)
			{
				this.dontStart = true;
			}
			else
			{
				this.Data = this.GetBackgroundWorkerData();
			}

		}



		private UnreadedMessagesFor messagesUnreadedFor
		{
			get
			{
				var toreturn = this.Data.GetPackByNumber( defaultProfileNumber );
				if (toreturn == null)
				{
					toreturn = new UnreadedMessagesFor { ReceiverNumber = defaultProfileNumber };
					this.Data.UnreadedMessagesPacks.Add( toreturn );
				}
				return toreturn;
			}

		}




		public delegate void OnBackgroundWorkerSuccess( int howMuchMessagesArrived );




		public WorkerData GetBackgroundWorkerData()
		{
			var data = new BackgroundWorkerData();
			data.Load();
			return data.Data;
		}

		private bool shouldNotRun()
		{
			if (this.dontStart == true)
			{
#if DEBUG
				ShowToast( "blad", "numer domyslny to zero, nie ma co wczytac" );
#endif
				return true;

			}
			var now = DateTime.Now;
			var isNight = (now.Hour < 8) || (now.Hour >= 22);
			var shouldCancelOnNight = AppGlobalData.Current.AdditionalSettings.AgentEnabledInNight == false;

			if (isNight && shouldCancelOnNight)
			{
#if DEBUG
				ShowToast( "stop!", String.Format( "noc: {0}, pow_w_noc: {1}", isNight, shouldCancelOnNight ) );
#endif
				return true;

			}
			else
			{
#if DEBUG
				ShowToast( "Dzialam!", String.Format( "noc: {0}, pow_w_noc: {1}", isNight, shouldCancelOnNight ) );
#endif
				return false;
			}
		}

		public void GetResult( OnBackgroundWorkerSuccess onSuccess, Action onFail )
		{
			if (this.shouldNotRun() == true) { onFail(); }
			else
			{
				if (this.shouldBypass() == true) { onFail(); }
				else
				{
					this.onSuccess = onSuccess;
					this.onFail = onFail;
					//var credentials = this.GetCredentials();
					//if (credentials != null)
					//{
					this.GetMessages();
					//}
				}
			}
		}

		public void SaveBackgroundWorkerData( WorkerData data )
		{
			var dataX = new BackgroundWorkerData();
			dataX.Data = data;
			dataX.Save();
		}

		private void Call()
		{
			if (doBeforeCall())
			{
				if (this.onFail != null)
				{
					this.onFail();
				}

			}
		}

		private void CallFinish( int howMuchMessages )
		{
			if (doBeforeCall())
			{
				if (this.onSuccess != null)
				{
					this.onSuccess( howMuchMessages );
				}
			}
		}

		private void Debug( Action debugAction )
		{
#if DEBUG
			this.helper.Diagnose( debugAction );
#endif

		}

		private void DiagMsg( string msg )
		{
#if DEBUG
			this.helper.DiagnosticMessage( msg );
#endif
		}

		private void DiagMsg( string msg, params string[] args )
		{
#if DEBUG
			this.helper.DiagnosticMessage( String.Format( msg, args ) );
#endif
		}

		private bool doBeforeCall()
		{
			if (this.isCallbackAlreadyCalled == false)
			{
				this.isCallbackAlreadyCalled = true;
				return true;
			}
			return false;
		}

		//        private GGCredentials GetCredentials()
		//        {

		//            if (defaultProfileNumber != 0)
		//            {
		//#if DEBUG
		//                this.Debug( () => this.DiagMsg( "Domyslny numer to {0}", defaultProfileNumber.ToString() ) );
		//#endif
		//                var profile = AppGlobalData.Current.GetUserProfileByNumber( defaultProfileNumber );
		//                if (profile != null)
		//                {
		//                    var credentials = new GGCredentials() { Number = defaultProfileNumber };
		//                    credentials.Password = profile.AccountPassword;
		//#if DEBUG
		//                    this.Debug( () => this.DiagMsg( "OK! wczytalem dane dla profilu. numer juz znasz," +
		//                        " haslo to {0}", credentials.Password ) );
		//#endif
		//                    return credentials;
		//                }
		//                else
		//                {
		//#if DEBUG
		//                    this.Debug( () => this.DiagMsg( "Niestety profil o numerze {0} nieistnieje",
		//                        defaultProfileNumber.ToString() ) );
		//#endif
		//                }
		//            }
		//            else
		//            {
		//#if DEBUG
		//                this.Debug( () => this.DiagMsg( "Domyslny numer to zero. nic nie wczytuje." ) );
		//#endif
		//            }
		//            return null;
		//        }
		private List<MessageModel> GetMessages()
		{
			if (this.messagesUnreadedFor == null) { return null; }

			var result = new List<MessageModel>();
			var appData = AppGlobalData.Current;
			this.secondsAmountOfWorking = 0;
			// timer ma 14 sekund na wszystko. pozostale dwie zostaiwam na zapisanie danych.
			this.timer = new Timer( 1, () =>
			{
				this.secondsAmountOfWorking++;

				if (this.secondsAmountOfWorking > 14)
				{
#if DEBUG
					ShowToast( "Stop!", "Timer nie wyrobil sie w czasie 14 sekund." );

#endif
					this.onSuccessfulFinish();
				}
			}, false, false ).Start();
			appData.StartLoginProcessForNumber( this.defaultProfileNumber, () =>
				{
#if DEBUG
					ShowToast( "Sukces", "wlasnie udalo sie zalogowac na serwer" );
#endif
					// skoro juz jestesmy zalogowani to sciagniecie wiadomosci powinno byc ultra szybkie.
					// tylko 4 sekundy daje na to;p

					new Timer( 4, () =>
						{
#if DEBUG
							ShowToast( "sukces", "Wiadomosci sciagniete" );
#endif
							this.onSuccessfulFinish();
						}, false, true ).Start();
					appData.StartReceivingMessages( this.onReceiveMessage );
				},
				( phase ) =>
				{
					//phase change
				},
				() =>
				{
					ShowToast( "-timeout", "timeout" );
					this.Call();
				},
				( exception ) =>
				{
					ShowToast( "-exception", "" );
					this.Call();
				},
				() =>
				{
					ShowToast( "-unhandled exception", "timeout" );
					this.Call();
				},
				false, false, true );
			return result;
		}

		private void onFinishingWork()
		{
			this.Data.BytesReceivedByAgent += AppGlobalData.Current.BytesDownloadedAtThisSession;
			this.Data.BytesSendedByAgent += AppGlobalData.Current.BytesSendedAtThisSession;
			this.SaveBackgroundWorkerData( this.Data );
		}

		private bool onReceiveMessage( GG_RECV_MSG80 message )
		{
#if DEBUG
		//	ShowToast( "bb", "dostalem wiadomosc " + message.HtmlMessage );
#endif
			MessageModel msg = AppGlobalData.MessagePacketToModel( message );
			this.messagesUnreadedFor.MessagesUnreaded.Add( msg );
			return true;
		}

		private void onSuccessfulFinish()
		{
			if (this.isCallbackAlreadyCalled == false && this.isSuccessfullFinishedAllready == false)
			{
				if (this.timer != null)
				{
					this.timer.Stop();
					this.timer = null;
				}
				this.isSuccessfullFinishedAllready = true;
#if DEBUG
				ShowToast( "sukces", "koncze dzialanie" );
#endif
				var howMuch = 0;
				this.onFinishingWork();
				try
				{
					howMuch = this.messagesUnreadedFor.MessagesUnreaded.Count;
				}
				catch (Exception) { }
				this.CallFinish( howMuch );
			}
		}

		private bool shouldBypass()
		{
			var howMuchToBypass = AppGlobalData.Current.AdditionalSettings.AgentHowMuchStartupsToBypass;
			var howMuchBypasesMade = this.Data.HowMuchBypassessMade;
			var value = false;
			if (howMuchToBypass > howMuchBypasesMade)
			{
#if DEBUG
				ShowToast( "stop!", String.Format( "{0} > {1}=true", howMuchToBypass,
					howMuchBypasesMade ) );
#endif
				this.Data.HowMuchBypassessMade++;
				value = true;
			}
			else
			{
#if DEBUG
				ShowToast( "dzialam", "nie omijam tylko sie od razu uruchamiam!" );
#endif
				this.Data.HowMuchBypassessMade = 0;
			}
			this.SaveBackgroundWorkerData( this.Data );
			return value;
		}


#if DEBUG
		private DiagnosticHelper helper;
		private const bool IsDiagnosticHelperEnabled = true;
#endif
	}



}
