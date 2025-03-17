namespace BusinessLogicLayer.ModelView.Microservice_Model_Views.EmailService
{
    public class EmailMetadata
    {
        public string ToAddress { get; set; }
        public string Subject { get; set; }
        public string EssentialData { get; set; }

        public EmailMetadata(string toAddress, string subject, string essentialData)
        {
            ToAddress = toAddress;
            Subject = subject;
            EssentialData = essentialData;
        }
    }
}
