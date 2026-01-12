using System;

namespace Tailor.DTO.DTOs.ShipmentDtos
{
    public class CreateShipmentDto
    {
        public int OrderId { get; set; }
        public string Carrier { get; set; }
        public string TrackingNumber { get; set; }
    }

    public class ResultShipmentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string TrackingNumber { get; set; }
        public string Carrier { get; set; }
        public string Status { get; set; }
        public DateTime ShippedDate { get; set; }
    }
}
