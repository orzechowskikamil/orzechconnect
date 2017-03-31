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
using OrzechowskiKamil.OrzechConnect.Foreground.Helpers;

namespace OrzechowskiKamil.OrzechConnect.Foreground.ViewModels
{
	public partial class EditProfile : PhoneApplicationPage
	{
		UserProfile editedProfile;

		enum Pages
		{
			Information = 0,
			Statuses = 1,
			Settings = 2
		}

		public EditProfile()
		{
			InitializeComponent();
		}

		protected override void OnNavigatedTo( System.Windows.Navigation.NavigationEventArgs e )
		{
			base.OnNavigatedTo( e );
			try
			{
				var editedProfileNumber = this.NavigationContext.QueryString["number"];
				int editedNumber = 0;
				int.TryParse( editedProfileNumber, out editedNumber );
				if (editedNumber != 0)
				{
					this.editedProfile = AppGlobalData.Current.GetUserProfileByNumber( editedNumber );
					this.onLoad();
				}
				else { throw new Exception( "" ); }
			}
			catch (Exception exception)
			{
				// jak nie ma profilu to nic tu po nas 
				NavigationService.GoBack();
			}
		}
		#region load data

		private void readDataForInformationPage()
		{
			//	this.ggNumber.Text = String.Format( "Numer: {0}", this.editedProfile.AccountNumber );
			this.Name.Text = String.Format( "{0}", this.editedProfile.AccountName );
			this.Password.Password = String.Format( "{0}", this.editedProfile.AccountPassword );

		}

		private void readDataForStatusesPage()
		{
			this.loadStatuses( this.Statuses );
			this.InitialStatus.Text = String.Format( "{0}", this.editedProfile.StatusDescriptionAfterLogin );
			this.setStatusStartupToogle.IsChecked = !this.editedProfile.SetLastSettedStatusAfterLogin;

			this.setStatusAfterTombstoneToogle.IsChecked = this.editedProfile.SetStatusOnTombstone;
			this.statusForTombstoning.Text = String.Format( "{0}", this.editedProfile.DescriptionStatusOnTombstone );
			this.showStatusToAll.IsChecked = !this.editedProfile.ForFriendsOnly;
		}

		private void loadStatuses( ListBox list )
		{
			var statuses = StatusesHelper.GetAllStatusesForListbox();
			list.ItemsSource = statuses;
			var key = 0;
			foreach (var item in statuses)
			{
				if (item.Status == this.editedProfile.StatusAfterLogin)
				{
					list.SelectedIndex = key;
				}
				key++;
			}
		}

		private void readDataForPage( int pageIndex )
		{
			switch (pageIndex)
			{
				case ( int ) Pages.Information: this.readDataForInformationPage(); break;
				case ( int ) Pages.Settings: this.readDataForSettingsPage(); break;
				case ( int ) Pages.Statuses: this.readDataForStatusesPage(); break;
			}
		}

		private void readDataForSettingsPage()
		{
		}

		#endregion

		#region refresh controls

		private void refreshControlsStatesInStatusesPage()
		{
			var visiblityOfSetStatusControls = Visibility.Collapsed;
			if (this.setStatusStartupToogle.IsChecked.GetValueOrDefault())
			{
				visiblityOfSetStatusControls = Visibility.Visible;
			}
			this.ShowIfSetStatusStartupToggled.Visibility = visiblityOfSetStatusControls;
			var visibilityOfTombstoneStatusControls = Visibility.Collapsed;
			if (this.setStatusAfterTombstoneToogle.IsChecked.GetValueOrDefault())
			{
				visibilityOfTombstoneStatusControls = Visibility.Visible;
			}
			this.ShowIfTombstoneStatusToogled.Visibility = visibilityOfTombstoneStatusControls;
			ToggleSwitchHelper.SetEnabledAndDisabledValues( this.setStatusStartupToogle,
				"Ten status", "Ostatni status" );
			ToggleSwitchHelper.SetEnabledAndDisabledValues( this.setStatusAfterTombstoneToogle, "Ustaw",
				"Nie ustawiaj" );
			ToggleSwitchHelper.SetEnabledAndDisabledValues( this.showStatusToAll, "Wszystkim", "Tylko znajomym" );
		}

		private void refreshControlsStatesInInformationPage()
		{
			//if (String.IsNullOrWhiteSpace( (this.Name.Text) ) == true)
			//{
			//    throw new Exception( Strings.Nazwa_profilu_nie_może_być_pusta );
			//}
			//if (String.IsNullOrWhiteSpace( this.Password.Password ) == true)
			//{
			//    throw new Exception( Strings.Hasło_nie_może_być_puste );
			//}
		}

