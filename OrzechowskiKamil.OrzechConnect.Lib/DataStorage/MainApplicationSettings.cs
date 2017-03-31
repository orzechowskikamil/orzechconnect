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
using System.Collections.Generic;
using System.Xml.Linq;
using System.IO;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;

namespace OrzechowskiKamil.OrzechConnect.Lib.DataStorage
{
	/// <summary>
	/// Główne ustawienia aplikacji
	/// </summary>
	public class MainApplicationSettings : DataStorage
	{

		private const string ADDITIONAL_APP_SETTINGS = "AdditionalAppSettings";
		private const string CONTACTS_LIST_VERSION = "contVer";
		private const string DEFAULT_PROFILE_GG_NUMBER = "DefGGNum";
		private const string FOR_FRIENDS_ONLY = "friendsonly";
		private const string INTERNET_BYTES_USED = "InternetBytesUsed";
		private const string INTERNET_BYTES_USED_OUTCOMING = "OutInternetBytesUsed";
		public int InternetBytesIncoming;
		public int InternetBytesOutcoming;
		private const string MAX_IMAGE_SIZE = "MaxImgSize";
		private const string MOBILE_CLIENT = "mobcl";
		private const string NAME = "Name";
		private const string NUMBER = "Number";
		private const string PASSWORD = "Passwd";
		private const string PROFILE_GG_NUMBER = "Number";
		private const string PROFILE_PASSWORD = "ProfPswd";
		private const string PROFILES_GG_NUMBERS = "AllNumbers";
		private const string RECEIVE_LINKS_FROM_UNKNOWNS = "reclinksfromUnknowns";
		private const string STATUS_AFTER_LOGIN = "statusAfterLogin";
		private const string STATUS_DESC_AFTER_LOGIN = "statusdescAfterLogin";



		public MainApplicationSettings()
			: base( "mainAppSettings.xml" )
		{
			this.UserProfiles = new List<UserProfile>();
			this.AdditionalAppSettings = new MainAppSettingsPack();

		}



		public MainAppSettingsPack AdditionalAppSettings { get; set; }

		public int DefaultProfileGGNumber { get; set; }

		private XmlSerializer SerializerForUserProfile
		{
			get
			{
				return new XmlSerializer( typeof( UserProfile ) );
			}
		}

		public List<UserProfile> UserProfiles { get; set; }

		private XmlSerializer xmlSerializerForAdditionalAppSettings
		{
			get
			{
				XmlSerializer serializer = new XmlSerializer( typeof( MainAppSettingsPack ) );
				return serializer;
			}
		}




		protected override void getContentsFromXmlDocument( XElement rootElement )
		{
			var intResult = 0;
			XmlHelper.readInt( rootElement, DEFAULT_PROFILE_GG_NUMBER, out intResult );
			this.DefaultProfileGGNumber = intResult;
			this.readProfiles( rootElement );
			XmlHelper.readInt( rootElement, INTERNET_BYTES_USED, out this.InternetBytesIncoming );
			XmlHelper.readInt( rootElement, INTERNET_BYTES_USED_OUTCOMING, out this.InternetBytesOutcoming );
			this.loadAdditionalAppSettings( rootElement );
		}

		protected override void putContentsIntoXmlDocument( XElement rootElement )
		{
			rootElement.Add( new XElement( DEFAULT_PROFILE_GG_NUMBER, this.DefaultProfileGGNumber ) );
			this.writeProfiles( rootElement );
			rootElement.Add( new XElement( INTERNET_BYTES_USED, this.InternetBytesIncoming ) );
			rootElement.Add( new XElement( INTERNET_BYTES_USED_OUTCOMING, this.InternetBytesOutcoming ) );
			this.saveAdditionalAppSettings( rootElement );
		}

		private void loadAdditionalAppSettings( XElement rootElement )
		{
			string additionalAppSettingsString = null;
			XmlHelper.readString( rootElement, ADDITIONAL_APP_SETTINGS, out additionalAppSettingsString );
			if (additionalAppSettingsString != null)
			{
				var serializer = this.xmlSerializerForAdditionalAppSettings;
				var appSettingsPack = serializer.Deserialize( new StringReader( additionalAppSettingsString ) );
				if (appSettingsPack != null)
				{
					this.AdditionalAppSettings = ( MainAppSettingsPack ) appSettingsPack;
				}
			}
			if (this.AdditionalAppSettings == null) { this.AdditionalAppSettings = new MainAppSettingsPack(); }
		}

