using System.Linq.Expressions;
using Tailor.Business.Abstract;
using Tailor.DataAccess.Abstract;
using Tailor.DataAccess.Concrete;
using Tailor.DTO.DTOs.CartDtos;
using Tailor.Entity.Entities;
using Tailor.Entity.Entities.Enums;

namespace Tailor.Business.Concrete
{
    public class CartManager : ICartService
    {
        private readonly ICartDal _cartDal;
        private readonly IProductDal _productDal;
        private readonly IStockService _stockService; // EKLENDİ
        

        public CartManager(ICartDal cartDal, IProductDal productDal, IStockService stockService)
        {
            _cartDal = cartDal;
            _productDal = productDal;
            _stockService = stockService;
         
        }

        // 1. SEPETE EKLEME
        public void AddToCart(int? userId, string? sessionId, AddCartItemDto dto)
        {
            // Doğrusu: Stoğu da yanına alarak getir
            var product = _productDal.GetListWithRelations(x => x.id == dto.ProductId, "Stock", "ProductVariants").FirstOrDefault();
            if (product == null) throw new Exception("Ürün bulunamadı.");

            // 1. Hazır Giyim (Ceket, Pantolon): Eğer varyantı VARSA mutlaka seçilmeli
            if (product.ProductType == ProductType.ReadyToWear && 
                product.ProductVariants != null && 
                product.ProductVariants.Any() && 
                dto.VariantId == null)
            {
                throw new Exception("Lütfen bu ürün için beden/renk seçiniz.");
            }

            // 2. Kumaş: Asla varyant seçilemez (Metraj ürünüdür)
            if (product.ProductType == ProductType.Fabric && dto.VariantId != null)
            {
                throw new Exception("Kumaş ürünlerinde varyant seçilemez, sadece miktar giriniz.");
            }


            var cart = _cartDal.GetCartWithItems(userId, sessionId);
            // 1. Mevcut sepet miktarını bul
            var existingItem = cart?.ShoppingCartItems.FirstOrDefault(x => x.ProductId == dto.ProductId && x.VariantId == dto.VariantId);
            decimal currentCartQuantity = existingItem != null ? existingItem.Quantity : 0;

            // 3. Kontrolü Yap (Varyant Desteğiyle)
            bool isStockSufficient = _stockService.CheckStock(dto.ProductId, currentCartQuantity + dto.Quantity, dto.VariantId);

            if (!isStockSufficient)
            {
                throw new Exception($"Yetersiz Stok! Eklemek istediğiniz miktar mevcut değil.");
            }

            // Sepet yoksa oluştur
            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    SessionId = sessionId,
                    ShoppingCartItems = new List<ShoppingCartItem>()
                };
                _cartDal.Add(cart);
            }

            existingItem = cart.ShoppingCartItems.FirstOrDefault(x => x.ProductId == dto.ProductId && x.VariantId == dto.VariantId);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                cart.ShoppingCartItems.Add(new ShoppingCartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    VariantId = dto.VariantId
                });
            }

            _cartDal.Update(cart);
        }


