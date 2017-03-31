using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using System.Collections.Generic;
using System;

namespace OrzechowskiKamil.OrzechConnect.Lib.OutPackets
{
	public enum UserNotifyTypeMask
	{
		/// <summary>
		/// Kazdy na naszej liscie kontaktów
		/// </summary>
		UserBuddy = 0x01,
		/// <summary>
		/// Widoczny w trybie tylko dla przyjaciół (tzn widzi nas)
		/// </summary>
		UserFriend = 0x02,
		/// <summary>
		/// Zablokowany przez nas.
		/// </summary>
		UserBlocked = 0x04
	}
	/// <summary>
	/// Strukt z których składa się pakiet notifyFirst. 
	/// Ma w sobie numer uzytkownika i jego status u nas.
	/// </summary>
	public class GG_NOTIFY : TcpStruct
	{
		/// <summary>
		/// Numer GG
		/// </summary>
		public uint Number;
		/// <summary>
		/// Bezposredni dostep do wartosci typu uzytkownika
		/// </summary>
		public byte UserTypeMask;
		/// <summary>
		/// Ustawia flage uzytkownika
		/// </summary>
		/// <param name="mask"></param>
		/// <param name="value"></param>
		public void SetValue( UserNotifyTypeMask mask, bool value )
		{
			this.UserTypeMask = ( byte ) this.setMask( this.UserTypeMask, ( uint ) mask, value );
		}
		/// <summary>
		/// Odczytuje flage uzytkownika
		/// </summary>
		/// <param name="mask"></param>
		/// <returns></returns>
		public bool GetValue( UserNotifyTypeMask mask )
		{
			return this.GetValue( this.UserTypeMask, ( uint ) mask );
		}

		public void writeToBytesArray( NullTerminatedBinaryWriter writer )
		{
			writer.Write( this.Number );
			writer.Write( this.UserTypeMask );
		}

		public override string ToString()
		{
			return String.Format( "Request for: {0}", this.Number );
		}
	}


	/// <summary>
	/// Pakiet wysyłany jako paczki max 400 structów GGNotify.
	/// Ten pakiet jest wysłany jeśli nie jest ostantim pakietem. Jesli jest do wysłania
	/// ostatni lub tylko jeden pakiet, wysyłamy notify last.
	/// </summary>
	public class GG_NOTIFY_FIRST : OutTcpPacket
	{

		public List<GG_NOTIFY> GGNotifies;
		public GG_NOTIFY_FIRST()
		{
			this.GGNotifies = new List<GG_NOTIFY>();
		}
		protected override uint packetCode
		{
			get { return 0x000f; }
		}

		protected override void writeToBytesArray( NullTerminatedBinaryWriter writer )
		{
			foreach (var notify in this.GGNotifies)
			{
				notify.writeToBytesArray( writer );
			}
		}
	}
}
