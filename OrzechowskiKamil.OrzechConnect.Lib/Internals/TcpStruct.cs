
namespace OrzechowskiKamil.OrzechConnect.Lib.Internals
{
	public interface ITcpStruct { }
	/// <summary>
	/// Baza dla wszelkich pakietów uzywanych w kliencie
	/// </summary>
	abstract public class TcpStruct : ITcpStruct
	{
		/// <summary>
		/// Ustawia zadany bit na 1 w danym incie
		/// </summary>
		/// <param name="input"></param>
		/// <param name="bitPosition"></param>
		/// <returns></returns>
		private uint setBitEnabled( uint input, int bitPosition )
		{
			var mask = 1 << bitPosition;
			return setMaskEnabled( input, ( uint ) mask );
		}

		/// <summary>
		/// Ustawia zadany bit na 0 w danym incie
		/// </summary>
		/// <param name="input"></param>
		/// <param name="bitPosition"></param>
		/// <returns></returns>
		private uint setBitDisabled( uint input, int bitPosition )
		{
			var mask = 1 << bitPosition;
			return setMaskDisabled( input, ( uint ) mask );
		}

		/// <summary>
		/// Ustawia zadanemu bitowi nową wartosc.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="bitPosition"></param>
		/// <param name="newValue"></param>
		/// <returns></returns>
		protected uint setBit( uint input, int bitPosition, bool newValue )
		{
			if (newValue == true)
			{
				return this.setBitEnabled( input, bitPosition );
			}
			else
			{
				return this.setBitDisabled( input, bitPosition );
			}
		}

		private uint setMaskEnabled( uint input, uint mask )
		{
			input |= mask;
			return input;
		}

		private uint setMaskDisabled( uint input, uint mask )
		{
			input &= (~mask);
			return input;
		}

		/// <summary>
		/// Włącza lub wyłącza daną maskę
		/// </summary>
		/// <param name="input">wejscie</param>
		/// <param name="mask">maska</param>
		/// <param name="isMaskOn">Czy maska ma zostac wlaczona czy wylaczona</param>
		/// <returns></returns>
		protected uint setMask( uint input, uint mask, bool isMaskOn )
		{
			if (isMaskOn)
			{
				return setMaskEnabled( input, mask );
			}
			else
			{
				return setMaskDisabled( input, mask );
			}
		}

		protected bool GetValue( uint input, uint mask )
		{
			var result = input & mask;
			if (result == mask)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}



}
