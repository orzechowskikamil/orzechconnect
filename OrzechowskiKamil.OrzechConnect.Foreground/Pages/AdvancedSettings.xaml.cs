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
using OrzechowskiKamil.OrzechConnect.Foreground.Helpers;
using OrzechowskiKamil.OrzechConnect.Lib.DataStorage;
using OrzechowskiKamil.OrzechConnect.Background;

namespace OrzechowskiKamil.OrzechConnect.Foreground.ViewModels
{
	public partial class AdvancedSettings : PhoneApplicationPage
	{
		public AdvancedSettings()
		{
			InitializeComponent();
			this.DataContext = this;
			this.RefreshDataCounters();
			this.setControlsValues();
			this.manageControlsStates();
		}

		/// <summary>
		/// Funkcja zamieniająca kolejne potęgi 1024 na jednostki
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		private string convertToBestUnit( int amount )
		{
			var units = new string[] { "B", "KB", "MB", "GB", "TB" };
			var treshold = 1024;

			for (int i = 0, max = units.Length; i < max; i++)
			{
				if (amount < treshold)
				{
					var amountInUnit = amount / (treshold / 1024);
					return String.Format( "{0} {1}", amountInUnit, units[i] );
				}
				else
				{
					treshold *= 1024;
				}
			}
			return null;
		}
		public MainAppSettingsPack settings { get { return AppGlobalData.Current.AdditionalSettings; } }
		public string DataSended
		{
			set
			{
				this.DataSendedField.Text = value;
			}
		}
		public string DataReceived
		{
			set
			{
				this.DataReceivedField.Text = value;
			}
		}
		public string LastCounterReset
		{
			set
			{
				this.LastCounterResetField.Text = value;
			}
		}
		public string DataSendedByAgent
		{
			set
			{
				this.DataSendedByAgentField.Text = value;
			}
		}
		public string DataReceivedByAgent
		{
			set
			{
				this.DataReceivedByAgentField.Text = value;
			}
		}

		public void RefreshDataCounters()
		{
			var appGlobalData = AppGlobalData.Current;
			this.DataSended = this.convertToBestUnit( appGlobalData.BytesSendCounter );
			this.DataReceived = this.convertToBestUnit( appGlobalData.BytesReceivedCounter );
			this.DataSendedByAgent = this.convertToBestUnit( appGlobalData.BytesSendedByAgent );
			this.DataReceivedByAgent = this.convertToBestUnit( appGlobalData.BytesReceivedByAgent );
			//	var data = appGlobalData.LastResetOfBytesCounter;
			//if (data.Year < 1900)
			//{
			//    this.LastCounterReset = "nigdy";
			//}
			//else
			//{
			//    this.LastCounterReset = data.ToString();
			//}
		}


		private void DataCountersPivotItemGotFocus()
		{
			this.RefreshDataCounters();
		}

		private void Pivot_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			switch (this.AdvSettingsPivot.SelectedIndex)
			{
				case 1: this.DataCountersPivotItemGotFocus(); break;
			}
		}

		private void Button_Click( object sender, RoutedEventArgs e )
		{
			var current = AppGlobalData.Current;
			current.BytesReceivedCounter = 0;
			current.BytesSendCounter = 0;
			//current.BytesReceivedByAgent = 0;
			//current.BytesSendedByAgent = 0;
			//	current.LastResetOfBytesCounter = DateTime.Now;
			this.RefreshDataCounters();
		}

