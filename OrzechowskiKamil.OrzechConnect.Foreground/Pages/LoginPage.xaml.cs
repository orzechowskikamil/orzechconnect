using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using OrzechowskiKamil.OrzechConnect.Lib;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using OrzechowskiKamil.OrzechConnect.Lib.Processess;
using OrzechowskiKamil.OrzechConnect.Lib.OutPackets;
using System.Threading;
using OrzechowskiKamil.OrzechConnect.Lib.Connection;
using OrzechowskiKamil.OrzechConnect.Foreground.Helpers;
using OrzechowskiKamil.OrzechConnect.Background;

namespace OrzechowskiKamil.OrzechConnect.Foreground.ViewModels
{



	public partial class LoginPage : PhoneApplicationPage
	{

		public LoginPage()
		{
			InitializeComponent();
			this.DataContext = this;
			//if (AppGlobalData.Current.UserProfilesList.Count == 0)
			//{
			//    data.AddNewUserProfile( "Testowy profil", 42375465, "decade2" );
			//    data.AddNewUserProfile( "Kamil", 2271216, "geniusz" );
			//    data.DefaultProfileNumber = 2271216;
			//    //InputScopeNameValue.
			//}
			this.hideProgressBar();
			this.RefreshDataInLoginListBox();
			this.SettingsPageRefreshSettings();
			this.SettingsPageRefreshControls();
			tryLogDefaultAccount();
			//	this.enableIdleDetectionStartValue = !AppGlobalData.Current.AdditionalSettings.DisableUserIdleLock;
		}

		private void tryLogDefaultAccount()
		{
			var defaultNumber = AppGlobalData.Current.DefaultProfileNumber;
			var items = this.loginListBox.Items;
			if (items != null)
			{
				foreach (var itemViewModel in items)
				{
					var profile = ( UserProfileViewModel ) itemViewModel;
					if (profile != null)
					{
						if (profile.AccountNumber == defaultNumber)
						{
							this.doOnLoginListBoxTap( profile );
						}
					}
				}
			}
		}


		/// <summary>
		/// Pozwala wstrzyknac do listboxa liste z loginListBoxItems.
		/// </summary>
		private List<UserProfileViewModel> dataForLoginListBox
		{
			set { this.loginListBox.ItemsSource = value; }
			get { return ( List<UserProfileViewModel> ) this.loginListBox.ItemsSource; }
		}

