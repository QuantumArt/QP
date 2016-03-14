using System;

namespace Quantumart.QP8.Utils
{
	public static class Randomizer
	{
		/// <summary>
		/// Генерирует случайную строку символов, которая необходима 
		/// для отключения кэширования графических файлов баннеров
		/// </summary>
		/// <param name="randomStringLength">длина строки</param>
		/// <returns>случайная строка символов</returns>
		public static string GenerateRandomString(int randomStringLength)
		{
			string symbolString = "QuantumArt98BCDEFGHIJKLMNOPRSTUVWXYZbcdefghijklopqsvwxyz01234567"; // строка символов
			int symbolStringLength = symbolString.Length; // длина строки символов
			int randomNumber = 0; // cлучайное число
			string randomSymbol = ""; // случайный символ
			Random randomizer = new Random(); // рандомайзер
			string result = ""; // результирующая переменная

			for (int i = 0; i < randomStringLength; i++)
			{
				randomNumber = randomizer.Next(symbolStringLength);
				randomSymbol = symbolString.Substring(randomNumber, 1);

				result += randomSymbol;
			}

			return result;
		}
	}
}
