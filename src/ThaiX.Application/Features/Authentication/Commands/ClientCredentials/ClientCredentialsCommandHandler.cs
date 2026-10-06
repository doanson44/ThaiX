using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Authentication.Commands.Login;

namespace ThaiX.Application.Features.Authentication.Commands.ClientCredentials;

/// <summary>
/// Handler for ClientCredentialsCommand.
/// Authenticates API client and generates JWT token for M2M communication.
/// </summary>
public sealed class ClientCredentialsCommandHandler : IRequestHandler<ClientCredentialsCommand, TokenDto>
{
    private readonly IAuthenticationService _authenticationService;

    public ClientCredentialsCommandHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task<TokenDto> Handle(ClientCredentialsCommand request, CancellationToken cancellationToken)
    {
        var tokenResult = await _authenticationService.AuthenticateClientAsync(
            request.ClientId,
            request.ClientSecret,
            request.Scope,
            cancellationToken);

        return new TokenDto
        {
            AccessToken = tokenResult.AccessToken,
            TokenType = tokenResult.TokenType,
            ExpiresIn = tokenResult.ExpiresIn,
            Scope = tokenResult.Scope
        };
    }
}
