namespace Tailor.Entity.Entities.Enums
{
    public enum PaymentMethod
    {
        CreditCard = 0,
        BankTransfer = 1,
        CashOnDelivery = 2
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2,
        Refunded = 3
    }
}
