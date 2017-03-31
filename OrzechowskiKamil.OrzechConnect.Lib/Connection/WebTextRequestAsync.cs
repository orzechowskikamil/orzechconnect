using System;
using System.Net;
using System.IO;

namespace OrzechowskiKamil.OrzechConnect.Lib.Connection
{

	public class WebTextRequestAsyncException : Exception
	{
		public WebTextRequestAsyncException( string message ) : base( message ) { }
	}


	public class WebTextRequestAsync
	{
		/// <summary>
		/// Delegat uruchamiany gdy zmienia się progress downloadu requestu.
		/// </summary>
		public Action<int> OnDownloadProgressChanged;
		/// <summary>
		/// Delegat uruchamiany gdy request sie zakonczy.
		/// </summary>
		public Action<string> OnRequestComplete;
		/// <summary>
		/// Delegat uruchamiany w razie wystapienia błędu
		/// </summary>
		public Action<Exception> OnErrorOccured;
		/// <summary>
		/// Uri requestu
		/// </summary>
		public string Uri;
		public WebTextRequestAsync( string uri, Action<string> onRequestCompleteCallback, Action<Exception> onErrorOccured )
		{
			this.Uri = uri;
			this.OnRequestComplete = onRequestCompleteCallback;
			this.OnErrorOccured = onErrorOccured;
		}
		/// <summary>
		/// Uruchamia request z takimi parametrami jak zostaly ustawione w propetries.
		/// </summary>
		public void Start()
		{
			if (string.IsNullOrWhiteSpace( this.Uri ))
			{
				throw new WebTextRequestAsyncException( "Uri is null." );
			}
			var webClient = new WebClient();
			webClient.OpenReadCompleted += new OpenReadCompletedEventHandler( webClient_OpenReadCompleted );
			webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler( webClient_DownloadProgressChanged );
			webClient.OpenReadAsync( new Uri( this.Uri ) );
		}


		/// <summary>
		/// Wykonuje sie gdy zmienia sie progress pobierania requestu
		/// </summary>


		private void webClient_DownloadProgressChanged( object sender, DownloadProgressChangedEventArgs e )
		{
			if (this.OnDownloadProgressChanged != null)
			{
				this.OnDownloadProgressChanged( e.ProgressPercentage );
			}
		}

		/// <summary>
		/// Wykonuje sie po zakonczeniu requestu
		/// </summary>


		private void webClient_OpenReadCompleted( object sender, OpenReadCompletedEventArgs eventArgs )
		{
			Stream resultStream = null;
			try
			{
				resultStream = eventArgs.Result;
				var streamReader = new StreamReader( resultStream );
				var resultString = streamReader.ReadToEnd();
				if (OnRequestComplete != null)
				{
					this.OnRequestComplete( resultString );
				}
			}
			catch (WebException)
			{
				this.OnErrorOccured( new InternetConnectionLostException( "Brak połączenia z internetem." ) );
			}
			catch (Exception e)
			{
				this.OnErrorOccured( e );
			}
		}
	}

	public class InternetConnectionLostException : WebTextRequestAsyncException
	{
		public InternetConnectionLostException( string message ) : base( message ) { }
	}

}

