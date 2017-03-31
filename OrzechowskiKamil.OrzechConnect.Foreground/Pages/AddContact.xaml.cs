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

namespace OrzechowskiKamil.OrzechConnect.Foreground.ViewModels
{

	public partial class AddContact : PhoneApplicationPage
	{
		public AddContact()
		{
			InitializeComponent();
		}

		private void Button_Click( object sender, RoutedEventArgs e )
		{
			var name = this.contactName.Text;
			var ggNUmber = this.ggNumber.Text;
			var number = 0;
			try
			{
				try
				{
					int.TryParse( ggNUmber, out number );
				}
				catch (Exception)
				{
					throw new Exception( Strings.NumerGGMozeBycZapisanyTylkoZaPomocaCyfrException );
				}
				AppGlobalData.Current.AddContact( new AddContactParams { Name = name, Number = number }, ( ver ) =>
					{
						AppGlobalData.Current.ComingFromAddContact = true;
						NavigationService.GoBack();
					}, () =>
					{
						MessageBox.Show( "Błąd przy wysyłaniu kontaktu na serwer. Spróbuj ponownie później" );
					} );
			}
			catch (Exception eee)
			{
				MessageBox.Show( eee.Message );
			}

		}

		private void ggNumber_KeyDown( object sender, KeyEventArgs e )
		{
			App.RefreshScreensaverTimer_();

		}

		private void contactName_KeyDown( object sender, KeyEventArgs e )
		{
			App.RefreshScreensaverTimer_();

		}
	}
}