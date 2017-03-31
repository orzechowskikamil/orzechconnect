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
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using System.Xml.Serialization;
using OrzechowskiKamil.OrzechConnect.Lib.InPackets;


namespace OrzechowskiKamil.OrzechConnect.Lib.DataStorage
{


	/// <summary>
	/// pojedynczy plik XML w data storage.
	/// </summary>
	abstract public class DataStorage
	{

		private XMLFileStorage storage;



		public DataStorage( string fileName )
		{
			this.storage = new XMLFileStorage( fileName );
		}




		public void Delete()
		{
			this.storage.Delete();
		}

		public void Load()
		{
			Exception exception;
			this.Load( out exception );
		}

		protected abstract void OnLoadingErrorError( Exception exception );
		public void Load( out Exception exception )
		{
			exception = null;
			try
			{

				var document = this.storage.loadXML();
				var root = document.Root;
				this.getContentsFromXmlDocument( root );
			}
			catch (Exception except)
			{
				this.OnLoadingErrorError( except );
				exception = except;
			}
		}

		public void Save()
		{
			try
			{
				XDocument document = new XDocument();
				XElement root = new XElement( "root" );
				document.Add( root );
				this.putContentsIntoXmlDocument( root );
				this.storage.saveXML( document );
			}
			catch (Exception exception)
			{
#if DEBUG
				var message = "";
				while (exception != null)
				{
					message += "\n>>> " + exception.Message;
					exception = exception.InnerException;
				}
				Diag.Diag( () => Diag.Msg( "Zapis do pliku zakończył się błędem. Plik: {0}. Błąd: {1}",
					this.storage.FileName, message ) );
#endif
			}
		}

		abstract protected void getContentsFromXmlDocument( XElement rootElement );

		abstract protected void putContentsIntoXmlDocument( XElement rootElement );
	}





}