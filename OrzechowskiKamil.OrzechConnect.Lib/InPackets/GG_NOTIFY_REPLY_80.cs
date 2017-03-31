using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using System.Collections.Generic;

namespace OrzechowskiKamil.OrzechConnect.Lib.InPackets
{

	/// <summary>
	/// Struct GG_NOTIFY_REPLY_80, zawiera status jednego użytkownika
	/// </summary>
	public class NotifyReply
	{
		public uint Number;
		public GGStatus Status;
		public uint Features;
		private uint remoteIp;
		private ushort remotePort;
		public byte ImageSizeInKb;
		private byte unknown;
		public uint Flags;
		public uint DescriptionSize;
		public string Description;
		/// <summary>
		/// Odczytuje jednego membera. Zwraca jego dlugosc w bajtach (dlugosc structa)
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public int readMember( NullTerminatedBinaryReader reader )
		{
			this.Number = reader.ReadUInt32();
			this.Status = ( GGStatus ) ( int ) reader.ReadUInt32();
			this.Features = reader.ReadUInt32();
			this.remoteIp = reader.ReadUInt32();
			this.remotePort = reader.ReadUInt16();
			this.ImageSizeInKb = reader.ReadByte();
			this.unknown = reader.ReadByte();
			this.Flags = reader.ReadUInt32();
			this.DescriptionSize = reader.ReadUInt32();
			this.Description = reader.ReadString( ( int ) this.DescriptionSize );
			// 28 to rozmiar bajtów stałych pól
			var length = 28 + ( int ) this.DescriptionSize;
			return length;
		}

		public override string ToString()
		{
			return this.Status.ToString();
		}
	}


	public class GG_NOTIFY_REPLY_80_BASE : InTcpPacket
	{
		public GG_NOTIFY_REPLY_80_BASE()
		{
			this.ContactsMembers = new List<NotifyReply>();
		}
		public List<NotifyReply> ContactsMembers;
		protected override void readFromBytesArray( NullTerminatedBinaryReader reader, uint packetContentLength )
		{
			int bytesLeft = ( int ) packetContentLength;
			while (bytesLeft > 0)
			{
				var member = new NotifyReply();
				var sizeOfLastMember = member.readMember( reader );
				bytesLeft -= sizeOfLastMember;
				// Jesli w liczbie bajtow do konca wyszloby ponizej zera to bardzo prawdopodobne ze ostatni
				// member sie zle wczytal i go po prostu pomijamy
				if (bytesLeft >= 0)
				{
					ContactsMembers.Add( member );
				}
			}
		}
	}

	/// <summary>
	/// Struct GG_NOTIFY_REPLY_80 zawierający w sobie kilka structów.
	/// Struct pojedynczy to struct tego typu tylko w liscie ma 1 element.
	/// 
	/// Ten pakiet powraca z serwera jako odpowiedz na NotifyFirst+NotifyLast lub NotifyEmpty
	/// </summary>
	public class GG_NOTIFY_REPLY_80 : GG_NOTIFY_REPLY_80_BASE
	{
	}

	/// <summary>
	/// Przychodzi gdy ktos zmienia status.
	/// </summary>
	public class GG_STATUS80 : GG_NOTIFY_REPLY_80_BASE
	{

	}
}
