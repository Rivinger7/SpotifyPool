namespace SetupLayer.Setting.Microservices.EmailSender
{
    public class SmtpSettings
    {
        public string? Host { get; set; } = string.Empty;
        public string? Port { get; set; } = string.Empty;
        public string? Username { get; set; } = string.Empty;
        public string? Password { get; set; } = string.Empty;
    }

    public class EmailSenderSetting
    {
        public SmtpSettings Smtp { get; set; } = null!;
        public string? FromAddress { get; set; } = string.Empty;
        public string? FromName { get; set; } = string.Empty;
    }
}
