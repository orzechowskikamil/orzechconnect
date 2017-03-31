using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;

namespace OrzechowskiKamil.OrzechConnect.Lib.OutPackets
{

	public enum UserType
	{
		Buddy = 1,
		Friend=2,
		Blocked=4
	}
	/// <summary>
	/// Ten pakiet ustawia daną flage bitowa na danym numerze gg.
	/// Chcac przywrocic uzytkownika z zablokowanych najpierw trzeba wyslac removeNotify(USER_BLOCKED)
	/// a nastepnie wyslac addNotify(USER_NORMAL).
	/// </summary>
	public class GG_ADD_NOTIFY : OutTcpPacket
	{
		public uint Number;
		public UserType UserType;
		//private uint mask;
		///// <summary>
		///// zwykly user w liscie kontaktow nie widzi nas gdy jestesmy prywatnie
		///// </summary>
		//public bool IsBuddy
		//{
		//    set { this.mask = this.setMask( this.mask, 1, value ); }
		//}
		///// <summary>
		///// zablokowany
		///// </summary>
		//public bool IsBlocked
		//{
		//    set { this.mask = this.setMask( this.mask, 4, value ); }
		//}

		///// <summary>
		///// przyjaciel, widzi nas nawet gdy jestesmy tylko dla znajomych
		///// </summary>
		//public bool IsFriend { set { this.mask = this.setMask( this.mask, 2, value ); } }



		protected override uint packetCode
		{
			get { return 0x000d; }
		}

		protected override void writeToBytesArray( NullTerminatedBinaryWriter writer )
		{
			writer.Write( Number );
			//writer.Write( ( uint ) this.mask ); chuj wie nie dziala zrobi sie na sztywno
			writer.Write( ( uint ) this.UserType );
		}
	}
}
