using Tailor.Business.Abstract;
using Tailor.DataAccess.Abstract;
using Tailor.DataAccess.Concrete;
using Tailor.DataAccess.Context; // Transaction için gerekli
using Tailor.DTO.DTOs.AddressDtos;
using Tailor.DTO.DTOs.OrderDtos;

using Tailor.DTO.DTOs.PaymentDtos;
using Tailor.DTO.DTOs.StockDtos;
using Tailor.Entity.Entities;
using Tailor.Entity.Entities.Enums;

namespace Tailor.Business.Concrete
{
    public class OrderManager : IOrderService
    {
        private readonly IOrderDal _orderDal;
        private readonly ICartService _cartService;
        private readonly IStockService _stockService; // EKLENDİ
        private readonly IPaymentService _paymentService;
        private readonly IPaymentDal _paymentDal;
        private readonly ApplicationDbContext _context; // Transaction için Context lazım


        public OrderManager(
            IOrderDal orderDal,
            ICartService cartService,
            IStockService stockService,
            IPaymentService paymentService,
            IPaymentDal paymentDal,
            ApplicationDbContext context)
        {
            _orderDal = orderDal;
            _cartService = cartService;
            _stockService = stockService;
            _paymentService = paymentService;
            _paymentDal = paymentDal;
            _context = context;
        }

        public void CreateOrder(int userId, CreateOrderDto dto)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // -----------------------------------------------------------
                // 1. SEPETİ GETİR VE SEÇİLENLERİ AYIKLA
                // -----------------------------------------------------------
                var fullCart = _cartService.GetCart(userId, null); // Senin GetCart metodun
                if (fullCart == null || !fullCart.Items.Any())
                    throw new Exception("Sepetiniz boş.");

                // Eğer kullanıcı hiç seçim yapmadıysa (liste null veya boşsa) hata ver
                if (dto.SelectedShoppingCartItemIds == null || !dto.SelectedShoppingCartItemIds.Any())
                    throw new Exception("Lütfen satın alınacak ürünleri seçiniz.");

                // Sepetteki ürünlerden, ID'si seçilenler listesinde olanları al
                // NOT: GetCart metodunun döndüğü Item'larda "ShoppingCartItemId" özelliği olmalı.
                var checkoutItems = fullCart.Items
                    .Where(x => dto.SelectedShoppingCartItemIds.Contains(x.ShoppingCartItemId))
                    .ToList();

                if (!checkoutItems.Any())
                    throw new Exception("Seçtiğiniz ürünler sepette bulunamadı veya daha önce silinmiş.");

                // -----------------------------------------------------------
                // 2. FİYAT HESAPLA (Sadece Seçilenler)
                // -----------------------------------------------------------
                decimal totalAmount = checkoutItems.Sum(x => x.UnitPrice * x.Quantity);

                // -----------------------------------------------------------
                // 3. SİPARİŞ OLUŞTUR (PendingPayment)
                // -----------------------------------------------------------
                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.PendingPayment, // Ödeme Bekliyor
                    TotalAmount = totalAmount,           // Sadece seçilenlerin tutarı
                    UserId = userId,
                    ShippingAddressId = dto.ShippingAddressId,
                    BillingAddressId = dto.BillingAddressId,
                    OrderItems = new List<OrderItem>()
                };

