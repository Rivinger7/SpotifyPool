﻿using BusinessLogicLayer.Interface.Services_Interface.Dashboard;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SetupLayer.Enum.Services.User;

namespace SpotifyPool._1._Controllers.Dashboard
{
	[Route("api/v1/dashboard")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //"Bearer"
	[Authorize(Roles = nameof(UserRole.Admin))]
	public class DashboardController(IDashboard dashboardBLL) : ControllerBase
	{
		private readonly IDashboard _dashboardBLL = dashboardBLL;

		/// <summary>
		/// Dữ liệu tổng quan hệ thống
		/// </summary>
		/// <returns></returns>
		[HttpGet("overview")]
		public async Task<IActionResult> GetSystemOverview()
		{
			var data = await _dashboardBLL.GetSystemOverviewAsync();
			return Ok(data);
		}

		/// <summary>
		/// Dữ liệu quản lý bài hát và nghệ sĩ
		/// </summary>
		/// <returns></returns>
		[HttpGet("track-artist")]
		public async Task<IActionResult> GetTrackArtistManagement()
		{
			var data = await _dashboardBLL.GetTrackArtistManagementAsync();
			return Ok(data);
		}
	}
}
