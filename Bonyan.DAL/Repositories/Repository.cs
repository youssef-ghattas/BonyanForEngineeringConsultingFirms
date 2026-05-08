using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonyan.DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace Bonyan.DAL.Repositories
{
	public class Repository<T> : IRepository<T> where T : class
	{
		protected readonly BonyanDbContext _context;
		protected readonly DbSet<T> _dbSet;

		public Repository(BonyanDbContext context)
		{
			_context = context;
			_dbSet = _context.Set<T>();
		}

		// ── Get All ──────────────────────────────────────
		public IEnumerable<T> GetAll()
		{
			return _dbSet.ToList();
		}

		// ── Get By Id ────────────────────────────────────
		public T GetById(int id)
		{
			return _dbSet.Find(id);
		}

		// ── Add ──────────────────────────────────────────
		public void Add(T entity)
		{
			_dbSet.Add(entity);
		}

		// ── Update ───────────────────────────────────────
		public void Update(T entity)
		{
			_dbSet.Update(entity);
		}

		// ── Delete ───────────────────────────────────────
		public void Delete(int id)
		{
			var entity = _dbSet.Find(id);
			if (entity != null)
			{
				_dbSet.Remove(entity);
			}
		}

		// ── Save ─────────────────────────────────────────
		public void Save()
		{
			_context.SaveChanges();
		}
	}
}
