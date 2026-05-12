using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonyan.DAL.Repositories;

namespace Bonyan.BLL.Services
{
	public class Service<T> : IService<T> where T : class
	{
		protected readonly IRepository<T> _repository;

		public Service(IRepository<T> repository)
		{
			_repository = repository;
		}

		// ── Get All ──────────────────────────────────────
		public IEnumerable<T> GetAll()
		{
			return _repository.GetAll();
		}

		// ── Get By Id ────────────────────────────────────
		public T GetById(int id)
		{
			return _repository.GetById(id);
		}

		// ── Add ──────────────────────────────────────────
		public void Add(T entity)
		{
			_repository.Add(entity);
			_repository.Save();
		}

		// ── Update ───────────────────────────────────────
		public void Update(T entity)
		{
			_repository.Update(entity);
			_repository.Save();
		}

		// ── Delete ───────────────────────────────────────
		public void Delete(int id)
		{
			_repository.Delete(id);
			_repository.Save();
		}

        public IQueryable<T> GetWithIncludes()
        {
            return _repository.GetWithIncludes();
        }
    }
}