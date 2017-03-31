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
using System.Diagnostics;

namespace OrzechowskiKamil.OrzechConnect.Lib.Internals
{
	/// <summary>
	/// Dodatkowa klasa dla krótszej nazwy.
	/// </summary>
	public class Diag : DiagnosticHelper
	{
		public Diag( bool enabled, string header, string importantsHeader )
			: base( enabled, header,
				importantsHeader ) { }
	}
	public class DiagnosticHelper
	{
		private bool enabled;
		private string header;
		private string importantsHeader;
		public DiagnosticHelper( bool enabled, string header, string importantsHeader )
		{
			this.enabled = enabled;
			this.header = header;
			this.importantsHeader = importantsHeader;
		}
		/// <summary>
		/// Static jest zrobony z intencja o krotkich komunikatach gdzie nie potrzebne jest ich duzo,
		/// wiec nazwy maja byc jak najkrotsze (metod)
		/// </summary>
		/// <param name="action"></param>
		public static void Diag( Action action )
		{
#if DEBUG
			try
			{
				action();
			}
			catch (Exception exception)
			{
				Msg( String.Format( "Wypisanie komunikatu diagnostycznego spowodowało błąd {0}.", exception.GetType().ToString() ) );
			}

#endif

		}

		public static void PrintException( Exception exception )
		{
			var message = "";
			var before = "";
			while (exception != null)
			{
				message += exception.Message;
				exception = exception.InnerException;
				before += ">>>";
			}
			Msg( message );
		}
		public static void Msg( string message, params object[] paramsAttr )
		{
			Msg( String.Format( message, paramsAttr ) );
		}

		public static void Msg( string message )
		{
#if DEBUG
			Debug.WriteLine( message, false );
#endif
		}
		public void Diagnose( Action action )
		{
			Diag( action );
		}
		public void DiagnosticMessage( string message )
		{
#if DEBUG
			this.DiagnosticMessage( message, false );
#endif
		}
		public void DiagnosticMessage( string message, bool important )
		{
#if DEBUG
			if (this.enabled)
			{
				var msg = this.header + " (" + DateTime.Now.ToLongTimeString() + ") " + ((important) ? this.importantsHeader + " " : "") + message;
				Msg( msg );
			}
#endif
		}
	}
}
