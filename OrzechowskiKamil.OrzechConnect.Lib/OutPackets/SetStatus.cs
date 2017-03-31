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
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using System.Text;

namespace OrzechowskiKamil.OrzechConnect.Lib.OutPackets
{
	public enum SetStatusFlags
	{
		/// <summary>
		/// Nie do konca znane, lepiej nie uzywac
		/// </summary>
		AudioConnections = 0x1,
		/// <summary>
		/// Klient obsluguje wideorozmowy
		/// </summary>
		VideoConversations = 0x2,
		/// <summary>
		/// Klient mobilny (wyswietla u innych ikonke komórki)
		/// </summary>
		MobileClient = 0x00100000,
		/// <summary>
		/// Chce otrzymywac linki od nieznajomych
		/// </summary>
		IWantToRecieveLinksFromUnknowns = 0x00800000
	}

	public class SetStatus : OutTcpPacket
	{


		public const int MaxLengthOfDescriptionInBytes = 250;


		/// <summary>
		/// Ustawia opis pilnujac by nie przekroczyl maksymalnej wartosci
		/// </summary>
		public string Description
		{
			set
			{
				if (value != null)
				{
					var newValue = Encoding.UTF8.GetBytes( value );
					Array.Resize<byte>( ref newValue, MaxLengthOfDescriptionInBytes );
					this.description = newValue;
				}
			}
		}
		private void setMaskInFlags( uint mask, bool value )
		{
			this.flags = this.setMask( this.flags, mask, value );
		}
		public void SetValue( SetStatusFlags flag, bool value )
		{
			this.setMaskInFlags( ( uint ) flag, value );
		}
		public void Enable( SetStatusFlags flag )
		{
			this.SetValue( flag, true );
		}
		public void Disable( SetStatusFlags flag )
		{
			this.SetValue( flag, false );
		}
		public void SetValue( GGStatusMasks flag, bool value )
		{
			this.setMaskInStatus( ( uint ) flag, value );
		}
		public void Enable( GGStatusMasks flag )
		{
			this.SetValue( flag, true );
		}
		public void Disable( GGStatusMasks flag )
		{
			this.SetValue( flag, false );
		}
		private void setMaskInStatus( uint mask, bool value )
		{
			this.statusMask = this.setMask( this.statusMask, mask, value );
		}
		private uint statusMask;
		public GGStatus Status;


		private uint statusWithMasks
		{
			get { return (( uint ) this.Status | this.statusMask); }
		}
		private uint flags { get; set; }
		private uint DescriptionSize
		{
			get
			{
				if (this.description != null)
				{
					return ( uint ) this.description.Length;
				}
				return 0;

			}
		}


		private byte[] description { get; set; }




		protected override uint packetCode
		{
			get { return ( uint ) OutPacketsCodes.SetStatus; }
		}

		protected override void writeToBytesArray( NullTerminatedBinaryWriter writer )
		{
			writer.Write( ( uint ) this.statusWithMasks );
			writer.Write( this.flags );
			writer.Write( this.DescriptionSize );
			writer.Write( this.description );

		}

	}
}