		private void refreshControlsStatesInSettingsPage()
		{
		}

		private void refreshControlsStatesForPage( int index )
		{
			switch (index)
			{
				case ( int ) Pages.Information: this.refreshControlsStatesInInformationPage(); break;
				case ( int ) Pages.Settings: this.refreshControlsStatesInSettingsPage(); break;
				case ( int ) Pages.Statuses: this.refreshControlsStatesInStatusesPage(); break;
			}
		}

		#endregion

		#region save data

		private void saveDataInStatusesPage()
		{
			this.editedProfile.StatusDescriptionAfterLogin = this.InitialStatus.Text;
			this.editedProfile.SetLastSettedStatusAfterLogin = this.setStatusStartupToogle.IsChecked.GetValueOrDefault();
			this.editedProfile.StatusAfterLogin = this.getStatusChoosedInInitialStatus();
			this.editedProfile.SetStatusOnTombstone = this.setStatusAfterTombstoneToogle.IsChecked.GetValueOrDefault();
			this.editedProfile.DescriptionStatusOnTombstone = this.statusForTombstoning.Text;
		}

		private void saveDataInInformationPage()
		{
		}

		private void saveDataInSettingsPage()
		{
		}

		private void saveDataInPage( int index )
		{
			switch (this.pivot.SelectedIndex)
			{
				case ( int ) Pages.Information: this.saveDataInInformationPage(); break;
				case ( int ) Pages.Settings: this.saveDataInSettingsPage(); break;
				case ( int ) Pages.Statuses: this.saveDataInStatusesPage(); break;
			}
		}

		#endregion

		private GGStatus getStatusChoosedInInitialStatus()
		{

			return (( AvailableStatusViewModel ) this.Statuses.SelectedItem).Status;

		}

		private void saveDataForInformation()
		{
			this.editedProfile.AccountName = (String.IsNullOrWhiteSpace( this.Name.Text ) ? "Bez nazwy" : this.Name.Text);
			this.editedProfile.AccountPassword = this.Password.Password;
		}

		private void saveDataForStatuses()
		{
			this.editedProfile.StatusDescriptionAfterLogin = this.InitialStatus.Text;
			this.editedProfile.SetLastSettedStatusAfterLogin = !this.setStatusStartupToogle.IsChecked.GetValueOrDefault();
			this.editedProfile.StatusAfterLogin = this.getStatusChoosedInInitialStatus();
			this.editedProfile.SetStatusOnTombstone = this.setStatusAfterTombstoneToogle.IsChecked.GetValueOrDefault();
			this.editedProfile.DescriptionStatusOnTombstone = this.statusForTombstoning.Text;
			this.editedProfile.ForFriendsOnly = !this.showStatusToAll.IsChecked.GetValueOrDefault();
		}
		private void saveDataForSettings() { }

		private void saveDataForCurrentPage( int index )
		{
			switch (index)
			{
				case ( int ) Pages.Information: this.saveDataForInformation(); break;
				case ( int ) Pages.Settings: this.saveDataForSettings(); break;
				case ( int ) Pages.Statuses: this.saveDataForStatuses(); break;
			}
		}


		private void controlValueChanged( object sender, RoutedEventArgs e )
		{
			this.controlValueChangedWork();
		}

		private void controlValueChangedWork()
		{
			try
			{
				var currentPage = this.pivot.SelectedIndex;
				this.refreshControlsStatesForPage( currentPage );
				this.saveDataForCurrentPage( currentPage );
			}
			catch (Exception exc)
			{
				MessageBox.Show( Strings.Błąd_w_edycji_profilu + exc.Message );
			}
		}

		private void onLoad()
		{
			for (int i = 0, max = this.pivot.Items.Count; i < max; i++)
			{
				this.readDataForPage( i );
				this.refreshControlsStatesForPage( i );
			}
		}

		private void Name_TextChanged( object sender, TextChangedEventArgs e )
		{
			this.controlValueChangedWork();
		}

		private void Name_TextChanged_1( object sender, TextChangedEventArgs e )
		{

		}

		private void Password_PasswordChanged( object sender, RoutedEventArgs e )
		{
			this.controlValueChangedWork();

		}

		private void InitialStatus_KeyDown( object sender, KeyEventArgs e )
		{
			App.RefreshScreensaverTimer_();
		}

		private void statusForTombstoning_KeyDown( object sender, KeyEventArgs e )
		{
			App.RefreshScreensaverTimer_();
		}

	}
}