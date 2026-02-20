using System;
using System.Collections.Generic;
using System.Text;

namespace FinanceTracker.Application.DTOs.Auth
{
	public record LoginDTO(string Email, string Password);

	public record RegisterDTO(
		string Email,
		string Password,
		string FirstName,
		string LastName
	);

	public record AuthResponseDTO(
		string Token,
		string RefreshToken,
		string Email,
		string FirstName,
		string LastName
	);

	public record RefreshTokenDTO(string RefreshToken);
}
