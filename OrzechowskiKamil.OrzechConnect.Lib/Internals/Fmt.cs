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

/// <summary>
/// Alias na string.format (krotsza nazwa).
/// </summary>
public class Fmt
{
	public static string Do( string format, params object[] args )
	{
		return String.Format( format, args );
	}

	public string Do( string format )
	{
		return format;
	}
}