		private void setControlsValues()
		{
			this.soundChangeStatus.IsChecked = this.settings.SoundForStatusesEnabled;
			this.soundsDuringConversationEnabled.IsChecked = this.settings.SoundsDuringConversationEnabled;
			this.soundsForMessagesEnabled.IsChecked = this.settings.SoundsForMessagesEnabled;

			this.soundForStatusesEnabled.IsChecked = this.settings.AnnoucementForChangeStatus;
			this.annoucementChat.IsChecked = this.settings.AnnoucementDuringChat;
			this.annoucementNewConversation.IsChecked = this.settings.AnnoucementForNewMessage;
			this.vibrationNewConversation.IsChecked = this.settings.VibrateForMessage;
			this.vibrationChat.IsChecked = this.settings.VibrateForMessageInChatWindow;
			this.vibrationSending.IsChecked = this.settings.VibrateForSendingMessage;
			this.vibrationChangeStatus.IsChecked = this.settings.VibrateForSomeoneStatusChange;

			var tagWanted = AppGlobalData.Current.AdditionalSettings.AgentHowMuchStartupsToBypass;
			this.AgentEnabledInNight.IsChecked = this.settings.AgentEnabledInNight;
			try
			{
				foreach (var item in this.items)
				{
					this.addItemToPicker( ( int ) item[0], ( string ) item[1] );
				}
				this.howOftenPicker.SelectedIndex = this.getBypassValueForTag( tagWanted );
			}
			catch (Exception e)
			{
				var chuj = "dupa";
			}




		}
		private void manageControlsStates()
		{
			ToggleSwitchHelper.SetEnabledAndDisabledValues( new ToggleSwitch[]{
				this.soundChangeStatus,
				this.soundsDuringConversationEnabled,
				this.soundsForMessagesEnabled
			}, "Odtwarzaj", "Wycisz" );
			ToggleSwitchHelper.SetEnabledAndDisabledValues( new ToggleSwitch[]
			{
				
				this.vibrationNewConversation,
				this.vibrationChat,this.
				vibrationSending,
				this.vibrationChangeStatus
			}, "Włącz", "Wyłącz" );
			ToggleSwitchHelper.SetEnabledAndDisabledValues( new ToggleSwitch[]
			{
				
					this.annoucementNewConversation,
				this.annoucementChat,
				this.soundForStatusesEnabled,
			}, "Pokazuj", "Nie pokazuj" );

			if (this.vibrationChat.IsChecked == false)
			{
				this.vibrationSending.IsChecked = false;
				this.vibrationSending.IsEnabled = false;
			}
			else
			{
				this.vibrationSending.IsEnabled = true;
			}
			//var forEnabledPanel = Visibility.Visible;
			var forDisabledPanel = Visibility.Collapsed;
			if (this.settings.AgentDisabled)
			{
				//forEnabledPanel = Visibility.Collapsed;
				forDisabledPanel = Visibility.Visible;
			}

			//this.howOftenEnabled.Visibility = forEnabledPanel;
			this.howOftenDisabled.Visibility = forDisabledPanel;
			ToggleSwitchHelper.SetEnabledAndDisabledValues( this.AgentEnabledInNight,
			 "Włączony", "Wyłączony" );
			var isenabled = this.AgentEnabledInNight.IsChecked.GetValueOrDefault();
			this.agentDisabledDescription.Visibility = (isenabled) ? Visibility.Collapsed : Visibility.Visible;

		}

		protected override void OnNavigatingFrom( System.Windows.Navigation.NavigatingCancelEventArgs e )
		{
			// odswiezamy helpera.
			//	ScheduledAgent.SetAgentLikeInSettings();
		}

		private void getControlsValues()
		{
			this.settings.SoundsDuringConversationEnabled =
				ToggleSwitchHelper.IsChecked( this.soundsDuringConversationEnabled );
			this.settings.SoundForStatusesEnabled =
				ToggleSwitchHelper.IsChecked( this.soundForStatusesEnabled );
			this.settings.SoundsForMessagesEnabled =
				ToggleSwitchHelper.IsChecked( this.soundsForMessagesEnabled );
			this.settings.AnnoucementForChangeStatus =
				ToggleSwitchHelper.IsChecked( this.soundForStatusesEnabled );
			this.settings.AnnoucementForNewMessage =
				ToggleSwitchHelper.IsChecked( this.annoucementNewConversation );
			this.settings.AnnoucementDuringChat =
				ToggleSwitchHelper.IsChecked( this.annoucementChat );
			///sfhetwhbethtweh/h/wr/hrw/hgr/w
			this.settings.VibrateForMessage =
				ToggleSwitchHelper.IsChecked( this.vibrationNewConversation );
			this.settings.VibrateForMessageInChatWindow =
				ToggleSwitchHelper.IsChecked( this.vibrationChat );
			this.settings.VibrateForSendingMessage =
				ToggleSwitchHelper.IsChecked( this.vibrationSending );
			this.settings.VibrateForSomeoneStatusChange =
				ToggleSwitchHelper.IsChecked( this.vibrationChangeStatus );
			var itemSelected = this.howOftenPicker.SelectedIndex;
			this.settings.AgentHowMuchStartupsToBypass = this.getBypassValueForIndex( itemSelected );
			this.settings.AgentEnabledInNight = ToggleSwitchHelper.IsChecked( this.AgentEnabledInNight );
		}

		private int getBypassValueForIndex( int index )
		{
			return ( int ) items[index][0] - 1;
		}

		private int getBypassValueForTag( int tag )
		{
			tag++;
			var i = 0;
			foreach (var item in this.items)
			{
				if (( int ) item[0] == tag)
				{
					return i;
				}
				i++;
			}
			return 0;
		}

		private object[][] items
		{
			get
			{
				return new object[][]{
					new object[]{1, "Co pół godziny"},
					new object[]{2, "Co godzinę"},
					new object[]{4, "Co dwie godziny"},
					new object[]{8, "Co cztery godziny"},
					new object[]{12, "Raz dziennie"}
				};
			}
		}



		private void addItemToPicker( int tag, string text )
		{
			this.howOftenPicker.Items.Add( new ListPickerItem { Content = text } );
		}



		private void controlValueChanged( object sender, RoutedEventArgs e )
		{
			this.manageControlsStates();
			this.getControlsValues();
		}

		private void ListBox_Tap( object sender, System.Windows.Input.GestureEventArgs e )
		{
			this.controlValueChanged( null, null );
		}

		private void howOftenPicker_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			this.controlValueChanged( null, null );
		}

		private void AgentEnabledInNight_Tap( object sender, System.Windows.Input.GestureEventArgs e )
		{
			this.controlValueChanged( null, null );
		}
	}
}