public ShoppingCartDto GetCart(int? userId, string? sessionId)
{
    Expression<Func<ShoppingCart, bool>> filter = null;
    if (userId.HasValue) filter = x => x.UserId == userId.Value;
    else if (!string.IsNullOrEmpty(sessionId)) filter = x => x.SessionId == sessionId;
    else return null;

    // ÖNEMLİ: String bazlı Include'larda bazen "Product.ProductVariants" yazımı sorun çıkarabilir.
    // Eğer veritabanı katmanınızda (DAL) bu stringleri parçalayıp Include(x => x.Product).ThenInclude(...) 
    // yapmıyorsanız ProductVariants null döner.
    var cart = _cartDal.GetListWithRelations(filter, 
        "ShoppingCartItems", 
        "ShoppingCartItems.Product", 
        "ShoppingCartItems.Product.ProductVariants")
                       .FirstOrDefault();

    if (cart == null) return null;

    var cartDto = new ShoppingCartDto
    {
        ShoppingCartId = cart.ShoppingCartId,
        UserId = cart.UserId.GetValueOrDefault(),
        Items = cart.ShoppingCartItems.Select(x =>
        {
            string imageUrl = x.Product?.ImageUrl ?? "";
            string baseProductName = x.Product?.Name ?? "Ürün";
            string variantInfo = ""; // Debug bilgisi yerine bunu JSON'da göreceğiz

            // VARYANT MANTIĞI
            if (x.VariantId.HasValue && x.Product?.ProductVariants != null)
            {
                var selectedVariant = x.Product.ProductVariants.FirstOrDefault(v => v.Id == x.VariantId.Value);
                if (selectedVariant != null)
                {
                    // Başarılı: İsim ve Resim güncelle
                    variantInfo = $"({selectedVariant.Color} / {selectedVariant.Size})";
                    if (!string.IsNullOrEmpty(selectedVariant.ImageUrl))
                    {
                        imageUrl = selectedVariant.ImageUrl;
                    }
                }
                else
                {
                    // Buraya düşüyorsa ID eşleşmiyordur
                    variantInfo = $"(Varyant Bulunamadı ID:{x.VariantId})";
                }
            }
            else if (x.VariantId.HasValue && x.Product?.ProductVariants == null)
            {
                // Buraya düşüyorsa veri tabanından ProductVariants GELMİYOR demektir (Include sorunu)
                variantInfo = "(Varyant Verisi Yuklenemedi!)";
            }

            return new CartItemDetailDto
            {
                ShoppingCartItemId = x.ShoppingCartItemId,
                ProductId = x.ProductId,
                ProductName = $"{baseProductName} {variantInfo}".Trim(),
                ProductImage = imageUrl,
                Quantity = x.Quantity,
                UnitPrice = x.Product?.price ?? 0,
                VariantId = x.VariantId
            };
        }).ToList()
    };

    cartDto.TotalPrice = cartDto.Items.Sum(x => x.Quantity * x.UnitPrice);
    return cartDto;
}

    // 3. SEPETTEN SİLME
    public void RemoveFromCart(int? userId, string? sessionId, int cartItemId)
        {
            var cart = _cartDal.GetCartWithItems(userId, sessionId);
            if (cart == null) throw new Exception("Sepet bulunamadı.");

            var itemExists = cart.ShoppingCartItems.Any(x => x.ShoppingCartItemId == cartItemId);
            if (!itemExists) throw new Exception("Ürün sepetinizde bulunamadı.");

            _cartDal.DeleteCartItem(cartItemId);
        }

        // 4. SEPETİ TEMİZLEME
        public void ClearCart(int? userId, string? sessionId)
        {
            var cart = _cartDal.GetCartWithItems(userId, sessionId);
            if (cart == null) throw new Exception("Sepet bulunamadı.");

            cart.ShoppingCartItems.Clear();
            _cartDal.Update(cart);
        }

        // 5. MİKTAR GÜNCELLEME
        public void UpdateQuantity(int? userId, string? sessionId, UpdateCartItemDto dto)
        {
            var cart = _cartDal.GetCartWithItems(userId, sessionId);
            if (cart == null) throw new Exception("Sepet bulunamadı.");

            var item = cart.ShoppingCartItems.FirstOrDefault(x => x.ShoppingCartItemId == dto.CartItemId);
            if (item == null) throw new Exception("Ürün bulunamadı.");

            item.Quantity = dto.NewQuantity;
            _cartDal.Update(cart);
        }

        public void MergeCarts(string sessionId, int userId)
        {
            // 1. Misafir sepetini bul
            var guestCart = _cartDal.GetCartWithItems(null, sessionId);

            // Eğer misafirin sepeti yoksa veya boşsa bir şey yapmaya gerek yok
            if (guestCart == null || !guestCart.ShoppingCartItems.Any()) return;

            // 2. Kullanıcının zaten bir sepeti var mı?
            var userCart = _cartDal.GetCartWithItems(userId, null);

            if (userCart == null)
            {
                // SENARYO A: Kullanıcının hiç sepeti yok.
                // Çözüm: Misafir sepetini direkt kullanıcının üzerine yap.
                guestCart.SessionId = string.Empty; // Session bağını kopar (Null hata verdiği için boş string yapıyoruz)
                guestCart.UserId = userId;  // Kullanıcıya bağla
                _cartDal.Update(guestCart);
            }
            else
            {
                // SENARYO B: Kullanıcının da eskiden kalma bir sepeti var.
                // Çözüm: Misafir sepetindeki ürünleri, kullanıcı sepetine taşı.

                foreach (var guestItem in guestCart.ShoppingCartItems)
                {
                    // Kullanıcı sepetinde bu ürün zaten var mı? (Varyant dahil kontrol et)
                    var existingItem = userCart.ShoppingCartItems
                        .FirstOrDefault(x => x.ProductId == guestItem.ProductId && x.VariantId == guestItem.VariantId);

                    if (existingItem != null)
                    {
                        // Varsa miktarını artır
                        existingItem.Quantity += guestItem.Quantity;
                    }
                    else
                    {
                        // Yoksa yeni satır olarak ekle
                        // Not: Entity Framework'te ilişkiyi koparıp yenisine eklemek için 
                        // yeni bir nesne oluşturmak daha güvenlidir.
                        userCart.ShoppingCartItems.Add(new ShoppingCartItem
                        {
                            ProductId = guestItem.ProductId,
                            Quantity = guestItem.Quantity,
                            VariantId = guestItem.VariantId,
                            // ShoppingCartId otomatik atanacak
                        });
                    }
                }

                // Kullanıcı sepetini güncelle
                _cartDal.Update(userCart);

                // Eski misafir sepetini artık silebiliriz, içi boşaldı veya aktarıldı
                _cartDal.Delete(guestCart);
            }
        }

        // Parametre olarak int userId ve int shoppingCartItemId alıyoruz
        public void RemoveCartItem(int userId, int shoppingCartItemId)
        {
            // 1. Sepeti getir ve içindeki "ShoppingCartItems" listesini doldur (Include)
            // DİKKAT: Entity sınıfında listenin adı "ShoppingCartItems" olduğu için buraya onu yazdık.
            var cart = _cartDal.GetListWithRelations(x => x.UserId == userId, "ShoppingCartItems")
                               .FirstOrDefault();

            if (cart != null && cart.ShoppingCartItems != null)
            {
                // 2. Silinecek satırı ID'sine göre bul
                var itemToDelete = cart.ShoppingCartItems
                                       .FirstOrDefault(x => x.ShoppingCartItemId == shoppingCartItemId);

                if (itemToDelete != null)
                {
                    // 3. Listeden çıkar
                    cart.ShoppingCartItems.Remove(itemToDelete);

                    // 4. Güncelle (EF Core bunu veritabanından silecektir)
                    _cartDal.Update(cart);
                }
            }
        }
    }
}