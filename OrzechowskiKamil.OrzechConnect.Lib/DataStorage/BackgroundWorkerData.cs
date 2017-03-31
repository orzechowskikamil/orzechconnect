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
using System.Xml.Serialization;
using OrzechowskiKamil.OrzechConnect.Lib.InPackets;
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;

namespace OrzechowskiKamil.OrzechConnect.Lib.DataStorage
{



	public class MessageModel
	{

		[XmlAttribute( "IsIncoming" )]
		public bool IsIncoming { get; set; }

		[XmlElement( "Message" )]
		public string MessageContent { get; set; }

		[XmlAttribute( "SendDate" )]
		public DateTime SendDate { get; set; }

		[XmlAttribute( "Sender" )]
		public int SenderNumber { get; set; }




		//internal static MessageModel FromPacket( GG_RECV_MSG80 message )
		//{
			
		//}
	}

	public class UnreadedMessagesFor
	{

		public UnreadedMessagesFor()
		{
			this.MessagesUnreaded = new List<MessageModel>();
		}



		[XmlArray( "MessagesUnreaded" )]
		[XmlArrayItem( "MessageUnreaded", Type = typeof( MessageModel ) )]
		public List<MessageModel> MessagesUnreaded { get; set; }

		[XmlAttribute( "MessageReceiver" )]
		public int ReceiverNumber { get; set; }
	}

	[XmlRoot( "BackgroundWorkerData" )]
	public class WorkerData
	{
		[XmlElement( "HowMuchBypassessMade" )]
		public int HowMuchBypassessMade { get; set; }
		[XmlArray( "MessagesPack" )]
		[XmlArrayItem( "Pack", Type = typeof( UnreadedMessagesFor ) )]
		public List<UnreadedMessagesFor> UnreadedMessagesPacks;



		public WorkerData()
		{
			this.UnreadedMessagesPacks = new List<UnreadedMessagesFor>();
		}


		/// <summary>
		/// Licznik bajtów odebranych przez agenta
		/// </summary>
		[XmlElement( "Received" )]
		public int BytesReceivedByAgent { get; set; }

		/// <summary>
		/// Bajty wyslane przez agenta
		/// </summary>
		[XmlElement( "Sended" )]
		public int BytesSendedByAgent { get; set; }

		public DateTime DateOfLastResetCounter { get; set; }



		public UnreadedMessagesFor GetPackByNumber( int number )
		{
			if (this.UnreadedMessagesPacks != null)
			{
				foreach (var item in this.UnreadedMessagesPacks)
				{
					if (item.ReceiverNumber == number) { return item; }
				}
			}
			return null;
		}
	}

	public class BackgroundWorkerData : DataStorage
	{

		public WorkerData Data;



		public BackgroundWorkerData()
			: base( "BackgroundWorkerData.xml" )
		{ }



		private XmlSerializer Serializer
		{
			get
			{
				return new XmlSerializer( typeof( WorkerData ) );
			}
		}




		protected override void getContentsFromXmlDocument( XElement rootElement )
		{

			this.Data = ( WorkerData ) this.Serializer.Deserialize( new StringReader( rootElement.Value ) );


		}

		protected override void putContentsIntoXmlDocument( XElement rootElement )
		{
			string xmlString = null;
			var stringWriter = new StringWriter();
			this.Serializer.Serialize( stringWriter, this.Data );
			xmlString = stringWriter.ToString();
			rootElement.Value = xmlString;
		}

		protected override void OnLoadingErrorError( Exception exception )
		{
			this.Data = new WorkerData();
		}
	}
}
