using System;
using System.Net;
using System.Net.Sockets;

namespace OrzechowskiKamil.OrzechConnect.Lib.Connection
{
	public class Connection
	{
		private const int DefaultGGPort = 8074;

		/// <summary>
		/// To co musimy przekazać aby się zalogować
		/// </summary>
		public class ConnectionArgs
		{
			public int GGNumber;
			public Action OnSuccessfulSend;
			public Action<Exception> OnError;
			public Action<SocketAsyncEventArgs> OnReceieveData;
			public Action OnDisconnection;
			public Action OnConnect;
		}

		public void Disconnect()
		{

			if (this.socket != null)
			{
				this.socket.Disconnect();
			}
		}



		private ConnectionArgs args;
		private TcpSocketAsync socket;


		public Connection( ConnectionArgs args )
		{
			this.args = args;
		}

		/// <summary>
		/// Sam znajduje adres aktywnego serwera GG i łączy sie z nim.
		/// </summary>
		public void Connect()
		{
			var ggActServersEngine = new GaduGaduActiveServers( this.args.GGNumber, ( endPointOfServer ) =>
				{
					if (endPointOfServer == null)
					{
						this.args.OnError( new Exception( "" ) );
					}
					this.Connect( endPointOfServer );
				}, ( exception ) =>
				{
					this.args.OnError( exception );
				} );
			ggActServersEngine.Start();
		}

		/// <summary>
		/// Łączy sie z zadanym serwerem GG
		/// </summary>
		/// <param name="endPointOfServer"></param>
		public void Connect( IPEndPoint endPointOfServer )
		{

			this.socket = new TcpSocketAsync( new TcpSocketAsync.TcpSocketAsyncParams()
			{
				IpEndPoint = endPointOfServer,
				OnError = this.args.OnError,
				OnConnect = this.args.OnConnect,
				OnReceiveData = this.args.OnReceieveData,
				OnSuccessfulSend = this.args.OnSuccessfulSend,
				OnDisconnection = this.args.OnDisconnection
			} );
			this.socket.StartConnecting();
		}

		/// <summary>
		/// Wysyla dane do serwera gg aktualnie podlaczonego
		/// </summary>
		/// <param name="data"></param>
		public void Send( byte[] data )
		{
			this.socket.StartSending( data );
		}
	}

}
