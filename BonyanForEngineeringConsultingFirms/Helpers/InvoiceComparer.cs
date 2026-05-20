using Bonyan.DAL.Models;

namespace Bonyan.PL.Helpers
{
	public class InvoiceComparer : IEqualityComparer<Invoice>
	{
		public bool Equals(Invoice x, Invoice y)
		{
			if (x == null || y == null) return false;
			return x.Invoice_ID == y.Invoice_ID;
		}

		public int GetHashCode(Invoice obj)
		{
			return obj.Invoice_ID.GetHashCode();
		}
	}
}