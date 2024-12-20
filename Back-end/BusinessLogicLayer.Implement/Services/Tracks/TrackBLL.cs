﻿using AutoMapper;
using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Interface.Services_Interface.Tracks;
using BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack;
using BusinessLogicLayer.ModelView.Service_Model_Views.TopTrack.Response;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Security.Claims;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Tracks
{
    public class TrackBLL(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor) : ITrack
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public async Task<IEnumerable<TrackResponseModel>> GetAllTracksAsync(int offset, int limit)
        {
            // Lấy tất cả các track với artist
            IEnumerable<ASTrack> tracks = await _unitOfWork.GetRepository<ASTrack>().GetAllTracksWithArtistAsync(offset, limit);

            // Map the aggregate result to TrackResponseModel
            IEnumerable<TrackResponseModel> responseModel = _mapper.Map<IEnumerable<TrackResponseModel>>(tracks);

            return responseModel;
        }



		public async Task<TrackResponseModel> GetTrackAsync(string id)
        {
            // Lấy track với artist
            ASTrack track = await _unitOfWork.GetRepository<ASTrack>().GetTrackWithArtistAsync(id);

            // Map the aggregate result to TrackResponseModel
            TrackResponseModel responseModel = _mapper.Map<TrackResponseModel>(track);

            return responseModel;
        }

		public async Task<IEnumerable<TrackResponseModel>> SearchTracksAsync(string searchTerm)
        {
            // Nếu không có searchTerm thì trả về mảng rỗng  
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return [];
            }

            // Xử lý các ký tự đặc biệt
            string searchTermEscaped = Util.EscapeSpecialCharacters(searchTerm);

            // Empty Pipeline
            IAggregateFluent<Track> pipeline = _unitOfWork.GetCollection<Track>().Aggregate();

            // Chỉ lấy những field cần thiết  
            ProjectionDefinition<ASTrack> projectionDefinition = Builders<ASTrack>.Projection
                .Include(track => track.Id)
                .Include(track => track.Name)
                .Include(track => track.Description)
                .Include(track => track.PreviewURL)
                .Include(track => track.Duration)
                .Include(track => track.Images)
                .Include(track => track.Artists);

            // Tạo bộ lọc cho ASTrack riêng biệt sau khi Lookup  
            FilterDefinition<ASTrack> artistFilter = Builders<ASTrack>.Filter.Or(
                Builders<ASTrack>.Filter.Regex(astrack => astrack.Name, new BsonRegularExpression(searchTermEscaped, "i")),
                Builders<ASTrack>.Filter.Regex(astrack => astrack.Description, new BsonRegularExpression(searchTermEscaped, "i")),
                Builders<ASTrack>.Filter.ElemMatch(track => track.Artists, artist => artist.Name.Contains(searchTermEscaped, StringComparison.CurrentCultureIgnoreCase))
            );

            // Lookup from Artist collection to Track collection  
            IAggregateFluent<ASTrack> trackPipelines = pipeline.Lookup<Track, Artist, ASTrack> // Stage 1  
                (_unitOfWork.GetCollection<Artist>(),
                track => track.ArtistIds,
                artist => artist.Id,
                result => result.Artists)
                .Match(artistFilter)  // Tìm kiếm theo Artist hoặc TrackName // Stage 2  
                .Project(projectionDefinition)
                .As<ASTrack>();

            // To list  
            IEnumerable<Track> tracks = await trackPipelines.ToListAsync();

            // Mapping to response model  
            IEnumerable<TrackResponseModel> responseModels = _mapper.Map<IEnumerable<TrackResponseModel>>(tracks);

            return responseModels;
        }		
        
        public async Task<TopTrackResponseModel?> GetTopTracksAsync()
		{
            string? userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                                ?? throw new DataNotFoundCustomException("Your session is limit. Please login again.");
            

            //join TopTrack với Track, aggregate lại dạng list ASTopTrack để tí sửa trong list này, ko để IEnumerable
            List<ASTopTrack> topTracksWithTracks = await _unitOfWork.GetRepository<TopTrack>().Collection
                .Aggregate()
                .Match(topTrack => topTrack.UserId == userId)
                .Lookup<TopTrack, Track, ASTopTrack>(
                    foreignCollection: _unitOfWork.GetRepository<Track>().Collection,
                    localField: topTrack => topTrack.TrackInfo.Select(info => info.TrackId),
                    foreignField: track => track.Id,
                    @as: result => result.Tracks
                )
            .ToListAsync();

        

            // gộp thông tin từ list trên vào ASTopTrack mới
            TopTrackResponseModel? enrichedResult = topTracksWithTracks
            .Select(topTrack => new TopTrackResponseModel
            { 
                TopTrackId = topTrack.TopTrackId,
                UserId = topTrack.UserId,
                TrackInfo = topTrack.TrackInfo.Select(info => new TracksInfoResponse
                {
                    //lấy mấy cái cần thiết liên quan đến Track rồi gán vào TrackInfo
                    TrackId = info.TrackId,
                    StreamCount = info.StreamCount,
                    FirstAccessTime = info.FirstAccessTime,
                    Track = _mapper.Map<TrackInTopTrackResponseModel>(topTrack.Tracks.FirstOrDefault(t => t.Id == info.TrackId)),
                    Artists = topTrack.Tracks.Where(t => t.Id == info.TrackId) //lấy track từ topTrack.Tracks có Id trùng với info.TrackId
                                    //lấy artist từ các track trong TrackInfo và duy nhất
                                    .SelectMany(track => track.ArtistIds)
                                    .Distinct()
                                    // lấy artist có artistId tương ứng đã select ở bên trên
                                    .Select(artistId => _unitOfWork.GetRepository<Artist>().Collection
                                        .Find(artist => artist.Id == artistId)
                                        .Project(artist => artist.Name)
                                        .FirstOrDefault()
                                    ) 
                                    .ToList() 
                }).OrderByDescending(info => info.StreamCount).Skip((1-1)*50).Take(50).ToList()
            }).FirstOrDefault();

            return enrichedResult;
		}
    }
}
