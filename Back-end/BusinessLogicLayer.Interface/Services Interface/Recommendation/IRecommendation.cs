﻿using BusinessLogicLayer.ModelView.Service_Model_Views.Tracks.Response;
using DataAccessLayer.Repository.Entities;

namespace BusinessLogicLayer.Interface.Services_Interface.Recommendation
{
    public interface IRecommendation
    {
        Task<IEnumerable<TrackResponseModel>> GetRecommendations(string trackId, Func<AudioFeatures, AudioFeatures, double> similarityScore, int k = 5);
    }
}
