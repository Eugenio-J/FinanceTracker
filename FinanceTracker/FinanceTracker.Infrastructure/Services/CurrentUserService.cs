using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using FinanceTracker.Application.Interfaces;

namespace FinanceTracker.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public CurrentUserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public int? UserId
	{
		get
		{
			var userIdClaim = _httpContextAccessor.HttpContext?.User
				.FindFirstValue(ClaimTypes.NameIdentifier);

			return int.TryParse(userIdClaim, out var userId) ? userId : null;
		}
	}

	public bool IsAuthenticated => UserId.HasValue;
}