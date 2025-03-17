using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.Implement.Services.DataAnalysis;
using BusinessLogicLayer.Interface.Microservices_Interface.AWS;
using BusinessLogicLayer.Interface.Microservices_Interface.Spotify;
using BusinessLogicLayer.Interface.Services_Interface.FFMPEG;
using BusinessLogicLayer.Interface.Services_Interface.Tracks;
using BusinessLogicLayer.ModelView.Service_Model_Views.Artists.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Images.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Request;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using CloudinaryDotNet.Actions;
using CsvHelper;
using CsvHelper.Configuration;
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
using System.Globalization;
using System.Security.Claims;
using Utility.Coding;
using Xabe.FFmpeg;

namespace BusinessLogicLayer.Implement.Services.Tracks
{
    public class TrackBLL(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, ISpotify spotifyService, IAmazonWebService amazonWebService, IFFmpegService fFmpegService, CloudinaryService cloudinaryService) : ITrack
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ISpotify _spotifyService = spotifyService;
        private readonly IAmazonWebService _amazonWebService = amazonWebService;
        private readonly IFFmpegService _fFmpegService = fFmpegService;
        private readonly CloudinaryService _cloudinaryService = cloudinaryService;

        #region Chỉ dùng cho mục đích sửa cdn thành streamingUrl
        //public async Task ChangeStreamUrl()
        //{
        //    string urlPrefix = "https://p.scdn.co/mp3-preview";

        //    // Tạo filter để kiểm tra StreamingUrl khác null
        //    var notNullFilter = Builders<Track>.Filter.Ne(t => t.StreamingUrl, null);

        //    // Tạo filter để kiểm tra StreamingUrl bắt đầu với urlPrefix
        //    var regexFilter = Builders<Track>.Filter.Regex(
        //        t => t.StreamingUrl,
        //        new BsonRegularExpression($"^{Regex.Escape(urlPrefix)}", "i") // "i" để không phân biệt hoa thường
        //    );

        //    // Kết hợp cả hai điều kiện
        //    var filter = Builders<Track>.Filter.And(notNullFilter, regexFilter);

        //    // Lấy tất cả track có streamUrl != null
        //    IList<Track> tracks = await _unitOfWork.GetCollection<Track>()
        //        .Find(filter)
        //        .Project<Track>(Builders<Track>.Projection.Include(t => t.StreamingUrl).Include(t => t.Id).Include(t => t.Name))
        //        .ToListAsync();

        //    // Folder từ ConvertToHls
        //    string inputFileTemp = string.Empty;
        //    string inputFolderPath = string.Empty;
        //    string outputFolderPath = string.Empty;

        //    using HttpClient httpClient = new();

        //    // Danh sách update hàng loạt
        //    List<WriteModel<Track>> updates = [];

        //    // Download file từ streamUrl về local
        //    foreach (Track track in tracks)
        //    {
        //        try
        //        {
        //            string basePath = AppDomain.CurrentDomain.BaseDirectory;

        //            // Lấy tên file từ streamUrl
        //            string fileName = track.Id + ".mp3";

        //            // Tạo đường dẫn file
        //            string filePath = Path.Combine(basePath, "Commons", "input_temp_audio", fileName);

        //            // Tải file về local
        //            using (HttpResponseMessage response = await httpClient.GetAsync(track.StreamingUrl))
        //            {
        //                response.EnsureSuccessStatusCode();
        //                await using (FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        //                {
        //                    await response.Content.CopyToAsync(fileStream);
        //                }
        //            }

        //            // Chuyển file thành IFormFile
        //            IFormFile formFile = ConvertToIFormFile(filePath, fileName);

        //            // Convert audio file sang dạng streaming
        //            (inputFileTemp, inputFolderPath, outputFolderPath) = await _fFmpegService.ConvertToHls(formFile, track.Id);

        //            // Upload streaming files lên AWS S3
        //            string streamingUrl = await _amazonWebService.UploadFolderAsync(outputFolderPath, track.Id, track.Name);

        //            // Upload file lên AWS S3
        //            string publicUrl = await _amazonWebService.UploadFileAsync(formFile, track.Id);

