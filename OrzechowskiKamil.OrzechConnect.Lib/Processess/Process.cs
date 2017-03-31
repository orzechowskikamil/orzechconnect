using OrzechowskiKamil.OrzechConnect.Lib.InPackets;

namespace OrzechowskiKamil.OrzechConnect.Lib.Processess
{
	/// <summary>
	/// Jeden z procesów które mogą się odbywać w gadu aktualnie.
	/// </summary>
	abstract public class Process
	{
		/// <summary>
		/// Próbuje obsłużyć nadchodzący pakiet. Jeśli go obsłuży, zwraca true, jeżeli nie jest zainteresowana
		/// takim pakietem, zwraca false.
		/// </summary>
		/// <param name="packet"></param>
		/// <returns></returns>
		abstract public bool OnPacketReceived( IInTcpPacket packet );

	}

}
