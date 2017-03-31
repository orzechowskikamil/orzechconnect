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
using OrzechowskiKamil.OrzechConnect.Lib;
using Microsoft.Phone.Scheduler;

namespace OrzechowskiKamil.OrzechConnect.Foreground.Helpers
{
	public class ScheduledAgentHelper
	{
		private const string AgentName = "Orzech Connect Agent";

		private static PeriodicTask GetTaskObj()
		{
			var periodicTask = new PeriodicTask( AgentName );
			periodicTask.Description = "Pozwala pobierać w tle wiadomości z domyślnego profilu GG, i informuje cię o nich, poprzez powiadomienie w górnym rogu ekranu oraz na kafelku.";
			periodicTask.ExpirationTime = System.DateTime.Now.AddDays( 14 );
			return periodicTask;


		}

		public static void SetAgentLikeInSettings()
		{
			try
			{
				RemoveAgent();
				if (AppGlobalData.Current.AdditionalSettings.AgentDisabled == false)
				{
					AddAgent();
#if DEBUG
		//			
		//		ScheduledActionService.LaunchForTest( AgentName, TimeSpan.FromMilliseconds( 1 ) );
#endif

				}
			}
			catch (Exception exception) { }

		}

		/// <summary>
		/// 
		/// </summary>
		/// <exception cref="InvalidOperationException">Wyjatek jezeli zabraklo miejsca dla naszego agenta
		/// </exception>
		private static void AddAgent()
		{
			var periodicTask = GetTaskObj();
			ScheduledActionService.Add( periodicTask );
		}

		private static void RemoveAgent()
		{
			try
			{
				// If the agent is already registered with the system,
				if (ScheduledActionService.Find( AgentName ) != null)
				{
					ScheduledActionService.Remove( AgentName );
				}
			}
			catch (Exception exception)
			{
				var chuj = "dupa";
			}
		}
	}
}
