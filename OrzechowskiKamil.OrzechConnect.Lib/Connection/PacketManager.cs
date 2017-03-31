using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;
using OrzechowskiKamil.OrzechConnect.Lib.InPackets;
using OrzechowskiKamil.OrzechConnect.Lib.Exceptions;
using System.Collections.Generic;
using OrzechowskiKamil.OrzechConnect.Lib.Processess;
using System;
using System.Net.Sockets;
using System.Diagnostics;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using OrzechowskiKamil.OrzechConnect.Lib;

namespace OrzechowskiKamil.OrzechConnect.Lib.Connection
{
	/// <summary>
	/// Nadzoruje wysyłanie i odbieranie pakietów
	/// </summary>
	/// 
	public class PacketManager
	{

		private Connection connection;
		private bool isDisconected;
		private byte[] lastPacketChunk;
		private Action<Exception> onException;
		/// <summary>
		/// Lista procesów mogących wysyłać i odbierać pakiety
		/// </summary>
		private List<Process> processess;
		private const int TimeoutBetweenPacketsInMs = 0;
		private Timer uncompletedTimer;
		/// <summary>
		/// Czas oczekiwania na pozostałe partie pakietu zanim zostanie on odrzucony w całości.
		/// </summary>
		public const int WaitingForUncompletedPacketChunksInterval = Config.WaitingForUncompletedPacketChunksInterval;



		public PacketManager()
		{
			this.processess = new List<Process>();
#if DEBUG
			this.diagnostic = new DiagnosticHelper( DebugMessagesFromPacketManager, DiagnosticHeader, Important );
#endif
			this.RegisterProcess( new HandleUnusedPacketsProcess() );

		}



		public bool Authenticated { get; set; }




		/// <summary>
		/// Event uruchamiany gdy pakiet manager odbierze zadana liczbe bajtów
		/// </summary>
		public event Action<int> BytesReceived;

		/// <summary>
		/// Event uruchamiany gdy pakiet manager wysle zadaną ilosc bajtów
		/// </summary>
		public event Action<int> BytesSended;

		public event Action Disconnected;




		/// <summary>
		/// Ustanawia połączenie TCP z najlepszym dostepnym serwerem
		/// </summary>
		/// <param name="args"></param>
		public void Connect( int ggNumber, Action<Exception> onException )
		{
			this.onException = onException;
			var connection = new Connection( new Connection.ConnectionArgs
			{
				GGNumber = ggNumber,
				OnConnect = this.onConnect,
				OnError = this.onError,
				OnReceieveData = this.OnReceiveData,
				OnSuccessfulSend = this.onPacketSended,
				OnDisconnection = this.onDisconnection
			} );
			connection.Connect();
			this.connection = connection;
#if DEBUG
			this.diagnose( () =>
			{
				this.debug(
					String.Format( "Rozpoczęto podłączanie sie do serwera na numer {0}.", ggNumber ), true );
			} );
#endif
		}

		public void Disconnect()
		{
#if DEBUG
			this.diagnose( () => { this.debug( String.Format( "Rozpoczęto rozłączanie się." ), true ); } );
#endif
			this.isDisconected = true;
			this.connection.Disconnect();
		}

		/// <summary>
		/// Rejestruje nowy proces do odbierania pakietów
		/// </summary>
		/// <param name="process"></param>
		public void RegisterProcess( Process process )
		{
#if DEBUG
			this.diagnose( () => { this.debug( String.Format( "Zarejestrowano proces {0}.", process.ToString() ), false ); } );
#endif
			if (this.processess.Contains( process ) == false)
			{
				this.processess.Add( process );
			}
		}

