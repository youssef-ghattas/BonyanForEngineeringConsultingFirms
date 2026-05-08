using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonyan.BLL.Services
{
	public interface IService<T> where T : class
	{
		// ── Get Operations ───────────────────────────────
		IEnumerable<T> GetAll();
		T GetById(int id);

		// ── CUD Operations ───────────────────────────────
		void Add(T entity);
		void Update(T entity);
		void Delete(int id);
	}
}
