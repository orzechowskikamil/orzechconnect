
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.IO;
namespace OrzechowskiKamil.OrzechConnect.Lib.DataStorage
{
	/// <summary>
	/// Odczyt / zapis do isolatedStorage
	/// </summary>
	public class XMLFileStorage
	{

		public XMLFileStorage( string fileName )
		{
			this.FileName = fileName;
		}



		public string FileName { get; private set; }




		public void Delete()
		{
			IsolatedStorageFile storageFile = IsolatedStorageFile.GetUserStoreForApplication();
			storageFile.DeleteFile( this.FileName );
		}

		public XDocument loadXML()
		{
			using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
			{
				using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream( this.FileName, FileMode.Open,
					isoStore ))
				{
					XDocument xdocument = XDocument.Load( isoStream );
					return xdocument;
				}
			}
		}

		public void saveXML( XDocument xDocument )
		{
			using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
			{
				using (IsolatedStorageFileStream isoStream =
					new IsolatedStorageFileStream( this.FileName, FileMode.Create, isoStore ))
				{
					xDocument.Save( isoStream );
				}
			}
		}
	}

}