		private void readProfiles( XElement rootElement )
		{
			var profilesGGNumbers = rootElement.Element( PROFILES_GG_NUMBERS );
			if (profilesGGNumbers != null)
			{
				var allProfilesGGNumbers = profilesGGNumbers.Elements( PROFILE_GG_NUMBER );
				// do debugu
				//var list = new List<XElement>();
				//foreach (var id in allProfilesGGNumbers)
				//{
				//    list.Add( id );
				//}
				if (allProfilesGGNumbers != null)
				{
					foreach (var number in allProfilesGGNumbers)
					{
						// oldWay do do this.
						//try
						//{
						//    var version = int.Parse( number.Attribute( CONTACTS_LIST_VERSION ).Value );
						//    var userProfile = new UserProfile();

						//    userProfile.AccountName = number.Attribute( NAME ).Value;
						//    userProfile.AccountNumber = int.Parse( number.Attribute( NUMBER ).Value );
						//    userProfile.AccountPassword = number.Attribute( PASSWORD ).Value;
						//    userProfile.ContactsListVersion = ( uint ) version;
						//    userProfile.ForFriendsOnly = bool.Parse( number.Attribute( FOR_FRIENDS_ONLY ).Value );
						//    userProfile.MaxImageSize = ( byte ) int.Parse( number.Attribute( MAX_IMAGE_SIZE ).Value );
						//    userProfile.MobileClient = bool.Parse( number.Attribute( MOBILE_CLIENT ).Value );
						//    var profPasswElem = number.Attribute( PROFILE_PASSWORD );
						//    if (profPasswElem != null)
						//    {
						//        userProfile.ProfilePassword = profPasswElem.Value;
						//    }
						//    userProfile.ReceiveLinksFromUnknowns = bool.Parse(
						//        number.Attribute( RECEIVE_LINKS_FROM_UNKNOWNS ).Value );
						//    var statusAfterLoginNumeric = 1;
						//    var statusValue = number.Attribute( STATUS_AFTER_LOGIN ).Value;
						//    int.TryParse( statusValue, out statusAfterLoginNumeric );
						//    userProfile.StatusAfterLogin = ( GGStatus ) statusAfterLoginNumeric;
						//    var statusDescAfterLoginElem = number.Attribute( STATUS_DESC_AFTER_LOGIN );
						//    if (statusDescAfterLoginElem != null)
						//    {
						//        userProfile.StatusDescriptionAfterLogin = statusDescAfterLoginElem.Value;
						//    }
						//    this.UserProfiles.Add( userProfile );
						//}
						//catch (Exception)
						//{

						//}
						try
						{
							var xmlString = number.Value;
							var userProfile = this.SerializerForUserProfile.Deserialize( new StringReader( xmlString ) );
							this.UserProfiles.Add( ( UserProfile ) userProfile );
						}
						catch (Exception exception) { }
					}
				}
			}
		}

		private void saveAdditionalAppSettings( XElement rootElement )
		{
			if (this.AdditionalAppSettings != null)
			{
				var output = new StringWriter();
				this.xmlSerializerForAdditionalAppSettings.Serialize( output, this.AdditionalAppSettings );
				var xmlString = output.ToString();
				if (xmlString != null)
				{
					rootElement.Add( new XElement( ADDITIONAL_APP_SETTINGS, xmlString ) );
				}
			}
		}

		private void writeProfiles( XElement rootElement )
		{
			var profileGGNumbers = new XElement( PROFILES_GG_NUMBERS );
			this.UserProfiles.ForEach( userProfile =>
			{
				// stary sposob na serializacje
				//var element = new XElement( PROFILE_GG_NUMBER );
				//element.SetAttributeValue( NAME, number.AccountName );
				//element.SetAttributeValue( NUMBER, number.AccountNumber );
				//element.SetAttributeValue( PASSWORD, number.AccountPassword );
				////element.SetAttributeValue( PROFILE_PASSWORD, number.ProfilePassword );-
				//element.SetAttributeValue( CONTACTS_LIST_VERSION, number.ContactsListVersion );
				//element.SetAttributeValue( FOR_FRIENDS_ONLY, number.ForFriendsOnly );
				//element.SetAttributeValue( MAX_IMAGE_SIZE, number.MaxImageSize );
				//element.SetAttributeValue( MOBILE_CLIENT, number.MobileClient );
				//element.SetAttributeValue( PROFILE_PASSWORD, number.ProfilePassword );
				//element.SetAttributeValue( RECEIVE_LINKS_FROM_UNKNOWNS, number.ReceiveLinksFromUnknowns );
				//element.SetAttributeValue( STATUS_AFTER_LOGIN, ( int ) number.StatusAfterLogin );
				//element.SetAttributeValue( STATUS_DESC_AFTER_LOGIN, number.StatusDescriptionAfterLogin );
				//profileGGNumbers.Add( element );
				try
				{
					var writer = new StringWriter();
					this.SerializerForUserProfile.Serialize( writer, userProfile );
					var xmlString = writer.ToString();
					profileGGNumbers.Add( new XElement( PROFILE_GG_NUMBER ) { Value = xmlString } );
				}
				catch (Exception exception)
				{
					var chuj = "cipa";
				}
			} );
			rootElement.Add( profileGGNumbers );
		}

