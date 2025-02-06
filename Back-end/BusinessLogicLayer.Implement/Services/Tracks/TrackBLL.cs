using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.Implement.Microservices.NAudio;
using BusinessLogicLayer.Implement.Services.DataAnalysis;
using BusinessLogicLayer.Interface.Services_Interface.Tracks;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using CloudinaryDotNet.Actions;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.ML.OnnxRuntime.Tensors;
using MongoDB.Bson;
using MongoDB.Driver;
using SetupLayer.Enum.Microservices.Cloudinary;
using SetupLayer.Enum.Services.Track;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Claims;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Tracks
{
    public class TrackBLL(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, CloudinaryService cloudinaryService) : ITrack
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly CloudinaryService _cloudinaryService = cloudinaryService;

        #region Chỉ dùng cho mục đích test và sửa lỗi artist null
        //public async Task<IEnumerable<TrackResponseModel>> GetTracksWithArtistIsNull()
        //{
        //    // Projection
        //    ProjectionDefinition<ASTrack, TrackResponseModel> trackWithArtistProjection = Builders<ASTrack>.Projection.Expression(track =>
        //        new TrackResponseModel
        //        {
        //            Id = track.Id,
        //            Name = track.Name,
        //            Description = track.Description,
        //            Lyrics = track.Lyrics,
        //            PreviewURL = track.PreviewURL,
        //            Duration = track.Duration,
        //            Images = track.Images.Select(image => new ImageResponseModel
        //            {
        //                URL = image.URL,
        //                Height = image.Height,
        //                Width = image.Width
        //            }),
        //            Artists = track.Artists.Select(artist => new ArtistResponseModel
        //            {
        //                Id = artist.Id,
        //                Name = artist.Name,
        //                Followers = artist.Followers,
        //                GenreIds = artist.GenreIds,
        //                Images = artist.Images.Select(image => new ImageResponseModel
        //                {
        //                    URL = image.URL,
        //                    Height = image.Height,
        //                    Width = image.Width
        //                })
        //            })
        //        });

        //    // Empty Pipeline
        //    IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

        //    // Lấy thông tin Tracks với Artist
        //    // Lookup
        //    IEnumerable<TrackResponseModel> tracksResponseModel = await aggregateFluent
        //        //.Match(track => track.Id == "6734cb13b92ba642dd3c9059" ||
        //        //track.Name == "Black Out")
        //        .Lookup<Track, Artist, ASTrack>(
        //            _unitOfWork.GetCollection<Artist>(),
        //            track => track.ArtistIds,
        //            artist => artist.Id,
        //            result => result.Artists)
        //        .Match(tracks => !tracks.Artists.Any())
        //        .Project(trackWithArtistProjection)
        //        .ToListAsync();

        //    //if (tracksResponseModel.First().Artists.First().Name is null)
        //    //{
        //    //    Console.WriteLine("Nulllllllllllllllllllllllllllllllllll");
        //    //}
        //    //if (tracksResponseModel.First().Artists.First().Name == "")
        //    //{
        //    //    Console.WriteLine("Emptyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy");
        //    //}

        //    Console.WriteLine($"Counting: {tracksResponseModel.Count()}");

        //    return tracksResponseModel;
        //}
        #endregion

        public async Task<IEnumerable<TrackResponseModel>> GetAllTracksAsync(int offset, int limit)
        {
            // Projection
            ProjectionDefinition<ASTrack, TrackResponseModel> trackWithArtistProjection = Builders<ASTrack>.Projection.Expression(track =>
                new TrackResponseModel
                {
                    Id = track.Id,
                    Name = track.Name,
                    Description = track.Description,
                    Lyrics = track.Lyrics,
                    PreviewURL = track.PreviewURL,
                    Duration = track.Duration,
                    Images = track.Images.Select(image => new ImageResponseModel
                    {
                        URL = image.URL,
                        Height = image.Height,
                        Width = image.Width
                    }),
                    Artists = track.Artists.Select(artist => new ArtistResponseModel
                    {
                        Id = artist.Id,
                        Name = artist.Name,
                        Followers = artist.Followers,
                        GenreIds = artist.GenreIds,
                        Images = artist.Images.Select(image => new ImageResponseModel
                        {
                            URL = image.URL,
                            Height = image.Height,
                            Width = image.Width
                        })
                    })
                });

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thông tin Tracks với Artist
            // Lookup
            IEnumerable<TrackResponseModel> tracksResponseModel = await aggregateFluent
                .Skip((offset - 1) * limit)
                .Limit(limit)
                .Lookup<Track, Artist, ASTrack>(
                    _unitOfWork.GetCollection<Artist>(),
                    track => track.ArtistIds,
                    artist => artist.Id,
                    result => result.Artists)
                .Project(trackWithArtistProjection)
                .ToListAsync();

            return tracksResponseModel;
        }

        public async Task<TrackResponseModel> GetTrackAsync(string id)
        {
            // Projection
            ProjectionDefinition<ASTrack, TrackResponseModel> trackWithArtistProjection = Builders<ASTrack>.Projection.Expression(track =>
                new TrackResponseModel
                {
                    Id = track.Id,
                    Name = track.Name,
                    Description = track.Description,
                    Lyrics = track.Lyrics,
                    PreviewURL = track.PreviewURL,
                    Duration = track.Duration,
                    // Lý do không dùng được vì Expression không hỗ trợ các hàm extension
                    // DurationFormated = Util.FormatTimeFromMilliseconds(track.Duration),
                    Images = track.Images.Select(image => new ImageResponseModel
                    {
                        URL = image.URL,
                        Height = image.Height,
                        Width = image.Width
                    }),
                    Artists = track.Artists.Select(artist => new ArtistResponseModel
                    {
                        Id = artist.Id,
                        Name = artist.Name,
                        Followers = artist.Followers,
                        GenreIds = artist.GenreIds,
                        Images = artist.Images.Select(image => new ImageResponseModel
                        {
                            URL = image.URL,
                            Height = image.Height,
                            Width = image.Width
                        })
                    })
                });

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thông tin Tracks với Artist
            // Lookup
            TrackResponseModel trackResponseModel = await aggregateFluent
                .Match(track => track.Id == id)
                .Lookup<Track, Artist, ASTrack>(
                    _unitOfWork.GetCollection<Artist>(),
                    track => track.ArtistIds,
                    artist => artist.Id,
                    result => result.Artists)
                .Project(trackWithArtistProjection)
                .FirstOrDefaultAsync();

            return trackResponseModel;
        }

        public async Task<IEnumerable<TrackResponseModel>> SearchTracksAsync(string searchTerm)
        {
            // Xử lý các ký tự đặc biệt
            string searchTermEscaped = Util.EscapeSpecialCharacters(searchTerm);

            // Empty Pipeline
            IAggregateFluent<Track> pipeline = _unitOfWork.GetCollection<Track>().Aggregate();

            // Projection
            ProjectionDefinition<ASTrack, TrackResponseModel> trackWithArtistProjection = Builders<ASTrack>.Projection.Expression(track =>
                new TrackResponseModel
                {
                    Id = track.Id,
                    Name = track.Name,
                    Description = track.Description,
                    Lyrics = track.Lyrics,
                    PreviewURL = track.PreviewURL,
                    Duration = track.Duration,
                    Images = track.Images.Select(image => new ImageResponseModel
                    {
                        URL = image.URL,
                        Height = image.Height,
                        Width = image.Width
                    }),
                    Artists = track.Artists.Select(artist => new ArtistResponseModel
                    {
                        Id = artist.Id,
                        Name = artist.Name,
                        Followers = artist.Followers,
                        GenreIds = artist.GenreIds,
                        Images = artist.Images.Select(image => new ImageResponseModel
                        {
                            URL = image.URL,
                            Height = image.Height,
                            Width = image.Width
                        })
                    })
                });

            // Tạo bộ lọc cho ASTrack riêng biệt sau khi Lookup  
            FilterDefinition<ASTrack> trackWithArtistFilter = Builders<ASTrack>.Filter.Or(
                Builders<ASTrack>.Filter.Regex(astrack => astrack.Name, new BsonRegularExpression(searchTermEscaped, "i")),
                Builders<ASTrack>.Filter.Regex(astrack => astrack.Description, new BsonRegularExpression(searchTermEscaped, "i")),
                Builders<ASTrack>.Filter.ElemMatch(track => track.Artists, artist => artist.Name.Contains(searchTermEscaped, StringComparison.CurrentCultureIgnoreCase))
            );

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lấy thông tin Tracks với Artist
            // Lookup
            IEnumerable<TrackResponseModel> tracksResponseModel = await aggregateFluent
                .Lookup<Track, Artist, ASTrack>(
                    _unitOfWork.GetCollection<Artist>(),
                    track => track.ArtistIds,
                    artist => artist.Id,
                    result => result.Artists)
                .Match(trackWithArtistFilter)
                .Project(trackWithArtistProjection)
                .ToListAsync();

            return tracksResponseModel;
        }

        public async Task UploadTrackAsync(UploadTrackRequestModel request)
        {
            string userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");

            // Lấy artistId
            string? artistId = await _unitOfWork.GetCollection<Artist>().Find(artist => artist.UserId == userID)
                .Project(artist => artist.UserId)
                .FirstOrDefaultAsync();

            //map request sang Track
            Track newTrack = _mapper.Map<Track>(request);

            //thêm các thông tin cần thiết cho Track
            newTrack.IsExplicit = false;
            newTrack.ArtistIds = [artistId];
            newTrack.UploadBy = artistId ?? throw new ArgumentNullCustomException($"{artistId}");
            newTrack.UploadDate = DateTime.Now.ToString("yyyy-MM-dd");

            //string fileNameUnique = $"{Path.GetFileNameWithoutExtension(request.File.FileName)}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}{Path.GetExtension(request.File.FileName)}";
            //Console.WriteLine(fileNameUnique);

            //lấy đường dẫn tuyệt đối của file upload - đã tạo sẵn thư mục temp_uploads trong wwwroot
            string inputPath = Path.Combine(Directory.GetCurrentDirectory(), "AudioTemp", "input", request.File.FileName);

            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "AudioTemp", "output", request.File.FileName);

            try
            {
                // lưu tạm thời file upload vào thư mục input
                using (var stream = new FileStream(inputPath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                //cắt vid từ giây 0:00 đến giây thứ 30
                NAudioService.TrimAudioFile(out int duration, inputPath, outputPath, TimeSpan.FromSeconds(30));

                newTrack.Duration = duration;

                //lấy file audio đã cắt từ folder output rồi chuyển nó sang dạng IFormFile, tận dụng hàm UploadTrack của CloudinaryService
                using (var outputStream = new FileStream(outputPath, FileMode.Open))
                {
                    IFormFile outputFile = new FormFile(outputStream, 0, outputStream.Length, "preview_audio", Path.GetFileName(outputPath))
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "audio/wav"
                    };

                    // upload lên cloudinary
                    VideoUploadResult result = _cloudinaryService.UploadTrack(outputFile, AudioTagParent.Tracks, AudioTagChild.Preview);
                    newTrack.PreviewURL = result.SecureUrl.AbsoluteUri;

                    //chuyển file audio thành spectrogram và dự đoán audio features
                    Bitmap spectrogram = SpectrogramProcessor.ConvertToSpectrogram(newTrack.PreviewURL);

                    // Lưu bitmap vào MemoryStream thay vì ổ cứng
                    using MemoryStream memoryStream = new();

                    // Lưu ở định dạng PNG (hoặc định dạng khác nếu muốn)
                    spectrogram.Save(memoryStream, ImageFormat.Png);

                    // Giải phóng tài nguyên từ bitmap
                    spectrogram.Dispose();

                    // Đặt lại vị trí đầu stream để upload
                    memoryStream.Position = 0;

                    // Khởi tạo audio features id
                    string audioFeaturesId = ObjectId.GenerateNewId().ToString();

                    // Khởi tạo IFormFile từ MemoryStream
                    IFormFile spectrogramFile = new FormFile(memoryStream, 0, memoryStream.Length, "spectrogram", $"{audioFeaturesId}.png")
                    {
                        Headers = new HeaderDictionary(),
                        ContentType = "image/png"
                    };

                    //upload spectrogram lên cloudinary
                    ImageUploadResult imageResult = _cloudinaryService.UploadImage(spectrogramFile, ImageTag.Spectrogram);

                    // Chuyển spectrogram thành tensor
                    Tensor<float> tensor = await SpectrogramProcessor.ProcessImageToTensor(imageResult.SecureUrl.AbsoluteUri);

                    // Dự đoán audio features từ tensor
                    float[] spectroPredict = SpectrogramProcessor.Predict(tensor);

                    ////tạo mới audio features từ spectroPredict 
                    //AudioFeatures audioFeature = new()
                    //{
                    //    Id = audioFeaturesId,
                    //    Duration = (int)Math.Round(spectroPredict[0], 2),
                    //    Key = (int)Math.Round(spectroPredict[1], 2),
                    //    TimeSignature = (int)Math.Round(spectroPredict[2], 2),
                    //    Mode = (int)Math.Round(spectroPredict[3], 2),
                    //    Acousticness = spectroPredict[4],
                    //    Danceability = spectroPredict[5],
                    //    Energy = spectroPredict[6],
                    //    Instrumentalness = spectroPredict[7],
                    //    Liveness = spectroPredict[8],
                    //    Loudness = spectroPredict[9],
                    //    Speechiness = spectroPredict[10],
                    //    Tempo = spectroPredict[11],
                    //    Valence = spectroPredict[12]
                    //};

                    // Tạo mới audio features từ spectroPredict
                    AudioFeatures audioFeature = new()
                    {
                        Id = audioFeaturesId,
                        Acousticness = spectroPredict[0],
                        Danceability = spectroPredict[1],
                        Energy = spectroPredict[2],
                        Instrumentalness = spectroPredict[3],
                        Key = (int)Math.Round(spectroPredict[4], 2),
                        Liveness = spectroPredict[5],
                        Loudness = spectroPredict[6],
                        Mode = (int)Math.Round(spectroPredict[7], 2),
                        Speechiness = spectroPredict[8],
                        Tempo = spectroPredict[9],
                        TimeSignature = (int)Math.Round(spectroPredict[10], 2),
                        Valence = spectroPredict[11]
                    };


                    await _unitOfWork.GetCollection<AudioFeatures>().InsertOneAsync(audioFeature);

                    newTrack.AudioFeaturesId = audioFeature.Id;

                    //lưu track và audio features vào database
                    await _unitOfWork.GetCollection<Track>().InsertOneAsync(newTrack);

                }
                return;
            }
            //catch (Exception ex)
            //{
            //    throw new Exception("Upload failed: " + ex.Message);
            //}

            finally
            {
                //xóa các file tạm không cần nữa trong wwwroot
                File.Delete(inputPath);
                File.Delete(outputPath);
            }
        }

        public async Task<IEnumerable<TrackResponseModel>> GetTracksByMoodAsync(Mood mood)
        {
            // Filter
            FilterDefinition<AudioFeatures> audioFeaturesFilter;
            audioFeaturesFilter = mood switch
            {
                Mood.Sad => Builders<AudioFeatures>.Filter.And(
                                      Builders<AudioFeatures>.Filter.Eq(af => af.Mode, 0),
                                      Builders<AudioFeatures>.Filter.Lte(af => af.Tempo, 100),
                                      Builders<AudioFeatures>.Filter.Lte(af => af.Valence, 0.4)),
                Mood.Neutral => Builders<AudioFeatures>.Filter.And(
                                        Builders<AudioFeatures>.Filter.Eq(af => af.Mode, 1),
                                        Builders<AudioFeatures>.Filter.Gte(af => af.Tempo, 100) &
                                        Builders<AudioFeatures>.Filter.Lte(af => af.Tempo, 120),
                                        Builders<AudioFeatures>.Filter.Gte(af => af.Valence, 0.4) &
                                        Builders<AudioFeatures>.Filter.Lte(af => af.Valence, 0.6)),
                Mood.Happy => Builders<AudioFeatures>.Filter.And(
                                        Builders<AudioFeatures>.Filter.Eq(af => af.Mode, 1),
                                        Builders<AudioFeatures>.Filter.Gte(af => af.Tempo, 120) &
                                        Builders<AudioFeatures>.Filter.Lte(af => af.Tempo, 160),
                                        Builders<AudioFeatures>.Filter.Gte(af => af.Valence, 0.6) &
                                        Builders<AudioFeatures>.Filter.Lte(af => af.Valence, 0.8)),
                Mood.Blisfull => Builders<AudioFeatures>.Filter.And(
                                        Builders<AudioFeatures>.Filter.Eq(af => af.Mode, 1),
                                        Builders<AudioFeatures>.Filter.Gte(af => af.Tempo, 140) &
                                        Builders<AudioFeatures>.Filter.Lte(af => af.Tempo, 180),
                                        Builders<AudioFeatures>.Filter.Gte(af => af.Valence, 0.8) &
                                        Builders<AudioFeatures>.Filter.Lte(af => af.Valence, 1)),
                Mood.Focus => Builders<AudioFeatures>.Filter.And(
                                        Builders<AudioFeatures>.Filter.Gte(af => af.Instrumentalness, 0.7),
                                        Builders<AudioFeatures>.Filter.Lte(af => af.Energy, 0.5)),
                Mood.Random => Builders<AudioFeatures>.Filter.Empty,
                _ => throw new InvalidDataCustomException("The mood is not supported"),
            };

            // Mapping tracks to TrackResponseModel
            ProjectionDefinition<ASTrack, TrackResponseModel> projectionDefinition = Builders<ASTrack>.Projection.Expression(track =>
                new TrackResponseModel
                {
                    Id = track.Id,
                    Name = track.Name,
                    Description = track.Description,
                    PreviewURL = track.PreviewURL,
                    Duration = track.Duration,
                    Images = track.Images.Select(image => new ImageResponseModel()
                    {
                        URL = image.URL,
                        Height = image.Height,
                        Width = image.Width
                    }),
                    Artists = track.Artists.Select(artist => new ArtistResponseModel
                    {
                        Id = artist.Id,
                        Name = artist.Name,
                        Followers = artist.Followers,
                        GenreIds = artist.GenreIds,
                        Images = artist.Images.Select(image => new ImageResponseModel
                        {
                            URL = image.URL,
                            Height = image.Height,
                            Width = image.Width
                        })
                    })
                });

            // Lấy AudioFeaturesId
            IEnumerable<string> audioFeaturesIds = await _unitOfWork.GetCollection<AudioFeatures>()
                .Find(audioFeaturesFilter)
                .Project(af => af.Id)
                .ToListAsync();

            // Stage
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            // Lookup
            IEnumerable<TrackResponseModel> tracks = await aggregateFluent
                .Match(track => audioFeaturesIds.Contains(track.AudioFeaturesId))
                .Sample(7)
                .Lookup<Track, Artist, ASTrack>
                (
                    _unitOfWork.GetCollection<Artist>(),
                    track => track.ArtistIds,
                    artist => artist.Id,
                    result => result.Artists
                )
                .Project(projectionDefinition)
                .ToListAsync();

            // Lấy thời gian hiện tại
            DateTime currentTime = Util.GetUtcPlus7Time();

            return tracks;
        }
    }
}
