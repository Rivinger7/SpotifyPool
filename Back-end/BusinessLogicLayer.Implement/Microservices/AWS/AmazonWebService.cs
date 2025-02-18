using Amazon.MediaConvert;
using Amazon.MediaConvert.Model;
using Amazon.S3;
using Amazon.S3.Transfer;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Microservices_Interface.AWS;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace BusinessLogicLayer.Implement.Microservices.AWS
{
    public class AmazonWebService(IAmazonS3 s3Client, IAmazonMediaConvert mediaConvert) : IAmazonWebService
    {
        private readonly IAmazonS3 _s3Client = s3Client;
        private readonly IAmazonMediaConvert _mediaConvert = mediaConvert;

        public async Task UploadAudioFileAsync(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                throw new InvalidDataCustomException("File is empty");
            }

            // Kiểm tra bằng content-type (image/webp)
            string fileType = audioFile.ContentType.Split('/').First();
            if (fileType != "audio")
            {
                throw new BadRequestCustomException("Unsupported audioFile type");
            }

            var bucketName = Environment.GetEnvironmentVariable("");
            var key = $"uploads/{Guid.NewGuid()}_{audioFile.FileName}";


            using var stream = audioFile.OpenReadStream();
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                BucketName = bucketName,
                Key = key,
                ContentType = "audio/mpeg"
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            return;
        }

        public async Task<string> CreateHlsJobAsync(string inputFile)
        {
            string bucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME");
            string outputPath = "streaming-audio/";

            var jobRequest = new CreateJobRequest
            {
                Role = Environment.GetEnvironmentVariable("AWS_MediaConvertRole"),
                Settings = new JobSettings
                {
                    Inputs = [new Input { FileInput = inputFile }],
                    OutputGroups =
                [
                    new OutputGroup
                    {
                        Name = "Apple HLS",
                        OutputGroupSettings = new OutputGroupSettings
                        {
                            HlsGroupSettings = new HlsGroupSettings
                            {
                                Destination = $"s3://{bucketName}/{outputPath}"
                            }
                        },
                        Outputs =
                        [
                            new Output
                            {
                                NameModifier = "_hls",
                                ContainerSettings = new ContainerSettings { Container = ContainerType.M3U8 },
                                AudioDescriptions =
                                [
                                    new() {
                                        CodecSettings = new AudioCodecSettings
                                        {
                                            Codec = AudioCodec.AAC
                                        }
                                    }
                                ]
                            }
                        ]
                    }
                ]
                }
            };

            var response = await _mediaConvert.CreateJobAsync(jobRequest);
            return response.Job.Id;
        }

        public async Task<string> UploadAndConvertToStreamingFile(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
                throw new BadRequestCustomException("File is empty");

            // 🟢 Lấy thông tin từ biến môi trường
            string bucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME");
            string mediaConvertRole = Environment.GetEnvironmentVariable("AWS_MediaConvertRole");
            string endpoint = Environment.GetEnvironmentVariable("AWS_MediaConvertEndpoint");
            string queueArn = Environment.GetEnvironmentVariable("AWS_MediaConvertQueue"); // Thêm Queue ARN

            // 🟢 Kiểm tra biến môi trường có null không
            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(mediaConvertRole) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(queueArn))
                throw new Exception("Missing AWS Configuration in Environment Variables");

            // 🟢 Chuẩn bị thông tin file
            string outputFolder = "streaming-audio/";
            string fileName = ObjectId.GenerateNewId() + Path.GetExtension(audioFile.FileName);
            string s3Key = outputFolder + fileName;
            string s3Url = $"s3://{bucketName}/{s3Key}";

            // 🔹 **1. Upload file lên S3**
            using (var stream = audioFile.OpenReadStream())
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = stream,
                    BucketName = bucketName,
                    Key = s3Key,
                    ContentType = "audio/mpeg"
                };
                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            // 🔹 **2. Khởi tạo MediaConvert Client với endpoint**
            var mediaConvertClient = new AmazonMediaConvertClient(new AmazonMediaConvertConfig
            {
                ServiceURL = endpoint
            });

            // 🔹 **3. Gửi Job MediaConvert**
            var createJobRequest = new CreateJobRequest
            {
                Role = mediaConvertRole,
                Queue = queueArn, // 🟢 Thêm Queue để AWS xử lý job nhanh hơn
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
                                    AudioSourceName = "Audio Selector 1", // 🔹 Tham chiếu đến `Audio Selector`
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

            var jobResponse = await mediaConvertClient.CreateJobAsync(createJobRequest);

            Console.WriteLine(jobResponse.ToJson());

            // 🔹 **4. Trả về link file `.m3u8`**
            string m3u8Url = $"https://{bucketName}.s3.amazonaws.com/{outputFolder}{Path.GetFileNameWithoutExtension(fileName)}_hls/{Path.GetFileNameWithoutExtension(fileName)}_hls.m3u8";

            return m3u8Url;
        }



    }
}