        //            // Tạo update để cập nhật hàng loạt
        //            UpdateOneModel<Track> update = new(
        //                Builders<Track>.Filter.Eq(t => t.Id, track.Id),
        //                Builders<Track>.Update.Set(t => t.StreamingUrl, streamingUrl)
        //            );

        //            updates.Add(update);

        //            // Xóa file sau khi upload để giải phóng bộ nhớ
        //            AudioFile.Delete(filePath);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Lỗi khi xử lý track {track.Id}: {ex.Message}");
        //        }
        //        finally {
        //            if (Directory.Exists(inputFileTemp))
        //            {
        //                Directory.Delete(inputFileTemp, true);
        //                //Console.WriteLine($"Deleted folder: {inputFileTemp}");
        //            }

        //            if (Directory.Exists(inputFolderPath))
        //            {
        //                Directory.Delete(inputFolderPath, true);
        //                //Console.WriteLine($"Deleted folder: {inputFolderPath}");
        //            }

        //            if (Directory.Exists(outputFolderPath))
        //            {
        //                Directory.Delete(outputFolderPath, true);
        //                //Console.WriteLine($"Deleted folder: {outputFolderPath}");
        //            }
        //        }
        //    }

        //    // Cập nhật hàng loạt nếu có dữ liệu
        //    if (updates.Count > 0)
        //    {
        //        await _unitOfWork.GetCollection<Track>().BulkWriteAsync(updates);
        //    }
        //}

        //// Cập nhật hàm ConvertToIFormFile để tránh giữ file mở
        //private static IFormFile ConvertToIFormFile(string filePath, string fileName)
        //{
        //    byte[] fileBytes = AudioFile.ReadAllBytes(filePath); // Đọc file vào bộ nhớ
        //    var memoryStream = new MemoryStream(fileBytes); // Chuyển thành MemoryStream

        //    return new FormFile(memoryStream, 0, fileBytes.Length, "file", fileName)
        //    {
        //        Headers = new HeaderDictionary(),
        //        ContentType = "audio/mpeg" // Cập nhật kiểu MIME nếu cần
        //    };
        //}
        #endregion

        public async Task FetchTracksByCsvAsync(IFormFile csvFile, string accessToken)
        {
            if (csvFile is null || csvFile.Length == 0)
            {
                throw new ArgumentNullCustomException($"{csvFile} is null");
            }

            List<Track> tracks = [];
            List<Artist> newArtists = [];

            CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                Quote = '"',
                BadDataFound = null,
                MissingFieldFound = null
            };

            using StreamReader reader = new(csvFile.OpenReadStream());
            using CsvReader csv = new(reader, csvConfig);

            List<TrackCsvModel> records = csv.GetRecords<TrackCsvModel>().ToList();

            foreach (TrackCsvModel? record in records)
            {
                string trackId = record.TrackId.Trim();

                string? existingTrack = await _unitOfWork.GetCollection<Track>()
                    .Find(m => m.SpotifyId == trackId)
                    .Project(track => track.SpotifyId)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(existingTrack))
                {
                    Console.WriteLine($"Track {trackId} đã tồn tại. Bỏ qua.");
                    continue;
                }

                var (trackImages, artistDictionary, artistImages, artistPopularity, artistFollower) = await _spotifyService.FetchTrackAsync(accessToken, trackId);

                List<string> artistNames = record.ArtistNames.Split(',')
                    .Select(a => a.Trim().Trim('"'))
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToList();

                List<string> artistIds = [];
                foreach (string artistName in artistNames)
                {
                    string existingArtistId = await _unitOfWork.GetCollection<Artist>()
                        .Find(a => a.Name == artistName)
                        .Project(artist => artist.Id)
                        .FirstOrDefaultAsync();

                    if (string.IsNullOrEmpty(existingArtistId))
                    {
                        Artist newArtist = new()
                        {
                            Id = ObjectId.GenerateNewId().ToString(),
                            UserId = null,
                            SpotifyId = artistDictionary[artistName] ?? null,
                            Name = artistName,
                            Followers = artistFollower[artistName],
                            Popularity = artistPopularity[artistName],
                            Images = artistImages[artistName] ?? [],
                            CreatedTime = Util.GetUtcPlus7Time(),
                        };
                        artistIds.Add(newArtist.Id);
                        newArtists.Add(newArtist);
                    }
                    else
                    {
                        artistIds.Add(existingArtistId);
                    }
                }

