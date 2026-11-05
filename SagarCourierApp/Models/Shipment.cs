namespace SagarCourierApp.Models
{
    public class Shipment
    {
        public int ShipmentId { get; set; }

        public string TrackingNumber { get; set; } = string.Empty;

        public string SenderName { get; set; } = string.Empty;

        public string ReceiverName { get; set; } = string.Empty;

        public string ReceiverPhone { get; set; } = string.Empty;

        public string PickupAddress { get; set; } = string.Empty;

        public string DeliveryAddress { get; set; } = string.Empty;

        public decimal Weight { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
    }
}