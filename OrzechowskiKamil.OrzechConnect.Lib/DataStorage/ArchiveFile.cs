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
using System.Xml.Linq;

namespace OrzechowskiKamil.OrzechConnect.Lib.DataStorage
{

	/// <summary>
	/// Niezaimplementowane
	/// </summary>
	public class ArchiveFile : DataStorage
	{
		public ArchiveFile( int ggNumber ) : base( String.Format( "Archive_{0}", ggNumber ) ) { }
		protected override void getContentsFromXmlDocument( XElement rootElement )
		{
			throw new NotImplementedException();
		}

		protected override void putContentsIntoXmlDocument( XElement rootElement )
		{
			throw new NotImplementedException();
		}

		protected override void OnLoadingErrorError( Exception exception )
		{
			throw new NotImplementedException();
		}
	}

}