		/// <summary>
		/// Wysyla pakiet korzystajac z aktualnie utworzonego połączenia
		/// </summary>
		/// <param name="packet"></param>
		public bool SendPacket( IOutTcpPacket packet )
		{
			var packetBinarySize = 0;
			if (this.isDisconected == false)
			{
				if (this.connection == null)
				{
#if DEBUG
					this.diagnose( () => { this.debug( String.Format( "Nie połączono TCP z serwerem GG." ), true ); } );
#endif
					throw new NotEstablishedConnectionException( "Nie połączono jeszcze TCP z serwerem GG." );
				}
				try
				{
					var binaryPacket = packet.ToBinaryPacket();
					if (binaryPacket != null)
					{
						packetBinarySize = binaryPacket.Length;
						if (this.BytesSended != null)
						{
							this.BytesSended( packetBinarySize );
						}
					}
#if DEBUG

					// tutaj uzywam conditional if debug bo co z tego ze diagnostic message sie nie wykona jak 
					// refleksja i tak bedzie musiala pobrac typ wiec nie chce obciazac niepotrzebnie procka.
					// to akurat bedzie bardzo czesto wysyłane. Tam gdzie rzadko albo argumnty są lajtowe to mozna
					nie pisac #if DEBUG
					this.diagnose( () => { this.debug( String.Format( "Próba wysłania pakietu typu {0} o wielkości {1}.", packet.GetType().ToString(), packetBinarySize ), false ); } );

#endif
					this.connection.Send( binaryPacket );
					return true;
				}
				catch (Exception exception)
				{
#if DEBUG
					this.diagnose( () => { this.debug( String.Format( "Błąd podczas wysyłania pakietu typu {0}, o wielkości {1}.", packet.GetType().ToString(), packetBinarySize ), true ); } );
#endif
					// prawdopodobnie nas rozłączyło
					this.onError( exception );
				
				}
			}
			return false;
		}

		/// <summary>
		/// Wyrejestrowuje  proces
		/// </summary>
		/// <param name="process"></param>
		public void UnregisterProcess( Process process )
		{
#if DEBUG
			this.diagnose( () => { this.debug( String.Format( "Proba wyrejestrowania procesu {0}.", process.ToString() ), false ); } );
#endif
			if (this.processess.Contains( process ) == true)
			{
				this.processess.Remove( process );
			}
		}

		/// <summary>
		/// Uruchamia się gdy zostaniemy podłączeni do serwera
		/// </summary>
		protected void onConnect()
		{
#if DEBUG
			this.diagnose( () => { this.debug( String.Format( "Zakończono podłączanie się do serwera." ), false ); } );
#endif
		}

		/// <summary>
		/// Uruchamia sie w momencie wystapienia błędu na linii klient-serwer
		/// </summary>
		/// <param name="error"></param>
		protected void onError( Exception error )
		{
			if (error is System.Net.Sockets.SocketException)
			{
#if DEBUG
				this.diagnose( () =>
				{
					this.debug(
						String.Format( "Wystąpił błąd typu {1} o tresci \"{0}\". Rozłączam sie.", error.Message, error.GetType().ToString() ), true );
				} );
#endif
				this.Disconnect();
			}
			else
			{
#if DEBUG
				this.diagnose( () =>
				{
					this.debug(
						String.Format( "Wystąpił błąd typu {1} o tresci \"{0}\". Uruchamiam zewnetrzny handler błędu.", error.Message, error.GetType().ToString() ), true );
				} );
#endif
				this.onException( error );
			}
		}

		/// <summary>
		/// Uruchamia sie w momencie pomyślnego wysłania jakiegoś pakietu.
		/// </summary>
		protected void onPacketSended()
		{
#if DEBUG
			this.diagnose( () => { this.debug( "Próba wysłania pakietu zakończona sukcesem.", false ); } );
#endif
		}

