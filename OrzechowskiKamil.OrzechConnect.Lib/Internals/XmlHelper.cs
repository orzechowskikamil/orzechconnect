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
using System.Xml.Linq;

namespace OrzechowskiKamil.OrzechConnect.Lib.Internals
{

	public class XmlHelper
	{
		public static void readBool( XElement element, string elementName, out bool result )
		{
			result = false;
			var elem = element.Element( elementName );
			if (elem != null)
			{
				bool.TryParse( elem.Value, out result );
			}
		}

		public static void readInt( XElement element, string elementName, out int result )
		{
			result = 0;
			var elem = element.Element( elementName );
			if (elem != null)
			{
				int.TryParse( elem.Value, out result );
			}
		}

		public static void readGuid( XElement element, string elementName, out Guid result )
		{
			result = Guid.Empty;
			var elem = element.Element( elementName );
			if (elem != null)
			{
				Guid.TryParse( elem.Value, out result );
			}
		}

		public static void readString( XElement element, string elementName, out string result )
		{
			result = null;
			var elem = element.Element( elementName );
			if (elem != null)
			{
				result = elem.Value;
			}
		}
	}
}
