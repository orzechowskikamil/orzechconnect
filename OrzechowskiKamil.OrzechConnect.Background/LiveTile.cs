using System;
using Microsoft.Phone.Shell;
#if DEBUG
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
#endif

namespace OrzechowskiKamil.OrzechConnect.Background
{

	public class LiveTile 
	{
#if DEBUG
		private static DiagnosticHelper helper = new DiagnosticHelper( true, "LiveTile: ", "wazne!----" );
#endif
		private static ShellTile GetShellTile()
		{
			try
			{
				var enumerator = ShellTile.ActiveTiles.GetEnumerator();

				enumerator.MoveNext();
				var first = enumerator.Current;

				return first;
			}
			catch (Exception)
			{

			}
			return null;
		}

		public static void MakeTileDefault()
		{
			UpdateTile( "Orzech Connect", 0, null );
		}

		public static void UpdateTile( string title, int count, Uri backgroundImage )
		{
			ShellTile TileToFind = GetShellTile();

			// Application should always be found
			if (TileToFind != null)
			{

				// Set the properties to update for the Application Tile.
				// Empty strings for the text values and URIs will result in the property being cleared.
				StandardTileData NewTileData = new StandardTileData
				{
					Title = title,
					BackgroundImage = backgroundImage,
					Count = count,
					//BackTitle = textBoxBackTitle.Text,
					//BackBackgroundImage = new Uri( textBoxBackBackgroundImage.Text, UriKind.Relative ),
					//BackContent = textBoxBackContent.Text
				};

				// Update the Application Tile
				TileToFind.Update( NewTileData );
			}
			else
			{
#if DEBUG
				helper.Diagnose( () => helper.DiagnosticMessage( "niestety tile to find okazal sie null" ) );
#endif
			}
		}

	}
}
