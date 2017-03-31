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

namespace OrzechowskiKamil.OrzechConnect.Foreground.Pages
{
	public partial class Screensaver : PhoneApplicationPage
	{
		private bool isNotFirstTime;
		public Screensaver()
		{
			InitializeComponent();

		}

		protected override void OnBackKeyPress( System.ComponentModel.CancelEventArgs e )
		{
			e.Cancel = true;
			base.OnBackKeyPress( e );

		}

		private void Pivot_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if (isNotFirstTime == true)
			{
				(( App ) App.Current).RefreshScreensaverTimer();
				if (this.NavigationService.CanGoBack == true)
				{
					this.NavigationService.GoBack();
				}
			}
			isNotFirstTime = true;

		}
	}
}