		protected override void OnLoadingErrorError( Exception exception )
		{

		}
	}

	/// <summary>
	/// Zapomnialem ze mozna serializowac klase poprzez serialize i nie trzeba pisac recznie calego data contractu.
	/// dlatego od teraz wrzucam wszystko do tej klasy i bede ja serializowal tym serialize, a to co juz jest
	/// to juz niech zostanie bo przeciez dziala.
	/// </summary>
	public class MainAppSettingsPack
	{

		public MainAppSettingsPack()
		{
			// ustawiam tutaj wartosci domyslne
			this.MaxImageSize = Config.MaxImageSizeDefaultSet;
			this.MobileClient = Config.MobileClientDefaultSet;
			this.ReceiveLinksFromUnknowns = Config.ReceiveLinksFromUnknownsDefaultSet;
			this.VibrateForMessage = true;
			this.VibrateForMessageInChatWindow = true;
			this.VibrateForSendingMessage = false;
			this.VibrateForSomeoneStatusChange = false;
			this.VibrationsDisabled = false;
			this.VibrateDuration = 5;
			this.AnnoucementDuringChat = true;
			this.AnnoucementForChangeStatus = true;
			this.AnnoucementForNewMessage = true;
			this.SoundsDisabled = false;
			this.SoundsDuringConversationEnabled = false;
			this.SoundsForMessagesEnabled = true;
			this.SoundForStatusesEnabled = true;
		}

		public DateTime LastTimeOfResetCounter { get; set; }


		public bool AgentDisabled { get; set; }

		public bool AnnoucementDuringChat { get; set; }

		public bool AnnoucementForChangeStatus { get; set; }

		public bool AnnoucementForNewMessage { get; set; }

		public bool AnnoucementsDisabled { get; set; }

		/// <summary>
		/// Ustawienie screen locka
		/// </summary>
		public bool DisableUserIdleLock { get; set; }

		public byte MaxImageSize { get; set; }

		public bool MobileClient { get; set; }

		public bool ReceiveLinksFromUnknowns { get; set; }

		/// <summary>
		/// jezeli true to wyswietla zamiast oczu ikonki z gg, niezaimplementowane.
		///  TODO zaimplementuj na pohybel skurwysynom :D
		/// </summary>
		public bool SecretModeGGOrginalIconsEnabled { get; set; }

		public bool SoundsDisabled { get; set; }

		public bool SoundsForMessagesEnabled { get; set; }
		public bool SoundForStatusesEnabled { get; set; }
		public bool SoundsDuringConversationEnabled { get; set; }

		/// <summary>
		/// 1-10
		/// </summary>
		public int VibrateDuration { get; set; }

		/// <summary>
		/// czy dla wiadomosci nowej konwersacji jest wibracja
		/// </summary>
		public bool VibrateForMessage { get; set; }

		public bool VibrateForMessageInChatWindow { get; set; }

		public bool VibrateForSendingMessage { get; set; }

		public bool VibrateForSomeoneStatusChange { get; set; }

		public bool VibrationsDisabled { get; set; }

		/// <summary>
		/// Oznacza ile agent uruchomien ma olać.To pozwala na regulacje pobierania - co 30 minut/ 60 minut/
		/// 2 h / 3h/ 5h/ raz dziennie itd. 
		/// </summary>
		[XmlElement( "AgentHowMuchStartupsToBypass" )]
		public int AgentHowMuchStartupsToBypass { get; set; }
		[XmlElement( "enabledInNight" )]
		public bool AgentEnabledInNight { get; set; }
	}
}
