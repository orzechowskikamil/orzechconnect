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
using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;
using System.Text;
using GisSharpBlog.NetTopologySuite.Encodings;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;

namespace OrzechowskiKamil.OrzechConnect.Lib.OutPackets
{
	public enum MessageClass
	{
		/// <summary>
		/// Bit ustawiany wyłącznie przy odbiorze wiadomości, gdy wiadomość została wcześniej zakolejkowania z powodu 
		/// nieobecności
		/// </summary>
		Queued = 0x1,
		/// <summary>
		/// Wiadomość jest częścią toczącej się rozmowy i zostanie wyświetlona w istniejącym okienku
		/// </summary>
		Chat = 0x08,
		/// <summary>
		/// Klient nie życzy sobie potwierdzenia wiadomości
		/// </summary>
		ReceiveConfirmationDisable = 0x20
	}

	public class SendMessage : OutTcpPacket
	{
		public const int MaxLengthForMessage = 1980;
		private uint getCurrentTimestamp()
		{
			TimeSpan ts = (DateTime.UtcNow - new DateTime( 1970, 1, 1, 0, 0, 0 ));
			return ( uint ) ts.TotalSeconds;
		}

		public SendMessage()
		{
			this.SetValue( MessageClass.Chat, true );
		}

		public void SetValue( MessageClass msgClass, bool value )
		{
			this.messageClass = this.setMask( this.messageClass, ( uint ) msgClass, value );
		}

		public string HtmlMessage
		{
			set
			{
				this.htmlMessage = this.encodeTextAndTrimToMaxLength( value, false );
				this.plainMessage = this.encodeTextAndTrimToMaxLength( value, true );
			}
		}

		private byte[] encodeTextAndTrimToMaxLength( string strVal, bool isWindows1250 )
		{
			var message = Encoding.UTF8.GetBytes( strVal + '\0' );
			if (isWindows1250)
			{
				message =Encoding.Convert( new UTF8Encoding(), new CP1250(), message );
			}
			message = this.trimMessageToMaxLength( message );
			return message;
		}

		private byte[] trimMessageToMaxLength( byte[] message )
		{
			if (message.Length > MaxLengthForMessage)
			{
				Array.Resize<byte>( ref message, MaxLengthForMessage );
			}
			return message;
		}

		#region Pola pakietu
		public uint ReceiverNumber;
		private uint sequenceNumber { get { return getCurrentTimestamp(); } }
		private byte[] htmlMessage { get; set; }
		private uint messageClass { get; set; }
		// pięć pól jest przed plainMessage uint + htmlMessage
		private uint offsetPlain { get { return ( uint ) ((sizeof( uint ) * 5) + this.htmlMessage.Length); } }
		private uint offsetAttributes { get { return (this.offsetPlain + ( uint ) this.plainMessage.Length); } }
		private byte[] plainMessage { get; set; }
		private string attributes { get { return "\0\0\0\b\0\0\0\0"; } }
		#endregion



		protected override uint packetCode
		{
			get { return ( uint ) OutPacketsCodes.SendMessage; }
		}

		protected override void writeToBytesArray( NullTerminatedBinaryWriter writer )
		{
			writer.Write( ReceiverNumber );
			writer.Write( sequenceNumber );
			writer.Write( messageClass );
			writer.Write( offsetPlain );
			writer.Write( offsetAttributes );
			writer.Write( htmlMessage );
			writer.Write( plainMessage );
			writer.Write( attributes );
		}
	}
}
