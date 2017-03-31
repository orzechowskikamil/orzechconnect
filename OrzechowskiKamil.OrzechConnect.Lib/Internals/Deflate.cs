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
using ComponentAce.Compression.Libs.zlib;
using System.Text;

namespace OrzechowskiKamil.OrzechConnect.Lib.Internals
{
	public class Deflate
	{
		/// <summary>
		/// Dekompresuje tablice bajtow uzywajac algorytmu deflate.
		/// </summary>
		public static string Uncompress( byte[] inputBytes )
		{
			MemoryStream stream = new MemoryStream( inputBytes );
			ZInputStream input = new ZInputStream( stream );
			MemoryStream outStream = new MemoryStream();
			int data = 0;
			int stopByte = -1;
			while (stopByte != (data = input.Read()))
			{
				byte _dataByte = ( byte ) data;
				outStream.WriteByte( _dataByte );
			}
			input.Close();
			stream.Close();
			var outputBytes = outStream.ToArray();
			var outputString = Encoding.UTF8.GetString( outputBytes, 0, outputBytes.Length );
			return outputString;
		}

		/// <summary>
		/// Kompresuje stringa do postaci tablicy bajtow algorytmem deflate.
		/// </summary>
		public static byte[] Compress( string inputString )
		{
			if (String.IsNullOrWhiteSpace( inputString ) == false)
			{
				var outByteStream = new MemoryStream();
				var output = new ZOutputStream( outByteStream, zlibConst.Z_BEST_COMPRESSION );
				var input = Encoding.UTF8.GetBytes( inputString );
				output.Write( input, 0, input.Length );
				output.Flush();
				output.Close();
				var result = outByteStream.ToArray();
				return result;
			}
			return null;
		}
	}
}
