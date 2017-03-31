using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using OrzechowskiKamil.OrzechConnect.Foreground.ViewModels;
using OrzechowskiKamil.OrzechConnect.Lib;
using OrzechowskiKamil.OrzechConnect.Lib.Processess;
using OrzechowskiKamil.OrzechConnect.Background;
using System.Windows.Input;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using OrzechowskiKamil.OrzechConnect.Foreground.Helpers;

namespace OrzechowskiKamil.OrzechConnect.Foreground
{
	public partial class App : Application
	{
		/// <summary>
		/// Provides easy access to the root frame of the Phone Application.
		/// </summary>
		/// <returns>The root frame of the Phone Application.</returns>
		public PhoneApplicationFrame RootFrame { get; private set; }

		// ekran wylacza sie po 60 sekundach.
		// TODO dorob ustawienie w settings.
		private int screensaverDelay
		{
			get { return 60; }
		}
		private Timer screensaverTimer;
		private void StopTimer()
		{
			if (this.screensaverTimer != null)
			{
				this.screensaverTimer.Stop();
			}
		}
		private void onShowScreensaver()
		{
			if (this.RootFrame != null)
			{
				if (this.RootFrame.Content is Page)
				{
					(this.RootFrame.Content as Page).NavigationService.Navigate(
						GetPathToPage( "Screensaver.xaml" ) );
				}
			}
		}

		/// <summary>
		/// Metoda pozwala zresetowac licznik liczacy do wygaszacza ekranu. 
		/// Nalezy miec na uwadze ze apka sama z siebie reaguje na prawie wszystkie gesty, a ręcznie
		/// to uruchamiac nalezy tylko w textboxach i passwordboxach, i jest do tego statyczna metoda.
		/// </summary>
		public void RefreshScreensaverTimer()
		{
			if (this.ScreensaverModeEnabled)
			{
				this.StopTimer();
				if (this.screensaverTimer == null)
				{
					this.screensaverTimer = new Timer( this.screensaverDelay, this.onShowScreensaver,
						false, true ).Start();
				}
				else
				{
					this.screensaverTimer.StartAgain();
				}
			}
		}

		/// <summary>
		/// Constructor for the Application object.
		/// </summary>
		public App()
		{
			this.createStaticResources();
			// Global handler for uncaught exceptions. 
			UnhandledException += Application_UnhandledException;

			// Standard Silverlight initialization
			InitializeComponent();

			// Phone-specific initialization
			InitializePhoneApplication();

			// Show graphics profiling information while debugging.
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// Display the current frame rate counters.
				Application.Current.Host.Settings.EnableFrameRateCounter = false;

			}


			this.RootFrame.Obscured += new EventHandler<ObscuredEventArgs>( RootFrame_Obscured );

			this.RootFrame.Unobscured += new EventHandler( RootFrame_Unobscured );



		}



		private void createStaticResources()
		{
			var currRes = Application.Current.Resources;
			AppGlobalData.Current.IsForegroundProcess = true;
			currRes.Add( "MessageAnnoucementImageSourcePath",
						 ImageHelper.GetImageNameForCurrentComposition( "Other", "mail.png", ColorScheme.OpacityMask ) );
			var howMuchDarker = Config.HowMuchDarkerAccentIsDarker;
			var multipier = (AppGlobalData.Current.IsBlackThemeSelected) ? 1 : -1;
			var accentColor = ( Color ) currRes["PhoneAccentColor"];
			var lighterColor = ColorHelper.subFromLuminance( accentColor, howMuchDarker * -1 );
			var darkerColor = ColorHelper.subFromLuminance( accentColor, howMuchDarker );
			var relativeColor = ColorHelper.addToLuminance( accentColor,
				howMuchDarker * (multipier) );
			var oppositeColor = ColorHelper.addToLuminance( accentColor, howMuchDarker * multipier * -1 );
			currRes.Add( "PhoneDarkAccentColor", darkerColor );
			currRes.Add( "PhoneDarkAccentBrush", new SolidColorBrush( darkerColor ) );
			currRes.Add( "PhoneLightAccentColor", lighterColor );
			currRes.Add( "PhoneLightAccentBrush", new SolidColorBrush( lighterColor ) );
			// ten kolor jest ciemniejszy jezeli jest biale tło a jasniejszy jesli czarne
			currRes.Add( "PhoneRelativeAccentBrush", new SolidColorBrush( relativeColor ) );
			currRes.Add( "PhoneRelativeAccentColor", relativeColor );
			currRes.Add( "PhoneRelativeOppositeAccentColor", oppositeColor );
			currRes.Add( "PhoneRelativeOppositeAccentBrush", new SolidColorBrush( oppositeColor ) );

			// szary ale nie tak bardzo niewidoczny jakten srodkowy
			currRes.Add( "PhoneGrayBrush", new SolidColorBrush( ColorHelper.addToLuminance( Colors.Gray, 50 * multipier ) ) );

		}

		void RootFrame_Unobscured( object sender, EventArgs e )
		{

		}

		void RootFrame_Obscured( object sender, ObscuredEventArgs e )
		{

		}

		private bool DisableUserLock
		{
			get { return AppGlobalData.Current.AdditionalSettings.DisableUserIdleLock; }
			set { AppGlobalData.Current.AdditionalSettings.DisableUserIdleLock = value; }
		}

