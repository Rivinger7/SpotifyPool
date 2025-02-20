using Amazon.MediaConvert;
using Amazon.MediaConvert.Model;
using Amazon.S3;
using Amazon.S3.Transfer;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Microservices_Interface.AWS;
using Microsoft.AspNetCore.Http;
using SetupLayer.Setting.Microservices.AWS;

namespace BusinessLogicLayer.Implement.Microservices.AWS
{
    public class AmazonWebService(IAmazonS3 s3Client, AWSSettings aWSSettings) : IAmazonWebService
    {
        private readonly IAmazonS3 _s3Client = s3Client;
        private readonly AWSSettings _aWSSettings = aWSSettings;

        public async Task<(string, string)> UploadAndConvertToStreamingFile(IFormFile audioFile, string trackIdName)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                throw new BadRequestCustomException("File is empty");
            }
                
            // Lấy thông tin từ settings
            string bucketName = _aWSSettings.BucketName;
            string region = _aWSSettings.Region;
            string mediaConvertRole = _aWSSettings.MediaConvertRole;
            string endpoint = _aWSSettings.MediaConvertEndpoint;
            string queueArn = _aWSSettings.MediaConvertQueue; // Thêm Queue ARN

            // Kiểm tra biến môi trường có null không
            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(mediaConvertRole) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(queueArn))
                throw new Exception("Missing AWS Configuration in Environment Variables");

            // Chuẩn bị thông tin file
            string outputFolder = "streaming-audio/";
            string fileName = trackIdName + Path.GetExtension(audioFile.FileName);
            string s3Key = outputFolder + fileName;
            string s3Url = $"s3://{bucketName}/{s3Key}";

            // Upload file lên S3
            using (var stream = audioFile.OpenReadStream())
            {
                TransferUtilityUploadRequest uploadRequest = new()
                {
                    InputStream = stream,
                    BucketName = bucketName,
                    Key = s3Key,
                    ContentType = "audio/mpeg"
                };
                TransferUtility fileTransferUtility = new(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            // Tạo URL đúng chuẩn AWS S3 Public
            string s3UrlPublic = $"https://{bucketName}.s3.{region}.amazonaws.com/{s3Key}";

            // Khởi tạo MediaConvert Client với endpoint
            AmazonMediaConvertClient mediaConvertClient = new(new AmazonMediaConvertConfig
            {
                ServiceURL = endpoint
            });

            // Gửi Job MediaConvert
            CreateJobRequest createJobRequest = new()
            {
                Role = mediaConvertRole,
                Queue = queueArn, // Thêm Queue để AWS xử lý job nhanh hơn
                Settings = new JobSettings
                {
                    Inputs =
                    [
                        new Input
                        {
                            FileInput = s3Url,
                            AudioSelectors = new Dictionary<string, AudioSelector>
                            {
                                { "Audio Selector 1", new AudioSelector { DefaultSelection = AudioDefaultSelection.DEFAULT } }
                            }
                        }
                    ],
                    OutputGroups =
                    [
                        new OutputGroup
                        {
                            Name = "HLS",
                            OutputGroupSettings = new OutputGroupSettings
                            {
                                Type = OutputGroupType.HLS_GROUP_SETTINGS,
                                HlsGroupSettings = new HlsGroupSettings
                                {
                                    Destination = $"s3://{bucketName}/{outputFolder}{Path.GetFileNameWithoutExtension(fileName)}_hls/",
                                    MinSegmentLength = 0,
                                    SegmentLength = 10 // Độ dài mỗi segment HLS
                                }
                            },
                            Outputs =
                            [
                                new Output
                                {
                                    NameModifier = "_hls",
                                    ContainerSettings = new ContainerSettings
                                    {
                                        Container = ContainerType.M3U8
                                    },
                                    AudioDescriptions =
                                    [
                                        new AudioDescription
                                        {
                                            AudioSourceName = "Audio Selector 1", // Tham chiếu đến Audio Selector
                                            CodecSettings = new AudioCodecSettings
                                            {
                                                Codec = AudioCodec.AAC,
                                                AacSettings = new AacSettings
                                                {
                                                    CodingMode = AacCodingMode.CODING_MODE_2_0,
                                                    Bitrate = 128000,
                                                    SampleRate = 48000
                                                }
                                            }
                                        }
                                    ],
                                    OutputSettings = new OutputSettings
                                    {
                                        HlsSettings = new HlsSettings
                                        {
                                            SegmentModifier = "_hls"
                                        }
                                    }
                                }
                            ]
                        }
                    ]
                }
            };

            // Tạo Job
            CreateJobResponse jobResponse = await mediaConvertClient.CreateJobAsync(createJobRequest);

            //Console.WriteLine(jobResponse.ToJson());

            // Trả về link file .m3u8
            string m3u8Url = $"https://{bucketName}.s3.{region}.amazonaws.com/{outputFolder}{Path.GetFileNameWithoutExtension(fileName)}_hls/{Path.GetFileNameWithoutExtension(fileName)}_hls.m3u8";

            return (s3UrlPublic, m3u8Url);
        }
    }
}
