
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
using Microsoft.Phone.Controls;

namespace OrzechowskiKamil.OrzechConnect.Foreground.Helpers
{
	/// <summary>
	/// Czesto wykonywane czynnosci przy toggleswitchu
	/// </summary>
	public class ToggleSwitchHelper
	{
		/// <summary>
		/// Ustawia widocznosc danego destination bazujac na IsEnabled elementu source
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="reverseVisibility">jezeli true to zamiast collapsed pojavi sie visible i odwrotnie</param>
		public static void SetVisibilityByToogleValue( ToggleSwitch source, FrameworkElement destination,
			bool reverseVisibility )
		{
			var isenabled = source.IsChecked.GetValueOrDefault();
			if (reverseVisibility) { isenabled = !isenabled; }
			SetVisibilityByBoolValue( isenabled, destination );
		}

		/// <summary>
		/// Ustawia widocznosc 1go framework element na visible jesli source jest enabled, jesli nie to 
		/// ten drugi a pierwszy jest chowany.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="visibleOnEnabled"></param>
		/// <param name="visibleOnDisabled"></param>
		public static void SetVisibilityByToogleValue( ToggleSwitch source, FrameworkElement visibleOnEnabled,
			FrameworkElement visibleOnDisabled )
		{
			SetVisibilityByToogleValue( source, visibleOnEnabled, false );
			SetVisibilityByToogleValue( source, visibleOnDisabled, true );
		}

		public static void SetEnabledAndDisabledValues( ToggleSwitch[] arrayOfSwitches, string YesValue,
			string noValue )
		{
			foreach (var togSwitch in arrayOfSwitches)
			{
				if (togSwitch.IsChecked == true)
				{
					togSwitch.Content = YesValue;
				}
				else
				{
					togSwitch.Content = noValue;
				}
			}
		}

		public static void SetEnabledAndDisabledValues( ToggleSwitch switchControl, string YesValue, string novalue )
		{
			SetEnabledAndDisabledValues( new ToggleSwitch[] { switchControl }, YesValue, novalue );
		}

		public static void SetVisibilityByBoolValue( bool value, FrameworkElement element )
		{
			element.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed;
		}

		public static bool IsChecked( ToggleSwitch swit )
		{
			return swit.IsChecked.GetValueOrDefault();
		}
	}
}
