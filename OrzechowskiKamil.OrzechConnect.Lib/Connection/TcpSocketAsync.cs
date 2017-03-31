using System;
using System.Net;
using System.Net.Sockets;

namespace OrzechowskiKamil.OrzechConnect.Lib.Connection
{

	public class TcpSocketAsyncException : Exception
	{
		public TcpSocketAsyncException( string message )
			: base( message )
		{
		}
	}

	public class TcpSocketAsync
	{

		private TcpSocketAsyncParams allParams;
		/// <summary>
		/// Najwiekszy pakiet gg ma 2x2000 bajtów + headery.
		/// </summary>
		private const int MaxTcpPacketSize = 5000;

		private Socket socket;



		/// <summary>
		/// przestarzale
		/// </summary>
		public enum TcpSocketAsyncErrors
		{
			OtherError, NotConnected, CannotDownloadServerList
		}




		public TcpSocketAsync( TcpSocketAsyncParams allParams )
		{
			this.allParams = allParams;
		}
		/// <summary>
		/// wyglusza wszystkie wyjatki jakie moglyby sie pojawic po uzyciu Disconnect
		/// </summary>
		private bool disposedByForce;
		public void Disconnect()
		{
			this.disposedByForce = true;
			try
			{
				this.socket.Dispose();
			}
			catch (Exception) { }
		}




		private void startListen()
		{
			try
			{
				var responseListener = new SocketAsyncEventArgs();
				responseListener.Completed += onDataReceived;
				var responseBuffer = new byte[MaxTcpPacketSize];
				responseListener.SetBuffer( responseBuffer, 0, MaxTcpPacketSize );
				this.socket.ReceiveAsync( responseListener );
			}
			catch (Exception exception)
			{
				if (this.disposedByForce == false)
				{
					this.allParams.OnError( exception );
				}

			}
		}

		public void StartSending( byte[] data )
		{
			var asyncEvent = new SocketAsyncEventArgs { RemoteEndPoint = this.allParams.IpEndPoint };
			asyncEvent.Completed += onDataSended;
			asyncEvent.SetBuffer( data, 0, data.Length );
			this.socket.SendAsync( asyncEvent );

		}

		public void StartConnecting()
		{
			this.socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
			var connectingConfiguration = new SocketAsyncEventArgs
			{
				RemoteEndPoint = this.allParams.IpEndPoint
			};
			connectingConfiguration.Completed += this.onConnectingFinish;
			this.socket.ConnectAsync( connectingConfiguration );
		}

		private void onConnectingFinish( object sender, SocketAsyncEventArgs e )
		{
			if (this.allParams.OnConnect != null)
			{
				this.allParams.OnConnect();
			}
			this.startListen();
		}

		private void onDataSended( object sender, SocketAsyncEventArgs e )
		{
			if (e.SocketError != SocketError.Success)
			{
				if (this.disposedByForce == false)
				{
					this.allParams.OnDisconnection();
				}
			}
			if (this.allParams.OnSuccessfulSend != null)
			{
				this.allParams.OnSuccessfulSend();
			}
		}

		private void onDataReceived( object sender, SocketAsyncEventArgs e )
		{
			if (e.SocketError != SocketError.Success)
			{
				if (this.disposedByForce == false)
				{
					this.allParams.OnDisconnection();
				}
			}
			if (this.allParams.OnReceiveData != null)
			{
				if (e.BytesTransferred > 0)
				{
					this.allParams.OnReceiveData( e );
				}

			}
			this.startListen();

		}



		public class TcpSocketAsyncParams
		{

			/// <summary>
			/// End point (adres:port) do którego socket ma się łączyć
			/// </summary>
			public IPEndPoint IpEndPoint;
			/// <summary>
			/// Event uruchamiany po pomyślnym połączeniu do serwera
			/// </summary>
			public Action OnConnect;
			/// <summary>
			/// Event uruchamiany gdy wystąpi jakiś błąd
			/// </summary>
			public Action<Exception> OnError;
			/// <summary>
			/// Event uruchamiany po odebraniu danych, zwraca dane w postaci tablicy bajtów(opcjonalny)
			/// </summary>
			public Action<SocketAsyncEventArgs> OnReceiveData;
			/// <summary>
			/// Event uruchamiany po pomyslnym wyslaniu danych
			/// </summary>
			public Action OnSuccessfulSend;
			public Action OnDisconnection;
		}
	}
}
