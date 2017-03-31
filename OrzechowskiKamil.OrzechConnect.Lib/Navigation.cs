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

namespace OrzechowskiKamil.OrzechConnect.Lib
{
	abstract public class AbstractNavigation
	{
		private static AbstractNavigation currentNavigation;
		public static AbstractNavigation CurrentNavigation
		{
			get
			{
				if (AbstractNavigation.currentNavigation == null)
				{
					AbstractNavigation.currentNavigation = AbstractNavigation.GetCurrentChoosedNavigation();
				}
				return AbstractNavigation.currentNavigation;
			}
			set
			{
				AbstractNavigation.currentNavigation = value;
			}
		}
		public static AbstractNavigation GetCurrentChoosedNavigation()
		{
			return new DefaultNavigation();
		}
	
		public void GoToLoginPage()
		{ 
		}
		public void NavigateTo()
		{
		}
	}

	public class DefaultNavigation : AbstractNavigation
	{
	}
}
