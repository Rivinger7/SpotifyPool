﻿namespace SetupLayer.Setting.Microservices.AWS
{
    public class AWSSettings
    {
        public required string BucketName { get; set; }
        public required string Region { get; set; }
        public required string MediaConvertRole { get; set; }
        public required string MediaConvertEndpoint { get; set; }
        public required string MediaConvertQueue { get; set; } // Queue ARN
    }
}
