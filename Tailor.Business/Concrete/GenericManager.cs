using System.Linq.Expressions;
using Tailor.Business.Abstract;
using Tailor.DataAccess.Abstract;

namespace Tailor.Business.Concrete
{
    public class GenericManager<T> : IGenericService<T> where T : class
    {
        // DataAccess katmanındaki Generic yapıyı çağırıyoruz
        private readonly IGenericDal<T> _dal;

        public GenericManager(IGenericDal<T> dal)
        {
            _dal = dal;
        }

        // --- Temel CRUD İmplementasyonu ---

        public void TAdd(T entity)
        {
            // İsterseniz buraya validasyon ekleyebilirsiniz
            _dal.Add(entity);
        }

        public void TDelete(T entity)
        {
            _dal.Delete(entity);
        }

        public void TUpdate(T entity)
        {
            _dal.Update(entity);
        }

        public List<T> TGetList()
        {
            return _dal.GetAll();
        }

        public T TGetById(int id)
        {
            return _dal.GetById(id);
        }

        // --- Gelişmiş Filtreleme İmplementasyonu ---

        public T TGetByFilter(Expression<Func<T, bool>> filter)
        {
            return _dal.GetByFilter(filter);
        }

        public List<T> TGetListByFilter(Expression<Func<T, bool>> filter)
        {
            return _dal.GetListByFilter(filter);
        }

        public List<T> TGetListWithRelations(Expression<Func<T, bool>> filter = null, params string[] includeProperties)
        {
            return _dal.GetListWithRelations(filter, includeProperties);
        }
    }
}