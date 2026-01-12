using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Tailor.DataAccess.Abstract;
using Tailor.DataAccess.Context;

namespace Tailor.DataAccess.Repositories
{
    public class GenericRepository<T> : IGenericDal<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet; // DbSeti bir kere tanımlayıp kullanalım

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public void Add(T entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();

        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
            _context.SaveChanges();
        }

        public List<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public T GetByFilter(Expression<Func<T, bool>> filter)
        {
            // Tek bir kayıt döner, filtreye göre (örn: FirstOrDefault)
            return _dbSet.SingleOrDefault(filter);
        }
        public T GetByFilter1(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate).FirstOrDefault();
        }

        public T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        public List<T> GetListByFilter(Expression<Func<T, bool>> filter)
        {
            // Verilen lambda ifadesine (x=>x.IsActive) göre filtreler
            return _dbSet.Where(filter).ToList();
        }

        //En kritik metot : İlişkili tabloları (join) dhil ederren veri getirme
        public List<T> GetListWithRelations(Expression<Func<T, bool>> filter = null, params string[] includeProperties)
        {
            IQueryable<T> query = _dbSet;
            //Eğer filtre varsa uygula (örn fiyatı 100 den büyük olanlar)
            if (filter != null)
            {
                query = query.Where(filter);
            }

            //İlişkili tabloları dahil et(örn: Category, ProductImages)

            if (includeProperties != null)
            {
                foreach (var item in includeProperties)
                {
                    query = query.Include(item);
                }
            }
            return query.ToList();
        }
        public void Update(T entity)
        {
           _dbSet.Update(entity);
            _context.SaveChanges();
        }

        // Hem boş çalışır (hepsini getirir) hem de filtre kabul eder
        public List<T> GetAll(Expression<Func<T, bool>> filter = null)
        {
            // _dbSet zaten yukarıda tanımlı, onu kullanıyoruz.
            return filter == null
                ? _dbSet.ToList()
                : _dbSet.Where(filter).ToList();
        }

        public int GetCount(Expression<Func<T, bool>> filter = null)
        {
            return filter == null ? _dbSet.Count() : _dbSet.Where(filter).Count();
        }
    }
}
