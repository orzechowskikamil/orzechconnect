using System.Windows;
using Microsoft.Phone.Scheduler;
using OrzechowskiKamil.OrzechConnect.Lib;
using Microsoft.Phone.Shell;
#if DEBUG
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
#endif

namespace OrzechowskiKamil.OrzechConnect.Background
{
	public class ScheduledAgent : ScheduledTaskAgent, IToastCaller
	{
#if DEBUG
		private DiagnosticHelper helper = new DiagnosticHelper( true, "ScheduledAgent", "!!!!!WAŻNE!!!!" );
#endif
		private static volatile bool _classInitialized;

		/// <remarks>
		/// ScheduledAgent constructor, initializes the UnhandledException handler
		/// </remarks>
		public ScheduledAgent()
		{
			if (!_classInitialized)
			{
				_classInitialized = true;
				// Subscribe to the managed exception handler
				Deployment.Current.Dispatcher.BeginInvoke( delegate
				{
					Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
				} );
			}
		}

		/// Code to execute on Unhandled Exceptions
		private void ScheduledAgent_UnhandledException( object sender, ApplicationUnhandledExceptionEventArgs e )
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// An unhandled exception has occurred; break into the debugger
				System.Diagnostics.Debugger.Break();
			}
		}

		/// <summary>
		/// Agent that runs a scheduled task
		/// </summary>
		/// <param name="task">
		/// The invoked task
		/// </param>
		/// <remarks>
		/// This method is called when a periodic or resource intensive task is invoked
		/// </remarks>
		protected override void OnInvoke( ScheduledTask task )
		{
#if DEBUG
		//	ShowToast( "Title", "Wlasnie rozpoczalem dzialanie" );
#endif
			//TODO: Add code to perform your task in background
			var worker = new BackgroundWorker() { ToastCaller = this };
			worker.GetResult( ( howmuch ) =>
			{
				if (howmuch > 0)
				{
					var fmt = "{0} nowa wiadomość.";
					if (howmuch > 1 && howmuch <= 4)
					{
						fmt = "{0} nowe wiadomości.";
					}
					else if (howmuch > 4)
					{
						fmt = "{0} nowych wiadomości.";
					}
					ShowToast( "Orzech Connect", Fmt.Do( fmt, howmuch ) );
				}
#if DEBUG
				this.helper.Diagnose( () => this.helper.DiagnosticMessage( "Agent wlasnie zakonczył prace. odebral " + howmuch + " wiadomości." ) );
		//		ShowToast( "Tile", "Wlasnie aktualizuje tile i koniec" );
			
#endif
				LiveTile.UpdateTile( "Orzech connect", howmuch, null );
				NotifyComplete();
			}, () =>
			{ NotifyComplete(); } );

		}

		public void ShowToast( string title, string message )
		{
			ShowToastStatic( title, message );
		}

		private static void ShowToastStatic( string title, string message )
		{
			ShellToast toast = new ShellToast();
			toast.Title = title;
			toast.Content = message;
			toast.Show();
		}
	}
}