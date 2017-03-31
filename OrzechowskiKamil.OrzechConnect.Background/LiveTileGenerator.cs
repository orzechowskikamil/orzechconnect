using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;

namespace OrzechowskiKamil.OrzechConnect.Background
{
	public class TileGenerator
	{
		public delegate void OnFinish( Uri imageUri );
		/// <summary>
		/// Generates a custom layout tile from a set of parameters.
		/// </summary>
		public static void GenerateTile( string timeOfDay, string temperature, Uri cloudImagePath, 
			string tileTitle, OnFinish onFinish)
		{
			// Setup the font style for our tile.
			var fontFamily = new FontFamily( "Segoe WP" );
			var fontForeground = new SolidColorBrush( Colors.White );
			var tileSize = new Size( 173, 173 );

			// Create a background rectagle for a custom colour background.
			var backgroundRectangle = new Rectangle();
			backgroundRectangle.Width = tileSize.Width;
			backgroundRectangle.Height = tileSize.Height;
			backgroundRectangle.Fill = new SolidColorBrush( Colors.Blue );

			// Load our 'cloud' image.
			var source = new BitmapImage( cloudImagePath );
			source.CreateOptions = BitmapCreateOptions.None;
			source.ImageOpened += ( sender, e ) => // This is important. The image can't be rendered before it's loaded.
			{
				// Create our image as a control, so it can be rendered to the WriteableBitmap.
				var cloudImage = new Image();
				cloudImage.Source = source;
				cloudImage.Width = 100;
				cloudImage.Height = 64;

				// TextBlock for the time of the day.
				TextBlock timeOfDayTextBlock = new TextBlock();
				timeOfDayTextBlock.Text = timeOfDay;
				timeOfDayTextBlock.FontSize = 20;
				timeOfDayTextBlock.Foreground = fontForeground;
				timeOfDayTextBlock.FontFamily = fontFamily;

				// Temperature TextBlock.
				TextBlock temperatureTextBlock = new TextBlock();
				temperatureTextBlock.Text = temperature + '°';
				temperatureTextBlock.FontSize = 30;
				temperatureTextBlock.Foreground = fontForeground;
				temperatureTextBlock.FontFamily = fontFamily;

				// Define the filename for our tile. Take note that a tile image *must* be saved in /Shared/ShellContent
				// or otherwise it won't display.
				var tileImage = string.Format( "/Shared/ShellContent/{0}.jpg", timeOfDay );

				// Define the path to the isolatedstorage, so we can load our generated tile from there.
				var isoStoreTileImage = string.Format( "isostore:{0}", tileImage );

				// Open the ISF store, 
				using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
				{
					// Create our bitmap, in our selected dimension.
					var bitmap = new WriteableBitmap( ( int ) tileSize.Width, ( int ) tileSize.Height );

					// Render our background. Remember the renders are in the same order as XAML,
					// so whatever is rendered first, is rendered behind the next element.
					bitmap.Render( backgroundRectangle, new TranslateTransform() );

					// Render our cloud image
					bitmap.Render( cloudImage, new TranslateTransform()
					{
						X = 8, // Left margin offset.
						Y = 54 // Top margin offset.
					} );

					// Render the temperature text.
					bitmap.Render( temperatureTextBlock, new TranslateTransform()
					{
						X = 124,
						Y = 63
					} );

					// Render the time of the day text.
					bitmap.Render( timeOfDayTextBlock, new TranslateTransform()
					{
						X = 12,
						Y = 6
					} );

					// Create a stream to store our file in.
					var stream = store.CreateFile( tileImage );

					// Invalidate the bitmap to make it actually render.
					bitmap.Invalidate();

					// Save it to our stream.
					bitmap.SaveJpeg( stream, 173, 173, 0, 100 );

					// Close the stream, and by that saving the file to the ISF.
					stream.Close();
					onFinish( new Uri( isoStoreTileImage, UriKind.Absolute ) );
					
				}

			
			};
			
		}
	}
}
