using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tailor.Entity.Entities;

namespace Tailor.DataAccess.Context
{
    // IdentityDbContext: Identity tablolarını (Users, Roles, Claims) otomatik oluşturur.
    // AppUser: Sizin özel kullanıcı sınıfınız.
    // IdentityRole<int>: Rol sınıfı (int tipinde ID).
    // int: Primary Key türü.
    // IdentityRole<int> yerine kendi oluşturduğun AppRole sınıfını yazmalısın.
    public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // =========================================================
        // 1. DBSET TANIMLAMALARI (Tüm Entity'ler)
        // =========================================================

        // Müşteri & İletişim & Destek
        public DbSet<Address> Addresses { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<Testimonial> Testimonials { get; set; }

        // Ürün Yönetimi
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductProperty> ProductProperties { get; set; }
        public DbSet<ProductDisplay> ProductDisplays { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; } // Çoka-çok ilişki
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<TailoredProduct> TailoredProducts { get; set; }
        public DbSet<Category> Categories { get; set; }

        // Blog Yönetimi
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogCategory> BlogCategories { get; set; }
        public DbSet<BlogCategoryAssignment> BlogCategoryAssignments { get; set; }

        // Sipariş & Sepet & Finans & Lojistik
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ShoppingCart> ShoppingCards { get; set; } // DİKKAT: Sınıf adınız ShoppingCard
        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Shipping> Shippings { get; set; }

        // İçerik & Analiz & Diğerleri
        public DbSet<About> Abouts { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<SearchLog> SearchLogs { get; set; }
        public DbSet<StockLog> StockLogs { get; set; }
        public DbSet<CustomerSocial> SocialMedias { get; set; } // SocialMedia.cs dosyanız var
        public DbSet<Favorite> Favorites { get; set; }


        // =========================================================
        // 2. FLUENT API AYARLARI (İlişkiler & Kısıtlamalar)
        // =========================================================
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // 1. Identity Ayarları (ZORUNLU)
            base.OnModelCreating(builder);

            // Favoriler İlişkisi
            builder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Favorite>()
                .HasOne(f => f.Product)
                .WithMany()
                .HasForeignKey(f => f.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // --------------------------------------------------------
            // A. ÇOKA-ÇOK İLİŞKİLER (Bileşik Anahtarlar)
            // --------------------------------------------------------

            // Product <-> Tag İlişkisi
            builder.Entity<ProductTag>()
                .HasKey(pt => new { pt.ProductId, pt.TagId }); // İki ID birleşip anahtar olur.

            builder.Entity<ProductTag>()
                .HasOne(pt => pt.Product).WithMany(p => p.ProductTags).HasForeignKey(pt => pt.ProductId);

            builder.Entity<ProductTag>()
                .HasOne(pt => pt.Tag).WithMany(t => t.ProductTags).HasForeignKey(pt => pt.TagId);

           /* // Blog <-> BlogCategory İlişkisi
            builder.Entity<BlogCategoryAssignment>()
                .HasKey(bc => new { bc.BlogId, bc.BlogCategoryId }); // BlogId ve CategoryId birleşip anahtar olur.

            builder.Entity<BlogCategoryAssignment>()
                .HasOne(bc => bc.Blog).WithMany(b => b.CategoryAssignments).HasForeignKey(bc => bc.BlogId);

            builder.Entity<BlogCategoryAssignment>()
                .HasOne(bc => bc.BlogCategory).WithMany(c => c.CategoryAssignments).HasForeignKey(bc => bc.BlogCategoryId);

            */

            // --------------------------------------------------------
            // B. KRİTİK İLİŞKİLER VE SİLME DAVRANIŞLARI
            // --------------------------------------------------------

            // Kategori Ağacı (Self-Referencing)
            // Alt kategorisi olan bir Ana Kategori silinmesin (Restrict).
            builder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Kullanıcı silinirse Siparişleri SİLİNMESİN (Veri kaybı önleme)
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sipariş <-> Adres İlişkileri (Restrict - Adres silinirse sipariş bozulmasın)
            builder.Entity<Order>()
                .HasOne(o => o.ShippingAddress)
                .WithMany() // Address tarafında ICollection<Order> yoksa boş bırakılır
                .HasForeignKey(o => o.ShippingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.BillingAddress)
                .WithMany()
                .HasForeignKey(o => o.BillingAddressId)
                .OnDelete(DeleteBehavior.Restrict);


            // ============================================================
            // 🎫 SUPPORT TICKET İLİŞKİ ÇÖZÜMLEMESİ
            // ============================================================

            // 1. Müşteri İlişkisi (User -> SupportTickets)
            // "Bir biletin bir User'ı vardır ve o User'ın çokça SupportTicket'ı (açtığı) vardır."
            builder.Entity<SupportTicket>()
                .HasOne(t => t.User)
                .WithMany(u => u.SupportTickets) // AppUser'daki listeyi buraya bağlıyoruz
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Kullanıcı silinirse bilet geçmişi silinmesin

            // ============================================================
            // 🛒 SHOPPING CART & USER İLİŞKİSİ
            // ============================================================
            builder.Entity<AppUser>()
                .HasOne(u => u.ShoppigCard)
                .WithOne() // ShoppingCart tarafında User property'si açıksa belirtilir, şu an kapalı görünüyor.
                .HasForeignKey<ShoppingCart>(sc => sc.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Kullanıcı silinirse sepeti de silinsin.

            // 2. Yönetici İlişkisi (Admin -> ?)
            // "Bir biletin bir Admin'i olabilir ama Admin'in cevapladığı biletler için özel bir listesi YOKTUR (WithMany boş)."
            builder.Entity<SupportTicket>()
                .HasOne(t => t.Admin)
                .WithMany() // Admin'in cevapladığı biletleri AppUser içinde ayrı bir listede tutmuyoruz.
                .HasForeignKey(t => t.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================================================

            // C. BİRE-BİR İLİŞKİLER (One-to-One)

            // ============================================================

            // EF Core bazen bunları karıştırabilir, elle belirtmek en iyisidir.



            // Product -> Stock (Bir ürünün bir stok kaydı vardır)

            builder.Entity<Product>()

                .HasOne(p => p.Stock)

                .WithOne(s => s.Product)

                .HasForeignKey<Stock>(s => s.ProductId)

                .OnDelete(DeleteBehavior.Cascade); // Ürün silinirse stoğu da silinsin.



            // Order -> Shipping (Bir siparişin bir kargo politikası vardır)

            builder.Entity<Order>()

                .HasOne(o => o.Shipping)

                .WithOne(s => s.Order)

                .HasForeignKey<Shipping>(s => s.OrderId)

                .OnDelete(DeleteBehavior.Cascade); // Sipariş silinirse kargo bilgisi de gitsin.



            // Product -> TailoredProduct (Özel dikim vitrini)

            builder.Entity<TailoredProduct>()

                .HasOne(tp => tp.Product)

                .WithOne() // Product tarafında TailoredProduct referansı yoksa boş bırak

                .HasForeignKey<TailoredProduct>(tp => tp.ProductId)

                .OnDelete(DeleteBehavior.Cascade);

            // --- BLOG & PRODUCT İLİŞKİSİ (Çoka-Çok) ---

            // 1. Composite Key Tanımı (İkisi birleşip tek anahtar olur)
            builder.Entity<BlogProductAssignment>()
                .HasKey(x => new { x.BlogId, x.ProductId });

            // 2. İlişkilerin Yönünü Belirtme (Opsiyonel ama Garanti Yöntem)
            // "Bir Blog'un çoklu ürün ataması vardır ve bunlar BlogId ile bağlanır"
            builder.Entity<BlogProductAssignment>()
                .HasOne(x => x.Blog)
                .WithMany(y => y.ProductAssignments)
                .HasForeignKey(x => x.BlogId);

            // "Bir Ürün'ün çoklu blog ataması vardır ve bunlar ProductId ile bağlanır"
            builder.Entity<BlogProductAssignment>()
                .HasOne(x => x.Product)
                .WithMany(y => y.BlogAssignments)
                .HasForeignKey(x => x.ProductId);

            // --- BLOG & BLOG CATEGORY İLİŞKİSİ (Bunu da eklesin) ---
            builder.Entity<BlogCategoryAssignment>()
                .HasKey(x => new { x.BlogId, x.BlogCategoryId });


            // --------------------------------------------------------
            // D. PARA BİRİMİ AYARLARI (Decimal Precision)
            // --------------------------------------------------------
            // SQL Server'da "Price" ve "Amount" alanlarının hata vermemesi için

            var decimalProps = builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?));

            foreach (var property in decimalProps)
            {
                property.SetColumnType("decimal(18,2)");
            }
        }
    }
}