                AudioFeatures audioFeatures = new()
                {
                    Duration = int.TryParse(record.DurationMs, out int duration) ? duration : 0,
                    Key = int.TryParse(record.Key, out int key) ? key : 0,
                    TimeSignature = int.TryParse(record.TimeSignature, out int timeSignature) ? timeSignature : 0,
                    Mode = int.TryParse(record.Mode, out int mode) ? mode : 0,
                    Acousticness = float.TryParse(record.Acousticness, NumberStyles.Float, CultureInfo.InvariantCulture, out float acousticness) ? acousticness : 0f,
                    Danceability = float.TryParse(record.Danceability, NumberStyles.Float, CultureInfo.InvariantCulture, out float danceability) ? danceability : 0f,
                    Energy = float.TryParse(record.Energy, NumberStyles.Float, CultureInfo.InvariantCulture, out float energy) ? energy : 0f,
                    Instrumentalness = float.TryParse(record.Instrumentalness, NumberStyles.Float, CultureInfo.InvariantCulture, out float instrumentalness) ? instrumentalness : 0f,
                    Liveness = float.TryParse(record.Liveness, NumberStyles.Float, CultureInfo.InvariantCulture, out float liveness) ? liveness : 0f,
                    Loudness = float.TryParse(record.Loudness, NumberStyles.Float, CultureInfo.InvariantCulture, out float loudness) ? loudness : 0f,
                    Speechiness = float.TryParse(record.Speechiness, NumberStyles.Float, CultureInfo.InvariantCulture, out float speechiness) ? speechiness : 0f,
                    Tempo = float.TryParse(record.Tempo, NumberStyles.Float, CultureInfo.InvariantCulture, out float tempo) ? tempo : 0f,
                    Valence = float.TryParse(record.Valence, NumberStyles.Float, CultureInfo.InvariantCulture, out float valence) ? valence : 0f
                };