		/// <summary>
		/// Uruchamia sie w momencie gdy serwer GG wyśle jakieś dane.
		/// </summary>
		/// <param name="data"></param>
		protected void OnReceiveData( SocketAsyncEventArgs data )
		{
			try
			{
				var dataBuffer = data.Buffer;
				var dataBytesTransferred = data.BytesTransferred;
#if DEBUG
				this.diagnose( () => { this.debug( String.Format( " Odebrano dane o wielkości {0}.", dataBytesTransferred ), false ); } );
#endif
				if (this.BytesReceived != null)
				{
					this.BytesReceived( dataBytesTransferred );
				}
				this.parseReceivedDataAndHandleGluedData( dataBuffer, dataBytesTransferred );
			}
			catch (Exception exception)
			{
				this.onError( exception );
			}
		}

		/// <summary>
		/// Skleja 2 tablice bajtów
		/// </summary>
		private byte[] appendSecondArray( byte[] array1, byte[] array2, int array2Length )
		{
			var array1Length = (array1 != null) ? (array1.Length) : 0;
			var newArray = new byte[array1Length + array2Length];
			if (array1 != null)
			{
				Array.Copy( array1, newArray, array1Length );
			}
			if (array2 != null)
			{
				Array.Copy( array2, 0, newArray, array1Length, array2Length );
			}
			return newArray;
		}

		/// <summary>
		/// Uruchamia handler dla pakietu
		/// </summary>
		private void callHandlerForPacket( IInTcpPacket packet )
		{
			// Iteruje po procesach do czasu az ktorys nie obsluzy pakietu.
			// Proces zwraca true jezeli obsluzyl pakiet.
			// Kopia potrzebna jest zeby bylo thread-safe
			var list = new List<Process>( this.processess );
#if DEBUG
			var isHandled = false;
			if (packet != null)
			{
				this.diagnose( () =>
				{
					this.debug(
						String.Format( "Otrzymany pakiet został rozpoznany i jest typu {0}", packet.GetType().ToString
						() ), false );
				} );
			}
			else
			{
				this.diagnose( () => { this.debug( String.Format( "Otrzymany pakiet nie został rozpoznany!" ), true ); } );
			}
#endif
			if (packet != null)
			{
				foreach (var process in list)
				{
					if (process.OnPacketReceived( packet ))
					{
#if DEBUG
						isHandled = true;
#endif
						break;
					}
				}
#if DEBUG
				if (isHandled == false)
				{
					this.diagnose( () => { this.debug( String.Format( "Nie znaleziono handlera który umiałby obsłużyc pakiet typu {0}.", packet.GetType().ToString() ), true ); } );
				}
#endif
			}
		}

		private void Diagnose( Action action )
		{

		}

		private void onDisconnection()
		{
			if (this.isDisconected == false)
			{
				this.connection.Disconnect();
				this.isDisconected = true;
				if (this.Disconnected != null)
				{
					this.Disconnected();
				}
			}
		}

		/// <summary>
		/// Parsuje zadane dane w poszukiwaniu pakietow i uruchamia jego handler.
		/// Jesli napotka doklejony pakiet, zwraca go na zewnatrz.
		/// </summary>
		private byte[] parseReceivedDataAndCallHandler( byte[] dataBuffer, int dataBytesTransferred )
		{
			IInTcpPacket packet = null;
			bool isUncomplete = false;
			// czy to pierwsza partia danych jesli pakiet jest rozczlonkowany
			bool isFirstChunk = this.lastPacketChunk == null;
			byte[] gluedData;
			this.lastPacketChunk = this.appendSecondArray( this.lastPacketChunk, dataBuffer, dataBytesTransferred );
			packet = InTcpPacket.CreatePacket( this.lastPacketChunk, out isUncomplete, out gluedData );
			// otrzymanie kompletnego pakietu zatrzymuje deadtimer
			if (packet != null)
			{
				this.stopUncompletedTimer();
			}
			if (isUncomplete == false)
			{
#if DEBUG
				if (packet == null && isUncomplete == false)
				{
					this.diagnose( () => { this.debug( String.Format( "Napotkałem chujową porcje danych ktora jest wycinkiem z nie wiadomo czego i nie ma prawa być pakietem. Została odrzucona. Długość tej partii: {0}.", dataBytesTransferred ), true ); } );
				}
#endif

				this.lastPacketChunk = null;

				//if (packet == null)
				//{
				//    System.Threading.Thread.Sleep( TimeoutBetweenPacketsInMs );
				//}
#if DEBUG
				this.diagnose( () => { this.debug( String.Format( "Ostatnio otrzymany pakiet był w jednej części." ), false ); } );
#endif
				this.callHandlerForPacket( packet );
			}
			else
			{
				// uruchamia deadtimer - masz x sekund zeby wyslac reszte pakietu zanim go olejemy , panie serwerze
				if (isFirstChunk == true)
				{
					this.startUncompletedTimer();
				}
#if DEBUG
				this.diagnose( () =>
				{
					this.debug(
						String.Format( "Ostatnio otrzymany pakiet był rozczłonkowany. Czekanie na reszte pakietu." ), false );
				} );
#endif
			}
			return gluedData;
		}

