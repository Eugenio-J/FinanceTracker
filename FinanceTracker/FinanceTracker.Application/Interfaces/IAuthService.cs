using FinanceTracker.Application.DTOs.Auth;
using FinanceTracker.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.Interfaces
{
	public interface IAuthService
	{
		Task<Result<AuthResponseDTO>> LoginAsync(LoginDTO dto);
		Task<Result<AuthResponseDTO>> RegisterAsync(RegisterDTO dto);
		Task<Result<AuthResponseDTO>> RefreshTokenAsync(string refreshToken);
	}
}
