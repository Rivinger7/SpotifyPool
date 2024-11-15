using BusinessLogicLayer.Implement.CustomExceptions;
using BusinessLogicLayer.Implement.Microservices.Cloudinaries;
using BusinessLogicLayer.Interface.Services_Interface.Playlists.Own;
using BusinessLogicLayer.ModelView.Service_Model_Views.Playlists.Request;
using CloudinaryDotNet.Actions;
using DataAccessLayer.Interface.MongoDB.UOW;
using DataAccessLayer.Repository.Entities;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using SetupLayer.Enum.Microservices.Cloudinary;
using SetupLayer.Enum.Services.Playlist;
using Utility.Coding;

namespace BusinessLogicLayer.Implement.Services.Playlists.Own
{
	public class OwnPlaylistBLL(IHttpContextAccessor contextAccessor, IUnitOfWork unitOfWork, CloudinaryService cloudinaryService) : IOwnPlaylist 
	{

		private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
		private readonly IUnitOfWork _unitOfWork = unitOfWork;
		private readonly CloudinaryService _cloudinaryService = cloudinaryService;

		public async Task AddNewPlaylistAsync()
		{
			string userID = _contextAccessor.HttpContext.Session.GetString("UserID");
			Playlist playlist = new()
			{
				Name = PlaylistName.NewPlaylist,
				Description = string.Empty,
				UserID = userID,
				TrackIds = null,
				Images = null
			};
			await _unitOfWork.GetRepository<Playlist>().AddAsync(playlist);
		}

		public async Task DeletePlaylistAsync(string playlistID)
		{
			Playlist playlist = await _unitOfWork.GetRepository<Playlist>().GetByIdAsync(playlistID)
								?? throw new DataNotFoundCustomException("This playlist is not found");
			await _unitOfWork.GetRepository<Playlist>().DeleteAsync(playlistID);
		}

		public Task GetAllPlaylistAsync()
		{
			throw new NotImplementedException();
		}

		public async Task PinPlaylistAsync(string playlistID)
		{
			Playlist playlist = await _unitOfWork.GetRepository<Playlist>().GetByIdAsync(playlistID)
								?? throw new DataNotFoundCustomException("Playlist not found!");

			UpdateDefinition<Playlist> update = Builders<Playlist>.Update.Set(playlist => playlist.IsPinned, true);
			await _unitOfWork.GetRepository<Playlist>().UpdateAsync(playlistID, update);
		}

		public async Task UnpinPlaylistAsync(string playlistID)
		{
			Playlist playlist = await _unitOfWork.GetRepository<Playlist>().GetByIdAsync(playlistID)
							    ?? throw new DataNotFoundCustomException("Playlist not found!");

			UpdateDefinition<Playlist> update = Builders<Playlist>.Update.Set(playlist => playlist.IsPinned, false);
			await _unitOfWork.GetRepository<Playlist>().UpdateAsync(playlistID, update);
		}

		public async Task UpdatePlaylistAsync(string playlistID, UpdatePlaylistRequestModel request)
		{
			//FE sẽ bắt lỗi chỗ này, bắt buộc phải có playlist Name

			UpdateDefinition<Playlist> updateDefinition = Builders<Playlist>.Update
				.Set(playlist => playlist.Name, (PlaylistName)Enum.Parse(typeof(PlaylistName), request.Name))
				.Set(playlist => playlist.Description, request.Description);
			//if (request.Image is not null)
			//{
			//	ImageUploadResult upload = _cloudinaryService.UploadImage(request.Image, ImageTag.Playlist);
			//	Image image = new()
			//	{
			//		URL = upload.SecureUrl.ToString(),
			//		Height = upload.Height,
			//		Width = upload.Width
			//	};
			//	updateDefinition.Set(playlist => playlist.Images[0], image);
			//}

			await _unitOfWork.GetRepository<Playlist>().UpdateAsync(playlistID, updateDefinition);
		}
	}
}
