namespace BusinessLogicLayer.ModelView.Service_Model_Views.Payments
{
    public class PayOSWebhookModel
    {
        public long orderCode { get; set; }
        public int amount { get; set; }
        public string description { get; set; }
        public string status { get; set; } // "PAID", "CANCELLED", "PENDING"
        public long transactionId { get; set; }
        public long createdAt { get; set; }
        public string signature { get; set; } // chữ ký xác thực
    }
}
