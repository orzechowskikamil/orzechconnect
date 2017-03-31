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
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace OrzechowskiKamil.OrzechConnect.Lib.Internals
{
	public interface XmlSerializable
	{
		void SaveToXml( XElement element );
		void LoadFromXml( XElement element );
	}



	/// <summary>
	/// Obiektowa wersja XMLowej listy kontaktów Gadu Gadu
	/// </summary>
	/// <example>
	/// przyklad listy na ktorej wystąpily same podstawowe elementy.
	/// <ContactBook>
	/// <Groups>
	///        <Group>
	///            <Id>Identyfikator</Id>
	///            <Name>Nazwa grupy</Name>
	///            <IsExpanded>Boolean</IsExpanded>
	///            <IsRemovable>Boolean</IsRemovable>
	///         </Group>
	///     </Groups>
	///     <Contacts>
	///         <Contact>
	///             <Guid>Identyfikator</Guid>
	///             <GGNumber>Numer GG</GGNumber>
	///             <ShowName>Wyświetlana nazwa kontaktu</ShowName>
	///             <Groups>
	///                 <GroupId>Identyfikator grupy</GroupId>
	///             </Groups>
	///             <Avatars>
	///                 <URL></URL>
	///            </Avatars>
	///             <FlagNormal>Boolean</FlagNormal>
	///         </Contact>
	///     </Contacts>
	/// </ContactBook>
	/// </example>
	[XmlRoot( "ContactBook", Namespace = null )]
	public class ContactBook
	{

		[XmlArrayItem( "Group", typeof( Group ) )]
		[XmlArray( "Groups" )]
		public List<Group> Groups;
		[XmlArrayItem( "Contact", typeof( Contact ) )]
		[XmlArray( "Contacts" )]
		public List<Contact> Contacts;

		public ContactBook()
		{
			this.Groups = new List<Group>();
			this.Contacts = new List<Contact>();
		}


		/// <summary>
		/// Serializuje liste kontaktów do stringa z XML-em
		/// </summary>
		public string SaveToXml()
		{
			var stringWriter = new StringWriter();
			var xml = XmlWriter.Create( stringWriter, new XmlWriterSettings
			{
				Indent = false,
				OmitXmlDeclaration = true,
				CloseOutput = true
			} );
			this.Serializer.Serialize( stringWriter, this );
			var output = stringWriter.ToString();
			// zeby nie bylo rozowo to trzeba wyciąć stąd te smieci ktore ce kratka dodaje
			var pos = output.IndexOf( "<Groups" );
			var result = "<ContactBook>" + output.Substring( pos );
			return result;

		}


		private XmlSerializer Serializer { get { return new XmlSerializer( typeof( ContactBook ) ); } }
		/// <summary>
		/// Deserializuje liste kontaktów z XMLStringa i wkleja dane do obecnego obiektu this
		/// </summary>
		/// <param name="xmlString"></param>
		public void LoadFromXml( string xmlString )
		{
			try
			{
				var newObj = ( ContactBook ) this.Serializer.Deserialize( new StringReader( xmlString ) );
				if (newObj.Contacts != null)
				{
					this.Contacts = newObj.Contacts;
				}
				if (newObj.Groups != null)
				{
					this.Groups = newObj.Groups;
				}
			}
			catch (Exception e)
			{
			
			}

		}
	}



	/// <summary>
	/// Obiektowa reprezentacja grupy uzytkownikow w kontaktach GG
	/// </summary>

	public class Group
	{
		[XmlElement( "Id", Type = typeof( Guid ) )]
		public Guid Id;
		[XmlElement( "Name", Type = typeof( string ) )]
		public string Name;
		[XmlElement( "IsExpanded", Type = typeof( bool ) )]
		public bool IsExpanded;
		[XmlElement( "IsRemovable", Type = typeof( bool ) )]
		public bool IsRemovable;

	}

	/// <summary>
	/// Obiektowa reprezentacja uzytkownika w kontaktach GG
	/// </summary>
	/// <example>
	/// przykladowy kontakt:
	/// <Contact>
	///             <Guid>Identyfikator</Guid>
	///             <GGNumber>Numer GG</GGNumber>
	///             <ShowName>Wyświetlana nazwa kontaktu</ShowName>
	///             <Groups>
	///                 <GroupId>Identyfikator grupy</GroupId>
	///             </Groups>
	///             <Avatars>
	///                 <URL></URL>
	///            </Avatars>
	///             <FlagNormal>Boolean</FlagNormal>
	///         </Contact>
	/// </example>

	public class Contact
	{
		/// <summary>
		/// Unikalne ID uzytkownika (dowolny guid)
		/// </summary>
		public Guid Guid { get; set; }
		/// <summary>
		/// Numer gadu
		/// </summary>
		public int GGNumber { get; set; }
		/// <summary>
		/// Nazwa kontaktu, która jest wyświetlana na liście kontaktów komunikatora.
		/// </summary>
		public string ShowName { get; set; }
		/// <summary>
		/// Identyfikator grupy, której członkiem jest dany kontakt. Każdy kontakt musi należeć przynajmniej do jednej grupy 
		/// (właśnie dlatego istnieje nieusuwalna grupa Moje kontakty). Każdy kontakt może należeć do wielu grup jednocześnie;
		/// górna granica nie jest zbadana, jednak wynosi więcej niż dwanaście. Kontakt ignorowany należy do grupy Ignorowani. 
		/// Kontakt może należeć do grupy Ignorowani oraz innych grup jednocześnie.
		/// </summary>
		[XmlArray( "Groups" )]
		[XmlArrayItem( "GroupId", typeof( Guid ) )]
		public List<Guid> Groups { get; set; }
		/// <summary>
		/// Prawdopodobnie ścieżka do awataru użytkownika. W trakcie testów nie udało się doprowadzić do sytuacji, gdy element 
		/// przyjąłby jakąkolwiek wartość.
		/// </summary>
		[XmlArray( "Avatars" )]
		[XmlArrayItem( "URL", typeof( string ) )]
		public List<string> Avatars { get; set; }
		/// <summary>
		/// Element zastępczy który występuje jeśli kontakt nie jest ignorowany ani nie ma zaznaczonej flagi niewysyłania 
		/// informacji o statusie przy zaznaczonej opcji Tylko dla znajomych.
		/// </summary>
		public bool FlagNormal { get; set; }
		/// <summary>
		/// Informacja, czy danemu kontaktowi wysyłać status jeśli w komunikatorze jest zaznaczona opcja Tylko dla znajomych. 
		/// Jeśli jest ona zaznaczona, informacje o statusie są wysyłane tylko osobom znajdującym się na liście kontaktów 
		/// (pozostałym jest wysyłany status Niedostępny). Jeśli dodatkowo przy kontakcie jest zaznaczona ta opcja, danemu 
		/// kontaktowi również wysyłany jest status Niedostępny. Czyli w praktyce jak mu ustawisz na true to bedzie cie nie 
		/// widzial
		/// </summary>
		public bool FlagBuddy { get; set; }
		/// <summary>
		/// Informacja o tym, czy dany kontakt znajduje się na liście ignorowanych użytkowników. Każdy ignorowany kontakt ma 
		/// równocześnie identyfikator grupy Ignorowani w elemencie <Groups/>, tak więc listę ignorowanych użytkowników można 
		/// uzyskać na dwa sposoby.
		/// </summary>
		public bool FlagIgnored { get; set; }
		/// <summary>
		/// Nie jestem pewien czy powinno wystąpić
		/// </summary>
		public bool FlagFriend { get; set; }


	}

}