		private bool ScreensaverModeEnabled
		{
			get { return this.DisableUserLock == true; }
		}
		/// <summary>
		/// uruchomienie tego pozwala "nastawic" to ustawienie na kolejne uruchomienie aplikacji.
		/// Value idzie z ustawien. bezposrednio nie mozna poniewaz appGLobalData.current nie istnieje wtedy jeszcze.
		/// </summary>
		public void SetDisableUserLockBasedOnValueInSettings()
		{
			if (this.DisableUserLock == true)
			{
				PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
			}
			else
			{
				PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
			}
		}

		// Code to execute when the application is launching (eg, from Start)
		// This code will not execute when the application is reactivated
		private void Application_Launching( object sender, LaunchingEventArgs e )
		{
			this.SetDisableUserLockBasedOnValueInSettings();
			this.StartScreensaverTimer();
		}

		private void StartScreensaverTimer()
		{
			if (this.ScreensaverModeEnabled)
			{
				Touch.FrameReported += new TouchFrameEventHandler( this.Touch_FrameReported );
				this.RefreshScreensaverTimer();
			}
		}

		/// <summary>
		/// uruchamia sie przy kazdym dotyku ekranu poza wprowadzaniem tekstu na klawiaturze ekranowej.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Touch_FrameReported( object sender, TouchFrameEventArgs e )
		{
			this.RefreshScreensaverTimer();
		}



		// Code to execute when the application is activated (brought to foreground)
		// This code will not execute when the application is first launched
		private void Application_Activated( object sender, ActivatedEventArgs e )
		{
			// i tak socket sie rozłączył wiec lepiej posprzatac i anulowac wyjątki
			AppGlobalData.Current.OnUntombstone();
		}

		// Code to execute when the application is deactivated (sent to background)
		// This code will not execute when the application is closing
		private void Application_Deactivated( object sender, DeactivatedEventArgs e )
		{
			AppGlobalData.Current.OnTombstone();
		}

		// Code to execute when the application is closing (eg, user hit Back)
		// This code will not execute when the application is deactivated
		private void Application_Closing( object sender, ClosingEventArgs e )
		{
			ScheduledAgentHelper.SetAgentLikeInSettings();
			AppGlobalData.Current.SaveData();
		}

		// Code to execute if a navigation fails
		private void RootFrame_NavigationFailed( object sender, NavigationFailedEventArgs e )
		{
			// Dla łatwiejszego debugowania.
			string errorMessage = e.Exception.Message;
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// A navigation has failed; break into the debugger
				System.Diagnostics.Debugger.Break();
			}
		}

		public static Uri GetPathToPage( string pageName )
		{
			return new Uri( String.Format( "/Pages/{0}", pageName ), UriKind.Relative );
		}

		// Code to execute on Unhandled Exceptions
		private void Application_UnhandledException( object sender, ApplicationUnhandledExceptionEventArgs e )
		{
			// Dla łatwiejszego debugowania.
			string errorMessage = e.ExceptionObject.Message;
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// An unhandled exception has occurred; break into the debugger
				System.Diagnostics.Debugger.Break();
			}
		}

		#region Phone application initialization

		// Avoid double-initialization
		private bool phoneApplicationInitialized = false;

		// Do not add any additional code to this method
		private void InitializePhoneApplication()
		{
			if (phoneApplicationInitialized)
				return;

			// Create the frame but don't set it as RootVisual yet; this allows the splash
			// screen to remain active until the application is ready to render.
			RootFrame = new PhoneApplicationFrame();
			RootFrame.Navigated += CompleteInitializePhoneApplication;

			// Handle navigation failures
			RootFrame.NavigationFailed += RootFrame_NavigationFailed;

			// Ensure we don't initialize again
			phoneApplicationInitialized = true;
		}

		// Do not add any additional code to this method
		private void CompleteInitializePhoneApplication( object sender, NavigationEventArgs e )
		{
			// Set the root visual to allow the application to render
			if (RootVisual != RootFrame)
				RootVisual = RootFrame;

			// Remove this handler since it is no longer needed
			RootFrame.Navigated -= CompleteInitializePhoneApplication;
		}

		#endregion

		private void sendMessageTextBox_GotFocus( object sender, RoutedEventArgs e )
		{
			AppGlobalData.Current.OnSendMessageTextBoxGotFocus( sender );
		}

		private void sendMessageTextBox_LostFocus( object sender, RoutedEventArgs e )
		{
			AppGlobalData.Current.OnSendMessageTextBoxLostFocus();
		}

		public static void onSendMessageByTextBox( object sender )
		{
			var msgBox = ( TextBox ) sender;
			var number = ( int ) msgBox.Tag;
			var text = msgBox.Text;
			if (String.IsNullOrWhiteSpace( text ) == false)
			{
				AppGlobalData.Current.Engine.SendMessage( new SendMessageParameters
				{
					Message = text,
					ReceiverNumber = number

				} );
				msgBox.Text = "";
				AppGlobalData.Current.CallMessageSended( number, text );
			}
		}

		/// <summary>
		/// Metoda pozwala zresetowac licznik wygaszacza ekranu w miejscach gdzie nie jest możliwe
		/// wykrycie dotyku automatycznie, czyli w textboxach wszelkiej maści.
		/// </summary>
		public static void RefreshScreensaverTimer_()
		{
			(( App ) App.Current).RefreshScreensaverTimer();
		}

		private void sendMessageTextBox_KeyUp( object sender, System.Windows.Input.KeyEventArgs e )
		{
			// wylacza wygaszacz ekranu
			RefreshScreensaverTimer_();
			if (e.Key == Key.Enter)
			{
				onSendMessageByTextBox( sender );
			}
		}



	}


}