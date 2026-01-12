using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Key i�in gerekebilir
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tailor.Entity.Entities
{
    public class Category
    {
        // SQL scriptinde "SELECT @KumasID = Id" dedi�i i�in ismin "Id" olmas� �ok �nemli.
        public int Id { get; set; }

        public string Name { get; set; }

        // SQL'de Description s�tunu ekledi�imiz i�in burada da olmal�
        public string? Description { get; set; }

        // G�rsel i�in
        public string? ImageUrl { get; set; }

        // Aktif/Pasif durumu (SQL'de IsActive var)
        public bool IsActive { get; set; } = true;

        // --- Parent-Child �li�kisi (Kendi kendine referans) ---
        public int? ParentCategoryId { get; set; }
        public Category ParentCategory { get; set; }
        public ICollection<Category> ChildCategories { get; set; }

        // �r�nlerle �li�ki
        public ICollection<Product> Products { get; set; }
    }
}