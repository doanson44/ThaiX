using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ThaiX.Client.Models.Ai;
using ThaiX.Client.Models.ApiClients;
using ThaiX.Client.Models.AssetPositions;
using ThaiX.Client.Models.Auth;
using ThaiX.Client.Models.Blog;
using ThaiX.Client.Models.Contacts;
using ThaiX.Client.Models.CredentialAccounts;
using ThaiX.Client.Models.ExpenseTracker;
using ThaiX.Client.Models.ExternalData;
using ThaiX.Client.Models.JsonBins;
using ThaiX.Client.Models.Lottery;
using ThaiX.Client.Models.MasterData;
using ThaiX.Client.Models.MarketScanner;
using ThaiX.Client.Models.Notes;
using ThaiX.Client.Models.Notifications;
using ThaiX.Client.Models.Portfolios;
using ThaiX.Client.Models.PriceAlerts;
using ThaiX.Client.Models.Resumes;
using ThaiX.Client.Models.Slack;
using ThaiX.Client.Models.Trading;
using ThaiX.Client.Models.Users;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Notifications;


namespace ThaiX.Client.Services.Http.Demo;

public sealed partial class DemoResponseFactory
{
    private HttpResponseMessage HandleAuth(string method, string path)
    {
        if (method == "POST" && path.Equals("api/auth/login", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new LoginResponse
            {
                Token = _adminToken,
                TokenType = "Bearer",
                ExpiresIn = 3600 * 24 * 365,
                RequiresTwoFactor = false
            });
        }

        if (method == "POST" && path.Equals("api/auth/logout", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.Success();
        }

        if (method == "POST" && path.Equals("api/auth/register", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new RegisterResponse
            {
                UserId = Guid.NewGuid(),
                Email = "new.user@local",
                RequiresEmailConfirmation = false
            });
        }

        if (method == "POST" && path.Equals("api/auth/change-password", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.Success();
        }

        if (method == "GET" && path.Equals("api/auth/user", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new UserInfoResponse
            {
                UserId = Guid.Parse(DemoJwtFactory.DemoUserId),
                Email = DemoJwtFactory.DemoEmail,
                EmailConfirmed = true,
                Permissions = DemoSessionStore.AllPermissions.ToList()
            });
        }

        return method is "POST" or "PUT" or "DELETE"
            ? DemoEnvelope.Success()
            : DemoEnvelope.SuccessData(new { demo = true });
    }

    private HttpResponseMessage HandleAccount(string method, string path)
    {
        if (method == "POST" &&
            (path.Equals("api/account/login-2fa", StringComparison.OrdinalIgnoreCase) ||
             path.Equals("api/account/login-recovery", StringComparison.OrdinalIgnoreCase)))
        {
            return DemoEnvelope.SuccessData(new LoginResponse
            {
                Token = _adminToken,
                TokenType = "Bearer",
                ExpiresIn = 3600 * 24 * 365,
                RequiresTwoFactor = false
            });
        }

        if (path.Equals("api/account/2fa-status", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new TwoFactorStatusResponse
            {
                Is2faEnabled = false,
                HasAuthenticator = false,
                RecoveryCodesLeft = 0
            });
        }

        if (method == "POST" && path.Equals("api/account/enable-authenticator", StringComparison.OrdinalIgnoreCase))
        {
            const string sharedKey = "JBSWY3DPEHPK3PXP";
            return DemoEnvelope.SuccessData(new EnableAuthenticatorResponse
            {
                SharedKey = sharedKey,
                AuthenticatorUri =
                    $"otpauth://totp/ThaiX:{Uri.EscapeDataString(DemoJwtFactory.DemoEmail)}?secret={sharedKey}&issuer=ThaiX&digits=6"
            });
        }

        if (method == "POST" && path.Equals("api/account/verify-authenticator", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new VerifyAuthenticatorResponse
            {
                RecoveryCodes = DemoRecoveryCodes()
            });
        }

        if (method == "POST" && path.Equals("api/account/generate-recovery-codes", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new GenerateRecoveryCodesResponse
            {
                RecoveryCodes = DemoRecoveryCodes()
            });
        }

        if (path.Equals("api/account/personal-data", StringComparison.OrdinalIgnoreCase))
        {
            if (method == "DELETE")
            {
                return DemoEnvelope.Success();
            }

            return DemoEnvelope.SuccessData(new Dictionary<string, string>
            {
                ["Id"] = DemoJwtFactory.DemoUserId,
                ["Email"] = DemoJwtFactory.DemoEmail,
                ["EmailConfirmed"] = "true",
                ["PhoneNumber"] = "",
                ["TwoFactorEnabled"] = "false"
            });
        }

        if (path.Equals("api/account/external-logins", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new ExternalLoginsResponse
            {
                ExternalLogins =
                [
                    new ExternalLoginDto
                    {
                        LoginProvider = "Google",
                        ProviderKey = "demo-google-key",
                        ProviderDisplayName = "Google"
                    }
                ],
                HasPassword = true
            });
        }

        if (method == "DELETE" && path.Equals("api/account/external-login", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.Success();
        }

        // Always fail so VisualTests can exercise NotifyError toast on the resend page.
        if (method == "POST" && path.Equals("api/account/resend-confirmation", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.Failure(
                "DEMO_RESEND_CONFIRMATION",
                "Demo mode cannot resend confirmation email.");
        }

        if (method == "GET")
        {
            return DemoEnvelope.SuccessData(new { demo = true, path });
        }

        return DemoEnvelope.Success();
    }

    private static string[] DemoRecoveryCodes() =>
    [
        "DEMO-AAAA-1111", "DEMO-BBBB-2222", "DEMO-CCCC-3333",
        "DEMO-DDDD-4444", "DEMO-EEEE-5555", "DEMO-FFFF-6666",
        "DEMO-GGGG-7777", "DEMO-HHHH-8888", "DEMO-IIII-9999", "DEMO-JJJJ-0000"
    ];

}
