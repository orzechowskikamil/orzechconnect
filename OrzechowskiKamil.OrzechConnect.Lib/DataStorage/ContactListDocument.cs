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
using System.Xml.Linq;

namespace OrzechowskiKamil.OrzechConnect.Lib.DataStorage
{
	/// <summary>
	/// Przechowuje liste kontaktow dla jednego profilu w osobnym pliku.
	/// Dziala na "starych" zasadach recznej serializacji gdyz jej format musi byc zgodny z protokołem GG.
	/// </summary>
	public class ContactsListDocument : DataStorage
	{
		public ContactBook ContactsList { get; set; }

		public ContactsListDocument( int userNumber )
			: base( String.Format( "ContactsList_{0}.xml", userNumber ) )
		{
		}
		public const string CONTACTS_LIST = "ContactsList";


		protected override void putContentsIntoXmlDocument( XElement rootElement )
		{
			var contactsList = new XElement( CONTACTS_LIST );
			contactsList.Add( this.ContactsList.SaveToXml() );
			rootElement.Add( contactsList );
		}

		protected override void getContentsFromXmlDocument( XElement rootElement )
		{
			string contactsXmlString;
			XmlHelper.readString( rootElement, CONTACTS_LIST, out  contactsXmlString );
			if (String.IsNullOrWhiteSpace( contactsXmlString ) == false)
			{
				this.ContactsList = new ContactBook();
				this.ContactsList.LoadFromXml( contactsXmlString );
			}
		}

		protected override void OnLoadingErrorError( Exception exception )
		{
		//	throw new NotImplementedException();
		}
	}

}
