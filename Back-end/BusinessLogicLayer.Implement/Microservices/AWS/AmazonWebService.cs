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

        public async Task<string> UploadFileAsync(IFormFile audioFile, string trackIdName)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                throw new BadRequestCustomException("AudioFile is empty");
            }

            // Lấy thông tin từ settings
            string bucketName = _aWSSettings.BucketName;
            string region = _aWSSettings.Region;
            if (string.IsNullOrEmpty(bucketName))
            {
                throw new Exception("Missing AWS Configuration in Environment Variables");
            }

            // Chuẩn bị thông tin file
            string outputFolder = "streaming-audio/";
            string fileName = trackIdName + Path.GetExtension(audioFile.FileName);
            string s3Key = outputFolder + fileName;

            // Kiểm tra xem file đã tồn tại trên S3 chưa (Tránh upload lại)
            try
            {
                await _s3Client.GetObjectMetadataAsync(bucketName, s3Key);
                Console.WriteLine($"AudioFile {s3Key} đã tồn tại trên S3, không cần upload lại.");
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // AudioFile chưa tồn tại → Thực hiện upload
                using var stream = audioFile.OpenReadStream();
                TransferUtilityUploadRequest uploadRequest = new()
                {
                    InputStream = stream,
                    BucketName = bucketName,
                    Key = s3Key,
                    ContentType = "audio/mpeg"
                };
                using TransferUtility fileTransferUtility = new(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            // URL public cho file gốc
            string s3UrlPublic = $"https://{bucketName}.s3.{region}.amazonaws.com/{s3Key}";
            
            return s3UrlPublic;
        }

        public async Task<string> UploadFolderAsync(string localFolderPath, string trackId, string trackName)
        {
            // Đặt tên file theo id và name của track
            string trackIdName = $"{trackId}_{trackName}";

            try
            {
                ArgumentNullException.ThrowIfNull(localFolderPath);

                if (!Directory.Exists(localFolderPath))
                {
                    throw new DirectoryNotFoundException($"The folder does not exist: {localFolderPath}");
                }

                // Sử dụng TransferUtility để upload cả folder
                TransferUtility transferUtility = new(_s3Client);

                await transferUtility.UploadDirectoryAsync(new TransferUtilityUploadDirectoryRequest
                {
                    BucketName = _aWSSettings.BucketName,
                    Directory = localFolderPath,
                    KeyPrefix = $"streaming-audio/{trackIdName}", // Định dạng folder trên S3
                    SearchOption = SearchOption.AllDirectories, // Upload cả file trong sub-folder nếu có
                  //CannedACL = S3CannedACL.PublicRead // (Tùy chọn) Set quyền public nếu cần
                });
            }
            finally
            {
                // Xóa thư mục output/trackIdName sau khi upload
                if (Directory.Exists(localFolderPath))
                {
                    Directory.Delete(localFolderPath, true);
                }
            }

            return $"https://{_aWSSettings.BucketName}.s3.{_aWSSettings.Region}.amazonaws.com/streaming-audio/{trackIdName}/{trackId}_output.m3u8";
        }

        #region Tiêu tiền cao cấp
        //public async Task<(string, string)> UploadAndConvertToStreamingFile(IFormFile audioFile, string trackIdName)
        //{
        //    if (audioFile == null || audioFile.Length == 0)
        //    {
        //        throw new BadRequestCustomException("AudioFile is empty");
        //    }

        //    // Lấy thông tin từ settings
        //    string bucketName = _aWSSettings.BucketName;
        //    string region = _aWSSettings.Region;
        //    string mediaConvertRole = _aWSSettings.MediaConvertRole;
        //    string endpoint = _aWSSettings.MediaConvertEndpoint;
        //    string queueArn = _aWSSettings.MediaConvertQueue; // Thêm Queue ARN

        //    // Kiểm tra biến môi trường có null không
        //    if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(mediaConvertRole) || string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(queueArn))
        //        throw new Exception("Missing AWS Configuration in Environment Variables");

        //    // Chuẩn bị thông tin file
        //    string outputFolder = "streaming-audio/";
        //    string fileName = trackIdName + Path.GetExtension(audioFile.FileName);
        //    string s3Key = outputFolder + fileName;
        //    string s3Url = $"s3://{bucketName}/{s3Key}";

        //    // Upload file lên S3
        //    using (var stream = audioFile.OpenReadStream())
        //    {
        //        TransferUtilityUploadRequest uploadRequest = new()
        //        {
        //            InputStream = stream,
        //            BucketName = bucketName,
        //            Key = s3Key,
        //            ContentType = "audio/mpeg"
        //        };
        //        TransferUtility fileTransferUtility = new(_s3Client);
        //        await fileTransferUtility.UploadAsync(uploadRequest);
        //    }

        //    // Tạo URL đúng chuẩn AWS S3 Public
        //    string s3UrlPublic = $"https://{bucketName}.s3.{region}.amazonaws.com/{s3Key}";

        //    // Khởi tạo MediaConvert Client với endpoint
        //    AmazonMediaConvertClient mediaConvertClient = new(new AmazonMediaConvertConfig
        //    {
        //        ServiceURL = endpoint
        //    });

        //    // Gửi Job MediaConvert
        //    CreateJobRequest createJobRequest = new()
        //    {
        //        Role = mediaConvertRole,
        //        Queue = queueArn, // Thêm Queue để AWS xử lý job nhanh hơn
        //        Settings = new JobSettings
        //        {
        //            Inputs =
        //            [
        //                new Input
        //                {
        //                    FileInput = s3Url,
        //                    AudioSelectors = new Dictionary<string, AudioSelector>
        //                    {
        //                        { "Audio Selector 1", new AudioSelector { DefaultSelection = AudioDefaultSelection.DEFAULT } }
        //                    }
        //                }
        //            ],
        //            OutputGroups =
        //            [
        //                new OutputGroup
        //                {
        //                    Name = "HLS",
        //                    OutputGroupSettings = new OutputGroupSettings
        //                    {
        //                        Type = OutputGroupType.HLS_GROUP_SETTINGS,
        //                        HlsGroupSettings = new HlsGroupSettings
        //                        {
        //                            Destination = $"s3://{bucketName}/{outputFolder}{Path.GetFileNameWithoutExtension(fileName)}_hls/",
        //                            MinSegmentLength = 0,
        //                            SegmentLength = 10 // Độ dài mỗi segment HLS
        //                        }
        //                    },
        //                    Outputs =
        //                    [
        //                        new Output
        //                        {
        //                            NameModifier = "_hls",
        //                            ContainerSettings = new ContainerSettings
        //                            {
        //                                Container = ContainerType.M3U8
        //                            },
        //                            AudioDescriptions =
        //                            [
        //                                new AudioDescription
        //                                {
        //                                    AudioSourceName = "Audio Selector 1", // Tham chiếu đến Audio Selector
        //                                    CodecSettings = new AudioCodecSettings
        //                                    {
        //                                        Codec = AudioCodec.AAC,
        //                                        AacSettings = new AacSettings
        //                                        {
        //                                            CodingMode = AacCodingMode.CODING_MODE_2_0,
        //                                            Bitrate = 128000,
        //                                            SampleRate = 48000
        //                                        }
        //                                    }
        //                                }
        //                            ],
        //                            OutputSettings = new OutputSettings
        //                            {
        //                                HlsSettings = new HlsSettings
        //                                {
        //                                    SegmentModifier = "_hls"
        //                                }
        //                            }
        //                        }
        //                    ]
        //                }
        //            ]
        //        }
        //    };

        //    // Tạo Job
        //    CreateJobResponse jobResponse = await mediaConvertClient.CreateJobAsync(createJobRequest);

        //    //Console.WriteLine(jobResponse.ToJson());

        //    // Trả về link file .m3u8
        //    string m3u8Url = $"https://{bucketName}.s3.{region}.amazonaws.com/{outputFolder}{Path.GetFileNameWithoutExtension(fileName)}_hls/{Path.GetFileNameWithoutExtension(fileName)}_hls.m3u8";

        //    return (s3UrlPublic, m3u8Url);
        //}
        #endregion

        #region Tối ưu tiền
        public async Task<(string, string)> UploadAndConvertToStreamingFile(IFormFile audioFile, string trackIdName)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                throw new BadRequestCustomException("AudioFile is empty");
            }

            // Lấy thông tin từ settings
            string bucketName = _aWSSettings.BucketName;
            string region = _aWSSettings.Region;
            string mediaConvertRole = _aWSSettings.MediaConvertRole;
            string endpoint = _aWSSettings.MediaConvertEndpoint;
            string queueArn = _aWSSettings.MediaConvertQueue;

            if (string.IsNullOrEmpty(bucketName) || string.IsNullOrEmpty(mediaConvertRole) ||
                string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(queueArn))
            {
                throw new Exception("Missing AWS Configuration in Environment Variables");
            }

            // Chuẩn bị thông tin file
            string outputFolder = "streaming-audio/";
            string fileName = trackIdName + Path.GetExtension(audioFile.FileName);
            string s3Key = outputFolder + fileName;
            string s3Url = $"s3://{bucketName}/{s3Key}";

            // Kiểm tra xem file đã tồn tại trên S3 chưa (Tránh upload lại)
            try
            {
                await _s3Client.GetObjectMetadataAsync(bucketName, s3Key);
                Console.WriteLine($"AudioFile {s3Key} đã tồn tại trên S3, không cần upload lại.");
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // AudioFile chưa tồn tại → Thực hiện upload
                using var stream = audioFile.OpenReadStream();
                TransferUtilityUploadRequest uploadRequest = new()
                {
                    InputStream = stream,
                    BucketName = bucketName,
                    Key = s3Key,
                    ContentType = "audio/mpeg"
                };
                using TransferUtility fileTransferUtility = new(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            // URL public cho file gốc
            string s3UrlPublic = $"https://{bucketName}.s3.{region}.amazonaws.com/{s3Key}";

            // Kiểm tra xem job MediaConvert đã chạy cho file này chưa (Tránh chạy lại)
            using AmazonMediaConvertClient mediaConvertClient = new(new AmazonMediaConvertConfig { ServiceURL = endpoint });
            ListJobsRequest jobCheckRequest = new()
            {
                MaxResults = 10,
                Order = Order.DESCENDING,
                Queue = queueArn,
                Status = JobStatus.SUBMITTED
            };

            var jobCheckResponse = await mediaConvertClient.ListJobsAsync(jobCheckRequest);
            if (jobCheckResponse.Jobs.Any(j => j.Settings.Inputs.Any(i => i.FileInput == s3Url)))
            {
                Console.WriteLine($"MediaConvert job đã tồn tại cho file {fileName}, không cần tạo lại.");
                return (s3UrlPublic, ""); // Không tạo job mới nếu job đã chạy
            }

            // Tạo MediaConvert Job (Tối ưu để chỉ tạo 1 bitrate)
            CreateJobRequest createJobRequest = new()
            {
                Role = mediaConvertRole,
                Queue = queueArn,
                AccelerationSettings = new AccelerationSettings
                {
                    Mode = AccelerationMode.DISABLED // Chắc chắn không sử dụng Professional Tier
                },
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
                                    SegmentLength = 10,
                                    SegmentControl = HlsSegmentControl.SINGLE_FILE, // Gộp nhiều segment thành 1 file .ts
                                    ManifestCompression = HlsManifestCompression.NONE, // Không nén manifest
                                    ManifestDurationFormat = HlsManifestDurationFormat.FLOATING_POINT, // Giữ thời gian chuẩn
                                    OutputSelection = HlsOutputSelection.SEGMENTS_ONLY, // Chỉ tạo file .ts, không tạo playlist dư thừa
                                    ProgramDateTime = HlsProgramDateTime.INCLUDE, // (Tùy chọn) giúp debug thời gian chuẩn
                                }
                            },
                            Outputs =
                            [
                                new Output
                                {
                                    NameModifier = "_hls",
                                    ContainerSettings = new ContainerSettings
                                    {
                                        Container = ContainerType.M3U8,
                                        M3u8Settings = new M3u8Settings
                                        {
                                            AudioFramesPerPes = 4, // Gộp nhiều frame vào .ts để giảm file nhỏ lẻ
                                            PcrControl = M3u8PcrControl.PCR_EVERY_PES_PACKET // Đảm bảo thời gian chính xác trong .ts
                                        }
                                    },
                                    AudioDescriptions =
                                    [
                                        new AudioDescription
                                        {
                                            AudioSourceName = "Audio Selector 1",
                                            CodecSettings = new AudioCodecSettings
                                            {
                                                Codec = AudioCodec.AAC,
                                                AacSettings = new AacSettings
                                                {
                                                    CodingMode = AacCodingMode.CODING_MODE_2_0,
                                                    Bitrate = 128000, // Chỉ tạo 1 bitrate thay vì nhiều bitrate
                                                    SampleRate = 48000
                                                }
                                            }
                                        }
                                    ],
                                    OutputSettings = new OutputSettings { HlsSettings = new HlsSettings { SegmentModifier = "_hls" } }
                                }
                            ]
                        }
                    ]
                }
            };

            // Gửi Job MediaConvert
            CreateJobResponse jobResponse = await mediaConvertClient.CreateJobAsync(createJobRequest);
            //Console.WriteLine($"MediaConvert job {jobResponse.Job.Id} đã được tạo.");

            // Trả về link file .m3u8
            string m3u8Url = $"https://{bucketName}.s3.{region}.amazonaws.com/{outputFolder}{Path.GetFileNameWithoutExtension(fileName)}_hls/{Path.GetFileNameWithoutExtension(fileName)}_hls.m3u8";

            return (s3UrlPublic, m3u8Url);
        }
        #endregion
    }
}
