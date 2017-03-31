using OrzechowskiKamil.OrzechConnect.Lib.InPackets;
using System;
using OrzechowskiKamil.OrzechConnect.Lib.Connection;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;

namespace OrzechowskiKamil.OrzechConnect.Lib.Processess
{
	public class LoginProcessArgs
	{
		/// <summary>
		/// Uruchamiany podczas pomyslnego zalogowania
		/// </summary>
		public Action OnLoginSuccess;
		/// <summary>
		/// Uruchamiany podczas niepomyslnego zalogowania (złe hasło)
		/// </summary>
		public Action OnLoginFail;
		/// <summary>
		/// Uruchamiany gdy nastąpi błąd podczas zalogowania
		/// </summary>
		public Action<Exception> OnLoginError;
		/// <summary>
		/// Numer i haslo GG
		/// </summary>
		public GGCredentials Credentials;
		/// <summary>
		/// Opis po zalogowaniu sie do serwera
		/// </summary>
		public GGStatus InitialStatus;
		/// <summary>
		/// Opis tekstowy po zalogowaniu sie do serwera
		/// </summary>
		public string InitialStatusDescription;
		/// <summary>
		/// Maksymalna wielkosc obrazka w kb
		/// </summary>
		public byte MaxImageSizeInKB;
	}
	public class LoginProcessException : Exception
	{
		public LoginProcessException( string message ) : base( message ) { }
	}

	public class LoginProcessTimedOut : LoginProcessException
	{
		public LoginProcessTimedOut( string message ) : base( message ) { }
	}

	public class LoginConnectionToServerError : LoginProcessException
	{
		public LoginConnectionToServerError( string message ) : base( message ) { }
	}


	/// <summary>
	/// Proces odpowiada za logowanie i podłączenie do serwera.
	/// </summary>
	public class LoginProcess : Process
	{
		/// <summary>
		/// tyle sekund to wystarczająco dużo by się wkurwic ze jeszcze nie zalogowało.
		/// </summary>
		private const int IntervalBeforeTimeout = 10;
		PacketManager manager;
		LoginProcessArgs args;
		private bool isConnected;
		public LoginProcess( PacketManager manager, LoginProcessArgs args )
		{
			this.manager = manager;
			this.args = args;
		}

		public void Start()
		{
			try
			{
				this.manager.RegisterProcess( this );
				try
				{
					this.manager.Connect( this.args.Credentials.Number, ( exception ) =>
						{
							this.args.OnLoginError( exception );
						} );
				}
				catch (LoginProcessException e)
				{
					throw e;
				}
				catch (Exception)
				{
					throw new LoginConnectionToServerError( "Wystąpił błąd podczas próby podłączenia do serwera." );
				}
				new Timer( IntervalBeforeTimeout, () =>
				{
					if (isConnected == false)
					{
						this.args.OnLoginError(
							new LoginProcessTimedOut( "Serwer nie odpowiada. Logowanie trwało zbyt długo i " +
							" zostało przerwane. Spróbuj ponownie." ) );
					}
				} );
			}
			catch (LoginProcessException e)
			{
				this.args.OnLoginError( e );
			}
			catch (Exception)
			{
				this.args.OnLoginError( new LoginProcessException( "Błąd podczas próby połączenia." ) );
			}
		}

		public override bool OnPacketReceived( IInTcpPacket packet )
		{

			try
			{
				if (packet is GG_WELCOME)
				{
					isConnected = true;
					this.OnWelcome( ( GG_WELCOME ) packet );
				}
				else if (packet is GG_LOGIN_OK80)
				{
					isConnected = true;
					this.OnLoginSuccess( ( GG_LOGIN_OK80 ) packet );
				}
				else if (packet is GG_LOGIN_FAIL)
				{
					isConnected = true;
					this.OnLoginFail( ( GG_LOGIN_FAIL ) packet );
				}
			}
			catch (Exception e)
			{
				this.args.OnLoginError( e );
			}
			return false;

		}  

		private void OnWelcome( GG_WELCOME packet )
		{
			var seed = packet.Seed;
			var loginPacket = new Login()
			{
				GGNumber = this.args.Credentials.Number,
				Password = this.args.Credentials.Password,
				Seed = seed,
				ImageSizeInKB = this.args.MaxImageSizeInKB,
				InitialStatus = GGStatus.InvisibleDesc,
				//this.args.InitialStatus,
				InitialStatusDescription = this.args.InitialStatusDescription
			};
			this.manager.SendPacket( loginPacket );
		}

		private void OnLoginSuccess( GG_LOGIN_OK80 packet )
		{
			this.args.OnLoginSuccess();
			this.endProcess();
		}

		private void OnLoginFail( GG_LOGIN_FAIL packet )
		{
			this.args.OnLoginFail();
			this.endProcess();
		}

		private void endProcess()
		{
			this.manager.UnregisterProcess( this );
		}

	}
}
