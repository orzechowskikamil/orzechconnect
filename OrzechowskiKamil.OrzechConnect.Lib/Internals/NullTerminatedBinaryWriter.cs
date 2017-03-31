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
using System.IO;
using System.Text;

namespace OrzechowskiKamil.OrzechConnect.Lib.Internals
{
	/// <summary>
	/// Binary writer który zapisuje stringi w UTF-8, oraz nie poprzedza ich informacją ile mają znaków.
	/// Automatycznie konwertuje C#powe typy na te wymagane przez gadu (np int na uint)
	/// </summary>
	/// Przy opisie struktur, założono, że char ma rozmiar 1 bajtu, short 2 bajtów, int 4 bajtów, long long 8 bajtów, 
	/// wszystkie bez znaku
	public class NullTerminatedBinaryWriter : BinaryWriter
	{
		public NullTerminatedBinaryWriter( MemoryStream stream ) : base( stream ) { }
		/// <summary>
		/// Zapisuje stringa w UTF-8
		/// </summary>
		/// <param name="value"></param>
		public override void Write( string value )
		{
			try
			{
				var convertedString = Encoding.UTF8.GetBytes( value );
				base.Write( convertedString );
			}
			catch (Exception) { }
		}

		/// <summary>
		/// Zapisuje tablice bajtów
		/// </summary>
		/// <param name="value"></param>
		public override void Write( byte[] value )
		{
			if (value != null)
			{
				base.Write( value );
			}
		}
		/// <summary>
		/// Zapisuje stringa dodajac do niego nullchar (W UTF-8)
		/// </summary>
		/// <param name="value"></param>
		public void WriteWithNullChar( string value )
		{
			this.Write( value + '\0' );
		}
		/// <summary>
		/// Zapisuje tablice bajtow dodajac na koncu nullchar (\0)
		/// </summary>
		/// <param name="value"></param>
		public void WriteWithNullChar( byte[] value )
		{
			var length = value.Length;
			var finalArray = new byte[length + 1];
			value.CopyTo( finalArray, 0 );
			finalArray[length] = 0;
			this.Write( finalArray );
		}
	}
}