		/// <summary>
		/// Parsuje zadaną tablicę bajtow szukajac w niej pakietow.
		/// jesli napotka doklejony pakiet, uruchamia sie rekurencyjnie dla doklejonego pakietu
		/// </summary>
		/// <param name="dataBuffer"></param>
		/// <param name="dataBytesTransferred"></param>
		private void parseReceivedDataAndHandleGluedData( byte[] dataBuffer, int dataBytesTransferred )
		{
			byte[] gluedData = this.parseReceivedDataAndCallHandler( dataBuffer, dataBytesTransferred );
			if (gluedData != null)
			{
#if DEBUG
				this.diagnose( () => { this.debug( String.Format( "Ostatnio otrzymany pakiet miał doklejoną do siebie część następnego. Pakiety rozdzielam. Wielkość doklejonego pakietu: {0}. uruchamiam handler tak jakby doklejony pakiet przyszedł normalnie, wiec nastepny komunikat powinien być \"otrzymałem handler\"", gluedData.Length ), false ); } );
#endif
				this.parseReceivedDataAndHandleGluedData( gluedData, gluedData.Length );
			}
		}

		/// <summary>
		/// uruchamia timer ktory w razie zbyt długiego czekania na dalsze czesci pakietu anuluje go w calosci
		/// </summary>
		private void startUncompletedTimer()
		{
			this.stopUncompletedTimer();
			this.uncompletedTimer = new Timer( WaitingForUncompletedPacketChunksInterval, () =>
				{
					this.stopUncompletedTimer();
					this.lastPacketChunk = null;
				}, true ).Start();
		}

		private void stopUncompletedTimer()
		{
			if (this.uncompletedTimer != null)
			{
				this.uncompletedTimer.Stop();
				this.uncompletedTimer = null;
#if DEBUG
				this.diagnose( () => { this.debug( "Właśnie zakończył się czas oczekiwania na kolejne części pakietu. Pobieranie pakietu zostalo przerwane, czekam na kolejne", true ); } );
#endif
			}
		}


#if DEBUG
		private const bool DebugMessagesFromPacketManager = true;
		private const string DiagnosticHeader = "        PacketManager: ";
		private const string Important = "-----";
		private DiagnosticHelper diagnostic;
		private void diagnose( Action action )
		{
			this.diagnostic.Diagnose( action );
		}
		private void debug( string message, bool isimportant )
		{
			this.diagnostic.DiagnosticMessage( message, isimportant );
		}
#endif
	}


	/// <summary>
	/// Obsluguje nieobslugiwane pakiety zeby nie smiecilo mi w debug window
	/// </summary>
	public class HandleUnusedPacketsProcess : Process
	{
		public override bool OnPacketReceived( IInTcpPacket packet )
		{
			if (packet is GG_UNUSED_PACKET)
			{
				return true;
			}
			return false;
		}
	}
}