                // -----------------------------------------------------------
                // 4. STOK DÜŞ VE ORDER ITEM EKLE
                // -----------------------------------------------------------
                foreach (var item in checkoutItems)
                {
                    // Stok Kontrolü ve Düşümü
                    _stockService.DecreaseStock(item.ProductId, item.Quantity, item.VariantId);

                    // Sipariş Kalemi Oluştur
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        VariantId = item.VariantId
                    });
                }

                // DB'ye Kaydet (ID oluşsun)
                _orderDal.Add(order);
                _context.SaveChanges(); // <-- HATA ALIRSAN ARTIK DETAYI AŞAĞIDA GÖRECEKSİN

                // -----------------------------------------------------------
                // 5. ÖDEME AL
                // -----------------------------------------------------------
                var paymentDto = new CreatePaymentDto
                {
                    OrderId = order.OrderId,
                    Amount = totalAmount,
                    CardHolderName = dto.CardHolderName,
                    CardNumber = dto.CardNumber,
                    ExpireDate = dto.ExpireDate,
                    Cvv = dto.Cvv
                };

                bool isPaid = _paymentService.ProcessPayment(paymentDto);
                if (!isPaid) throw new Exception("Ödeme banka tarafından reddedildi.");

                // -----------------------------------------------------------
                // 6. ONAYLA VE KAYDET
                // -----------------------------------------------------------
                order.Status = OrderStatus.Received;
                _orderDal.Update(order);

                var paymentRecord = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = totalAmount,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = PaymentMethod.CreditCard,
                    Status = PaymentStatus.Success,
                    TransactionId = Guid.NewGuid().ToString()
                };
                _paymentDal.Add(paymentRecord);

                // -----------------------------------------------------------
                // 7. TEMİZLİK: Sadece SEÇİLEN (Satın Alınan) Ürünleri Sil
                // -----------------------------------------------------------
                foreach (var itemId in dto.SelectedShoppingCartItemIds)
                {
                    // CartManager'daki yeni metodumuzu kullanıyoruz
                    _cartService.RemoveCartItem(userId, itemId);
                }

                // Tüm değişiklikleri kaydet ve işlemi bitir
                _context.SaveChanges();
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback(); // Hata varsa her şeyi geri al (Stoklar geri gelir)

                // --- HATA DETAYINI ORTAYA ÇIKARAN SİHİRLİ KOD ---
                var errorMessage = ex.Message;

                // Eğer bir iç hata varsa onu da mesaja ekle
                if (ex.InnerException != null)
                {
                    errorMessage += $" | SQL HATASI: {ex.InnerException.Message}";

                    // Bazen hata 2 katman içeride olabilir
                    if (ex.InnerException.InnerException != null)
                    {
                        errorMessage += $" | DETAY: {ex.InnerException.InnerException.Message}";
                    }
                }

                // Controller'a bu detaylı mesajı fırlat
                throw new Exception(errorMessage);
            }
        }

        private string GenerateOrderNumber()
        {
            // Örn: ORD-20251222-98745
            return "ORD-" + DateTime.Now.ToString("yyyyMMdd") + "-" + new Random().Next(10000, 99999);
        }

       

        public List<ResultOrderListDto> GetMyOrders(int userId)
        {
            var orders = _orderDal.GetOrdersByUserId(userId);
            return orders.Select(x => new ResultOrderListDto
            {
                OrderId = x.OrderId,
                OrderNumber = x.OrderNumber,
                OrderDate = x.OrderDate,
                Status = x.Status,
                TotalAmount = x.TotalAmount,
                TotalItemCount = x.OrderItems.Count
            }).ToList();
        }

        // ADMİN İÇİN: Tüm Siparişleri Listele
        public List<ResultOrderListDto> GetAllOrders()
        {
            var orders = _orderDal.GetAllOrdersWithDetails();
            return orders.Select(x => new ResultOrderListDto
            {
                OrderId = x.OrderId,
                OrderNumber = x.OrderNumber,
                OrderDate = x.OrderDate,
                Status = x.Status,
                TotalAmount = x.TotalAmount,
                TotalItemCount = x.OrderItems.Count,
                CustomerName = x.User.Name + " " + x.User.Surname // Ekstra bilgi
            }).ToList();
        }

        // ADMİN İÇİN: Sipariş Durumu Güncelle
        public void UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            var order = _orderDal.GetOrderWithDetails(orderId);
            if (order == null) throw new Exception("Sipariş bulunamadı.");

            // BASİT İADE/İPTAL MANTIĞI
            if (newStatus == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                // Sipariş iptal ediliyorsa stokları geri iade et
                foreach (var item in order.OrderItems)
                {
                    // Stok Girişi Yaparak İade Alıyoruz
                    _stockService.AddStock(new UpdateStockDto
                    {
                         ProductId = item.ProductId,
                         VariantId = item.VariantId,
                         Quantity = item.Quantity,
                         Location = "İade Rafı"
                    });
                }
            }

            order.Status = newStatus;
            _orderDal.Update(order);
        }

        public ResultOrderDto GetOrderDetails(int orderId)
        {
            var order = _orderDal.GetOrderWithDetails(orderId);
            if (order == null) throw new Exception("Sipariş bulunamadı.");

            return new ResultOrderDto
            {
                OrderId = order.OrderId,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                CustomerName = order.User.Name + " " + order.User.Surname,
                ShippingAddress = new ResultAddressDto
                {
                    Id = order.ShippingAddress.Id,
                    Title = order.ShippingAddress.Title,
                    City = order.ShippingAddress.City,
                    District = order.ShippingAddress.District,
                    FullAddress = order.ShippingAddress.FullAddress,
                    ZipCode = order.ShippingAddress.ZipCode
                },

                BillingAddress = new ResultAddressDto
                {
                    Id = order.BillingAddress.Id,
                    Title = order.BillingAddress.Title,
                    City = order.BillingAddress.City,
                    District = order.BillingAddress.District,
                    FullAddress = order.BillingAddress.FullAddress,
                    ZipCode = order.BillingAddress.ZipCode
                },

                OrderItems = order.OrderItems.Select(x => new ResultOrderItemDto
                {
                    ProductId = x.ProductId,
                    ProductName = x.Product.Name,
                    UnitPrice = x.UnitPrice, // Entity uses UnitPrice, DTO uses UnitPrice
                    Quantity = x.Quantity
                    // Variant info omitted for now
                }).ToList()
            };
        }

       /* private string GenerateOrderNumber()
        {
            var randomPart = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            return $"TRZ-{DateTime.Now.Year}-{randomPart}";
        }*/
    }
}