                Track track = new()
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    SpotifyId = trackId,
                    Name = record.TrackName,
                    Description = null,
                    Lyrics = null,
                    StreamingUrl = null,
                    Duration = int.TryParse(record.DurationMs, out int durationMs) ? durationMs : 0,
                    Images = trackImages,
                    ArtistIds = artistIds,
                    Popularity = int.TryParse(record.Popularity, out int popularity) ? popularity : 0,
                    Restrictions = new()
                    {
                        IsPlayable = true,
                        Reason = RestrictionReason.None,
                        Description = null,
                        RestrictionDate = null,
                    },
                    UploadBy = "672c3adb710b9b46a4fd80e8",
                    UploadDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    StreamCount = 0,
                    DownloadCount = 0,
                    FavoriteCount = 0,
                    AudioFeatures = audioFeatures, // Embed trực tiếp vào Track
                    LastUpdatedTime = null,
                };

                tracks.Add(track);
            }

            if (tracks.Count == 0)
            {
                throw new InvalidDataCustomException("No track data in CSV file");
            }

            if (newArtists.Count == 0)
            {
                throw new InvalidDataCustomException("No artist data in CSV file");
            }

            await _unitOfWork.GetCollection<Artist>().InsertManyAsync(newArtists);
            await _unitOfWork.GetCollection<Track>().InsertManyAsync(tracks);
        }


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
        //            StreamingUrl = track.StreamingUrl,
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

        public async Task<IEnumerable<TrackResponseModel>> GetAllTracksAsync(int offset, int limit, TrackFilterModel filterModel)
        {
            //Xử lý các ký tự đặc biệt trong search term
            string searchTermEscaped = filterModel.SearchTerm != null
                ? Util.EscapeSpecialCharacters(filterModel.SearchTerm)
                : string.Empty;

            // Projection
            ProjectionDefinition<ASTrack, TrackResponseModel> trackWithArtistProjection = Builders<ASTrack>.Projection.Expression(track =>
                new TrackResponseModel
                {
                    Id = track.Id,
                    Name = track.Name,
                    Description = track.Description,
                    Lyrics = track.Lyrics,
                    PreviewURL = track.StreamingUrl,
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
                        Images = artist.Images.Select(image => new ImageResponseModel
                        {
                            URL = image.URL,
                            Height = image.Height,
                            Width = image.Width
                        })
                    })
                });

            //Search
            FilterDefinition<Track> trackFilter = FilterDefinition<Track>.Empty;

            // Lấy tracks có streamingUrl != null và isPlayable = true
            trackFilter = Builders<Track>.Filter.And(
                Builders<Track>.Filter.Ne(t => t.StreamingUrl, null),
                Builders<Track>.Filter.Eq(t => t.Restrictions.IsPlayable, true)
            );

            // Tìm kiếm theo tên hoặc mô tả
            if (!string.IsNullOrEmpty(searchTermEscaped))
            {
                trackFilter = Builders<Track>.Filter.And(
                    trackFilter, // Giữ điều kiện StreamingUrl != null và IsPlayable = true
                        Builders<Track>.Filter.Or(
                        Builders<Track>.Filter.Regex(track => track.Name, new BsonRegularExpression(searchTermEscaped, "i")),
                        Builders<Track>.Filter.Regex(track => track.Description, new BsonRegularExpression(searchTermEscaped, "i"))
                    )
                );
            }
            else if (!string.IsNullOrEmpty(filterModel.Mood.ToString()))
            {
                trackFilter = Builders<Track>.Filter.And(
                    trackFilter, // Giữ điều kiện StreamingUrl != null và IsPlayable = true
                    filterModel.Mood switch
                    {
                        Mood.Sad => Builders<Track>.Filter.And(
                                        Builders<Track>.Filter.Eq(t => t.AudioFeatures.Mode, 0),
                                        Builders<Track>.Filter.Lte(t => t.AudioFeatures.Tempo, 100),
                                        Builders<Track>.Filter.Lte(t => t.AudioFeatures.Valence, 0.4)),

                        Mood.Neutral => Builders<Track>.Filter.And(
                                        Builders<Track>.Filter.Eq(t => t.AudioFeatures.Mode, 1),
                                        Builders<Track>.Filter.Gte(t => t.AudioFeatures.Tempo, 100) &
                                        Builders<Track>.Filter.Lte(t => t.AudioFeatures.Tempo, 120),
                                        Builders<Track>.Filter.Gte(t => t.AudioFeatures.Valence, 0.4) &
                                        Builders<Track>.Filter.Lte(t => t.AudioFeatures.Valence, 0.6)),

                        Mood.Happy => Builders<Track>.Filter.And(
                                        Builders<Track>.Filter.Eq(t => t.AudioFeatures.Mode, 1),
                                        Builders<Track>.Filter.Gte(t => t.AudioFeatures.Tempo, 120) &
                                        Builders<Track>.Filter.Lte(t => t.AudioFeatures.Tempo, 160),
                                        Builders<Track>.Filter.Gte(t => t.AudioFeatures.Valence, 0.6) &
                                        Builders<Track>.Filter.Lte(t => t.AudioFeatures.Valence, 0.8)),

                        Mood.Blisfull => Builders<Track>.Filter.And(
                                        Builders<Track>.Filter.Eq(t => t.AudioFeatures.Mode, 1),
                                        Builders<Track>.Filter.Gte(t => t.AudioFeatures.Tempo, 140) &
                                        Builders<Track>.Filter.Lte(t => t.AudioFeatures.Tempo, 180),
                                        Builders<Track>.Filter.Gte(t => t.AudioFeatures.Valence, 0.8) &
                                        Builders<Track>.Filter.Lte(t => t.AudioFeatures.Valence, 1)),

                        Mood.Focus => Builders<Track>.Filter.And(
                                        Builders<Track>.Filter.Gte(t => t.AudioFeatures.Instrumentalness, 0.7),
                                        Builders<Track>.Filter.Lte(t => t.AudioFeatures.Energy, 0.5)),

                        Mood.Random => Builders<Track>.Filter.Empty,
                        _ => throw new InvalidDataCustomException("The mood is not supported"),
                    }
                );
            }

            // Empty Pipeline
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            //Sorting
            if (filterModel.SortById.HasValue)
            {
                aggregateFluent = filterModel.SortById.Value
                    ? aggregateFluent.Sort(Builders<Track>.Sort.Ascending(track => track.Id))
                    : aggregateFluent.Sort(Builders<Track>.Sort.Descending(track => track.Id));
            }
            else if (filterModel.SortByName.HasValue)
            {
                aggregateFluent = filterModel.SortByName.Value
                    ? aggregateFluent.Sort(Builders<Track>.Sort.Ascending(track => track.Name))
                    : aggregateFluent.Sort(Builders<Track>.Sort.Descending(track => track.Name));
            }

            // Lấy thông tin Tracks với Artist
            // Lookup
            IEnumerable<TrackResponseModel> tracksResponseModel = await aggregateFluent
                .Match(trackFilter)
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
                    PreviewURL = track.StreamingUrl,
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

        #region SearchTracksAsync (Code này đã được gộp vào trong Code GetAllTrack)
        //public async Task<IEnumerable<TrackResponseModel>> SearchTracksAsync(string searchTerm)
        //{
        //    // Xử lý các ký tự đặc biệt
        //    string searchTermEscaped = Util.EscapeSpecialCharacters(searchTerm);

        //    // Empty Pipeline
        //    IAggregateFluent<Track> pipeline = _unitOfWork.GetCollection<Track>().Aggregate();

        //    // Projection
        //    ProjectionDefinition<ASTrack, TrackResponseModel> trackWithArtistProjection = Builders<ASTrack>.Projection.Expression(track =>
        //        new TrackResponseModel
        //        {
        //            Id = track.Id,
        //            Name = track.Name,
        //            Description = track.Description,
        //            Lyrics = track.Lyrics,
        //            PreviewURL = track.StreamingUrl,
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
        //                Images = artist.Images.Select(image => new ImageResponseModel
        //                {
        //                    URL = image.URL,
        //                    Height = image.Height,
        //                    Width = image.Width
        //                })
        //            })
        //        });

        //    // Tạo bộ lọc cho ASTrack riêng biệt sau khi Lookup  
        //    FilterDefinition<ASTrack> trackWithArtistFilter = Builders<ASTrack>.Filter.Or(
        //        Builders<ASTrack>.Filter.Regex(astrack => astrack.Name, new BsonRegularExpression(searchTermEscaped, "i")),
        //        Builders<ASTrack>.Filter.Regex(astrack => astrack.Description, new BsonRegularExpression(searchTermEscaped, "i")),
        //        Builders<ASTrack>.Filter.ElemMatch(track => track.Artists, artist => artist.Name.Contains(searchTermEscaped, StringComparison.CurrentCultureIgnoreCase))
        //    );

        //    // Empty Pipeline
        //    IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

        //    // Lấy thông tin Tracks với Artist
        //    // Lookup
        //    IEnumerable<TrackResponseModel> tracksResponseModel = await aggregateFluent
        //        .Lookup<Track, Artist, ASTrack>(
        //            _unitOfWork.GetCollection<Artist>(),
        //            track => track.ArtistIds,
        //            artist => artist.Id,
        //            result => result.Artists)
        //        .Match(trackWithArtistFilter)
        //        .Project(trackWithArtistProjection)
        //        .ToListAsync();

        //    return tracksResponseModel;
        //}
        #endregion

        public async Task UploadTrackAsync(UploadTrackRequestModel request)
        {
            string userID = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("Your session is limit, you must login again to edit profile!");

            // Lấy artistId
            string? artistId = await _unitOfWork.GetCollection<Artist>().Find(artist => artist.UserId == userID)
                .Project(artist => artist.Id)
                .FirstOrDefaultAsync();

            //map request sang Track
            Track newTrack = _mapper.Map<Track>(request);

            //thêm các thông tin cần thiết cho Track
            newTrack.Id = ObjectId.GenerateNewId().ToString();
            newTrack.ArtistIds = [artistId];
            newTrack.UploadBy = artistId ?? throw new ArgumentNullCustomException($"{artistId}");
            newTrack.UploadDate = DateTime.Now.ToString("yyyy-MM-dd");

            // Tạo hình ảnh cho track
            List<DataAccessLayer.Repository.Entities.Image> images = [];

            // Nếu không có file hình ảnh
            if (request.ImageFile is null)
            {
                images =
                [
                    new() {
                        URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1732779869/default-playlist-640_tsyulf.jpg",
                        Height = 640,
                        Width = 640
                    },
                    new() {
                        URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1732779653/default-playlist-300_iioirq.png",
                        Height = 300,
                        Width = 300
                    },
                    new() {
                        URL = "https://res.cloudinary.com/dofnn7sbx/image/upload/v1732779699/default-playlist-64_gek7wt.png",
                        Height = 64,
                        Width = 64
                    }
                ];
            }
            // Nếu có file hình ảnh
            else
            {
                // Kết quả upload hình ảnh
                ImageUploadResult uploadResult;
                // Kích thước ảnh
                IEnumerable<int> sizes = [640, 300, 64];
                // Kích thước ảnh cố định
                int fixedSize = 300;

                // Upload hình ảnh lên Cloudinary
                uploadResult = _cloudinaryService.UploadImage(request.ImageFile, ImageTag.Track, rootFolder: "Image", fixedSize, fixedSize);

                // Tạo 3 kích thước ảnh khác nhau nhưng cùng một URL với kích thước cố định
                foreach (int size in sizes)
                {
                    images.Add(new()
                    {
                        URL = uploadResult.SecureUrl.AbsoluteUri,
                        Height = size,
                        Width = size
                    });
                }
            }

            

            // Đường dẫn thư mục lưu trữ audio file tạm thời
            string basePath = string.Empty;
            string inputPath = string.Empty;
            string outputPath = string.Empty;

            if (Util.IsWindows())
            {
                basePath = AppDomain.CurrentDomain.BaseDirectory;

                inputPath = Path.Combine(basePath, "Commons", "input_temp_audio", Path.GetFileNameWithoutExtension(request.AudioFile.FileName));
                outputPath = Path.Combine(basePath, "Commons", "output_temp_audio", Path.GetFileNameWithoutExtension(request.AudioFile.FileName));
            }
            else if (Util.IsLinux())
            {
                basePath = "/var/data";
                //basePath = "/tmp";

                inputPath = Path.Combine(basePath, "input_temp_audio", Path.GetFileNameWithoutExtension(request.AudioFile.FileName));
                outputPath = Path.Combine(basePath, "output_temp_audio", Path.GetFileNameWithoutExtension(request.AudioFile.FileName));
            }
            else
            {
                throw new PlatformNotSupportedException("This platform is not supported");
            }

            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(inputPath))
            {
                Directory.CreateDirectory(inputPath);
            }

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            #region Kiểm tra quyền ghi trong môi trường Production (Linux)
            //try
            //{
            //    // Kiểm tra quyền ghi
            //    string testFile = Path.Combine(inputPath, "test.txt");
            //    AudioFile.WriteAllText(testFile, "Test write permission.");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Write test failed: {ex.Message}");
            //    throw new UnauthorizedAccessException("Write permission denied on `/var/data/input_temp_audio`.");
            //}
            #endregion


            // Cấp quyền cho thư mục
            //Syscall.chmod(inputPath, FilePermissions.ALLPERMS);
            //Syscall.chmod(outputPath, FilePermissions.ALLPERMS);

            // Tạo đường dẫn file
            string fileName = Path.GetFileName(request.AudioFile.FileName);
            string inputFilePath = Path.Combine(inputPath, fileName);
            string outputFilePath = Path.Combine(outputPath, fileName);

            // Folder từ ConvertToHls
            string inputFileTemp = string.Empty;
            string inputFolderPath = string.Empty;
            string outputFolderPath = string.Empty;

            try
            {
                // lưu tạm thời file upload vào thư mục input
                using (FileStream stream = new(inputFilePath, FileMode.Create))
                {
                    await request.AudioFile.CopyToAsync(stream);
                }

                // Lấy thông tin file audio
                IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(inputFilePath);

                // lấy tổng thời gian nhạc trên file mp3
                newTrack.Duration = (int)mediaInfo.Duration.TotalMilliseconds;

                // upload lên cloudinary
                //VideoUploadResult result = _cloudinaryService.UploadTrack(outputFile, AudioTagParent.Tracks, AudioTagChild.Preview);
                //newTrack.StreamingUrl = result.SecureUrl.AbsoluteUri;

                // Đặt tên file theo id và name của track
                string trackIdName = $"{newTrack.Id}_{newTrack.Name}";

                // url mp3 public của file audio
                string publicUrl = await _amazonWebService.UploadFileAsync(request.AudioFile, trackIdName);

                // Convert audio file sang dạng streaming
                (inputFileTemp, inputFolderPath, outputFolderPath) = await _fFmpegService.ConvertToHls(request.AudioFile, newTrack.Id);

                // Upload streaming files lên AWS S3
                newTrack.StreamingUrl = await _amazonWebService.UploadFolderAsync(outputFolderPath, newTrack.Id, newTrack.Name);
                //newTrack.StreamingUrl = "";

                // AWS chuyển file audio sang dạng streaming
                //(publicUrl, newTrack.StreamingUrl) = await _amazonWebService.UploadAndConvertToStreamingFile(outputFile, trackIdName);

                //chuyển file audio thành spectrogram và dự đoán audio features
                Bitmap spectrogram = await SpectrogramProcessor.ConvertToSpectrogram(inputFileTemp);

                // Lưu bitmap vào MemoryStream thay vì ổ cứng
                using MemoryStream memoryStream = new();

                // Lưu ở định dạng PNG (hoặc định dạng khác nếu muốn)
                spectrogram.Save(memoryStream, ImageFormat.Png);

                // Giải phóng tài nguyên từ bitmap
                spectrogram.Dispose();

                // Đặt lại vị trí đầu stream để upload
                memoryStream.Position = 0;

                // Khởi tạo IFormFile từ MemoryStream
                IFormFile spectrogramFile = new FormFile(memoryStream, 0, memoryStream.Length, "spectrogram", $"{newTrack.Id}.png")
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "image/png"
                };

                //upload spectrogram lên cloudinary
                //ImageUploadResult imageResult = _cloudinaryService.UploadImage(spectrogramFile, ImageTag.Spectrogram);

                // Chuyển spectrogram thành tensor
                //Tensor<float> tensor = await SpectrogramProcessor.ProcessImageToTensor(imageResult.SecureUrl.AbsoluteUri);
                Tensor<float> tensor = await SpectrogramProcessor.ProcessImageToTensor(spectrogramFile);

                // Dự đoán audio features từ tensor
                Dictionary<string, float> spectroPredict = SpectrogramProcessor.Predict(tensor);

                // Tạo mới audio features từ spectroPredict
                AudioFeatures audioFeature = new()
                {
                    Acousticness = spectroPredict["dense_1"],
                    Danceability = spectroPredict["dense_2"],
                    Energy = spectroPredict["dense_3"],
                    Instrumentalness = spectroPredict["dense_4"],
                    Key = (int)Math.Round(spectroPredict["dense_5"], 2),
                    Liveness = spectroPredict["dense_6"],
                    Loudness = spectroPredict["dense_7"],
                    Mode = (spectroPredict["dense_8"] >= 0.5 ? 1 : 0), // 0 hoặc 1
                    Speechiness = spectroPredict["dense_9"],
                    Tempo = spectroPredict["dense_10"],
                    TimeSignature = (int)Math.Round(spectroPredict["dense_11"], 2),
                    Valence = spectroPredict["dense_12"]
                };


                //await _unitOfWork.GetCollection<AudioFeatures>().InsertOneAsync(audioFeature);

                newTrack.AudioFeatures = audioFeature;

                //lưu track và audio features vào database
                await _unitOfWork.GetCollection<Track>().InsertOneAsync(newTrack);
                return;
            }
            //catch (Exception ex)
            //{
            //    throw new Exception("Upload failed: " + ex.Message);
            //}

            finally
            {
                //xóa các file tạm không cần nữa trong wwwroot
                //AudioFile.Delete(inputFilePath);
                //AudioFile.Delete(outputPath);

                // Xóa folder tạm, đảm bảo file không bị lock trước khi xóa
                try
                {
                    if (Directory.Exists(inputPath))
                    {
                        Directory.Delete(inputPath, true); // Tham số 'true' để xóa cả thư mục con và file bên trong
                        //Console.WriteLine($"Deleted folder: {inputPath}");
                    }

                    if (Directory.Exists(outputPath))
                    {
                        Directory.Delete(outputPath, true);
                        //Console.WriteLine($"Deleted folder: {outputPath}");
                    }

                    if (Directory.Exists(inputFileTemp))
                    {
                        Directory.Delete(inputFileTemp, true);
                        //Console.WriteLine($"Deleted folder: {inputFileTemp}");
                    }

                    if (Directory.Exists(inputFolderPath))
                    {
                        Directory.Delete(inputFolderPath, true);
                        //Console.WriteLine($"Deleted folder: {inputFolderPath}");
                    }

                    if (Directory.Exists(outputFolderPath))
                    {
                        Directory.Delete(outputFolderPath, true);
                        //Console.WriteLine($"Deleted folder: {outputFolderPath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting folders: {ex.Message}");
                }
            }
        }

        public async Task<IEnumerable<TrackResponseModel>> GetTracksByMoodAsync(Mood mood)
        {
            // Filter trực tiếp trên Track.AudioFeatures
            FilterDefinition<Track> trackFilter;
            trackFilter = mood switch
            {
                Mood.Sad => Builders<Track>.Filter.And(
                                Builders<Track>.Filter.Eq(t => t.AudioFeatures.Mode, 0),
                                Builders<Track>.Filter.Lte(t => t.AudioFeatures.Tempo, 100),
                                Builders<Track>.Filter.Lte(t => t.AudioFeatures.Valence, 0.4)),

                Mood.Neutral => Builders<Track>.Filter.And(
                                Builders<Track>.Filter.Eq(t => t.AudioFeatures.Mode, 1),
                                Builders<Track>.Filter.Gte(t => t.AudioFeatures.Tempo, 100) &
                                Builders<Track>.Filter.Lte(t => t.AudioFeatures.Tempo, 120),
                                Builders<Track>.Filter.Gte(t => t.AudioFeatures.Valence, 0.4) &
                                Builders<Track>.Filter.Lte(t => t.AudioFeatures.Valence, 0.6)),

                Mood.Happy => Builders<Track>.Filter.And(
                                Builders<Track>.Filter.Eq(t => t.AudioFeatures.Mode, 1),
                                Builders<Track>.Filter.Gte(t => t.AudioFeatures.Tempo, 120) &
                                Builders<Track>.Filter.Lte(t => t.AudioFeatures.Tempo, 160),
                                Builders<Track>.Filter.Gte(t => t.AudioFeatures.Valence, 0.6) &
                                Builders<Track>.Filter.Lte(t => t.AudioFeatures.Valence, 0.8)),

                Mood.Blisfull => Builders<Track>.Filter.And(
                                Builders<Track>.Filter.Eq(t => t.AudioFeatures.Mode, 1),
                                Builders<Track>.Filter.Gte(t => t.AudioFeatures.Tempo, 140) &
                                Builders<Track>.Filter.Lte(t => t.AudioFeatures.Tempo, 180),
                                Builders<Track>.Filter.Gte(t => t.AudioFeatures.Valence, 0.8) &
                                Builders<Track>.Filter.Lte(t => t.AudioFeatures.Valence, 1)),

                Mood.Focus => Builders<Track>.Filter.And(
                                Builders<Track>.Filter.Gte(t => t.AudioFeatures.Instrumentalness, 0.7),
                                Builders<Track>.Filter.Lte(t => t.AudioFeatures.Energy, 0.5)),

                Mood.Random => Builders<Track>.Filter.Empty,
                _ => throw new InvalidDataCustomException("The mood is not supported"),
            };

            // Projection
            ProjectionDefinition<ASTrack, TrackResponseModel> projectionDefinition = Builders<ASTrack>.Projection.Expression(track =>
                new TrackResponseModel
                {
                    Id = track.Id,
                    Name = track.Name,
                    Description = track.Description,
                    PreviewURL = track.StreamingUrl,
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
                        Images = artist.Images.Select(image => new ImageResponseModel
                        {
                            URL = image.URL,
                            Height = image.Height,
                            Width = image.Width
                        })
                    })
                });

            // Thực hiện aggregate
            IAggregateFluent<Track> aggregateFluent = _unitOfWork.GetCollection<Track>().Aggregate();

            IEnumerable<TrackResponseModel> tracks = await aggregateFluent
                .Match(trackFilter) // Lọc trực tiếp trên Track
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
