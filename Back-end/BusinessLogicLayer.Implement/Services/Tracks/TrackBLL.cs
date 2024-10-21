using AutoMapper;
using BusinessLogicLayer.Interface.Services_Interface.Tracks;
using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Aggregate_Storage;
using DataAccessLayer.Repository.Entities;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Search;

namespace BusinessLogicLayer.Implement.Services.Tracks
{
    // CLASS NÀY VẪN ĐANG TEST
    public class TrackBLL(IUnitOfWork unitOfWork, IMapper mapper) : ITrack
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<IEnumerable<TrackResponseModel>> SearchTracksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return [];
            }

            SearchFuzzyOptions fuzzyOptions = new()
            {
                MaxEdits = 1,
                PrefixLength = 1,
                MaxExpansions = 256,
            };

            TextSearchOptions searchOptions = new()
            {
                CaseSensitive = false,
                DiacriticSensitive = false,
            };

            //IEnumerable<Track> tracks = await _unitOfWork.GetCollection<Track>().Aggregate().Search(Builders<Track>.Search.Autocomplete(track => track.Name, searchTerm, fuzzy: options), indexName: "name").ToListAsync();

            var indexKeysDefinition = Builders<Track>.IndexKeys.Text(t => t.Name);
            await _unitOfWork.GetCollection<Track>().Indexes.CreateOneAsync(new CreateIndexModel<Track>(indexKeysDefinition));

            IAggregateFluent<Track> pipeline = _unitOfWork.GetCollection<Track>().Aggregate();

            FilterDefinition<Track> filter = Builders<Track>.Filter.Text(searchTerm, searchOptions);

            IAggregateFluent<ASTrack> trackPipelines = pipeline
                .Match(filter)
                .Lookup<Track, Artist, ASTrack>
                (_unitOfWork.GetCollection<Artist>(),
                track => track.ArtistIds,
                artist => artist.SpotifyId,
                result => result.Artists)
                .As<ASTrack>();

            IEnumerable<Track> tracks = await trackPipelines.ToListAsync();

            IEnumerable<TrackResponseModel> responseModels = _mapper.Map<IEnumerable<TrackResponseModel>>(tracks);

            return responseModels;
        }

    }
}
