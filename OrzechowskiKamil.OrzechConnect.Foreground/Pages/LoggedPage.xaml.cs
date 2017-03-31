using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using OrzechowskiKamil.OrzechConnect.Lib;
using OrzechowskiKamil.OrzechConnect.Lib.InPackets;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using System.Windows.Navigation;
using OrzechowskiKamil.OrzechConnect.Lib.DataStorage;
using OrzechowskiKamil.OrzechConnect.Background;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace OrzechowskiKamil.OrzechConnect.Foreground.ViewModels
{

	public partial class LoggedPage : PhoneApplicationPage
	{

		Timer annoucementPopupTimer;
		public const int ContactListListBoxHeight = 557;
		public const int ContactListListBoxHeightWhenFindContactPopup = 457;
		private const int ContactsIndex = 0;
		/// <summary>
		/// Rozmowy, kluczem jest numer GG
		/// </summary>
		private Dictionary<int, ConversationViewModel> conversations;
		private Timer findContactsPressKeyTimer;
		/// <summary>
		/// semafor blokujacy mozliwosc wyjscia z loginpage na czas reconnecta
		/// </summary>
		private bool isDuringReconnecting;
		/// <summary>
		/// jezeli true to znaczy ze wysunieta jest klawiatura umozliwiajaca pisanie wiadomosci 
		/// </summary>
		private bool isKeyboardSendMessageShowed;
		/// <summary>
		/// to jest po to zeby sie komunikat o bledzie 2x nie pokazywal
		/// </summary>
		private bool isNavigatedBack;
		/// <summary>
		/// To jest dla odroznienia eventu backkeyPress wywolanego sztucznie i wcisnieciem fizycznego przycisku
		/// </summary>
		private bool isNavigatedBackFake;
		PageOrientation lastOrientation;
		private object lastUsedTextBox;
		public Dictionary<int, int> NumbersHasPages;
		private bool timerIsStopped;



		public LoggedPage()
		{
			InitializeComponent();
			this.OrientationChanged += new EventHandler<OrientationChangedEventArgs>( MainPage_OrientationChanged );
			lastOrientation = this.Orientation;
			this.NumbersHasPages = new Dictionary<int, int>();
			this.conversations = new Dictionary<int, ConversationViewModel>();
			this.DataContext = this;
			var appGlobalData = AppGlobalData.Current;
			appGlobalData.StatusChanged += this.onUserStatusChanged;
			appGlobalData.MessageSended = this.onMessageSended;
			this.HandleMessagesFromAgent();
			appGlobalData.StartReceivingMessages( this.onReceiveMessage );
			this.refreshContactsInListBox();
			appGlobalData.StartListenForStatusChanges( this.onContactStatusChanged );
			this.reflectStatusOnAppBarButtonWithCurrentLoggedStatus();
			this.setAvailableStatusesInPopup();
			this.onEnteredContactsPage();
			this.hideFindContactPopup();
			appGlobalData.SendMessageTextBoxGotFocus += this.sendMessageTextBoxGotFocus;
			appGlobalData.SendMessageTextBoxLostFocus += this.sendMessageTextBoxLostFocus;
			appGlobalData.Disconnected = this.OnDisconnection;
			appGlobalData.ApplicationActivatedFromTombstone += this.OnDisconnection;
			this.SetAnnoucementOnPivotHeaderAboutMessagesUnreaded( 0 );
			this.onUnreadedMessagesAmountChanged();
		}

		/// <summary>
		/// wykonuje sie gdy ktos zmieni status
		/// </summary>
		/// <param name="status"></param>
		private void onContactStatusChanged( NotifyReply status )
		{

			//	var beforeChangeSelectedIndex = this.contactsList.SelectedIndex;
			this.refreshContactsInListBox();
			//	this.contactsList.SelectedIndex = (this.contactsList.Items.Count > beforeChangeSelectedIndex) ?
			//	beforeChangeSelectedIndex : this.contactsList.Items.Count - 1;
			if (status != null)
			{
				this.playStatusChangedSound();
				this.showPopupMessageAboutStatusChanged( ( int ) status.Number, status );
			}

		}
		private void HandleMessagesFromAgent()
		{
			AppGlobalData.Current.StartReceivingQueuedMessagesFromAgent( this.onReceiveMessage );
			// ustaw kafelki jako przeczytane
			LiveTile.MakeTileDefault();
		}



		private AppGlobalData appData
		{
			get
			{
				return AppGlobalData.Current;
			}
		}

		public string AvailableImage { get { return this.getPathForStatus( GGStatus.Available ); } }

		public string BusyImage { get { return this.getPathForStatus( GGStatus.Brb ); } }

		private string contactFilter { get; set; }

		///// <summary>
		///// Ustawia przycisk zamknij w appbarze jako aktywny/nieaktywny
		///// </summary>
		//public bool CloseButtonIsEnabled
		//{
		//    set
		//    {
		//        try
		//        {
		//            this.getCloseAppBarButton().IsEnabled = value;
		//        }
		//        catch (Exception) { }
		//    }
		//}
		//private IApplicationBarIconButton getCloseAppBarButton()
		//{
		//    return this.getButtonFromBar( 1 );
		//}
		/// <summary>
		/// Ustawia przycisk kontaktow w appBarze jako aktywny/nieaktywny
		/// </summary>
		public bool ContactsButtonIsEnabled
		{
			set
			{
				try
				{
					this.getContactsAppBarButton().IsEnabled = value;
				}
				catch (Exception) { }
			}
		}

		/// <summary>
		/// Pozwala ustawic src ikonki kontaktów
		/// </summary>
		public string ContactsIcon { get; set; }

		public string GGWithMeImage { get { return this.getPathForStatus( GGStatus.PleaseGGWithMe ); } }

		public string InvisibleImage { get { return this.getPathForStatus( GGStatus.Invisible ); } }

		private bool isFindContactPopupShowed
		{
			get
			{
				return this.FindContacts.Visibility == Visibility.Visible;
			}
		}

		private bool isStatusPopupOpened
		{
			get { return this.SetStatusPopup.IsOpen; }
		}

		/// <summary>
		/// Pozwala ustawic wyswietlane na ekranie kontakty za pomoca listy kontaktów
		/// </summary>
		public List<ContactViewModel> ListOfContacts
		{
			set
			{
				this.contactsList.ItemsSource = value;
			}
		}

		/// <summary>
		/// Pozwala ustawic wyswietlane na ekranie kontakty za pomoca listy notifyreply
		/// </summary>
		public List<NotifyReply> ListOfContactsByNotifyReply
		{
			set
			{
				this.ListOfContacts = this.getContactViewModelFromNotifyReplies( value, this.contactFilter );
			}
		}

		public string NotDisturbImage { get { return this.getPathForStatus( GGStatus.DontDisturb ); } }

		/// <summary>
		/// Zwraca przycisk "status" na appbarze.
		/// </summary>
		public IApplicationBarIconButton StatusButton
		{
			get
			{
				return this.getButtonFromBar( 1 );
			}
		}

		public string UnavailableImage { get { return this.getPathForStatus( GGStatus.NotAvailable ); } }




		public void HideProgressBar()
		{
			this.ProgressBar.Visibility = Visibility.Collapsed;
		}

		public void ReflectStatusOnAppBarButton( GGStatus status )
		{
			try
			{
				var imageSrc = "";
				//if (AppGlobalData.Current.IsBlackThemeSelected == false)
				//{
				//    imageSrc = StatusesHelper.StatusToPath( status, "/Images/Statuses/Metro/BlackStatus" );
				//}
				//else
				//{
				//    imageSrc = StatusesHelper.StatusToPath( status );
				//}
				imageSrc = StatusesHelper.GetImagePath( status, ColorScheme.OpacityMask );
				var description = StatusesHelper.GGStatusToTextualDescription( status );
				var button = this.StatusButton;
				button.IconUri = new Uri( imageSrc, UriKind.Relative );
				button.Text = description;
			}
			catch (Exception) { }
		}

		/// <summary>
		/// Odswieza kontakty wyswietlane pobierając ich listę ponownie z AppGlobalData.Current.
		/// Tam jest przechowywana aktualna wersja statusów.
		/// </summary>
		public void refreshContactsInListBox()
		{
			this.ListOfContactsByNotifyReply = AppGlobalData.Current.ListOfContactStatuses;
		}

		/// <summary>
		/// Ustawia tekst powiadomienia na headerze pivota
		/// </summary>
		/// <param name="annoucement"></param>
		public void SetAnnoucementOnPivotHeaderAboutMessagesUnreaded( int number )
		{
			var visibility = Visibility.Collapsed;
			if (number > 0)
			{
				visibility = Visibility.Visible;
			}
			this.pivot.Title = new PivotHeaderData { Visibility = visibility, Text = number.ToString() };
		}

		public void ShowProgressBar( string progressBarText )
		{
			this.ProgressBar.Visibility = Visibility.Visible;
			this.ProgressBarStatusLabel.Text = progressBarText;
		}

		public void VibrateFor( int milliseconds )
		{
			if ((this.pivot.SelectedIndex != ContactsIndex) &&
				(this.appData.AdditionalSettings.VibrateForMessageInChatWindow == false)) { return; }
			if (AppGlobalData.Current.AdditionalSettings.VibrationsDisabled == false)
			{
				var vibrateDurationMultipier =
					((( float ) AppGlobalData.Current.AdditionalSettings.VibrateDuration / 10.0) + 0.5);
				var miliseconds = Math.Round( milliseconds * vibrateDurationMultipier );
				VibrateController.Default.Start( TimeSpan.FromMilliseconds( milliseconds ) );
			}

		}

		/// <summary>
		/// Wibruje gdy ktos zmieni status
		/// </summary>
		public void VibrateForChangeStatus()
		{
			if (AppGlobalData.Current.AdditionalSettings.VibrateForSomeoneStatusChange)
				VibrateFor( Config.VibrationLengthOnNewMessage );
		}

		/// <summary>
		/// uruchami wibrator dla wiadomosci
		/// </summary>
		public void VibrateForMessage()
		{
			if (AppGlobalData.Current.AdditionalSettings.VibrateForMessage)
				VibrateFor( Config.VibrationLengthOnNewMessage );
		}

		/// <summary>
		/// uruchamia vibrator gdy jestesmy w oknie rozmowy i otrzymujemy wiadomosc
		/// </summary>
		public void VibrateForMessageInConversationWindowOpen()
		{
			if (AppGlobalData.Current.AdditionalSettings.VibrateForMessageInChatWindow)
				VibrateFor( Config.VibrationLengthOnNewMessage );
		}

		/// <summary>
		/// uruchamia wibrator przy wysylaniu wiadomosci
		/// </summary>
		public void VibrateForMessageSended()
		{
			if (AppGlobalData.Current.AdditionalSettings.VibrateForSendingMessage)
				VibrateFor( Config.VibrationLengthOnNewMessage );
		}

		protected override void OnBackKeyPress( System.ComponentModel.CancelEventArgs e )
		{

			{
				if (this.SetStatusPopup.IsOpen == true)
				{
					this.onBackButtonPressOnStatusPopup();
					e.Cancel = true;
				}
				else
				{
					if (!this.isOnContactsPivotItem( this.pivot ))
					{
						e.Cancel = true;
						this.onBackButtonPressOnConversationPivotItemOpened();
					}
					else
					{
						if (this.isFindContactPopupShowed == true)
						{
							this.onBackButtonPressOnFilterContactsPopupOpened();
							e.Cancel = true;
						}
						else
						{
							if (String.IsNullOrWhiteSpace( this.contactFilter ) == false)
							{
								this.startFilteringContactsList( null );
								e.Cancel = true;
							}
						}
					}
				}

			}
			base.OnBackKeyPress( e );
		}

		protected override void OnNavigatedTo( System.Windows.Navigation.NavigationEventArgs e )
		{
			if (this.isComingFromLoginPage( e ))
			{
			}
			else if (appData.ComingFromAddContact)
			{
				this.onComingFromAddContact();
			}
		}

		protected override void OnNavigatingFrom( System.Windows.Navigation.NavigatingCancelEventArgs e )
		{
			if (this.isNavigatedBackFake == false)
			{
				// TODO to obejscie moze byc zrodlem bledów
				// wyjscie z zalogowania tylko ze strony kontaktów.
				var url = e.Uri;
				if (this.isComingFromLoginPage( url ))
				{
					if (this.isDuringReconnecting == true)
					{
						e.Cancel = true;
					}
					else
					{
						try
						{
							var value = MessageBox.Show( Strings.Na_pewno_chcesz_wyjść,
								Strings.Potwierdzenie_tytuł, MessageBoxButton.OKCancel );
							if (value == MessageBoxResult.Cancel)
							{
								e.Cancel = true;
							}
							else
							{
								this.disconnect();
							}
						}
						catch (Exception) { }
						if (e.Cancel == false)
						{
							this.isNavigatedBack = true;
						}
						// reset do poczatkowego stanu
						this.isNavigatedBackFake = false;
					}
				}
			}
			base.OnNavigatingFrom( e );
		}

		private void addContact_Click( object sender, EventArgs e )
		{
			//NavigationService.Navigate( new Uri( "/MetroLayout/AddContact.xaml", UriKind.Relative ) );-
			NavigationService.Navigate( App.GetPathToPage( "AddContact.xaml" ) );
		}

		/// <summary>
		/// Znajduje (lub tworzy) konwersacje z takim uzytkwonikiem i dodaje nową wiadomosc do niej,
		/// o ile wiadomosc nie jest null, to wtedy tworzy pusta konwersacje lub ja wysweitla.
		/// </summary>
		private void addMessageToConversation( int sender, MessageViewModel msg )
		{
			ConversationViewModel conversation = null;
			var pageIndex = this.showConversationWork( sender, msg, out conversation );
			if (msg != null)
			{
				// pokazuje powiadomienie tylko jezeli nie jestesmy wlasnie w tej rozmowie, 
				if (pageIndex != this.pivot.SelectedIndex)
				{

					this.showPopupAboutIncomingMessage( sender, msg );

				}
				else
				{
					if (msg.IsIncoming == true)
					{
						this.VibrateForMessageInConversationWindowOpen();
					}
					// wlasnie przeczytalismy wiec odlicz te wiadomosci
					this.resetUnreadedMessagesForConversation( conversation );
				}
			}
		}
		private void playStatusChangedSound()
		{
			if (this.appData.AdditionalSettings.SoundForStatusesEnabled)
			{
				var name = "status.wav";
				this.playSound( name );
			}
		}

		private void playSound( string name )
		{
			if (this.appData.AdditionalSettings.SoundsDisabled == false)
			{
				this.sound.Stop();
				var playSound = true;
				#region logic

				var soundsDisabledDuringConversation =
					this.appData.AdditionalSettings.SoundsDuringConversationEnabled == false;
				var   isOnConversation = !this.isOnContactsPivotItem( this.pivot );

				#endregion
				if (isOnConversation && soundsDisabledDuringConversation)
				{
					playSound = false;
				}
				if (playSound == true)
				{
					var url = "Sounds/" + name;
					Stream stream = TitleContainer.OpenStream( url );

					SoundEffect effect = SoundEffect.FromStream( stream );

					FrameworkDispatcher.Update();

					effect.Play();



				}
			}
		}

		private void playNewMessageSound()
		{
			if (this.appData.AdditionalSettings.SoundsForMessagesEnabled == true)
			{
				this.playSound( "message.wav" );
			}
		}


		/// <summary>
		/// dodaje ilosc nieprzeczytanych wiadomosci do tej konwersacji i uruchamia event handler
		/// </summary>
		private void addUnreadedMessagesAmountForConversation( ConversationViewModel conversation, int howMuch )
		{
			if (conversation != null)
			{

				conversation.HowMuchUnreadedMessages += howMuch;
				this.onUnreadedMessagesAmountChanged();

			}
		}

		private void appbar_button3_Click( object sender, EventArgs e )
		{

		}

		private string appBarImagePath( string imageName )
		{
			var fileName = ImageHelper.GetImageNameForCurrentComposition( "AppBar", imageName, ColorScheme.OpacityMask );
			return fileName;
		}

		private void AvailableStatuses_Tap( object sender, System.Windows.Input.GestureEventArgs e )
		{
			// ustawia status i ukrywa popup
			this.setStatusByClickingPopupElement( () => { this.hideStatusPopup(); } );
		}

		/// <summary>
		/// oblicza ile mamy obecnie nieprzeczytanych wiadomosci
		/// </summary>
		/// <returns></returns>
		private int calculateTotalAmountOfUnreadedMessages()
		{
			var amount = 0;
			foreach (var pair in this.conversations)
			{
				amount += pair.Value.HowMuchUnreadedMessages;
			}
			return amount;
		}

		//private void closeButton_Click( object sender, EventArgs e )
		//{
		//    this.closeCurrentPivotItem();

		//}
		private void closeCurrentPivotItem()
		{
			var currSelectedIndex = this.pivot.SelectedIndex;
			this.pivot.Items.RemoveAt( currSelectedIndex );
			this.pivot.SelectedIndex = currSelectedIndex - 1;
			this.onPivotPageClosed( currSelectedIndex );
		}

		private void contactsButton_Click( object sender, EventArgs e )
		{
			if (this.isOnContactsPivotItem( this.pivot ))
			{
				this.onFindContactAppButtonPress();
			}
			else
			{
				if (this.isKeyboardSendMessageShowed)
				{
					this.onSendMessageAppBarButtonPress();
				}
				else
				{
					this.onGoToContactsAppButtonPress();
				}
			}
		}

		private void contactsList_Tap( object sender, System.Windows.Input.GestureEventArgs e )
		{
			new Timer( Config.DelayAfterSelectionChangedAndEventExecution, () =>
			{
				var listbox = ( ListBox ) sender;
				var selectedItem = ( ContactViewModel ) listbox.SelectedItem;
				if (selectedItem != null)
				{
					//var number = selectedItem.Number;
					//this.getConversationBySenderAndCreateIfNotExist( number );
					//var pageIndex = this.NumbersHasPages[number];
					//// skasuj filtr skoro i tak wychodzimy z kontaktow a user znalazl co chcial
					////	this.startFilteringContactsList( null );
					////this.createNewConversationPivotPage(
					////    this.getConversationBySenderAndCreateIfNotExist( selectedItem.Number ) );
					//this.pivot.SelectedIndex = pageIndex;
					var number = selectedItem.Number;
					this.goToConversationPivotPageWithThisSenderNumber( number );

				}
			}, true, true ).Start();
		}

		/// <summary>
		/// Tworzy nową pivot page dla nowej konwersacji.
		/// </summary>
		private void createNewConversationPivotPage( ConversationViewModel conversation )
		{
			var resources = App.Current.Resources;
			var template = ( DataTemplate ) resources["ConversationPivotPage"];
			// Z bólem serca musze to zakomentowac gdyz kontrolka pivot ma błąd ktory nie pozwala tego uzywac.
			//var headerTextBlock = new TextBlock
			//    {
			//        Text = conversation.ContactName,
			//        Foreground = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush
			//    };
			//conversation.HeaderTextBlock = headerTextBlock;
			var pivotItem = new PivotItem
			{
				Header = conversation.ContactName,
				DataContext = conversation,
				Content = conversation,
				ContentTemplate = template
			};
			this.pivot.Items.Add( pivotItem );
			var currentPivotIndex = this.pivot.Items.Count - 1;
			this.NumbersHasPages[conversation.GGNumber] = currentPivotIndex;
		}

		private void DeleteContact_Click( object sender, RoutedEventArgs e )
		{
			var result = MessageBox.Show( Strings.Czy_jesteś_pewien_że_chcesz_usunąć_dany_kontakt, Strings.Potwierdź, MessageBoxButton.OKCancel );
			if (result != MessageBoxResult.OK)
			{
				return;
			}
			var number = this.getContactNumberChoosedByContextMenu( sender );
			AppGlobalData.Current.DeleteContact( number, ( version ) =>
			{
				// success
				this.refreshContactsInListBox();
			},
			() =>
			{
				// FAIL - implement TODO implement
			} );

		}

		private void disconnect()
		{
			try
			{
				var data = AppGlobalData.Current;
				data.Engine.Disconnect();
				data.Engine = null;
				data.MessageSended = null;
				data.saveContactsListForCurrentInFile();
			}
			catch (Exception) { }
		}

		private void FindContactsTextBox_KeyUp( object sender, System.Windows.Input.KeyEventArgs e )
		{
			App.RefreshScreensaverTimer_();
			this.stopFindFriendsTriggerTimer();
			if (e.PlatformKeyCode == 13)
			{
				this.startFilteringAndHidePopup();
			}
		}

		private void FindContactsTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{

			if (String.IsNullOrWhiteSpace( this.FindContactsTextBox.Text ) == false)
			{
				this.timerIsStopped = false;
				this.findContactsPressKeyTimer = new Timer( Config.FindContactsAfterStopWritingInSeconds, () =>
				{
					if (this.timerIsStopped == false)
					{
						// powinno dzialac bez tego dodatkowego bool semafora, no ale jednak nie dziala ? kto wie dlaczego
						// niech mi powie.
						this.stopFindFriendsTriggerTimer();
						this.startFilteringContactsListByTextEnteredInFilteringTextBox();
					}
				} ).Start();
			}
		}

		private void FindContactsTextBox_TextChanged_1( object sender, TextChangedEventArgs e )
		{

		}

		private T FindFirstElementInVisualTree<T>( DependencyObject parentElement ) where T : DependencyObject
		{
			var count = VisualTreeHelper.GetChildrenCount( parentElement );
			if (count == 0)
				return null;

			for (int i = 0; i < count; i++)
			{
				var child = VisualTreeHelper.GetChild( parentElement, i );

				if (child != null && child is T)
				{
					return ( T ) child;
				}
				else
				{
					var result = FindFirstElementInVisualTree<T>( child );
					if (result != null)
						return result;

				}
			}
			return null;
		}

		private IApplicationBarIconButton getButtonFromBar( int indexOfButton )
		{
			return (( IApplicationBarIconButton ) ApplicationBar.Buttons[indexOfButton]);
		}

		private int getContactNumberChoosedByContextMenu( object sender )
		{
			var numberToDelete = 0;
			var item = this.selectedByMenuLoginListBoxItem( sender );
			if (item != null)
			{
				numberToDelete = item.Number;
			}
			return numberToDelete;
		}

		private IApplicationBarIconButton getContactsAppBarButton()
		{
			return this.getButtonFromBar( 0 );
		}

		private List<ContactViewModel> getContactViewModelFromNotifyReplies(
			List<NotifyReply> list, string filter )
		{
			if (filter != null)
			{
				filter = filter.Trim();
			}
			var collection = new List<ContactViewModel>();
			var appData = AppGlobalData.Current;
			foreach (var listItem in list)
			{
				var number = ( int ) listItem.Number;
				var contact = appData.GetContactByNumber( number );
				if (contact == null)
				{
					var chuj = "why?";
				}
				var name = (contact == null) ? number.ToString() : contact.ShowName;
				// optymalniej jest najpierw przefiltrowac liste kontaktow a dopiero pozniej ja posortowac niz odwrotnie
				var contactMatchFilter = ((String.IsNullOrWhiteSpace( filter )) ||
					(PolishChars.CutPolishChars( name.ToUpper() ).IndexOf(
					PolishChars.CutPolishChars( filter.ToUpper() ) ) != -1));
				if (contactMatchFilter)
				{
					collection.Add( new ContactViewModel
					{
						Name = name,
						Number = number,
						Status = listItem.Status,
						StatusDescription = listItem.Description
					} );
				}
			}
			this.sortContactsListByStatusesPriority( collection );
			return collection;
		}

		private ConversationViewModel getConversationByPivotIndex( int index )
		{
			return this.getConversationBySender( this.getGGSenderByPivotPage( index ) );
		}

		private ConversationViewModel getConversationBySender( int sender )
		{
			ConversationViewModel conversation = null;
			try
			{
				conversation = this.conversations[sender];
			}
			catch (Exception) { }
			return conversation;
		}

		private ConversationViewModel getConversationBySenderAndCreateIfNotExist( int sender )
		{
			ConversationViewModel conversation = this.getConversationBySender( sender );
			if (conversation == null)
			{
				var contact = AppGlobalData.Current.GetContactByNumber( sender );
				var contactName = (contact != null) ? contact.ShowName : sender.ToString();
				conversation = new ConversationViewModel
				{
					GGNumber = sender,
					ContactName = contactName,
					Messages = new ObservableCollection<MessageViewModel>()
				};
				this.conversations[sender] = conversation;
			}
			return conversation;
		}

		private PivotItem getCurrentPivotItem()
		{
			var pivotItemCurrent = (( PivotItem ) this.pivot.SelectedItem);
			return pivotItemCurrent;
		}

		private int getGGSenderByPivotPage( int pivotPageIndex )
		{
			foreach (var pair in this.NumbersHasPages)
			{
				if (pair.Value == pivotPageIndex)
				{
					return pair.Key;
				}
			}
			return 0;
		}

		private string getPathForStatus( GGStatus status )
		{
			return StatusesHelper.GetImagePath( status, ColorScheme.Current );
		}

		private int getPivotPageForConversationOrCreateIfNotExist( ConversationViewModel conversation )
		{
			var pageIndex = -1;
			var senderNr = conversation.GGNumber;
			try
			{
				pageIndex = this.NumbersHasPages[senderNr];
			}
			catch (Exception) { }
			if (pageIndex == -1)
			{
				this.createNewConversationPivotPage( conversation );
				pageIndex = this.pivot.Items.Count - 1;
			}
			this.scrollToLastMessage( conversation );
			return pageIndex;
		}

		/// <summary>
		/// Pozwala pobrac ostatnio wybrany status w popupie zmiany statusu
		/// </summary>
		private AvailableStatusViewModel getStatusChoosedByStatusPopup()
		{
			return ( AvailableStatusViewModel ) this.AvailableStatuses.SelectedItem;
		}

		/// <summary>
		/// Znajduje taka konwersacje w pamieci i przenosi nas do karty z tą konwersacją. Nalezy pdoac tylko numer GG
		/// </summary>
		/// <param name="number"></param>
		private void goToConversationPivotPageWithThisSenderNumber( int number )
		{
			var pivotIndexOfConversation = this.showConversationWork( number, null );
			this.pivot.SelectedIndex = pivotIndexOfConversation;
		}

		private void hideFindContactPopup()
		{
			this.contactsList.Height = ContactListListBoxHeight;
			this.FindContacts.Visibility = Visibility.Collapsed;
			this.FindContactsTextBox.Text = "";
			this.stopFindFriendsTriggerTimer();
		}

		private void hidePopupMessage()
		{
			//this.PopupMessageContainer.Visibility = Visibility.Collapsed;
			this.PushMessagePopup.IsOpen = false;
	

		}

		private void hideStatusPopup()
		{
			this.SetStatusPopup.IsOpen = false;
		}

		private bool isComingFromLoginPage( Uri url )
		{
			return (url == App.GetPathToPage( "LoginPage.xaml" ) && (this.isOnContactsPivotItem( this.pivot )));
		}

		private bool isComingFromLoginPage( NavigationEventArgs e )
		{
			return this.isComingFromLoginPage( e.Uri );
		}

		private bool isOnContactsPivotItem( Pivot pivot )
		{
			return pivot.SelectedIndex == 0;
		}

		private void LayoutRoot_Tap( object sender, System.Windows.Input.GestureEventArgs e )
		{
			this.hidePopupMessage();
		}

		void MainPage_OrientationChanged( object sender, OrientationChangedEventArgs e )
		{
			PageOrientation newOrientation = e.Orientation;


			// Orientations are (clockwise) 'PortraitUp', 'LandscapeRight', 'LandscapeLeft'

			RotateTransition transitionElement = new RotateTransition();

			switch (newOrientation)
			{
				case PageOrientation.Landscape:
				case PageOrientation.LandscapeRight:
				// Come here from PortraitUp (i.e. clockwise) or LandscapeLeft?
				if (lastOrientation == PageOrientation.PortraitUp)
					transitionElement.Mode = RotateTransitionMode.In90Counterclockwise;
				else
					transitionElement.Mode = RotateTransitionMode.In180Clockwise;
				break;
				case PageOrientation.LandscapeLeft:
				// Come here from LandscapeRight or PortraitUp?
				if (lastOrientation == PageOrientation.LandscapeRight)
					transitionElement.Mode = RotateTransitionMode.In180Counterclockwise;
				else
					transitionElement.Mode = RotateTransitionMode.In90Clockwise;
				break;
				case PageOrientation.Portrait:
				case PageOrientation.PortraitUp:
				// Come here from LandscapeLeft or LandscapeRight?
				if (lastOrientation == PageOrientation.LandscapeLeft)
					transitionElement.Mode = RotateTransitionMode.In90Counterclockwise;
				else
					transitionElement.Mode = RotateTransitionMode.In90Clockwise;
				break;
				default:
				break;
			}

			// Execute the transition
			PhoneApplicationPage phoneApplicationPage = ( PhoneApplicationPage ) ((( PhoneApplicationFrame ) Application.Current.RootVisual)).Content;
			ITransition transition = transitionElement.GetTransition( phoneApplicationPage );
			transition.Completed += delegate
			{
				transition.Stop();
			};
			transition.Begin();

			lastOrientation = newOrientation;
		}
		private void sendMessageTextBox_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
		{
			//if (e.PlatformKeyCode == 13)
			//{
			//    onSendMessageByTextBox( sender );
			//}
			App.RefreshScreensaverTimer_();
		}
		private void onBackButtonPressOnConversationPivotItemOpened()
		{
			this.closeCurrentPivotItem();
		}

		private void onBackButtonPressOnFilterContactsPopupOpened()
		{
			this.startFilteringAndHidePopup();
		}

		private void onBackButtonPressOnStatusPopup()
		{
			this.hideStatusPopup();
		}

		private void onClickOnPopupAboutNewIncomingMessage( int sender )
		{
			this.hidePopupMessage();
			if (sender != 0)
			{
				this.goToConversationPivotPageWithThisSenderNumber( sender );
			}
		}

		private void onComingFromAddContact()
		{
			//appData.ComingFromAddContact = false;
			//this.refreshContactsInListBox();
			// niestety z nieznanych powodow gg_notify_add nie chce dzialac wiec restartuje cala rozmowe i chuj 
			// TODO popraw to kiedys zeby bylo normalnei bo wstyd aż 
			this.tryReconnect( () =>
			{
				this.refreshContactsInListBox();
			} );
		}

		/// <summary>
		/// wykona sie gdy serwer nas rozłączy
		/// </summary>
		private void OnDisconnection()
		{

			this.tryReconnect();
		}

		private void onEnteredContactsPage()
		{
			//	this.CloseButtonIsEnabled = false;
			//this.ContactsButtonIsEnabled = false;
			//var fileName= ImageHelper.GetIconNameBySchemeColor( "/Images", "ab_find.png" ;
			var fileName = this.appBarImagePath( "find.png" );
			this.setContactButtonImageAndText( fileName, Strings.SzukajAppBarLabel );

		}

		private void onEnteredConversationPage()
		{
			// oznaczamy jako przeczytane.

			var conversation = this.getConversationByPivotIndex( pivot.SelectedIndex );
			this.resetUnreadedMessagesForConversation( conversation );
			if (this.isKeyboardSendMessageShowed)
			{

				this.setContactButtonImageAndText( this.appBarImagePath( "send_message.png" ),
				Strings.WyslijAppBarLabel );
			}
			else
			{
				this.setContactButtonImageAndText( this.appBarImagePath( "contact_book.png" ),
					Strings.KontaktyAppBarLabel );
				try
				{
					this.scrollToLastMessage( this.getConversationByPivotIndex( pivot.SelectedIndex ) );
				}
				catch (Exception) { }
			}
		}

		private void onFindContactAppButtonPress()
		{
			if (this.isFindContactPopupShowed == false)
			{
				this.showFindContactPopup();
			}
			else
			{
				this.hideFindContactPopup();
			}
		}

		private void onGoToContactsAppButtonPress()
		{
			this.pivot.SelectedIndex = 0;
		}

		private void onMessageSended( int number, string message )
		{
			try
			{
				this.addMessageToConversation( number, new MessageViewModel
				{
					IsIncoming = false,
					SendDate = DateTime.Now,
					MessageContent = message
				} );

			}
			catch (Exception) { }
		}

		private void onPivotItemGotFocus()
		{
			var pivot = this.pivot;
			if (this.isOnContactsPivotItem( pivot ))
			{
				this.onEnteredContactsPage();
			}
			else
			{
				this.onEnteredConversationPage();
			}
		}

		private void onPivotPageClosed( int indexOfPage )
		{
			var numberGGOfClosedPage = this.getGGSenderByPivotPage( indexOfPage );
			if (numberGGOfClosedPage != 0)
			{
				this.NumbersHasPages.Remove( numberGGOfClosedPage );
			}
		}

		private bool onReceiveMessage( GG_RECV_MSG80 message )
		{

			var msg = AppGlobalData.MessagePacketToModel( message );
			return this.onReceiveMessage( msg );
		}

		private bool onReceiveMessage( MessageModel message )
		{
			if (AppGlobalData.Current.Engine == null)
			{
				return false;
			}
			this.playNewMessageSound();
			MessageViewModel msg = new MessageViewModel
			{
				SendDate = message.SendDate,
				IsIncoming = message.IsIncoming,
				MessageContent = message.MessageContent
			};
			this.addMessageToConversation( ( int ) message.SenderNumber, msg );
			return true;
		}

		private bool onReceiveMessage( List<MessageModel> messages )
		{
			if (messages != null)
			{
				foreach (var message in messages)
				{
					if (this.onReceiveMessage( message ) == false) { return false; }
				}
			}
			return true;
		}

		private void onSendMessageAppBarButtonPress()
		{
			if (this.lastUsedTextBox != null)
			{
				App.onSendMessageByTextBox( this.lastUsedTextBox );
			}
		}

		private void onUnreadedMessagesAmountChanged()
		{
			var amount = this.calculateTotalAmountOfUnreadedMessages();
			//var textToShow = "";
			//if (amount > 0)
			//{
			//    textToShow = String.Format( Strings.NieprzeczytaneWiadomosciAnnoucementOnPivotHeader, amount );
			//}
			this.SetAnnoucementOnPivotHeaderAboutMessagesUnreaded( amount );
		}

		private void onUserStatusChanged( GGStatus status, string description )
		{
			this.ReflectStatusOnAppBarButton( status );
		}

		private void Pivot_PageChanged( object sender, SelectionChangedEventArgs e )
		{
			this.onPivotItemGotFocus();
		}

		private void PopupMessageContainer_Tap( object sender, System.Windows.Input.GestureEventArgs e )
		{
			var stackPanel = ( StackPanel ) sender;
			if (stackPanel.Tag is NewMessagePopupIndicatorViewModel)
			{
				this.onClickOnPopupAboutNewIncomingMessage( (( NewMessagePopupIndicatorViewModel ) stackPanel.Tag).senderNumber );
			}
		}

		private void reflectStatusOnAppBarButtonWithCurrentLoggedStatus()
		{
			this.ReflectStatusOnAppBarButton( AppGlobalData.Current.CurrentLoggedProfile.LastStatus );
		}

		private void RemoveContact_Click( object sender, RoutedEventArgs e )
		{

		}

		/// <summary>
		/// resetuje ilosc nieporzeczytanych wiadomosci dla tej konwersacji i uruchamia event handler
		/// </summary>
		/// <param name="conversation"></param>
		private void resetUnreadedMessagesForConversation( ConversationViewModel conversation )
		{
			if (conversation != null)
			{
				if (conversation.HowMuchUnreadedMessages != 0)
				{
					conversation.HowMuchUnreadedMessages = 0;
					this.onUnreadedMessagesAmountChanged();
				}
			}
		}

		private void scrollToLastMessage( ConversationViewModel conversation )
		{
			try
			{
				var currPivotItem = this.getCurrentPivotItem();
				var objectFound = this.FindFirstElementInVisualTree<ListBox>( currPivotItem );
				var listbox = ( ListBox ) objectFound;
				listbox.ScrollIntoView( listbox.Items[listbox.Items.Count - 1] );
			}
			catch (Exception) { }
		}

		private ContactViewModel selectedByMenuLoginListBoxItem( object sender )
		{
			var menuItem = sender as MenuItem;
			var fe = VisualTreeHelper.GetParent( menuItem ) as FrameworkElement;
			var obj = fe.DataContext as ContactViewModel;
			return obj;

		}

		//private List<NotifyReply> lastReplyList;
		//private ObservableCollection<ContactToDisplay> contacts;
		//public ObservableCollection<ContactToDisplay> Contacts
		//{
		//    get
		//    {
		//        if (lastReplyList != AppGlobalData.Current.ListOfContactStatuses)
		//        {
		//            this.lastReplyList = AppGlobalData.Current.ListOfContactStatuses;
		//            this.contacts = this.convertNotifyReplyListToObservableCollection( lastReplyList );
		//        }
		//        return this.contacts;
		//    }
		//}


		private void sendMessageTextBoxGotFocus( object textBox )
		{
			this.isKeyboardSendMessageShowed = true;
			pivot.IsHitTestVisible = false;
			this.lastUsedTextBox = textBox;
			this.onPivotItemGotFocus();
		}

		private void sendMessageTextBoxLostFocus()
		{
			// kurwa, co to tutaj robi? this.startFilteringAndHidePopup();
			pivot.IsHitTestVisible = true;
			this.isKeyboardSendMessageShowed = false;
			this.onPivotItemGotFocus();
		}

		/// <summary>
		/// Wstawia mozliwe do ustawienia statusy do listboxa w popupie.
		/// </summary>
		private void setAvailableStatusesInPopup()
		{
			this.AvailableStatuses.ItemsSource = StatusesHelper.GetAllStatusesForListbox();
		}

		private void setContactButtonImageAndText( string url, string text )
		{
			var contactsButton = this.getContactsAppBarButton();
			contactsButton.IconUri = new Uri( url,
				UriKind.Relative );
			contactsButton.Text = text;
		}

		private void setStatusButtonClick( object sender, RoutedEventArgs e )
		{
			try
			{
				this.setStatusChoosedByPopup();

			}
			catch (Exception) { }
		}

		private void setStatusByClickingPopupElement( Action afterAction )
		{
			// musi byc przez timer bo inaczej brzydko wyglada
			new Timer( Config.DelayAfterSelectionChangedAndEventExecution, () =>
			{
				if (this.AvailableStatuses.SelectedItem != null)
				{
					this.setStatusButtonClick( null, null );
					try
					{
						if (afterAction != null) { afterAction(); }
					}
					catch (Exception) { }
				}
			}, true, true ).Start();
		}

		private void setStatusChoosedByPopup()
		{
			var statusToSet = this.getStatusChoosedByStatusPopup().Status;
			var descriptionToSet = this.newDescription.Text;
			AppGlobalData.Current.SetStatusNormally( new OrzechowskiKamil.OrzechConnect.Lib.AppGlobalData.StatusArgs { status = statusToSet, description = descriptionToSet } );
		}

		private void settingsButton_Click( object sender, EventArgs e )
		{

		}

		/// <summary>
		/// Robi cala robote zwiazana ze znalrezieniem konwersacji i wyswietleniem jej. jesli doda sie msg, to ten 
		/// msg zostanie dodany do konwersacji. Zwraca page index tej konwersacji
		/// </summary>
		private int showConversationWork( int sender, MessageViewModel msg )
		{
			ConversationViewModel conversation = null;
			return this.showConversationWork( sender, msg, out conversation );
		}

		private int showConversationWork( int sender, MessageViewModel msg, out ConversationViewModel conversation )
		{
			ConversationViewModel conversationCreated = this.getConversationBySenderAndCreateIfNotExist( sender );
			if (msg != null)
			{
				// zinkrementuj licznik wiadomosci
				if (msg.IsIncoming == true)
				{
					this.addUnreadedMessagesAmountForConversation( conversationCreated, 1 );
				}
				else
				{
					this.VibrateForMessageSended();
				}
				conversationCreated.Messages.Add( msg );
			}
			var pageIndex = this.getPivotPageForConversationOrCreateIfNotExist( conversationCreated );
			conversation = conversationCreated;
			return pageIndex;
		}

		private void showFindContactPopup()
		{
			this.contactsList.Height = ContactListListBoxHeightWhenFindContactPopup;
			this.FindContacts.Visibility = Visibility.Visible;
			this.FindContactsTextBox.Text = "";
		}

		/// <summary>
		/// Pokazuje popup o przychodzacej wiadomosci
		/// </summary>
		private void showPopupAboutIncomingMessage( int sender, MessageViewModel msg )
		{
			if (AppGlobalData.Current.AdditionalSettings.AnnoucementForNewMessage == false) { return; }
			var profile = AppGlobalData.Current.GetContactByNumber( sender );
			var senderName = (profile == null) ? sender.ToString() : profile.ShowName;
			var titleMessage = String.Format( Strings.Ktoś_przesyła_ci_wiadomość, senderName );
			var descMessage = msg.MessageContent;
			var imageUrl = this.appBarImagePath( "send_message.png" );
			this.showPopupMessage( titleMessage, descMessage, imageUrl, Config.NewMessageAnnoucementDurationSeconds,
				new NewMessagePopupIndicatorViewModel { senderNumber = sender } );
			// zawibruj
			this.VibrateForMessage();
		}

		/// <summary>
		/// Pokazuje popupa z zadaną zawartoscią. Imitacja pusha.
		/// </summary>
		private void showPopupMessage( string message, string messageDetailed, string imageUrl,
			int displayDurationInSeconds, object tagObj )
		{
			var isAllowedToShowPopup = true;
			if (Config.HidePushPopupNotificationsWhileStatusPopupOpened == true)
			{
				if (this.isStatusPopupOpened == true)
				{
					isAllowedToShowPopup = false;
				}
			}
			if (Config.HidePushPopupNotificationsWhileWriting == true)
			{
				if (this.isKeyboardSendMessageShowed == true)
				{
					isAllowedToShowPopup = false;
				}
			}
			// jesli user wylaczyl powiadomienia
			if (this.appData.AdditionalSettings.AnnoucementsDisabled == true)
			{
				isAllowedToShowPopup = false;
			}
			// jesli powiadomienia podczas rozmow sa wylaczone
			if (this.pivot.SelectedIndex != ContactsIndex)
			{
				if (this.appData.AdditionalSettings.AnnoucementDuringChat == false)
				{
					isAllowedToShowPopup = false;
				}
			}
			if (isAllowedToShowPopup)
			{
				//this.PopupMessageContainer.Visibility = Visibility.Visible;
				this.PushMessagePopup.IsOpen = true;
				this.PopupMessageContainer.Tag = tagObj;
				this.PopupMessage.Text = message;
				this.PopupMessageDesc.Text = messageDetailed;
				BitmapImage logo = new BitmapImage();
				if (imageUrl != null)
				{
					logo.UriSource = new Uri( imageUrl, UriKind.Relative );
				}
				this.PopupIcon.Source = logo;
				if (this.annoucementPopupTimer != null)
				{
					this.annoucementPopupTimer.Stop();
				}
				this.annoucementPopupTimer = new Timer( displayDurationInSeconds, () =>
				{
					this.hidePopupMessage();
				}, false, true ).Start();
			}
		}

		/// <summary>
		/// Pokazuje wiadomosc o zmianie statusu przez koogs
		/// </summary>
		private void showPopupMessageAboutStatusChanged( int ggNumber, NotifyReply status )
		{
			// sprawdza czy nie to nie powiadomienie o nas 
			if (ggNumber == this.appData.CurrentLoggedProfile.AccountNumber) { return; }

			if (AppGlobalData.Current.AdditionalSettings.AnnoucementForChangeStatus == false) { return; }
			var contact = AppGlobalData.Current.GetContactByNumber( ggNumber );
			if (contact != null)
			{
				this.VibrateForChangeStatus();
				var name = contact.ShowName;
				var textualStatus = StatusesHelper.GGStatusToTextualDescription( status.Status );
				var message = String.Format( Strings.KtosZmienilSwojStatusPowiadomienie, name, textualStatus );
				this.showPopupMessage(
					message,
					status.Description,
					StatusesHelper.GetImagePath( status.Status, ColorScheme.Current ),
					Config.StatusChangeAnnoucementDurationSeconds, null );
			}
		}

		/// <summary>
		/// Wyswietla status popup
		/// </summary>
		private void showStatusPopup()
		{
			this.SetStatusPopup.IsOpen = true;
			this.newDescription.Text = AppGlobalData.Current.CurrentLoggedProfile.LastDescription;
			// powoduje ustawienie jako zaznaczonego obecnie wybranego statusu.
			for (int i = 0, max = this.AvailableStatuses.Items.Count; i < max; i++)
			{
				var curr = ( AvailableStatusViewModel ) this.AvailableStatuses.Items[i];
				if (curr.Status == AppGlobalData.Current.CurrentLoggedProfile.LastStatus)
				{
					this.AvailableStatuses.SelectedIndex = i;
					break;
				}
			}
		}

		/// <summary>
		/// Sortuje podana liste kontaktów tak aby najbardziej dostepne statusy znalazly sie najwyzej, a najmniej
		/// dostępne najdalej
		/// </summary>
		private void sortContactsListByStatusesPriority( List<ContactViewModel> collection )
		{
			collection.Sort( delegate( ContactViewModel item1, ContactViewModel item2 )
			{
				if (item1.ContactPriority > item2.ContactPriority) { return 1; }
				else if (item1.ContactPriority < item2.ContactPriority) { return -1; }
				else
				{
					// jesli statusy maja te sama waznosc to dalej lecim po nazwach
					return String.Compare( item1.Name, item2.Name );
				}
			} );
		}

		private void startFilteringAndHidePopup()
		{
			this.startFilteringContactsListByTextEnteredInFilteringTextBox();
			this.hideFindContactPopup();
		}

		/// <summary>
		/// Zastosowuje podany filter (mozna podac null, to wtedy usuwa filtr) do kontaktów.
		/// Filtr nie zglupieje jesli komus zmieni sie status.
		/// Odswieza kontakty jesli filtr rozni sie od starego filtra, jezeli sa takie same to nie reaguje.
		/// </summary>
		private void startFilteringContactsList( string filtering )
		{
			if (String.IsNullOrWhiteSpace( filtering )) { filtering = null; }
			// to musi sie odbywac tak naokolo, inaczej app by glupiala w momencie zmiany
			// przez kogos statusu
			if (filtering != this.contactFilter)
			{
				this.contactFilter = filtering;
				this.refreshContactsInListBox();
			}
		}

		private void startFilteringContactsListByTextEnteredInFilteringTextBox()
		{
			var filter = this.FindContactsTextBox.Text;
			this.startFilteringContactsList( filter );
		}

		private void statusButton_Click( object sender, EventArgs e )
		{
			if (!this.isStatusPopupOpened)
			{
				this.showStatusPopup();
			}
			else
			{
				this.setStatusButtonClick( null, null );
			}
		}

		private void statusButton_Click_1( object sender, EventArgs e )
		{

		}

		private void stopFindFriendsTriggerTimer()
		{
			this.timerIsStopped = true;
			if (this.findContactsPressKeyTimer != null)
			{
				this.findContactsPressKeyTimer.Stop();
			}
		}

		private void TextBlock_GotFocus( object sender, RoutedEventArgs e )
		{
			this.newDescription.Text = "";
		}

		private void TextBlock_LostFocus( object sender, RoutedEventArgs e )
		{
			// ustawia status ale nie ukrywa boxa
			this.setStatusByClickingPopupElement( null );
		}

		private void tryReconnect( Action onSuccess = null )
		{
			if (this.isNavigatedBack == false)
			{
				Action onReconnectFail = () =>
				{
					try
					{

						if (this.isDuringReconnecting == true)
						{
							AppGlobalData.Current.ForceDisconnect();
							MessageBox.Show( Strings.Połączenie_zerwane_zaloguj_ponownie );
							this.isNavigatedBack = true;
							this.isNavigatedBackFake = true;
							this.isDuringReconnecting = false;
							if (NavigationService.CanGoBack)
							{
								NavigationService.GoBack();
							}

						}
					}
					catch (Exception) { }
				};
				this.isDuringReconnecting = true;
				this.ShowProgressBar( Strings.Trwa_wznawianie_połączenia_z_serwerem );
				// wstawienie tutaj false pozwoli na niesciagniecie kontaktow po reconnecie
				// jezeli reconnect trwal bardzo krotko (np ponizej 10 sekund). male jest prawdopodobienstwo 
				// ze ktos wtedy zmienil opis TODO zrob ten reconnect.
				var shouldGetUserStatuses = true;
				if (Config.IfTombstoningTimeIsShortDontRequestUserStatuses)
				{
					var lastTombstoneDuration = AppGlobalData.Current.GetLastTombstoneDurationInSeconds();
					if (lastTombstoneDuration != -1)
					{
						if (lastTombstoneDuration <= Config.HowMuchSecondsOfTombstoningIsShortTime)
						{
							// faktycznie bylo krótko, wiec nie pobierajmy statusów.
							shouldGetUserStatuses = false;
						}
					}
				}
				AppGlobalData.Current.StartLoginProcess( AppGlobalData.Current.CurrentLoggedProfile,
					() =>
					{// onsuccess
						this.isDuringReconnecting = false;
						this.HideProgressBar();
						this.onUnreadedMessagesAmountChanged();
						AppGlobalData.Current.SetStatusLast();
						if (onSuccess != null) { onSuccess(); }
					}, () =>
					{//ontimeout
						onReconnectFail();
					}, ( error ) =>
					{//onError
						onReconnectFail();
					}, ( phase ) =>
					{//onphasechange

					}, () =>
					{// on unhandled exception
						this.isDuringReconnecting = false;
						this.HideProgressBar();
					}, false, shouldGetUserStatuses, true );
			}
		}

		private void vibrateFor_ms( int vibrateFor )
		{
			this.VibrateFor( vibrateFor );
		}




		public class PivotHeaderData
		{
			#region Properties (2)

			public string Text { get; set; }

			public Visibility Visibility { get; set; }

			#endregion Properties
		}

		private void newDescription_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
		{
			App.RefreshScreensaverTimer_();
		}

		private void sound_MediaOpened( object sender, RoutedEventArgs e )
		{
			this.sound.Play();
		}
	}

	public class NewMessagePopupIndicatorViewModel
	{

		public int senderNumber { get; set; }
	}

	/// <summary>
	/// ViewModel jednego kontaktu na liscie kontaktow
	/// </summary>
	public class ContactViewModel
	{

		/// <summary>
		/// Potrzebne do sortowania kontaktow po statusach
		/// </summary>
		public int ContactPriority
		{
			get
			{
				var status = GGStatusHelp.GGStatusToGGStatusWithoutDesc( this.Status );
				switch (status)
				{
					case GGStatus.PleaseGGWithMe: return 0;
					case GGStatus.Available: return 1;
					case GGStatus.Brb: return 2;
					case GGStatus.DontDisturb: return 3;
					case GGStatus.Invisible: return 4;
					case GGStatus.NotAvailable: return 5;
					case GGStatus.Blocked: return 6;
				}
				return 10;
			}
		}

		// niepotrzebne
		//public string ImageMaskSource
		//{
		//    get
		//    {
		//        var result = StatusesHelper.StatusToPathMask( this.Status ); return result;
		//    }
		//}
		public string ImageSource
		{
			get
			{

				var result = StatusesHelper.GetImagePath( this.Status, ColorScheme.Current );
				return result;
			}
		}

		public string Name { get; set; }

		public int Number { get; set; }

		public GGStatus Status { get; set; }

		public string StatusDescription { get; set; }

		public override string ToString()
		{
			return String.Format( "{0}, {1}, {2}", this.Name, this.Number, this.StatusDescription );

		}
	}
	public enum ColorScheme
	{
		Black = 0,
		White = 1,
		OpacityMask = 2,
		NoColorScheme = 3,
		Current = 4,
		ReversedCurrent = 5,
	}

	public static class ImageHelper
	{
		private static ColorScheme GetColorSchemeReversed( ColorScheme colorScheme )
		{
			if (colorScheme == ColorScheme.Black) { return ColorScheme.White; }
			if (colorScheme == ColorScheme.White) { return ColorScheme.Black; }
			return colorScheme;
		}

		public static string GetImageNameForCurrentComposition( string folder, string iconName, ColorScheme scheme )
		{
			return GetImageName( AppGlobalData.Current.CurrentCompositionName, folder, iconName, scheme );
		}

		public static string GetImageName( string compositionName, string folder, string iconName, ColorScheme scheme )
		{
			var schemeFolder = "";
			var newScheme = scheme;
			if (scheme == ColorScheme.Current || scheme == ColorScheme.ReversedCurrent)
			{
				newScheme = (AppGlobalData.Current.IsBlackThemeSelected) ? ColorScheme.Black : ColorScheme.White;
				if (scheme == ColorScheme.ReversedCurrent)
				{
					newScheme = GetColorSchemeReversed( newScheme );
				}
			}
			switch (newScheme)
			{
				case ColorScheme.Black: schemeFolder = "Black/"; break;
				case ColorScheme.White: schemeFolder = "White/"; break;
				case ColorScheme.OpacityMask: schemeFolder = "OpacityMask/"; break;
				case ColorScheme.NoColorScheme: schemeFolder = ""; break;
			}
			var imageUrl = String.Format( "/Images/{0}/{1}/{2}{3}", compositionName, folder, schemeFolder, iconName );
			return imageUrl;
		}
	}

	public class StatusesHelper
	{


		public static List<AvailableStatusViewModel> GetAllStatusesForListbox()
		{
			return new List<AvailableStatusViewModel>
				{
					createAvailableStatus(GGStatus.AvailableDesc),
					createAvailableStatus(GGStatus.PleaseGGWithMeDesc),
					createAvailableStatus(GGStatus.BrbDesc),
					createAvailableStatus(GGStatus.DontDisturbDesc),
					createAvailableStatus(GGStatus.InvisibleDesc)
					//this.createAvailableStatus(GGStatus.NotAvailableDesc),
				};
		}

		public static string GetImageNameForStatus( GGStatus statusToConvert )
		{
			var image = "";
			var status = GGStatusHelp.GGStatusToGGStatusWithoutDesc( statusToConvert );
			switch (status)
			{
				case GGStatus.Invisible: image = "invisible.png"; break;
				case GGStatus.PleaseGGWithMe: image = "write_to_me.png"; break;
				case GGStatus.Available: image = "available.png"; break;
				case GGStatus.DontDisturb: image = "do_not_disturb.png"; break;
				case GGStatus.Brb: image = "brb.png"; break;
				case GGStatus.NotAvailable: image = "unavailable.png"; break;
			}
			return image;
		}

		public static string GetImagePath( GGStatus statusToConvert, ColorScheme scheme )
		{
			return ImageHelper.GetImageNameForCurrentComposition( "Statuses", GetImageNameForStatus( statusToConvert ), scheme );
		}

		public static string GGStatusToTextualDescription( GGStatus status )
		{
			status = GGStatusHelp.GGStatusToGGStatusWithoutDesc( status );
			switch (status)
			{
				case GGStatus.Available: return Strings.Dostępny;
				case GGStatus.Blocked: return Strings.Zablokowany;
				case GGStatus.Brb: return Strings.Zaraz_wracam;
				case GGStatus.DontDisturb: return Strings.Nie_przeszkadzać;
				case GGStatus.Invisible: return Strings.Niewidoczny;
				case GGStatus.NotAvailable: return Strings.Niedostępny;
				case GGStatus.PleaseGGWithMe: return Strings.Pogadaj_ze_mną;
			}
			return null;
		}

		private static AvailableStatusViewModel createAvailableStatus( GGStatus status )
		{
			return new AvailableStatusViewModel
			{
				Status = status,
				StatusImagePath = StatusesHelper.GetImagePath( status, ColorScheme.Current ),
				StatusName = StatusesHelper.GGStatusToTextualDescription( status )
			};
		}
	}

	/// <summary>
	/// ViewModel konwersacji
	/// </summary>
	public class ConversationViewModel
	{

		public string ContactName { get; set; }

		public int GGNumber { get; set; }

		/// <summary>
		/// Nic nie robi, do usuniecia pozniej.
		/// </summary>
		public bool HasUnreadedMessages
		{
			get;
			set;
		}

		public TextBlock HeaderTextBlock { get; set; }

		public int HowMuchUnreadedMessages { get; set; }

		public ObservableCollection<MessageViewModel> Messages { get; set; }


		//private SolidColorBrush getBrush( bool isReaded )
		//{
		//    var resName = "";
		//    if (isReaded == false)
		//    {
		//        resName = "PhoneForegroundBrush";
		//    }
		//    else
		//    {
		//        resName = "PhoneAccentBrush";
		//    }
		//    return ( SolidColorBrush ) Application.Current.Resources[resName];
		//}
		//public SolidColorBrush TextColor
		//{
		//    set
		//    {
		//        if (this.HeaderTextBlock != null)
		//        {
		//            this.HeaderTextBlock.Foreground = value;
		//        }
		//    }
		//}
	}


	/// <summary>
	/// ViewModel wiadomosci
	/// </summary>
	public class MessageViewModel
	{
		public string MessageContent { get; set; }
		public DateTime SendDate { get; set; }

		private SolidColorBrush accentBrush;
		private SolidColorBrush accentDarkBrush;
		private SolidColorBrush getColorFromResources( string name )
		{
			return App.Current.Resources[name] as SolidColorBrush;
		}
		public SolidColorBrush AccentBrush
		{
			get
			{
				if (this.accentBrush == null)
				{
					this.accentBrush = this.getColorFromResources( "PhoneAccentBrush" );
				}
				return this.accentBrush;
			}
		}
		public SolidColorBrush AccentDarkBrush
		{
			get
			{
				if (this.accentDarkBrush == null)
				{
					this.accentDarkBrush = this.getColorFromResources( "PhoneDarkAccentBrush" );
				}
				return this.accentDarkBrush;
			}
		}
		public Brush Color
		{
			get
			{
				SolidColorBrush result;
				if (this.IsIncoming)
				{
					result = this.AccentBrush;
				}
				else
				{
					result = this.AccentDarkBrush;
				}
				return result;
			}
		}



		public bool IsIncoming { get; set; }

		public string DateFormatted
		{
			get
			{
				var dateToFormat = this.SendDate;
				var dateNow = DateTime.Now;
				var yearNow = dateNow.Year;
				var year = dateToFormat.Year;
				var monthNow = dateNow.Month;
				var month = dateToFormat.Month;
				var dayNow = dateNow.Day;
				var day = dateToFormat.Day;
				var yearString = "";
				if ((yearNow != year) || (monthNow != month) || (dayNow != day))
				{
					yearString = String.Format( "{0}-{1}, ", this.addLeadingZero( month ), this.addLeadingZero( day ) );
				}

				return String.Format( "{0} {1}:{2}:{3}", yearString, this.addLeadingZero( dateToFormat.Hour ),
					this.addLeadingZero( dateToFormat.Minute ), dateToFormat.Second );
			}
		}


		private string addLeadingZero( int number )
		{
			var hour = number.ToString();
			if (hour.Length < 2)
			{
				hour = "0" + hour;
			}
			return hour;
		}
		public Visibility VisibilityOfTopTriangle
		{
			get
			{
				if (this.IsIncoming) { return Visibility.Visible; }
				return Visibility.Collapsed;
			}
		}


		public Visibility VisibilityOfBottomTriangle
		{
			get
			{
				if (!this.IsIncoming) { return Visibility.Visible; }
				return Visibility.Collapsed;
			}
		}

		public HorizontalAlignment AlignmentOfMessage
		{
			get
			{
				if (this.IsIncoming)
				{
					return HorizontalAlignment.Left;
				}
				else
				{
					return HorizontalAlignment.Right;
				}
			}
		}

		internal static MessageViewModel FromMsg( MessageModel messageModel )
		{
			return new MessageViewModel
			{
				IsIncoming = messageModel.IsIncoming,
				MessageContent = messageModel.MessageContent,
				SendDate = messageModel.SendDate
			};
		}
	}

	/// <summary>
	/// ViewModel jednego elementu z listy statusów do wyboru przy ustawianiu statusu w popup
	/// </summary>
	public class AvailableStatusViewModel
	{

		public GGStatus Status { get; set; }

		public string StatusImagePath { get; set; }

		public string StatusName { get; set; }
	}
}