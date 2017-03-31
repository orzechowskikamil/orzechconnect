using System.IO;
using System.Text;
using OrzechowskiKamil.OrzechConnect.Lib.Internals;
using System;

namespace OrzechowskiKamil.OrzechConnect.Lib.OutPackets
{
	/// <summary>
	/// Pakiet ktory mozna wyslac za pomoca packetManagera
	/// </summary>
	public interface IOutTcpPacket : ITcpStruct
	{
		byte[] ToBinaryPacket();
	}

	/// <summary>
	/// pakiet serializowalny za pomocą factory OutTcpPacket
	/// </summary>
	public interface ISerializableGGPacket : IOutTcpPacket
	{
		void SerializeToBytesArray( NullTerminatedBinaryWriter writer );
		uint PacketCode { get; }
	}

	/// <summary>
	/// Klasa jest bazą dla pakietu Out - wysylanego od klienta
	/// </summary>
	abstract public class OutTcpPacket : TcpStruct, ISerializableGGPacket
	{

		abstract protected uint packetCode { get; }
		public byte[] ToBinaryPacket()
		{
			return OutTcpPacket.ToBinaryPacket( this );
		}

		public static byte[] ToBinaryPacket( ISerializableGGPacket packetToSerialize )
		{
			var stream = new MemoryStream();
			var insidePacketStream = new MemoryStream();
			var writer = new BinaryWriter( stream );
			var writerInside = new NullTerminatedBinaryWriter( insidePacketStream );
			packetToSerialize.SerializeToBytesArray( writerInside );
			var binaryFields = insidePacketStream.ToArray();
			writer.Write( packetToSerialize.PacketCode );
			writer.Write( ( uint ) binaryFields.Length );
			writer.Write( binaryFields );
			var result = stream.ToArray();
			writer.Close();
			insidePacketStream.Close();
			stream.Close();
			return result;
		}
		abstract protected void writeToBytesArray( NullTerminatedBinaryWriter writer );



		public void SerializeToBytesArray( NullTerminatedBinaryWriter writer )
		{
			this.writeToBytesArray( writer );
		}

		public uint PacketCode
		{
			get { return this.packetCode; }
		}
	}



	/// <summary>
	/// Kody pakietów ktore zostaja umieszczone w headerze (pakiety odbierane)
	/// </summary>
	public enum OutPacketsCodes
	{
		Login = 0x0031, SetStatus = 0x0038, SendMessage = 0x002d, ListEmpty = 0x0012, AddContact = 0x000d,
		RemoveContact = 0x000e,
	}


}
