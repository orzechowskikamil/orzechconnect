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

namespace OrzechowskiKamil.OrzechConnect.Lib.Exceptions
{
	public class MetroGaduException: Exception
	{
		public MetroGaduException( string message ) : base( message ) { }
	}

	public class NotEstablishedConnectionException : MetroGaduException
	{
		public NotEstablishedConnectionException( string message ) : base( message ) { }
	}
}
