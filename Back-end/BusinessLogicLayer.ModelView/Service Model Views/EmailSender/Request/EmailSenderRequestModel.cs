using BusinessLogicLayer.ModelView.Service_Model_Views.Users.Response;

namespace BusinessLogicLayer.ModelView.Service_Model_Views.EmailSender.Request
{
    public class EmailSenderRequestModel
    {
        public required IEnumerable<string> EmailTo { get; set; }
        public required string Subject { get; set; }
    }
}
