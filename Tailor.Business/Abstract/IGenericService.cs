using System.Linq.Expressions;

namespace Tailor.Business.Abstract
{
    public interface IGenericService<T> where T : class
    {
        // --- Temel CRUD İşlemleri ---
        // (Metot isimlerinin başına 'T' koyuyoruz ki DAL ile karışmasın)
        void TAdd(T entity);
        void TDelete(T entity);
        void TUpdate(T entity);
        List<T> TGetList();
        T TGetById(int id);

        // --- Gelişmiş Filtreleme İşlemleri ---

        // Şarta uyan İLK kaydı getirir (Örn: Email adresi 'x' olan kullanıcı)
        T TGetByFilter(Expression<Func<T, bool>> filter);

        // Şarta uyan TÜM kayıtları liste olarak getirir (Örn: Fiyatı 100'den büyük ürünler)
        List<T> TGetListByFilter(Expression<Func<T, bool>> filter);

        // İlişkili tabloları (Category, User vb.) dahil ederek (Include) listeleme
        List<T> TGetListWithRelations(Expression<Func<T, bool>> filter = null, params string[] includeProperties);
    }
}