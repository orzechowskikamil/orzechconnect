using System.Text;
using System.IO;
using System.Collections.Generic;

namespace OrzechowskiKamil.OrzechConnect.Lib.Internals
{
	/// <summary>
	/// Klasa ktora pozwala zczytywac z pamieci również null-terminated stringi
	/// </summary>
	public class NullTerminatedBinaryReader : BinaryReader
	{
		public NullTerminatedBinaryReader( MemoryStream stream ) : base( stream ) { }
		public string ReadNullTerminatedString()
		{
			var byteListArray = ReadNullTerminatedBytesArray();
			return this.bytesToUTF8String( byteListArray );
		}

		public byte[] ReadNullTerminatedBytesArray()
		{
			var byteList = new List<byte>();
			byte readedByte = 1;
			while ((readedByte = this.ReadByte()) != 0)
			{
				byteList.Add( readedByte );
			}
			var byteListArray = byteList.ToArray();
			return byteListArray;
		}

		private string bytesToUTF8String( byte[] byteListArray )
		{
			return UTF8Encoding.UTF8.GetString( byteListArray, 0, byteListArray.Length );
		}

		public string ReadString( int count )
		{
			byte[] buffer = new byte[count];
			for (int i = 0; i < count; i++)
			{
				buffer[i] = this.ReadByte();
			}
			return this.bytesToUTF8String( buffer );
		}
	}
}
