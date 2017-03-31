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
using System.Windows.Threading;

namespace OrzechowskiKamil.OrzechConnect.Lib.Internals
{
	public class Timer
	{
		private int interval;
		private Action onTick;
		private DispatcherTimer dt;
		private bool useMiliseconds;

		/// <summary>
		/// Konstruktor, pamietaj by odpalic start po utworzeniu timera
		/// </summary>
		/// <param name="interval">czas w (!)sekundach</param>
		/// <param name="onTick">callback</param>
		public Timer( int interval, Action onTick )
			: this( interval, onTick, false )
		{

		}

		private bool stopAfterFirstTick;

		/// <summary>
		/// Konstruktor, pamietaj by odpalic start po utworzeniu timera
		/// </summary>
		/// <param name="interval">czas</param>
		/// <param name="onTick">callback</param>
		/// <param name="useMiliseconds">Jezeli true to uzywa milisekund jezeli false to sekund</param>
		public Timer( int interval, Action onTick, bool useMiliseconds )
		{
			this.interval = interval;
			this.onTick = onTick;
			this.useMiliseconds = useMiliseconds;
		}

		public Timer( int interval, Action onTick, bool useMiliseconds, bool stopAfterFirstTick ) :
			this( interval, onTick, useMiliseconds )
		{
			this.stopAfterFirstTick = stopAfterFirstTick;
		}

		public Timer Start()
		{
			var span = (this.useMiliseconds) ?
				TimeSpan.FromMilliseconds( this.interval ) : TimeSpan.FromSeconds( this.interval );
			Deployment.Current.Dispatcher.BeginInvoke( () =>
			{
				dt = new DispatcherTimer();
				dt.Interval = span;
				dt.Tick += new EventHandler( ( obj, args ) =>
				{
					if (this.stopAfterFirstTick == true)
					{
						this.Stop();
					}
					this.onTick();

				} );
				dt.Start();
			} );
			return this;
		}

		public void Stop()
		{
			Deployment.Current.Dispatcher.BeginInvoke( () =>
				{
					if (dt != null)
					{
						dt.Stop();
					}
				} );
		}

		public void StartAgain()
		{
			Deployment.Current.Dispatcher.BeginInvoke( () =>
			{
				if (dt != null)
				{
					dt.Start();
				}
			} );
		}

	}
}
