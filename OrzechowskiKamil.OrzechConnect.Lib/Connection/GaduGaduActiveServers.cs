using System;
using System.Net;
using OrzechowskiKamil.OrzechConnect.Lib.Connection;

namespace OrzechowskiKamil.OrzechConnect.Lib.Connection
{
	/// <summary>
	/// Klasa odczytuje aktywne serwery z web servisu GG.
	/// </summary>
	public class GaduGaduActiveServers
	{
		public const string ClientVersion = "10.0.0.10450";
		/// <summary>
		/// Numer GG.
		/// </summary>
		public int GGNumber;
		/// <summary>
		/// Callback wykonywany w momencie zakończenia żądania.
		/// </summary>
		public Action<IPEndPoint> OnComplete;
		public Action<Exception> OnError;

		public GaduGaduActiveServers( int GGNumber, Action<IPEndPoint> onCompleteCallback,
			Action<Exception> onErrorCallback )
		{
			this.GGNumber = GGNumber;
			this.OnComplete = onCompleteCallback;
			this.OnError = onErrorCallback;
		}
		/// <summary>
		/// Próbuje odczytać aktywne serwery gadu gadu.
		/// </summary>
		public void Start()
		{
			if (this.OnComplete == null || this.GGNumber == 0)
			{
				throw new ArgumentNullException( "Jeden z wymaganych parametrów jest null." );
			}
			var uri = this.getUriOfActiveServersProvider( this.GGNumber, ClientVersion );
			var request = new WebTextRequestAsync( uri, 
				( content ) =>
				{
					var serverIp = this.getActiveServerIpAddress( content.ToString() );
					this.OnComplete( serverIp );
				}, 
				( exception ) =>
				{
					this.OnError( exception );
				} );
			request.Start();
		}
		/// <summary>
		/// Parsuje to co webservice zwrocil
		/// </summary>
		/// <param name="contentFromRequest"></param>
		/// <returns></returns>
		private IPEndPoint getActiveServerIpAddress( string contentFromRequest )
		{
			string[] responseParts = contentFromRequest.Split( ' ' );
			if (responseParts.Length > 0)
			{
				var ipAddressAsString = responseParts[2].Trim();
				var ipAddressAfterSplit = ipAddressAsString.Split( ':' );
				var ip = ipAddressAfterSplit[0];
				var port = Int32.Parse( ipAddressAfterSplit[1] );
				return new IPEndPoint( IPAddress.Parse( ip ), port );
			}
			return null;
		}
		/// <summary>
		/// zwraca Uri do webservicu zwracajacego aktywne servery
		/// </summary>
		/// <param name="ggNumber"></param>
		/// <returns></returns>
		private string getUriOfActiveServersProvider( int ggNumber, string clientVersion )
		{
			// fmnumber jest numerem Gadu-Gadu.
			// version jest wersją klienta w postaci „A.B.C.D” (na przykład „8.0.0.7669”).
			// fmt określa czy wiadomość systemowa będzie przesyłana czystym tekstem (brak zmiennej „fmt”) 
			// czy w HTMLu (wartość „2”).
			// lastmsg jest numerem ostatnio otrzymanej wiadomości systemowej.

			string url = string.Format( "http://appmsg.gadu-gadu.pl/appsvc/appmsg_ver8.asp?fmnumber={0}&" +
				"fmt={1}&lastmsg={2}&version={3}", ggNumber, null, null, clientVersion );
			return url;
		}
	}
}