		/// <summary>
		/// Pozwala wstrzyknac do listboxa z profilami liste profili uzytkownika z appGLobalData
		/// </summary>
		private List<UserProfile> dataForLoginListBoxByUserProfilesList
		{
			set
			{
				var convertedList = this.convertUserProfilesListToLoginListBoxItemsList( value, AppGlobalData.Current );
				this.dataForLoginListBox = convertedList;
				if ((this.txtNoItems != null) && (convertedList != null))
				{
					this.txtNoItems.Visibility = (convertedList.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
				}
			}
		}

		public string HyperlinkLink
		{
			get { return Strings.LinkDoGaduGadu; }
		}

		public string HyperlinkText
		{
			get { return Strings.TekstNaLinkuDoGaduGadu; }
		}

		public string MakeNewAccountText
		{
			get
			{
				return Strings.UtworzKontoTekst;
			}
		}

		public Visibility ProgressBarVisibility
		{
			set
			{
				this.ProgressBar.Visibility = value;
			}
		}

		/// <summary>
		/// Zwraca obecnie zacznaczony item w LoginListBox
		/// </summary>
		private UserProfileViewModel selectedLoginListBoxItem
		{
			get
			{
				return (this.loginListBox.SelectedItem == null) ? null
					: ( UserProfileViewModel ) this.loginListBox.SelectedItem;
			}
		}

		public string StatusLabelText
		{
			set
			{
				this.statusLabel.Text = value;
			}
		}

		public Visibility StatusLabelVisibility
		{
			set
			{
				this.statusLabel.Visibility = value;
			}
		}



		protected override void OnNavigatedFrom( System.Windows.Navigation.NavigationEventArgs e )
		{
			// na tej stronie moga sie zmienic ustawienia agenta wiec przy wyjsciu trzeba zapisac zmiany.
			//ScheduledAgent.SetAgentLikeInSettings();

			base.OnNavigatedFrom( e );

		}

		protected override void OnNavigatedTo( System.Windows.Navigation.NavigationEventArgs e )
		{
			base.OnNavigatedTo( e );
			this.RefreshDataInLoginListBox();
		}


		private void addNewProfile( string name, int numberGG, string password )
		{
			try
			{
				AppGlobalData.Current.AddNewUserProfile( name, numberGG, password );
				MessageBox.Show( Strings.ProfilZostalUtworzony );
			}
			catch (Exception)
			{
				MessageBox.Show( String.Format( Strings.MessageBoxProbaUtworzeniaDwochProfiliOTymSamymNumerze, numberGG ), Strings.MessageBoxTitleProbaUtworzeniaDwochProfiliOTymSamymNumerze,
					MessageBoxButton.OK );
			}
			this.RefreshDataInLoginListBox();
		}



		private void button1_Click( object sender, RoutedEventArgs e )
		{

			try
			{
				var accName = this.textBox1.Text;
				var ggStringNumber = this.textBox2.Text;
				var password = this.textBox3.Password;
				this.validateFieldIsNotNull( accName, Strings.NazwaKontaTextBoxLabel );
				this.validateFieldIsNotNull( ggStringNumber, Strings.NumerGGTextBoxLabel );
				this.validateFieldIsNotNull( password, Strings.HasloGGTextBoxLabel );
				var ggNumber = 0;
				try
				{
					ggNumber = int.Parse( ggStringNumber );
				}
				catch (Exception)
				{
					throw new Exception( Strings.NumerGGMozeBycZapisanyTylkoZaPomocaCyfrException );
				}
				if (String.IsNullOrWhiteSpace( accName ))
				{
					throw new Exception( Strings.Nazwa_profilu_nie_może_być_pusta );
				}
				if (String.IsNullOrWhiteSpace( password ))
				{
					throw new Exception( Strings.Hasło_nie_może_być_puste );
				}
				this.addNewProfile( accName, ggNumber, password );
			}
			catch (Exception exception)
			{
				MessageBox.Show( exception.Message );
			};
		}

		/// <summary>
		/// Konwertuje liste profili na liste zrozumiala dla loginlistboxa
		/// </summary>
		private List<UserProfileViewModel> convertUserProfilesListToLoginListBoxItemsList( List<UserProfile> profilesList,
			AppGlobalData data )
		{
			try
			{
				var result = new List<UserProfileViewModel>();
				profilesList.ForEach( profile =>
				{
					result.Add( new UserProfileViewModel
					{
						AccountName = profile.AccountName,
						AccountNumber = profile.AccountNumber,
						IsDefault = data.IsProfileDefault( profile )
					} );
				} );
				return result;
			}
			catch (Exception) { }
			return null;
		}

		private void doLoginProcess( UserProfileViewModel item )
		{
			// semafor nie pozwalajacy odpalic drugiego procesu logowania jesli pierwszy trwa.
			if (this.isLoginProcessNow == false)
			{
				this.showProgressBar( Strings.LogowanieKomunikatProgressBar );
				this.callCounter = 0;
				this.isLoginProcessNow = true;
				this.tryLoginProcess( item );
			}
		}

		private void tryLoginProcess( UserProfileViewModel item )
		{
			this.callCounter++;
			Action onTimeout = 	// ponowna próba, jesli nie bylo juz zbyt duzo razy
				() =>
				{
					if (callCounter < Config.MaxTriesForLogin)
					{
						//this.showProgressBar( String.Format(
						//"Logowanie trwało zbyt długo, {0} próba...", this.callCounter + 1 ) );
						this.tryLoginProcess( item );
					}
					else
					{
						MessageBox.Show(
							String.Format( Strings.SerwerNieOdpowiadaSprobojPozniej,
							Config.MaxTriesForLogin ) );
						this.onLoginProcessEnd();
					}
				};
			AppGlobalData.Current.StartLoginProcessForNumber( item.AccountNumber, () =>
			{
				// sukces
				this.onLoginProcessEnd();
				AppGlobalData.Current.SetStatusOnLogin();
				NavigationService.Navigate( App.GetPathToPage( "LoggedPage.xaml" ) );
			}, ( phase ) =>
			{
				/// odkomentowanie tego spowoduje ze beda sie zmieniac stany przy statusbarze
				//switch (phase)
				//{
				//    case AppGlobalData.LoginPhases.Connecting: break;
				//    case AppGlobalData.LoginPhases.DownloadingContactsStatuses:
				//    this.showProgressBar( "Pobieranie statusów znajomych..." );
				//    break;
				//    case AppGlobalData.LoginPhases.Logging:
				//    this.showProgressBar( "Logowanie..." ); break;
				//    case AppGlobalData.LoginPhases.VerifingContactsActuality:
				//    this.showProgressBar( "Weryfikowanie listy kontaktów..." ); break;
				//}
			},
				// on timeout
			() =>
			{
				onTimeout();
			},
				// on exception
			( exception ) =>
			{
				var reason = exception.Message;
				if (exception is InternetConnectionLostException)
				{
					reason = Strings.Brak_połączenia_z_internetem;
				}
				if (exception is LoginOrPasswordIncorrectException)
				{
					reason = Strings.Błędny_login_lub_hasło;
				}
				MessageBox.Show( String.Format( Strings.NieMoznaZalogowacSieDoProfilu,
				reason ) );
				this.onLoginProcessEnd();
			}, () =>
			{
				// on unexpected exception
				onTimeout();
			}, true, true, false );
		}
		private int callCounter = 0;

		private bool isLoginProcessNow;
		private void onLoginProcessEnd()
		{
			this.hideProgressBar();
			this.isLoginProcessNow = false;
		}

		private void edit( UserProfileViewModel item )
		{

			NavigationService.Navigate( App.GetPathToPage( String.Format( "EditProfile.xaml?number={0}", item.AccountNumber ) ) );
		}



		private void hideProgressBar()
		{
			this.ProgressBarVisibility = Visibility.Collapsed;
			this.StatusLabelVisibility = Visibility.Collapsed;
		}

		// przechowuje ustawienie enableIdleDetection z momentu startu strony
		private bool? enableIdleDetectionStartValue;

		/// <summary>
		/// ładuje wartosci z appcurrent data do kontrolek
		/// </summary>
		public void SettingsPageRefreshSettings()
		{
			var curr = AppGlobalData.Current;
			var addSett = curr.AdditionalSettings;
			if (this.enableIdleDetectionStartValue == null)
			{
				this.enableIdleDetectionStartValue = !addSett.DisableUserIdleLock;
			}
			this.enableIdleDetection.IsChecked = this.enableIdleDetectionStartValue.GetValueOrDefault();
			this.soundsEnabled.IsChecked = !addSett.SoundsDisabled;
			this.annoucementsEnabled.IsChecked = !addSett.AnnoucementsDisabled;
			this.agentEnabled.IsChecked = !addSett.AgentDisabled;
			this.vibrationsEnabled.IsChecked = !addSett.VibrationsDisabled;

		}

		/// <summary>
		/// ładuje wartosci z kontrolek do appCurrentData
		/// </summary>
		public void SettingsPageRefreshSettingsSetFromControls()
		{
			var addSett = AppGlobalData.Current.AdditionalSettings;
			addSett.DisableUserIdleLock = !this.enableIdleDetection.IsChecked.GetValueOrDefault();
			addSett.SoundsDisabled = !this.soundsEnabled.IsChecked.GetValueOrDefault();
			addSett.AnnoucementsDisabled = !this.annoucementsEnabled.IsChecked.GetValueOrDefault();
			addSett.AgentDisabled = !this.agentEnabled.IsChecked.GetValueOrDefault();
			addSett.VibrationsDisabled = !this.vibrationsEnabled.IsChecked.GetValueOrDefault();
		}

		/// <summary>
		/// ustawia polskie opisy kontrolkom
		/// </summary>
		public void SettingsPageRefreshControls()
		{
			ToggleSwitchHelper.SetEnabledAndDisabledValues( new ToggleSwitch[]{
				 this.soundsEnabled, this.vibrationsEnabled, this.agentEnabled, this.annoucementsEnabled
			}, "Włączone", "Wyłączone" );
			var restart = "";
			if (this.enableIdleDetectionStartValue.GetValueOrDefault() != this.enableIdleDetection.IsChecked)
			{
				restart = " (zrestartuj)";
			}
			ToggleSwitchHelper.SetEnabledAndDisabledValues(
				new ToggleSwitch[] { this.enableIdleDetection }, "Tak" + restart, "Nie" +
				restart );

			this.manageAnnoucementDescription();
			this.manageAgentDescription();
			this.manageDisableIdleDetectionDescription();
			this.manageDefaultProfileIsNullDesc();


		}

		private void manageDefaultProfileIsNullDesc()
		{
			ToggleSwitchHelper.SetVisibilityByBoolValue( AppGlobalData.Current.DefaultProfileNumber == 0,
						this.noUserDefaultDescription );
		}

		private void manageAnnoucementDescription()
		{
			ToggleSwitchHelper.SetVisibilityByToogleValue( this.annoucementsEnabled,
					this.AnnoucementsDescription, false );
		}

		private void manageAgentDescription()
		{
			ToggleSwitchHelper.SetVisibilityByToogleValue( this.agentEnabled,
				this.AgentLiveTilesDescription, false );

		}

		private void manageDisableIdleDetectionDescription()
		{
			ToggleSwitchHelper.SetVisibilityByToogleValue( this.enableIdleDetection,
				this.disableScreenEnabled, this.disableScreenDisabled );
		}



		private void loginListBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{

		}

		private void loginListBox_Tap( object sender, System.Windows.Input.GestureEventArgs e )
		{
			var item = this.selectedLoginListBoxItem;
			this.doOnLoginListBoxTap( item );
		}

		private void doOnLoginListBoxTap( UserProfileViewModel item )
		{
			if (item != null)
			{
				this.doLoginProcess( item );
			}
		}

		private void LoginProfilesList_Edit( object sender, RoutedEventArgs e )
		{
			this.edit( this.selectedByMenuLoginListBoxItem( sender ) );
		}

		private void LoginProfilesList_Remove( object sender, RoutedEventArgs e )
		{
			var result = MessageBox.Show( Strings.Czy_jesteś_pewien_że_chcesz_usunąć_profil, Strings.Potwierdź_title_usunięcie_profilu, MessageBoxButton.OKCancel );
			if (result != MessageBoxResult.OK)
			{ return; }
			try
			{
				this.removeProfile( this.selectedByMenuLoginListBoxItem( sender ) );
			}
			catch (Exception) { }
		}

		private void LoginProfilesList_SetAsDefault( object sender, RoutedEventArgs e )
		{
			try
			{
				var item = this.selectedByMenuLoginListBoxItem( sender );
				if (item.IsDefault == true)
				{
					this.unsetDefaultProfile();
				}
				else
				{
					this.setAsDefaultProfile( item );
				}
			}
			catch (Exception) { }
		}



		private void onLoginTimeout()
		{
			this.hideProgressBar();
			MessageBox.Show( Strings.LogowanieTrwaloZbytDlugoIZostaloPrzerwane );
		}

		/// <summary>
		/// Zaciaga swieze dane z appGlobalData do loginListBox
		/// </summary>
		private void RefreshDataInLoginListBox()
		{
			this.dataForLoginListBoxByUserProfilesList = AppGlobalData.Current.UserProfilesList;
		}

		private void removeProfile( UserProfileViewModel item )
		{
			AppGlobalData.Current.RemoveUserProfileByNumber( item.AccountNumber );
			this.RefreshDataInLoginListBox();
		}

		private UserProfileViewModel selectedByMenuLoginListBoxItem( object sender )
		{
			var menuItem = sender as MenuItem;
			var fe = VisualTreeHelper.GetParent( menuItem ) as FrameworkElement;
			var comment = fe.DataContext as UserProfileViewModel;
			return comment;

		}

		private void setAsDefaultProfile( UserProfileViewModel item )
		{
			var number = item.AccountNumber;
			this.setDefaultProfileNumberAndRefresh( number );
		}

		private void setDefaultProfileNumberAndRefresh( int number )
		{
			AppGlobalData.Current.DefaultProfileNumber = number;
			this.RefreshDataInLoginListBox();
		}



		private void showProgressBar( string statusDescription )
		{
			this.ProgressBarVisibility = Visibility.Visible;
			this.StatusLabelVisibility = Visibility.Visible;
			this.StatusLabelText = statusDescription;
		}

		private void unsetDefaultProfile()
		{
			this.setDefaultProfileNumberAndRefresh( 0 );
		}

		private void validateFieldIsNotNull( string fieldName, string fieldValue )
		{
			if (String.IsNullOrWhiteSpace( fieldValue ))
			{
				throw new Exception( String.Format( Strings.PoleNieMozeBycPuste, fieldName ) );
			}
		}

		private void textBlock2_KeyDown( object sender, KeyEventArgs e )
		{
			
		}

		private void tryMoveFocusToNextTextBox( Control textbox, KeyEventArgs e )
		{
			if (e.Key == Key.Enter)
			{
				textbox.Focus();
				e.Handled = true;
			}
		}

		private void textBox1_KeyDown( object sender, KeyEventArgs e )
		{
			App.RefreshScreensaverTimer_();
			this.tryMoveFocusToNextTextBox( this.textBox2, e );
		}

		private void textBox2_KeyDown( object sender, KeyEventArgs e )
		{
			App.RefreshScreensaverTimer_();
			this.tryMoveFocusToNextTextBox( this.textBox3, e );
		}

		private void textBox3_KeyDown( object sender, KeyEventArgs e )
		{
			App.RefreshScreensaverTimer_();
			if (e.Key == Key.Enter)
			{
				this.button1_Click( this.button1, null );
			}
		}

		private void Button_Click( object sender, RoutedEventArgs e )
		{
			NavigationService.Navigate( App.GetPathToPage( "AdvancedSettings.xaml" ) );
		}

		private void ToggleSwitch_Click( object sender, RoutedEventArgs e )
		{
			this.SettingsPageRefreshSettingsSetFromControls();
			this.SettingsPageRefreshControls();
		}

		private void Panorama_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			this.manageDefaultProfileIsNullDesc();
		}
	}

	/// <summary>
	/// ValueObject dla pojedynczej pozycji na liscie profili.
	/// </summary>
	public class UserProfileViewModel
	{

		public string AccountName { get; set; }

		public int AccountNumber { get; set; }

		public bool IsDefault { get; set; }

		public string ImageName
		{

			get
			{
				var path = ImageHelper.GetImageNameForCurrentComposition( "UserProfiles", "profile.png", ColorScheme.Current );
				return path;
			}
		}

		public Brush RectangleColor
		{
			get
			{
				return new SolidColorBrush( (this.IsDefault)
					? ( Color ) Application.Current.Resources["PhoneAccentColor"] : Colors.Gray );
			}
		}

		public string SetAsDefaultLabel
		{
			get
			{
				return (this.IsDefault) ? Strings.CofnijUstawienieJakoDomyslnyProfilMenu : Strings.UstawJakoDomyslnyProfilMenu;
			}
		}
	}
}