using System.Security.Cryptography;
using System.Text;

namespace BonyanForEngineeringConsultingFirms.Helpers
{
	public static class PasswordHelper
	{
		public static string HashMD5(string input)
		{
			using var md5 = MD5.Create();
			var bytes = Encoding.UTF8.GetBytes(input);
			var hash = md5.ComputeHash(bytes);
			return string.Concat(hash.Select(b => b.ToString("x2")));
		}
	}
}