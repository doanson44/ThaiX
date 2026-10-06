namespace ThaiX.Client.Constants;

/// <summary>
/// Centralized localization resource key constants for the Blazor client.
/// Use these instead of hardcoded string literals with IStringLocalizer.
/// </summary>
public static class ResourceKeys
{
    public static class App
    {
        public const string Title = "App.Title";
    }

    public static class Language
    {
        public const string English = "Language.English";
        public const string Vietnamese = "Language.Vietnamese";
        public const string Toggle = "Language.Toggle";
    }

    public static class Theme
    {
        public const string Dark = "Theme.Dark";
        public const string Light = "Theme.Light";
        public const string Toggle = "Theme.Toggle";
    }

    public static class Sidebar
    {
        public const string Settings = "Sidebar.Settings";
    }

    public static class Menu
    {
        public const string Home = "Menu.Home";
        public const string Profile = "Menu.Profile";
        public const string ChangePassword = "Menu.ChangePassword";
        public const string TwoFactor = "Menu.TwoFactor";
        public const string PersonalData = "Menu.PersonalData";
        public const string Users = "Menu.Users";
        public const string Contacts = "Menu.Contacts";
        public const string Logout = "Menu.Logout";
        public const string Login = "Menu.Login";
        public const string Register = "Menu.Register";
        public const string MasterData = "Menu.MasterData";
        public const string MasterDataCountries = "Menu.MasterData.Countries";
        public const string MasterDataCities = "Menu.MasterData.Cities";
        public const string MasterDataDistricts = "Menu.MasterData.Districts";
        public const string MasterDataBanks = "Menu.MasterData.Banks";
        public const string MarketData = "Menu.MarketData";
        public const string MarketResearch = "Menu.MarketResearch";
        public const string ChainBroker = "Menu.ChainBroker";
        public const string Crm = "Menu.CRM";
        public const string PortfolioGroup = "Menu.Portfolio";
        public const string NotificationCenter = "Menu.NotificationCenter";
        public const string Security = "Menu.Security";
        public const string Administration = "Menu.Administration";
        public const string CommandTester = "Menu.CommandTester";
        public const string NotificationSender = "Menu.NotificationSender";
        public const string AiTester = "Menu.AiTester";
        public const string Portfolios = "Menu.Portfolios";
        public const string ExpenseTracker = "Menu.ExpenseTracker";
        public const string ApiClients = "Menu.ApiClients";
        public const string Notes = "Menu.Notes";
        public const string NotificationSchedules = "Menu.NotificationSchedules";
        public const string CredentialAccounts = "Menu.CredentialAccounts";
        public const string Resume = "Menu.Resume";
        public const string PublicResume = "Menu.PublicResume";
        public const string Blog = "Menu.Blog";
        public const string BlogPosts = "Menu.BlogPosts";
        public const string BlogCategories = "Menu.BlogCategories";
        public const string BlogTags = "Menu.BlogTags";
        public const string JsonBins = "Menu.JsonBins";
        public const string Tools = "Menu.Tools";
    }

    public static class HomePage
    {
        public const string Welcome = "Home.Welcome";
        public const string Description = "Home.Description";
        public const string AuthenticatedAs = "Home.AuthenticatedAs";
        public const string NotAuthenticated = "Home.NotAuthenticated";
    }

    public static class Breadcrumb
    {
        /// <summary>Label for the Account segment in breadcrumb (e.g. /account/2fa).</summary>
        public const string Account = "Breadcrumb.Account";
        /// <summary>Label for the ChainBroker segment in breadcrumb (e.g. /market/chainbroker/...).</summary>
        public const string ChainBroker = "Breadcrumb.ChainBroker";
        /// <summary>Label for the Admin segment in breadcrumb (e.g. /admin/...).</summary>
        public const string Admin = "Breadcrumb.Admin";
        public const string JsonBinCreate = "Breadcrumb.JsonBinCreate";
        public const string JsonBinEdit = "Breadcrumb.JsonBinEdit";
    }

    public static class NotFound
    {
        public const string Title = "NotFound.Title";
        public const string Description = "NotFound.Description";
    }

    public static class Auth
    {
        public const string Login = "Auth.Login";
        public const string LoginTitle = "Auth.Login.Title";
        public const string LoginLink = "Auth.Login.Link";
        public const string LoginLinkGoToLogin = "Auth.Login.Link.GoToLogin";
        public const string LoginSuccess = "Auth.Login.Success";
        public const string LoginSuccessDetail = "Auth.Login.Success.Detail";
        public const string LoginFailed = "Auth.Login.Failed";

        public const string UsernameOrEmail = "Auth.UsernameOrEmail";
        public const string UsernameOrEmailPlaceholder = "Auth.UsernameOrEmail.Placeholder";
        public const string Password = "Auth.Password";
        public const string PasswordPlaceholder = "Auth.Password.Placeholder";
        public const string RememberMe = "Auth.RememberMe";
        public const string Email = "Auth.Email";
        public const string EmailPlaceholder = "Auth.Email.Placeholder";
        public const string ConfirmPassword = "Auth.ConfirmPassword";
        public const string ConfirmPasswordPlaceholder = "Auth.ConfirmPassword.Placeholder";

        public const string Register = "Auth.Register";
        public const string RegisterTitle = "Auth.Register.Title";
        public const string RegisterDescription = "Auth.Register.Description";
        public const string RegisterLink = "Auth.Register.Link";
        public const string RegisterSuccess = "Auth.Register.Success";
        public const string RegisterConfirmEmailDetail = "Auth.Register.ConfirmEmail.Detail";
        public const string RegisterSuccessDetail = "Auth.Register.Success.Detail";
        public const string RegisterFailed = "Auth.Register.Failed";

        public const string ForgotPasswordLink = "Auth.ForgotPassword.Link";
        public const string ResendConfirmationLink = "Auth.ResendConfirmation.Link";
        public const string ExternalLoginOr = "Auth.ExternalLogin.Or";
        public const string ExternalLoginGoogle = "Auth.ExternalLogin.Google";

        public const string LogoutSuccess = "Auth.Logout.Success";
        public const string LogoutSuccessDetail = "Auth.Logout.Success.Detail";
        public const string LogoutWarning = "Auth.Logout.Warning";

        public const string AccessDeniedTitle = "Auth.AccessDenied.Title";
        public const string AccessDeniedDescription = "Auth.AccessDenied.Description";
    }

    public static class Health
    {
        public const string PageTitle = "Health.PageTitle";
        public const string Title = "Health.Title";
        public const string Subtitle = "Health.Subtitle";
        public const string Frontend = "Health.Frontend";
        public const string ApiViaProxy = "Health.ApiViaProxy";
        public const string ApiDirect = "Health.ApiDirect";
        public const string StatusOk = "Health.Status.Ok";
        public const string StatusUnknown = "Health.Status.Unknown";
        public const string StatusError = "Health.Status.Error";
    }

    public static class Account
    {
        public const string ForgotPasswordTitle = "Account.ForgotPassword.Title";
        public const string ForgotPasswordDescription = "Account.ForgotPassword.Description";
        public const string ForgotPasswordEmailSent = "Account.ForgotPassword.EmailSent";
        public const string ForgotPasswordSubmit = "Account.ForgotPassword.Submit";
        public const string ForgotPasswordFailed = "Account.ForgotPassword.Failed";

        public const string ResetPasswordTitle = "Account.ResetPassword.Title";
        public const string ResetPasswordInvalidLink = "Account.ResetPassword.InvalidLink";
        public const string ResetPasswordSuccess = "Account.ResetPassword.Success";
        public const string ResetPasswordSubmit = "Account.ResetPassword.Submit";
        public const string ResetPasswordFailed = "Account.ResetPassword.Failed";

        public const string ConfirmEmailTitle = "Account.ConfirmEmail.Title";
        public const string ConfirmEmailProcessing = "Account.ConfirmEmail.Processing";
        public const string ConfirmEmailSuccess = "Account.ConfirmEmail.Success";
        public const string ConfirmEmailInvalidLink = "Account.ConfirmEmail.InvalidLink";

        public const string ResendConfirmationTitle = "Account.ResendConfirmation.Title";
        public const string ResendConfirmationDescription = "Account.ResendConfirmation.Description";
        public const string ResendConfirmationSubmit = "Account.ResendConfirmation.Submit";
        public const string ResendConfirmationSuccess = "Account.ResendConfirmation.Success";
        public const string ResendConfirmationFailed = "Account.ResendConfirmation.Failed";

        public const string LockoutTitle = "Account.Lockout.Title";
        public const string LockoutDescription = "Account.Lockout.Description";

        public const string LoginWith2faTitle = "Account.LoginWith2fa.Title";
        public const string LoginWith2faDescription = "Account.LoginWith2fa.Description";
        public const string LoginWith2faNoSession = "Account.LoginWith2fa.NoSession";
        public const string LoginWith2faCode = "Account.LoginWith2fa.Code";
        public const string LoginWith2faCodePlaceholder = "Account.LoginWith2fa.Code.Placeholder";
        public const string LoginWith2faSubmit = "Account.LoginWith2fa.Submit";
        public const string LoginWith2faUseRecoveryCode = "Account.LoginWith2fa.UseRecoveryCode";
        public const string LoginWith2faRecoveryCode = "Account.LoginWith2fa.RecoveryCode";
        public const string LoginWith2faRecoveryCodePlaceholder = "Account.LoginWith2fa.RecoveryCode.Placeholder";
        public const string LoginWith2faSubmitRecovery = "Account.LoginWith2fa.SubmitRecovery";
        public const string LoginWith2faUseAuthenticatorCode = "Account.LoginWith2fa.UseAuthenticatorCode";
        public const string LoginWith2faFailed = "Account.LoginWith2fa.Failed";
        public const string LoginWith2faRecoveryFailed = "Account.LoginWith2fa.RecoveryFailed";

        public const string ExternalLoginTitle = "Account.ExternalLogin.Title";
        public const string ExternalLoginProcessing = "Account.ExternalLogin.Processing";
        public const string ExternalLoginFailed = "Account.ExternalLogin.Failed";

        public const string ExternalLoginsTitle = "Account.ExternalLogins.Title";
        public const string ExternalLoginsDescription = "Account.ExternalLogins.Description";
        public const string ExternalLoginsRemove = "Account.ExternalLogins.Remove";
        public const string ExternalLoginsNoLogins = "Account.ExternalLogins.NoLogins";
        public const string ExternalLoginsHasPassword = "Account.ExternalLogins.HasPassword";
        public const string ExternalLoginsRemoveSuccess = "Account.ExternalLogins.RemoveSuccess";
        public const string ExternalLoginsRemoveFailed = "Account.ExternalLogins.RemoveFailed";

        public const string TwoFactorTitle = "Account.TwoFactor.Title";
        public const string TwoFactorStatus = "Account.TwoFactor.Status";
        public const string TwoFactorEnabled = "Account.TwoFactor.Enabled";
        public const string TwoFactorDisabled = "Account.TwoFactor.Disabled";
        public const string TwoFactorRecoveryCodes = "Account.TwoFactor.RecoveryCodes";
        public const string TwoFactorSetup = "Account.TwoFactor.Setup";
        public const string TwoFactorDisable = "Account.TwoFactor.Disable";
        public const string TwoFactorResetKey = "Account.TwoFactor.ResetKey";
        public const string TwoFactorGenerateCodes = "Account.TwoFactor.GenerateCodes";
        public const string TwoFactorRecoveryCodesNew = "Account.TwoFactor.RecoveryCodes.New";
        public const string TwoFactorRecoveryCodesWarning = "Account.TwoFactor.RecoveryCodes.Warning";
        public const string TwoFactorError = "Account.TwoFactor.Error";
        public const string TwoFactorResetKeySuccess = "Account.TwoFactor.ResetKey.Success";
        public const string TwoFactorGenerateCodesSuccess = "Account.TwoFactor.GenerateCodes.Success";
        public const string TwoFactorBackToStatus = "Account.TwoFactor.BackToStatus";

        public const string EnableAuthenticatorSuccess = "Account.EnableAuthenticator.Success";
        public const string EnableAuthenticatorStep1 = "Account.EnableAuthenticator.Step1";
        public const string EnableAuthenticatorStep1Description = "Account.EnableAuthenticator.Step1.Description";
        public const string EnableAuthenticatorStep2 = "Account.EnableAuthenticator.Step2";
        public const string EnableAuthenticatorQrCodeInstruction = "Account.EnableAuthenticator.QrCodeInstruction";
        public const string EnableAuthenticatorManualKey = "Account.EnableAuthenticator.ManualKey";
        public const string EnableAuthenticatorStep3 = "Account.EnableAuthenticator.Step3";
        public const string EnableAuthenticatorVerificationCode = "Account.EnableAuthenticator.VerificationCode";
        public const string EnableAuthenticatorVerificationCodePlaceholder = "Account.EnableAuthenticator.VerificationCode.Placeholder";
        public const string EnableAuthenticatorVerify = "Account.EnableAuthenticator.Verify";
        public const string EnableAuthenticatorFailed = "Account.EnableAuthenticator.Failed";
        public const string EnableAuthenticatorVerifyFailed = "Account.EnableAuthenticator.VerifyFailed";

        public const string PersonalDataTitle = "Account.PersonalData.Title";
        public const string PersonalDataDescription = "Account.PersonalData.Description";
        public const string PersonalDataDownload = "Account.PersonalData.Download";
        public const string PersonalDataDelete = "Account.PersonalData.Delete";
        public const string PersonalDataYourData = "Account.PersonalData.YourData";
        public const string PersonalDataDeleteWarning = "Account.PersonalData.Delete.Warning";
        public const string PersonalDataDeleteConfirm = "Account.PersonalData.Delete.Confirm";
        public const string PersonalDataLoadFailed = "Account.PersonalData.LoadFailed";
        public const string PersonalDataDeleteSuccess = "Account.PersonalData.Delete.Success";
        public const string PersonalDataDeleteFailed = "Account.PersonalData.Delete.Failed";
    }

    public static class Profile
    {
        public const string Title = "Profile.Title";
        public const string ChangePassword = "Profile.ChangePassword";
        public const string ChangePasswordDescription = "Profile.ChangePassword.Description";
        public const string CurrentPassword = "Profile.CurrentPassword";
        public const string NewPassword = "Profile.NewPassword";
        public const string ConfirmNewPassword = "Profile.ConfirmNewPassword";
        public const string ChangePasswordSuccess = "Profile.ChangePassword.Success";
        public const string ChangePasswordSuccessDetail = "Profile.ChangePassword.Success.Detail";
        public const string ChangePasswordFailed = "Profile.ChangePassword.Failed";
        public const string LoadFailed = "Profile.LoadFailed";
        public const string UserId = "Profile.UserId";
        public const string Email = "Profile.Email";
        public const string EmailConfirmed = "Profile.EmailConfirmed";
        public const string Permissions = "Profile.Permissions";
        public const string NoPermissions = "Profile.NoPermissions";
    }

    public static class Common
    {
        public const string Home = "Common.Home";
        public const string Loading = "Common.Loading";
        public const string Yes = "Common.Yes";
        public const string No = "Common.No";
        public const string Save = "Common.Save";
        public const string Refresh = "Common.Refresh";
        public const string Search = "Common.Search";
        public const string Cancel = "Common.Cancel";
        public const string Close = "Common.Close";
        public const string Copy = "Common.Copy";
        public const string Delete = "Common.Delete";
        public const string Back = "Common.Back";
        public const string UnhandledError = "Common.UnhandledError";
        public const string NoDataFound = "Common.NoDataFound";
        public const string Enabled = "Common.Enabled";
        public const string Disabled = "Common.Disabled";
        public const string Actions = "Common.Actions";
        public const string SortBy = "Common.SortBy";
        public const string Ascending = "Common.Ascending";
        public const string Descending = "Common.Descending";
        public const string Confirm = "Common.Confirm";
        public const string View = "Common.View";
        public const string CreatedAt = "Common.CreatedAt";
        public const string DemoModeBanner = "Common.DemoModeBanner";
        public const string Remove = "Common.Remove";
        public const string PoweredBy = "Common.PoweredBy";
    }

    public static class Donate
    {
        public const string Title = "Donate.Title";
        public const string ScanQr = "Donate.ScanQr";
        public const string QrAlt = "Donate.QrAlt";
        public const string ThankYou = "Donate.ThankYou";
    }

    public static class Validation
    {
        public const string Required = "Validation.Required";
        public const string Email = "Validation.Email";
        public const string PasswordMismatch = "Validation.PasswordMismatch";
        public const string MustBeGreaterThanZero = "Validation.MustBeGreaterThanZero";
    }

    public static class Users
    {
        public const string Title = "Users.Title";
        public const string CreateTitle = "Users.Create.Title";
        public const string Email = "Users.Email";
        public const string Password = "Users.Password";
        public const string PhoneNumber = "Users.PhoneNumber";
        public const string PhoneNumberPlaceholder = "Users.PhoneNumber.Placeholder";
        public const string LinkedContact = "Users.LinkedContact";
        public const string UnlinkContact = "Users.UnlinkContact";
        public const string NoLinkedContact = "Users.NoLinkedContact";
        public const string RequirePasswordChange = "Users.RequirePasswordChange";
        public const string SendActivationEmail = "Users.SendActivationEmail";
        public const string Create = "Users.Create";
        public const string ListTitle = "Users.List.Title";
        public const string SearchPlaceholder = "Users.Search.Placeholder";
        public const string EmailConfirmed = "Users.EmailConfirmed";
        public const string LockoutEnd = "Users.LockoutEnd";
        public const string Active = "Users.Active";
        public const string LockUser = "Users.LockUser";
        public const string LockReason = "Users.LockReason";
        public const string LockDurationMinutes = "Users.LockDurationMinutes";
        public const string ApplyLockout = "Users.ApplyLockout";
        public const string SetPermissions = "Users.SetPermissions";
        public const string ApplyPermissions = "Users.ApplyPermissions";
        public const string NewPasswordOptional = "Users.NewPasswordOptional";
        public const string ResetPassword = "Users.ResetPassword";
        public const string TemporaryPassword = "Users.TemporaryPassword";
        public const string FilterAll = "Users.Filter.All";
        public const string FilterActive = "Users.Filter.Active";
        public const string FilterInactive = "Users.Filter.Inactive";
        public const string LoadFailed = "Users.LoadFailed";
        public const string CreateSuccess = "Users.Create.Success";
        public const string CreatedUserId = "Users.CreatedUserId";
        public const string CreateFailed = "Users.Create.Failed";
        public const string UpdateSuccess = "Users.Update.Success";
        public const string OperationCompleted = "Users.OperationCompleted";
        public const string UpdateFailed = "Users.Update.Failed";
        public const string LockoutSuccess = "Users.Lockout.Success";
        public const string LockoutFailed = "Users.Lockout.Failed";
        public const string PermissionSuccess = "Users.Permission.Success";
        public const string PermissionFailed = "Users.Permission.Failed";
        public const string ResetPasswordSuccess = "Users.ResetPassword.Success";
        public const string ResetPasswordFailed = "Users.ResetPassword.Failed";
        public const string TabAccount = "Users.Tab.Account";
        public const string TabPermissions = "Users.Tab.Permissions";
        public const string TabProfile = "Users.Tab.Profile";
        public const string TabLockout = "Users.Tab.Lockout";
        public const string TabPassword = "Users.Tab.Password";
        public const string CurrentPermissions = "Users.CurrentPermissions";
        public const string NoPermissionsAssigned = "Users.NoPermissionsAssigned";
    }

    public static class MasterData
    {
        public const string Countries = "MasterData.Countries";
        public const string Cities = "MasterData.Cities";
        public const string Districts = "MasterData.Districts";
        public const string Banks = "MasterData.Banks";
        public const string Code = "MasterData.Code";
        public const string Name = "MasterData.Name";
        public const string Country = "MasterData.Country";
        public const string City = "MasterData.City";
        public const string Create = "MasterData.Create";
        public const string Delete = "MasterData.Delete";
        public const string ImportCsv = "MasterData.Import.Csv";
        public const string ListTitle = "MasterData.List.Title";
        public const string SearchPlaceholder = "MasterData.Search.Placeholder";
        public const string SearchLabel = "MasterData.Search.Label";
        public const string CreateTitle = "MasterData.Create.Title";
        public const string EditTitle = "MasterData.Edit.Title";
        public const string LoadFailed = "MasterData.LoadFailed";
        public const string CreateSuccess = "MasterData.Create.Success";
        public const string CreateFailed = "MasterData.Create.Failed";
        public const string UpdateSuccess = "MasterData.Update.Success";
        public const string UpdateFailed = "MasterData.Update.Failed";
        public const string DeleteSuccess = "MasterData.Delete.Success";
        public const string DeleteFailed = "MasterData.Delete.Failed";
        public const string ImportSuccess = "MasterData.Import.Success";
        public const string ImportFailed = "MasterData.Import.Failed";
        public const string ImportResult = "MasterData.Import.Result";
        public const string ImportButton = "MasterData.Import.Button";
        public const string ConfirmDelete = "MasterData.ConfirmDelete";
        public const string SelectPlaceholder = "MasterData.Select.Placeholder";
        public const string SelectEmpty = "MasterData.Select.Empty";
        public const string SelectScrollMore = "MasterData.Select.ScrollMore";
    }

    public static class Contacts
    {
        public const string Title = "Contacts.Title";
        public const string ListTitle = "Contacts.List.Title";
        public const string CreateTitle = "Contacts.Create.Title";
        public const string AddNew = "Contacts.AddNew";
        public const string SearchLabel = "Contacts.Search.Label";
        public const string SearchPlaceholder = "Contacts.Search.Placeholder";
        public const string FilterTag = "Contacts.Filter.Tag";
        public const string FilterArchive = "Contacts.Filter.Archive";
        public const string FilterArchiveAll = "Contacts.Filter.Archive.All";
        public const string FilterArchiveActive = "Contacts.Filter.Archive.Active";
        public const string FilterArchiveArchived = "Contacts.Filter.Archive.Archived";
        public const string ProfileTitle = "Contacts.Profile.Title";
        public const string FirstName = "Contacts.FirstName";
        public const string LastName = "Contacts.LastName";
        public const string PrimaryEmail = "Contacts.PrimaryEmail";
        public const string PrimaryPhone = "Contacts.PrimaryPhone";
        public const string Company = "Contacts.Company";
        public const string JobTitle = "Contacts.JobTitle";
        public const string AvatarUrl = "Contacts.AvatarUrl";
        public const string ChangeAvatar = "Contacts.ChangeAvatar";
        public const string AvatarFormatHint = "Contacts.AvatarFormatHint";
        public const string AvatarAlt = "Contacts.AvatarAlt";
        public const string RemoveAvatar = "Contacts.RemoveAvatar";
        public const string AvatarTooLarge = "Contacts.AvatarTooLarge";
        public const string AvatarUploadFailed = "Contacts.AvatarUploadFailed";
        public const string Birthday = "Contacts.Birthday";
        public const string Notes = "Contacts.Notes";
        public const string Status = "Contacts.Status";
        public const string Actions = "Contacts.Actions";
        public const string LastUpdated = "Contacts.LastUpdated";
        public const string View = "Contacts.View";
        public const string Edit = "Contacts.Edit";
        public const string Delete = "Contacts.Delete";
        public const string CreateSuccess = "Contacts.Create.Success";
        public const string CreateFailed = "Contacts.Create.Failed";
        public const string UpdateSuccess = "Contacts.Update.Success";
        public const string UpdateFailed = "Contacts.Update.Failed";
        public const string DeleteSuccess = "Contacts.Delete.Success";
        public const string DeleteFailed = "Contacts.Delete.Failed";
        public const string LoadFailed = "Contacts.LoadFailed";
        public const string SuggestedUsers = "Contacts.SuggestedUsers";
        public const string SuggestedUsersNone = "Contacts.SuggestedUsers.None";
        public const string LinkUser = "Contacts.LinkUser";
        public const string LinkToUserSection = "Contacts.LinkToUser.Section";
        public const string SelectUserToLinkPlaceholder = "Contacts.SelectUserToLink.Placeholder";
        public const string LinkSelectedUser = "Contacts.LinkSelectedUser";
        public const string LinkedUser = "Contacts.LinkedUser";
        public const string UnlinkUser = "Contacts.UnlinkUser";
        public const string NoLinkedUser = "Contacts.NoLinkedUser";
        public const string ConfirmDelete = "Contacts.ConfirmDelete";
        public const string Value = "Contacts.Value";
        public const string IsPrimary = "Contacts.IsPrimary";
        public const string SetAsPrimary = "Contacts.SetAsPrimary";
        public const string Emails = "Contacts.Emails";
        public const string Phones = "Contacts.Phones";
        public const string Addresses = "Contacts.Addresses";
        public const string SocialLinks = "Contacts.SocialLinks";
        public const string Tags = "Contacts.Tags";
        public const string BankAccounts = "Contacts.BankAccounts";
        public const string IdentityDocuments = "Contacts.IdentityDocuments";
        public const string CustomFields = "Contacts.CustomFields";
        public const string TabGeneral = "Contacts.Tab.General";
        public const string TabContactInfo = "Contacts.Tab.ContactInfo";
        public const string Street = "Contacts.Street";
        public const string CountryCode = "Contacts.CountryCode";
        public const string CityCode = "Contacts.CityCode";
        public const string DistrictCode = "Contacts.DistrictCode";
        public const string PostalCode = "Contacts.PostalCode";
        public const string Platform = "Contacts.Platform";
        public const string Url = "Contacts.Url";
        public const string Name = "Contacts.Name";
        public const string BankCode = "Contacts.BankCode";
        public const string BranchName = "Contacts.BranchName";
        public const string AccountNumber = "Contacts.AccountNumber";
        public const string AccountName = "Contacts.AccountName";
        public const string CurrencyCode = "Contacts.CurrencyCode";
        public const string DocumentType = "Contacts.DocumentType";
        public const string DocumentNumber = "Contacts.DocumentNumber";
        public const string IssuedBy = "Contacts.IssuedBy";
        public const string IssuedDate = "Contacts.IssuedDate";
        public const string Key = "Contacts.Key";
        public const string ImportCsv = "Contacts.Import.Csv";
        public const string ImportDownloadTemplate = "Contacts.Import.DownloadTemplate";
        public const string ImportTemplateNote = "Contacts.Import.TemplateNote";
        public const string ImportSuccess = "Contacts.Import.Success";
        public const string ImportFailed = "Contacts.Import.Failed";
        public const string ImportResult = "Contacts.Import.Result";
        public const string ImportButton = "Contacts.Import.Button";
        public const string Export = "Contacts.Export";
        public const string ExportSuccess = "Contacts.Export.Success";
        public const string ExportFailed = "Contacts.Export.Failed";
    }

    public static class BankInterestRates
    {
        public const string Title = "BankInterestRates";
        public const string Month1 = "1Month";
        public const string Month3 = "3Months";
        public const string Month6 = "6Months";
        public const string Month9 = "9Months";
        public const string Month12 = "12Months";
        public const string Month24 = "24Months";
        public const string ShowBig4Only = "BankInterestRates.ShowBig4Only";
        public const string Toolkit = "BankInterestRates.Toolkit";
        public const string AverageRates = "BankInterestRates.AverageRates";
        public const string CompoundInterestCalculator = "BankInterestRates.CompoundInterestCalculator";
        public const string InputSection = "BankInterestRates.InputSection";
        public const string PrincipalAmount = "BankInterestRates.PrincipalAmount";
        public const string TermMonths = "BankInterestRates.TermMonths";
        public const string AnnualRate = "BankInterestRates.AnnualRate";
        public const string CurrencySuffixVnd = "BankInterestRates.CurrencySuffixVnd";
        public const string RateSuffixPerYear = "BankInterestRates.RateSuffixPerYear";
        public const string MonthsSuffix = "BankInterestRates.MonthsSuffix";
        public const string CalculateAction = "BankInterestRates.CalculateAction";
        public const string FromBank = "BankInterestRates.FromBank";
        public const string NoBankSelected = "BankInterestRates.NoBankSelected";
        public const string ResultSummary = "BankInterestRates.ResultSummary";
        public const string FinalAmount = "BankInterestRates.FinalAmount";
        public const string Profit = "BankInterestRates.Profit";
        public const string TotalReturn = "BankInterestRates.TotalReturn";
        public const string OverMonths = "BankInterestRates.OverMonths";
    }

    public static class BankDepositRates
    {
        public const string Title = "BankDepositRates.Title";
        public const string Subtitle = "BankDepositRates.Subtitle";
        public const string OnlineChannel = "BankDepositRates.OnlineChannel";
        public const string OfflineChannel = "BankDepositRates.OfflineChannel";
        public const string Bank = "BankDepositRates.Bank";
        public const string Note = "BankDepositRates.Note";
        public const string SearchPlaceholder = "BankDepositRates.SearchPlaceholder";
        public const string UpdatedAt = "BankDepositRates.UpdatedAt";
        public const string LoadFailed = "BankDepositRates.LoadFailed";
    }

    public static class SacombankExchangeRates
    {
        public const string Title = "SacombankExchangeRates.Title";
        public const string CurrencyCode = "SacombankExchangeRates.CurrencyCode";
        public const string BidInCash = "SacombankExchangeRates.BidInCash";
        public const string BidInTransfer = "SacombankExchangeRates.BidInTransfer";
        public const string OfferInCash = "SacombankExchangeRates.OfferInCash";
        public const string OfferInTransfer = "SacombankExchangeRates.OfferInTransfer";
        public const string CreatedDate = "SacombankExchangeRates.CreatedDate";
        public const string UpdatedAt = "SacombankExchangeRates.UpdatedAt";
        public const string LoadFailed = "SacombankExchangeRates.LoadFailed";
    }

    public static class Grid
    {
        public const string PagingSummaryFormat = "PagingSummaryFormat";
    }

    public static class Market
    {
        public const string Title = "Market.Title";
        public const string Commodities = "Market.Commodities";
        public const string Currencies = "Market.Currencies";
        public const string Cryptocurrencies = "Market.Cryptocurrencies";
        public const string LastPrice = "Market.LastPrice";
        public const string Change = "Market.Change";
        public const string Price = "Market.Price";
        public const string MarketCap = "Market.MarketCap";
    }

    public static class Vix
    {
        public const string Title = "Vix.Title";
        public const string LoadFailed = "Vix.LoadFailed";
        public const string Trend = "Vix.Trend";
        public const string Ema10 = "Vix.Ema10";
        public const string Ema20 = "Vix.Ema20";
        public const string Uptrend = "Vix.Uptrend";
        public const string Downtrend = "Vix.Downtrend";
        public const string Momentum3D = "Vix.Momentum3D";
        public const string Momentum5D = "Vix.Momentum5D";
        public const string Liquidity = "Vix.Liquidity";
        public const string CapitalFlow = "Vix.CapitalFlow";
        public const string Percentile = "Vix.Percentile";
        public const string Spike = "Vix.Spike";
        public const string SpikeYes = "Vix.SpikeYes";
        public const string SpikeNo = "Vix.SpikeNo";
        // Regime display labels
        public const string RegimeRiskOn = "Vix.RegimeRiskOn";
        public const string RegimeEarlyRisk = "Vix.RegimeEarlyRisk";
        public const string RegimePanic = "Vix.RegimePanic";
        public const string RegimeRecovery = "Vix.RegimeRecovery";
        // Action values (derived from Regime)
        public const string ActionIncreaseExposure = "Vix.ActionIncreaseExposure";
        public const string ActionReduceExposure = "Vix.ActionReduceExposure";
        public const string ActionBuildPosition = "Vix.ActionBuildPosition";
        public const string ActionReduceRisk = "Vix.ActionReduceRisk";
        // Liquidity values (derived from Regime)
        public const string LiquidityHigh = "Vix.LiquidityHigh";
        public const string LiquidityLow = "Vix.LiquidityLow";
        public const string LiquidityImproving = "Vix.LiquidityImproving";
        public const string LiquidityTightening = "Vix.LiquidityTightening";
        // Capital flow values (derived from Regime)
        public const string CapitalFlowIntoRisk = "Vix.CapitalFlowIntoRisk";
        public const string CapitalFlowIntoSafe = "Vix.CapitalFlowIntoSafe";
        public const string CapitalFlowReEntry = "Vix.CapitalFlowReEntry";
        public const string CapitalFlowDefensive = "Vix.CapitalFlowDefensive";
        // AI assessment
        public const string AiAssessmentTitle = "Vix.AiAssessmentTitle";
        public const string GetAiAssessment = "Vix.GetAiAssessment";
        public const string AiAssessmentLoadFailed = "Vix.AiAssessmentLoadFailed";
    }

    public static class VnDirectChangePrices
    {
        public const string Title = "VnDirect.ChangePrices.Title";
        public const string LoadFailed = "VnDirect.ChangePrices.LoadFailed";
        public const string IndexValue = "VnDirect.ChangePrices.IndexValue";
        public const string Change = "VnDirect.ChangePrices.Change";
        public const string ChangePct = "VnDirect.ChangePrices.ChangePct";
        public const string LastUpdated = "VnDirect.ChangePrices.LastUpdated";
        public const string Previous = "VnDirect.ChangePrices.Previous";
        public const string Next = "VnDirect.ChangePrices.Next";
        public const string PageOf = "VnDirect.ChangePrices.PageOf";
    }

    public static class VnDirectTopStocks
    {
        public const string Title = "VnDirect.TopStocks.Title";
        public const string SearchPlaceholder = "VnDirect.TopStocks.SearchPlaceholder";
        public const string NoDataFound = "VnDirect.TopStocks.NoDataFound";
        public const string Code = "VnDirect.TopStocks.Code";
        public const string Index = "VnDirect.TopStocks.Index";
        public const string LastPrice = "VnDirect.TopStocks.LastPrice";
        public const string LastUpdated = "VnDirect.TopStocks.LastUpdated";
        public const string PriceChange1D = "VnDirect.TopStocks.PriceChange1D";
        public const string PriceChangePct1D = "VnDirect.TopStocks.PriceChangePct1D";
        public const string AccumulatedValue = "VnDirect.TopStocks.AccumulatedValue";
        public const string NmVolumeAvgCr20D = "VnDirect.TopStocks.NmVolumeAvgCr20D";
        public const string NmVolNmVolAvg20DPctCr = "VnDirect.TopStocks.NmVolNmVolAvg20DPctCr";
        public const string TotalVolumeAvgCr20D = "VnDirect.TopStocks.TotalVolumeAvgCr20D";
        public const string PtVolTotalVolAvg20DPctCr = "VnDirect.TopStocks.PtVolTotalVolAvg20DPctCr";
        public const string PtVolAvg5DTotalVolAvg20DPctCr = "VnDirect.TopStocks.PtVolAvg5DTotalVolAvg20DPctCr";
        public const string PtVolSumCr5D = "VnDirect.TopStocks.PtVolSumCr5D";
        public const string PtValAvgCr5D = "VnDirect.TopStocks.PtValAvgCr5D";
        public const string PtVolAvgCr5D = "VnDirect.TopStocks.PtVolAvgCr5D";
        // Technical enrichment
        public const string LongSignal = "VnDirect.TopStocks.LongSignal";
        public const string ShortSignal = "VnDirect.TopStocks.ShortSignal";
        public const string LongBuyCount = "VnDirect.TopStocks.LongBuyCount";
        public const string LongSellCount = "VnDirect.TopStocks.LongSellCount";
        public const string ShortBuyCount = "VnDirect.TopStocks.ShortBuyCount";
        public const string ShortSellCount = "VnDirect.TopStocks.ShortSellCount";
        // Portfolio enrichment
        public const string InTcbs = "VnDirect.TopStocks.InTcbs";
        public const string OnlyTcbsAllTimeHoldings = "VnDirect.TopStocks.OnlyTcbsAllTimeHoldings";
        public const string DragonFundCount = "VnDirect.TopStocks.DragonFundCount";
        public const string DragonTotalWeight = "VnDirect.TopStocks.DragonTotalWeight";
        // Event enrichment
        public const string EventRiskLevel = "VnDirect.TopStocks.EventRiskLevel";
        public const string RecentEventCount = "VnDirect.TopStocks.RecentEventCount";
        public const string MostSevereEventType = "VnDirect.TopStocks.MostSevereEventType";
        public const string LatestEventEffectiveDate = "VnDirect.TopStocks.LatestEventEffectiveDate";
        // Scoring
        public const string CompositeScore = "VnDirect.TopStocks.CompositeScore";
        public const string ScoreBadge = "VnDirect.TopStocks.ScoreBadge";
        public const string LongSignalBadge = "VnDirect.TopStocks.LongSignalBadge";
        public const string ShortSignalBadge = "VnDirect.TopStocks.ShortSignalBadge";
        public const string DragonCountBadge = "VnDirect.TopStocks.DragonCountBadge";
        public const string TcbsAllTimeBadge = "VnDirect.TopStocks.TcbsAllTimeBadge";
        public const string TechnicalScore = "VnDirect.TopStocks.TechnicalScore";
        public const string PortfolioScore = "VnDirect.TopStocks.PortfolioScore";
        public const string EventPenalty = "VnDirect.TopStocks.EventPenalty";
        public const string LiquidityPenalty = "VnDirect.TopStocks.LiquidityPenalty";
        public const string DataCompleteness = "VnDirect.TopStocks.DataCompleteness";
        public const string RunWeeklySuggestionJob = "VnDirect.TopStocks.RunWeeklySuggestionJob";
        public const string RunWeeklySuggestionJobQueued = "VnDirect.TopStocks.RunWeeklySuggestionJobQueued";
        public const string RunWeeklySuggestionJobFailed = "VnDirect.TopStocks.RunWeeklySuggestionJobFailed";
    }

    public static class MexcSocketPriceMoveAlert
    {
        public const string Enable = "Mexc.SocketPriceMoveAlert.Enable";
        public const string SettingsTitle = "Mexc.SocketPriceMoveAlert.SettingsTitle";
        public const string SettingsHint = "Mexc.SocketPriceMoveAlert.SettingsHint";
        public const string TimeWindowMinutes = "Mexc.SocketPriceMoveAlert.TimeWindowMinutes";
        public const string AbsoluteChangePercent = "Mexc.SocketPriceMoveAlert.AbsoluteChangePercent";
        public const string AbsoluteChangePercentHint = "Mexc.SocketPriceMoveAlert.AbsoluteChangePercentHint";
        public const string ActiveSummary = "Mexc.SocketPriceMoveAlert.ActiveSummary";
        public const string AlertTitle = "Mexc.SocketPriceMoveAlert.AlertTitle";
        public const string AlertDetail = "Mexc.SocketPriceMoveAlert.AlertDetail";
        public const string Enabled = "Mexc.SocketPriceMoveAlert.Enabled";
        public const string Disabled = "Mexc.SocketPriceMoveAlert.Disabled";
    }

    public static class MexcContractTickers
    {
        public const string Title = "Mexc.ContractTickers.Title";
        public const string SocketTitle = "Mexc.ContractTickers.SocketTitle";
        public const string LoadFailed = "Mexc.ContractTickers.LoadFailed";
        public const string SearchPlaceholder = "Mexc.ContractTickers.SearchPlaceholder";
        public const string NoDataFound = "Mexc.ContractTickers.NoDataFound";
        public const string Symbol = "Mexc.ContractTickers.Symbol";
        public const string ContractId = "Mexc.ContractTickers.ContractId";
        public const string LastPrice = "Mexc.ContractTickers.LastPrice";
        public const string Bid1 = "Mexc.ContractTickers.Bid1";
        public const string Ask1 = "Mexc.ContractTickers.Ask1";
        public const string High24Price = "Mexc.ContractTickers.High24Price";
        public const string Low24Price = "Mexc.ContractTickers.Low24Price";
        public const string Volume24 = "Mexc.ContractTickers.Volume24";
        public const string Amount24 = "Mexc.ContractTickers.Amount24";
        public const string HoldVol = "Mexc.ContractTickers.HoldVol";
        public const string FundingRate = "Mexc.ContractTickers.FundingRate";
        public const string IndexPrice = "Mexc.ContractTickers.IndexPrice";
        public const string FairPrice = "Mexc.ContractTickers.FairPrice";
        public const string MaxBidPrice = "Mexc.ContractTickers.MaxBidPrice";
        public const string MinAskPrice = "Mexc.ContractTickers.MinAskPrice";
        public const string RiseFallRate = "Mexc.ContractTickers.RiseFallRate";
        public const string RiseFallValue = "Mexc.ContractTickers.RiseFallValue";
        public const string Timestamp = "Mexc.ContractTickers.Timestamp";
        public const string CompositeScore = "Mexc.ContractTickers.CompositeScore";
    }

    public static class MexcSpotTicker24Hr
    {
        public const string Title = "Mexc.SpotTicker24Hr.Title";
        public const string SocketTitle = "Mexc.SpotTicker24Hr.SocketTitle";
        public const string LoadFailed = "Mexc.SpotTicker24Hr.LoadFailed";
        public const string SearchPlaceholder = "Mexc.SpotTicker24Hr.SearchPlaceholder";
        public const string NoDataFound = "Mexc.SpotTicker24Hr.NoDataFound";
        public const string Symbol = "Mexc.SpotTicker24Hr.Symbol";
        public const string PriceChange = "Mexc.SpotTicker24Hr.PriceChange";
        public const string PriceChangePercent = "Mexc.SpotTicker24Hr.PriceChangePercent";
        public const string PrevClosePrice = "Mexc.SpotTicker24Hr.PrevClosePrice";
        public const string LastPrice = "Mexc.SpotTicker24Hr.LastPrice";
        public const string BidPrice = "Mexc.SpotTicker24Hr.BidPrice";
        public const string BidQty = "Mexc.SpotTicker24Hr.BidQty";
        public const string AskPrice = "Mexc.SpotTicker24Hr.AskPrice";
        public const string AskQty = "Mexc.SpotTicker24Hr.AskQty";
        public const string OpenPrice = "Mexc.SpotTicker24Hr.OpenPrice";
        public const string HighPrice = "Mexc.SpotTicker24Hr.HighPrice";
        public const string LowPrice = "Mexc.SpotTicker24Hr.LowPrice";
        public const string Volume = "Mexc.SpotTicker24Hr.Volume";
        public const string QuoteVolume = "Mexc.SpotTicker24Hr.QuoteVolume";
        public const string OpenTime = "Mexc.SpotTicker24Hr.OpenTime";
        public const string CloseTime = "Mexc.SpotTicker24Hr.CloseTime";
        public const string Count = "Mexc.SpotTicker24Hr.Count";
        public const string CompositeScore = "Mexc.SpotTicker24Hr.CompositeScore";
        public const string RunWeeklySuggestionJob = "Mexc.SpotTicker24Hr.RunWeeklySuggestionJob";
        public const string RunWeeklySuggestionJobQueued = "Mexc.SpotTicker24Hr.RunWeeklySuggestionJobQueued";
        public const string RunWeeklySuggestionJobFailed = "Mexc.SpotTicker24Hr.RunWeeklySuggestionJobFailed";
        // Proto-sourced fields (spot@public.miniTickers.v3.api.pb)
        public const string Rate = "Mexc.SpotTicker24Hr.Rate";
        public const string ZonedRate = "Mexc.SpotTicker24Hr.ZonedRate";
        public const string LastCloseRate = "Mexc.SpotTicker24Hr.LastCloseRate";
        public const string LastCloseZonedRate = "Mexc.SpotTicker24Hr.LastCloseZonedRate";
        public const string LastCloseHigh = "Mexc.SpotTicker24Hr.LastCloseHigh";
        public const string LastCloseLow = "Mexc.SpotTicker24Hr.LastCloseLow";
    }
    public static class CoinGecko
    {
        public const string Title = "CoinGecko.Title";
        public const string CoinList = "CoinGecko.CoinList";
        public const string CoinName = "CoinGecko.CoinName";
        public const string SelectCoinHint = "CoinGecko.SelectCoinHint";
        public const string DetailsTitle = "CoinGecko.DetailsTitle";
        public const string CurrentPrice = "CoinGecko.CurrentPrice";
        public const string MarketCapRank = "CoinGecko.MarketCapRank";
        public const string High24H = "CoinGecko.High24H";
        public const string Low24H = "CoinGecko.Low24H";
        public const string PriceChange24H = "CoinGecko.PriceChange24H";
        public const string LoadListFailed = "CoinGecko.LoadListFailed";
        public const string LoadDetailFailed = "CoinGecko.LoadDetailFailed";
    }

    public static class CurrencyConverter
    {
        public const string Title = "CurrencyConverter.Title";
        public const string FromCurrency = "CurrencyConverter.FromCurrency";
        public const string ToCurrency = "CurrencyConverter.ToCurrency";
        public const string Amount = "CurrencyConverter.Amount";
        public const string Swap = "CurrencyConverter.Swap";
        public const string RateType = "CurrencyConverter.RateType";
        public const string Cash = "CurrencyConverter.Cash";
        public const string Transfer = "CurrencyConverter.Transfer";
        public const string ConvertedResult = "CurrencyConverter.ConvertedResult";
        public const string SelectCurrency = "CurrencyConverter.SelectCurrency";
    }

    public static class ChainBrokerFunds
    {
        public const string Title = "ChainBrokerFunds.Title";
        public const string SearchPlaceholder = "ChainBrokerFunds.SearchPlaceholder";
        public const string NoDataFound = "ChainBrokerFunds.NoDataFound";
        public const string LoadFailed = "ChainBrokerFunds.LoadFailed";
        public const string SyncSuccess = "ChainBrokerFunds.SyncSuccess";
        public const string SyncFailed = "ChainBrokerFunds.SyncFailed";
        public const string ColName = "ChainBrokerFunds.ColName";
        public const string ColFundType = "ChainBrokerFunds.ColFundType";
        public const string ColStatus = "ChainBrokerFunds.ColStatus";
        public const string ColProjectCount = "ChainBrokerFunds.ColProjectCount";
        public const string ColAvgRoi = "ChainBrokerFunds.ColAvgRoi";
        public const string ColAvgPriceChange24h = "ChainBrokerFunds.ColAvgPriceChange24h";
        public const string ColAvgPriceChange7d = "ChainBrokerFunds.ColAvgPriceChange7d";
        public const string ColAvgPriceChange30d = "ChainBrokerFunds.ColAvgPriceChange30d";
        public const string ColYearFounded = "ChainBrokerFunds.ColYearFounded";
        public const string ColLastInvestment = "ChainBrokerFunds.ColLastInvestment";
        public const string ColGainers = "ChainBrokerFunds.ColGainers";
        public const string ColLosers = "ChainBrokerFunds.ColLosers";
        public const string SyncButton = "ChainBrokerFunds.SyncButton";
        public const string SyncConfirmTitle = "ChainBrokerFunds.SyncConfirmTitle";
        public const string SyncConfirmMessage = "ChainBrokerFunds.SyncConfirmMessage";
    }

    public static class ChainBrokerProjects
    {
        public const string Title = "ChainBrokerProjects.Title";
        public const string SearchPlaceholder = "ChainBrokerProjects.SearchPlaceholder";
        public const string NoDataFound = "ChainBrokerProjects.NoDataFound";
        public const string LoadFailed = "ChainBrokerProjects.LoadFailed";
        public const string SyncSuccess = "ChainBrokerProjects.SyncSuccess";
        public const string SyncFailed = "ChainBrokerProjects.SyncFailed";
        public const string ColName = "ChainBrokerProjects.ColName";
        public const string ColTicker = "ChainBrokerProjects.ColTicker";
        public const string ColRank = "ChainBrokerProjects.ColRank";
        public const string ColCurrentPrice = "ChainBrokerProjects.ColCurrentPrice";
        public const string ColPublicPrice = "ChainBrokerProjects.ColPublicPrice";
        public const string ColPrivatePrice = "ChainBrokerProjects.ColPrivatePrice";
        public const string ColAthPrice = "ChainBrokerProjects.ColAthPrice";
        public const string ColPublicRoi = "ChainBrokerProjects.ColPublicRoi";
        public const string ColPrivateRoi = "ChainBrokerProjects.ColPrivateRoi";
        public const string ColPublicAthRoi = "ChainBrokerProjects.ColPublicAthRoi";
        public const string ColPrivateAthRoi = "ChainBrokerProjects.ColPrivateAthRoi";
        public const string ColBrokerScore = "ChainBrokerProjects.ColBrokerScore";
        public const string ColSecurityScore = "ChainBrokerProjects.ColSecurityScore";
        public const string ColTwitterScore = "ChainBrokerProjects.ColTwitterScore";
        public const string ColPublicRaise = "ChainBrokerProjects.ColPublicRaise";
        public const string ColPrivateRaise = "ChainBrokerProjects.ColPrivateRaise";
        public const string ColTotalRaise = "ChainBrokerProjects.ColTotalRaise";
        public const string ColPriceChange24h = "ChainBrokerProjects.ColPriceChange24h";
        public const string ColPriceChange7d = "ChainBrokerProjects.ColPriceChange7d";
        public const string ColPriceChange30d = "ChainBrokerProjects.ColPriceChange30d";
        public const string ColPriceChange1y = "ChainBrokerProjects.ColPriceChange1y";
        public const string ColMarketCap = "ChainBrokerProjects.ColMarketCap";
        public const string ColFdmc = "ChainBrokerProjects.ColFdmc";
        public const string ColVolume24h = "ChainBrokerProjects.ColVolume24h";
        public const string ColCurrentCirculation = "ChainBrokerProjects.ColCurrentCirculation";
        public const string ColTotalCirculation = "ChainBrokerProjects.ColTotalCirculation";
        public const string ColPercentCirculating = "ChainBrokerProjects.ColPercentCirculating";
        public const string ColPrivateAnnounceDate = "ChainBrokerProjects.ColPrivateAnnounceDate";
        public const string ColListingDate = "ChainBrokerProjects.ColListingDate";
        public const string ColIdoDate = "ChainBrokerProjects.ColIdoDate";
        public const string ColNextUnlockDate = "ChainBrokerProjects.ColNextUnlockDate";
        public const string ColFundCount = "ChainBrokerProjects.ColFundCount";
        public const string ColBlockchains = "ChainBrokerProjects.ColBlockchains";
        public const string SyncButton = "ChainBrokerProjects.SyncButton";
        public const string SyncConfirmTitle = "ChainBrokerProjects.SyncConfirmTitle";
        public const string SyncConfirmMessage = "ChainBrokerProjects.SyncConfirmMessage";
        public const string FilterByFund = "ChainBrokerProjects.FilterByFund";
    }

    public static class ChainBrokerUnlocks
    {
        public const string Title = "ChainBrokerUnlocks.Title";
        public const string SearchPlaceholder = "ChainBrokerUnlocks.SearchPlaceholder";
        public const string NoDataFound = "ChainBrokerUnlocks.NoDataFound";
        public const string LoadFailed = "ChainBrokerUnlocks.LoadFailed";
        public const string SyncSuccess = "ChainBrokerUnlocks.SyncSuccess";
        public const string SyncFailed = "ChainBrokerUnlocks.SyncFailed";
        public const string ColName = "ChainBrokerUnlocks.ColName";
        public const string ColTicker = "ChainBrokerUnlocks.ColTicker";
        public const string ColNextUnlockDate = "ChainBrokerUnlocks.ColNextUnlockDate";
        public const string ColUnlockAmount = "ChainBrokerUnlocks.ColUnlockAmount";
        public const string ColUnlockValueUsd = "ChainBrokerUnlocks.ColUnlockValueUsd";
        public const string ColRoundName = "ChainBrokerUnlocks.ColRoundName";
        public const string ColCirculationPercent = "ChainBrokerUnlocks.ColCirculationPercent";
        public const string ColUnlockPercent = "ChainBrokerUnlocks.ColUnlockPercent";
        public const string ColVolume24h = "ChainBrokerUnlocks.ColVolume24h";
        public const string ColPriceChange24h = "ChainBrokerUnlocks.ColPriceChange24h";
        public const string ColPriceChange7d = "ChainBrokerUnlocks.ColPriceChange7d";
        public const string SyncButton = "ChainBrokerUnlocks.SyncButton";
        public const string SyncConfirmTitle = "ChainBrokerUnlocks.SyncConfirmTitle";
        public const string SyncConfirmMessage = "ChainBrokerUnlocks.SyncConfirmMessage";
    }

    public static class TcbsTop10
    {
        public const string Title = "TcbsTop10.Title";
        public const string NoDataFound = "TcbsTop10.NoDataFound";
        public const string LoadFailed = "TcbsTop10.LoadFailed";
        public const string PostedAt = "TcbsTop10.PostedAt";
        public const string EffectiveDate = "TcbsTop10.EffectiveDate";
        public const string AddedTickers = "TcbsTop10.AddedTickers";
        public const string RemovedTickers = "TcbsTop10.RemovedTickers";
        public const string ImagePortfolioDetail = "TcbsTop10.ImagePortfolioDetail";
        public const string ImagePeriodPerformance = "TcbsTop10.ImagePeriodPerformance";
        public const string ImageYtdPerformance = "TcbsTop10.ImageYtdPerformance";
        public const string Images = "TcbsTop10.Images";
        public const string SyncButton = "TcbsTop10.SyncButton";
        public const string SyncSuccess = "TcbsTop10.SyncSuccess";
        public const string SyncFailed = "TcbsTop10.SyncFailed";
        public const string SyncConfirmTitle = "TcbsTop10.SyncConfirmTitle";
        public const string SyncConfirmMessage = "TcbsTop10.SyncConfirmMessage";
        public const string CurrentHoldings = "TcbsTop10.CurrentHoldings";
    }

    public static class DragonCapitalFunds
    {
        public const string Title = "DragonCapitalFunds.Title";
        public const string NoDataFound = "DragonCapitalFunds.NoDataFound";
        public const string LoadFailed = "DragonCapitalFunds.LoadFailed";
        public const string TradingDate = "DragonCapitalFunds.TradingDate";
        public const string Top10Holdings = "DragonCapitalFunds.Top10Holdings";
        public const string AssetAllocation = "DragonCapitalFunds.AssetAllocation";
        public const string SectorAllocation = "DragonCapitalFunds.SectorAllocation";
        public const string ColAssetId = "DragonCapitalFunds.ColAssetId";
        public const string ColWeight = "DragonCapitalFunds.ColWeight";
        public const string ColExchange = "DragonCapitalFunds.ColExchange";
        public const string ColIndustry = "DragonCapitalFunds.ColIndustry";
        public const string ColSector = "DragonCapitalFunds.ColSector";
        public const string ColHoldingVolume = "DragonCapitalFunds.ColHoldingVolume";
        public const string ColMarketValueBillion = "DragonCapitalFunds.ColMarketValueBillion";
        public const string MostFrequentAssets = "DragonCapitalFunds.MostFrequentAssets";
    }

    public static class TradingSuggestion
    {
        public const string Title = "TradingSuggestion.Title";
        public const string Action = "TradingSuggestion.Action";
        public const string Symbol = "TradingSuggestion.Symbol";
        public const string Market = "TradingSuggestion.Market";
        public const string Timeframe = "TradingSuggestion.Timeframe";
        public const string UseLongTermTimeframe = "TradingSuggestion.UseLongTermTimeframe";
        public const string LongTermTimeframe = "TradingSuggestion.LongTermTimeframe";
        public const string Week1 = "TradingSuggestion.Week1";
        public const string Month1 = "TradingSuggestion.Month1";
        public const string Run = "TradingSuggestion.Run";
        public const string Trend = "TradingSuggestion.Trend";
        public const string Momentum = "TradingSuggestion.Momentum";
        public const string Setup = "TradingSuggestion.Setup";
        public const string Signal = "TradingSuggestion.Signal";
        public const string EntryPrice = "TradingSuggestion.EntryPrice";
        public const string StopLoss = "TradingSuggestion.StopLoss";
        public const string TakeProfit1 = "TradingSuggestion.TakeProfit1";
        public const string TakeProfit2 = "TradingSuggestion.TakeProfit2";
        public const string Confidence = "TradingSuggestion.Confidence";
        public const string LoadFailed = "TradingSuggestion.LoadFailed";
        public const string EnableAiVerdict = "TradingSuggestion.EnableAiVerdict";
        public const string VerdictLoadFailed = "TradingSuggestion.VerdictLoadFailed";
        public const string PriceComparison = "TradingSuggestion.PriceComparison";
        public const string PriceComparisonNoData = "TradingSuggestion.PriceComparisonNoData";
        public const string Period = "TradingSuggestion.Period";
        public const string BetterVolume = "TradingSuggestion.BetterVolume";
        public const string WorseVolume = "TradingSuggestion.WorseVolume";
        public const string AveragePrice = "TradingSuggestion.AveragePrice";
    }

    public static class WeeklySuggestionHistory
    {
        public const string Title = "WeeklySuggestionHistory.Title";
        public const string Refresh = "WeeklySuggestionHistory.Refresh";
        public const string EvaluateVisibleSymbols = "WeeklySuggestionHistory.EvaluateVisibleSymbols";
        public const string LookbackReports = "WeeklySuggestionHistory.LookbackReports";
        public const string Matched = "WeeklySuggestionHistory.Matched";
        public const string RunAt = "WeeklySuggestionHistory.RunAt";
        public const string ReportKey = "WeeklySuggestionHistory.ReportKey";
        public const string Status = "WeeklySuggestionHistory.Status";
        public const string Picks = "WeeklySuggestionHistory.Picks";
        public const string Symbol = "WeeklySuggestionHistory.Symbol";
        public const string Timeframe = "WeeklySuggestionHistory.Timeframe";
        public const string Entry = "WeeklySuggestionHistory.Entry";
        public const string Current = "WeeklySuggestionHistory.Current";
        public const string ChangePercent = "WeeklySuggestionHistory.ChangePercent";
        public const string TemplateFiles = "WeeklySuggestionHistory.TemplateFiles";
        public const string DownloadCsv = "WeeklySuggestionHistory.DownloadCsv";
        public const string DownloadMarkdown = "WeeklySuggestionHistory.DownloadMarkdown";
        public const string DownloadFailed = "WeeklySuggestionHistory.DownloadFailed";
        public const string ViewPicks = "WeeklySuggestionHistory.ViewPicks";
        public const string PicksTitle = "WeeklySuggestionHistory.PicksTitle";
        public const string Rank = "WeeklySuggestionHistory.Rank";
        public const string Signal = "WeeklySuggestionHistory.Signal";
    }


    public static class Notifications
    {
        public const string Title = "Notifications.Title";
        public const string Description = "Notifications.Description";
        public const string EventType = "Notifications.EventType";
        public const string Target = "Notifications.Target";
        public const string TargetSlack = "Notifications.Target.Slack";
        public const string TargetTelegram = "Notifications.Target.Telegram";
        public const string TargetBoth = "Notifications.Target.Both";
        public const string SeverityOptional = "Notifications.SeverityOptional";
        public const string TitleOptional = "Notifications.TitleOptional";
        public const string Message = "Notifications.Message";
        public const string Max64 = "Notifications.Max64";
        public const string Max256 = "Notifications.Max256";
        public const string Max20000 = "Notifications.Max20000";
        public const string SentSuccess = "Notifications.SentSuccess";
        public const string SendFailed = "Notifications.SendFailed";
        public const string EventMarketOverview = "Notifications.Event.MarketOverview";
        public const string EventPriceUpdate = "Notifications.Event.PriceUpdate";
        public const string EventCryptoSpike = "Notifications.Event.CryptoSpike";
        public const string EventGoodEntryCrypto = "Notifications.Event.GoodEntryCrypto";
        public const string EventGoodEntryStocks = "Notifications.Event.GoodEntryStocks";
        public const string EventSystemAlert = "Notifications.Event.SystemAlert";
        public const string EventDataSync = "Notifications.Event.DataSync";
        public const string EventMarketScanner = "Notifications.Event.MarketScanner";
        public const string EventPriceAlert = "Notifications.Event.PriceAlert";
    }

    public static class SlackCommands
    {
        public const string Description = "SlackCommands.Description";
        public const string Channel = "SlackCommands.Channel";
        public const string ExternalUserId = "SlackCommands.ExternalUserId";
        public const string ExternalChannelId = "SlackCommands.ExternalChannelId";
        public const string ChannelPlaceholder = "SlackCommands.ChannelPlaceholder";
        public const string ChannelLoadFailed = "SlackCommands.ChannelLoadFailed";
        public const string SampleCommands = "SlackCommands.SampleCommands";
        public const string SampleCommandsHint = "SlackCommands.SampleCommandsHint";
        public const string RawCommand = "SlackCommands.RawCommand";
        public const string RawCommandHint = "SlackCommands.RawCommandHint";
        public const string ExecuteButton = "SlackCommands.ExecuteButton";
        public const string ExecuteSuccess = "SlackCommands.ExecuteSuccess";
        public const string ExecuteFailed = "SlackCommands.ExecuteFailed";
        public const string ExecutionResult = "SlackCommands.ExecutionResult";
        public const string NoExecutionResult = "SlackCommands.NoExecutionResult";
        public const string Status = "SlackCommands.Status";
        public const string ExecutionId = "SlackCommands.ExecutionId";
        public const string PlainText = "SlackCommands.PlainText";
        public const string ErrorCode = "SlackCommands.ErrorCode";
    }

    public static class AiTester
    {
        public const string Title = "AiTester.Title";
        public const string Description = "AiTester.Description";
        public const string SystemPromptOptional = "AiTester.SystemPromptOptional";
        public const string Prompt = "AiTester.Prompt";
        public const string PromptHint = "AiTester.PromptHint";
        public const string GenerateButton = "AiTester.GenerateButton";
        public const string GenerateSuccess = "AiTester.GenerateSuccess";
        public const string GenerateFailed = "AiTester.GenerateFailed";
        public const string Result = "AiTester.Result";
        public const string NoResult = "AiTester.NoResult";
        public const string Provider = "AiTester.Provider";
        public const string Model = "AiTester.Model";
        public const string Output = "AiTester.Output";
        public const string Max12000 = "AiTester.Max12000";
    }

    public static class MarketScanner
    {
        public const string ListTitle = "MarketScanner.List.Title";
        public const string Title = "MarketScanner.Title";
        public const string Name = "MarketScanner.Name";
        public const string SignalType = "MarketScanner.SignalType";
        public const string SignalTypePricePump = "MarketScanner.SignalType.PricePump";
        public const string SignalTypeFundingFlip = "MarketScanner.SignalType.FundingFlip";
        public const string SignalTypeFundingMultiple = "MarketScanner.SignalType.FundingMultiple";
        public const string SignalTypeVolumeSpike = "MarketScanner.SignalType.VolumeSpike";
        public const string Window = "MarketScanner.Window";
        public const string WindowThreeMinutes = "MarketScanner.Window.ThreeMinutes";
        public const string WindowSixMinutes = "MarketScanner.Window.SixMinutes";
        public const string WindowTwelveMinutes = "MarketScanner.Window.TwelveMinutes";
        public const string WindowThirtyMinutes = "MarketScanner.Window.ThirtyMinutes";
        public const string WindowOneHour = "MarketScanner.Window.OneHour";
        public const string NameSuggestionPricePump5 = "MarketScanner.NameSuggestion.PricePump5";
        public const string NameSuggestionPricePump10 = "MarketScanner.NameSuggestion.PricePump10";
        public const string NameSuggestionPricePump20 = "MarketScanner.NameSuggestion.PricePump20";
        public const string NameSuggestionVolumeSpike2x = "MarketScanner.NameSuggestion.VolumeSpike2x";
        public const string NameSuggestionVolumeSpike3x = "MarketScanner.NameSuggestion.VolumeSpike3x";
        public const string NameSuggestionVolumeSpike5x = "MarketScanner.NameSuggestion.VolumeSpike5x";
        public const string NameSuggestionVolumeSpike10x = "MarketScanner.NameSuggestion.VolumeSpike10x";
        public const string NameSuggestionFundingFlipAlert = "MarketScanner.NameSuggestion.FundingFlipAlert";
        public const string NameSuggestionFundingRate2x = "MarketScanner.NameSuggestion.FundingRate2x";
        public const string NameSuggestionFundingRate5x = "MarketScanner.NameSuggestion.FundingRate5x";
        public const string NameSuggestionFundingRate10x = "MarketScanner.NameSuggestion.FundingRate10x";
        public const string NameSuggestionAbnormalFundingAlert = "MarketScanner.NameSuggestion.AbnormalFundingAlert";
        public const string NameSuggestionMomentumSurge = "MarketScanner.NameSuggestion.MomentumSurge";
        public const string NameSuggestionWhaleVolumeAlert = "MarketScanner.NameSuggestion.WhaleVolumeAlert";
        public const string NameSuggestionBreakoutSignal = "MarketScanner.NameSuggestion.BreakoutSignal";
        public const string Threshold = "MarketScanner.Threshold";
        public const string IsEnabled = "MarketScanner.IsEnabled";
        public const string CreateTitle = "MarketScanner.Create.Title";
        public const string EditTitle = "MarketScanner.Edit.Title";
        public const string SearchPlaceholder = "MarketScanner.Search.Placeholder";
        public const string LoadFailed = "MarketScanner.LoadFailed";
        public const string CreateSuccess = "MarketScanner.Create.Success";
        public const string CreateFailed = "MarketScanner.Create.Failed";
        public const string UpdateSuccess = "MarketScanner.Update.Success";
        public const string UpdateFailed = "MarketScanner.Update.Failed";
        public const string DeleteSuccess = "MarketScanner.Delete.Success";
        public const string DeleteFailed = "MarketScanner.Delete.Failed";
        public const string ConfirmDelete = "MarketScanner.ConfirmDelete";
    }

    public static class PriceAlerts
    {
        public const string ListTitle = "PriceAlerts.List.Title";
        public const string Title = "PriceAlerts.Title";
        public const string Symbol = "PriceAlerts.Symbol";
        public const string AssetType = "PriceAlerts.AssetType";
        public const string Condition = "PriceAlerts.Condition";
        public const string TargetPrice = "PriceAlerts.TargetPrice";
        public const string Note = "PriceAlerts.Note";
        public const string IsEnabled = "PriceAlerts.IsEnabled";
        public const string IsOneTime = "PriceAlerts.IsOneTime";
        public const string TriggerCount = "PriceAlerts.TriggerCount";
        public const string LastTriggeredAt = "PriceAlerts.LastTriggeredAt";
        public const string CreateTitle = "PriceAlerts.Create.Title";
        public const string EditTitle = "PriceAlerts.Edit.Title";
        public const string SearchPlaceholder = "PriceAlerts.Search.Placeholder";
        public const string LoadFailed = "PriceAlerts.LoadFailed";
        public const string CreateSuccess = "PriceAlerts.Create.Success";
        public const string CreateFailed = "PriceAlerts.Create.Failed";
        public const string UpdateSuccess = "PriceAlerts.Update.Success";
        public const string UpdateFailed = "PriceAlerts.Update.Failed";
        public const string DeleteSuccess = "PriceAlerts.Delete.Success";
        public const string DeleteFailed = "PriceAlerts.Delete.Failed";
        public const string ConfirmDelete = "PriceAlerts.ConfirmDelete";
    }

    public static class CredentialAccounts
    {
        public const string Title = "CredentialAccounts.Title";
        public const string ListTitle = "CredentialAccounts.List.Title";
        public const string CreateTitle = "CredentialAccounts.Create.Title";
        public const string EditTitle = "CredentialAccounts.Edit.Title";
        public const string SearchPlaceholder = "CredentialAccounts.Search.Placeholder";
        public const string FilterStatus = "CredentialAccounts.Filter.Status";
        public const string FilterAll = "CredentialAccounts.Filter.All";
        public const string FilterUsed = "CredentialAccounts.Filter.Used";
        public const string FilterUnused = "CredentialAccounts.Filter.Unused";
        public const string Username = "CredentialAccounts.Username";
        public const string Password = "CredentialAccounts.Password";
        public const string NewPassword = "CredentialAccounts.NewPassword";
        public const string Description = "CredentialAccounts.Description";
        public const string Status = "CredentialAccounts.Status";
        public const string Used = "CredentialAccounts.Used";
        public const string Unused = "CredentialAccounts.Unused";
        public const string LastUsed = "CredentialAccounts.LastUsed";
        public const string UsedBy = "CredentialAccounts.UsedBy";
        public const string UsageCount = "CredentialAccounts.UsageCount";
        public const string Actions = "CredentialAccounts.Actions";
        public const string ViewPassword = "CredentialAccounts.ViewPassword";
        public const string CopyUsername = "CredentialAccounts.CopyUsername";
        public const string CopyPassword = "CredentialAccounts.CopyPassword";
        public const string CopyAll = "CredentialAccounts.CopyAll";
        public const string MarkUsed = "CredentialAccounts.MarkUsed";
        public const string ResetUsed = "CredentialAccounts.ResetUsed";
        public const string ViewAuditLogs = "CredentialAccounts.ViewAuditLogs";
        public const string AuditLogsTitle = "CredentialAccounts.AuditLogs.Title";
        public const string AuditAction = "CredentialAccounts.Audit.Action";
        public const string AuditUser = "CredentialAccounts.Audit.User";
        public const string AuditCreatedAt = "CredentialAccounts.Audit.CreatedAt";
        public const string PasswordDialogTitle = "CredentialAccounts.PasswordDialog.Title";
        public const string Details = "CredentialAccounts.Details";
        public const string LoadFailed = "CredentialAccounts.LoadFailed";
        public const string CreateSuccess = "CredentialAccounts.Create.Success";
        public const string CreateFailed = "CredentialAccounts.Create.Failed";
        public const string UpdateSuccess = "CredentialAccounts.Update.Success";
        public const string UpdateFailed = "CredentialAccounts.Update.Failed";
        public const string DeleteSuccess = "CredentialAccounts.Delete.Success";
        public const string DeleteFailed = "CredentialAccounts.Delete.Failed";
        public const string ConfirmDelete = "CredentialAccounts.ConfirmDelete";
        public const string MarkUsedSuccess = "CredentialAccounts.MarkUsed.Success";
        public const string MarkUsedFailed = "CredentialAccounts.MarkUsed.Failed";
        public const string ResetUsedSuccess = "CredentialAccounts.ResetUsed.Success";
        public const string ResetUsedFailed = "CredentialAccounts.ResetUsed.Failed";
        public const string PasswordChangedSuccess = "CredentialAccounts.PasswordChanged.Success";
        public const string CopySuccess = "CredentialAccounts.Copy.Success";
        public const string CopyFailed = "CredentialAccounts.Copy.Failed";
        public const string ViewPasswordFailed = "CredentialAccounts.ViewPassword.Failed";
        public const string AuditLoadFailed = "CredentialAccounts.Audit.LoadFailed";
    }

    public static class Portfolios
    {
        public const string ListTitle = "Portfolios.List.Title";
        public const string Title = "Portfolios.Title";
        public const string Name = "Portfolios.Name";
        public const string Description = "Portfolios.Description";
        public const string PortfolioType = "Portfolios.PortfolioType";
        public const string TypeTrading = "Portfolios.Type.Trading";
        public const string TypeLongTerm = "Portfolios.Type.LongTerm";
        public const string TypeRetirement = "Portfolios.Type.Retirement";
        public const string TypeSavings = "Portfolios.Type.Savings";
        public const string CreateTitle = "Portfolios.Create.Title";
        public const string EditTitle = "Portfolios.Edit.Title";
        public const string SearchPlaceholder = "Portfolios.Search.Placeholder";
        public const string LoadFailed = "Portfolios.LoadFailed";
        public const string CreateSuccess = "Portfolios.Create.Success";
        public const string UpdateSuccess = "Portfolios.Update.Success";
        public const string DeleteSuccess = "Portfolios.Delete.Success";
        public const string DeleteFailed = "Portfolios.Delete.Failed";
        public const string ConfirmDelete = "Portfolios.ConfirmDelete";
        public const string Export = "Portfolios.Export";
        public const string ExportSuccess = "Portfolios.Export.Success";
        public const string ExportFailed = "Portfolios.Export.Failed";
        public const string ImportCsv = "Portfolios.Import.Csv";
        public const string ImportDownloadTemplate = "Portfolios.Import.DownloadTemplate";
        public const string ImportTemplateNote = "Portfolios.Import.TemplateNote";
        public const string ImportSuccess = "Portfolios.Import.Success";
        public const string ImportFailed = "Portfolios.Import.Failed";
        public const string ImportResult = "Portfolios.Import.Result";
        public const string ImportButton = "Portfolios.Import.Button";
    }

    public static class AssetPositions
    {
        public const string CryptoTitle = "AssetPositions.Crypto.Title";
        public const string StockTitle = "AssetPositions.Stock.Title";
        public const string SavingTitle = "AssetPositions.Saving.Title";
        public const string Open = "AssetPositions.Open";
        public const string Closed = "AssetPositions.Closed";
        public const string Symbol = "AssetPositions.Symbol";
        public const string StockSymbolPlaceholder = "AssetPositions.StockSymbolPlaceholder";
        public const string CryptoSymbolPlaceholder = "AssetPositions.CryptoSymbolPlaceholder";
        public const string Quantity = "AssetPositions.Quantity";
        public const string AverageEntryPrice = "AssetPositions.AverageEntryPrice";
        public const string TotalInvested = "AssetPositions.TotalInvested";
        public const string RealizedPnl = "AssetPositions.RealizedPnl";
        public const string TargetPrice = "AssetPositions.TargetPrice";
        public const string StopLoss = "AssetPositions.StopLoss";
        public const string EnablePriceAlerts = "AssetPositions.EnablePriceAlerts";
        public const string EnablePriceAlertsHint = "AssetPositions.EnablePriceAlertsHint";
        public const string Note = "AssetPositions.Note";
        public const string Exchange = "AssetPositions.Exchange";
        public const string ExchangeHose = "AssetPositions.Exchange.Hose";
        public const string ExchangeHnx = "AssetPositions.Exchange.Hnx";
        public const string ExchangeUpcom = "AssetPositions.Exchange.Upcom";
        public const string ExchangeOther = "AssetPositions.Exchange.Other";
        public const string BankName = "AssetPositions.BankName";
        public const string AccountNumber = "AssetPositions.AccountNumber";
        public const string PrincipalAmount = "AssetPositions.PrincipalAmount";
        public const string InterestRate = "AssetPositions.InterestRate";
        public const string InterestType = "AssetPositions.InterestType";
        public const string InterestTypeSimple = "AssetPositions.InterestType.Simple";
        public const string InterestTypeCompound = "AssetPositions.InterestType.Compound";
        public const string DepositDate = "AssetPositions.DepositDate";
        public const string MaturityDate = "AssetPositions.MaturityDate";
        public const string Status = "AssetPositions.Status";
        public const string LoadFailed = "AssetPositions.LoadFailed";
        public const string CreateSuccess = "AssetPositions.Create.Success";
        public const string CreateFailed = "AssetPositions.Create.Failed";
        public const string DeleteSuccess = "AssetPositions.Delete.Success";
        public const string DeleteFailed = "AssetPositions.Delete.Failed";
        public const string ConfirmDelete = "AssetPositions.ConfirmDelete";
        public const string TransactionType = "AssetPositions.TransactionType";
        public const string TransactionTypeBuy = "AssetPositions.TransactionType.Buy";
        public const string TransactionTypeSell = "AssetPositions.TransactionType.Sell";
        public const string Price = "AssetPositions.Price";
        public const string Fee = "AssetPositions.Fee";
        public const string TransactedAt = "AssetPositions.TransactedAt";
        public const string AddTransaction = "AssetPositions.AddTransaction";
        public const string Withdraw = "AssetPositions.Withdraw";
        public const string WithdrawSuccess = "AssetPositions.Withdraw.Success";
        public const string WithdrawFailed = "AssetPositions.Withdraw.Failed";
        public const string CurrentPrice = "AssetPositions.CurrentPrice";
        public const string UnrealizedPnl = "AssetPositions.UnrealizedPnl";
        public const string RefreshPrices = "AssetPositions.RefreshPrices";
    }

    public static class ApiClients
    {
        public const string Title = "ApiClients.Title";
        public const string ListTitle = "ApiClients.ListTitle";
        public const string CreateTitle = "ApiClients.Create.Title";
        public const string EditTitle = "ApiClients.Edit.Title";
        public const string ScopesTitle = "ApiClients.Scopes.Title";
        public const string SecretTitle = "ApiClients.Secret.Title";
        public const string ClientId = "ApiClients.ClientId";
        public const string ClientIdPlaceholder = "ApiClients.ClientIdPlaceholder";
        public const string Name = "ApiClients.Name";
        public const string Description = "ApiClients.Description";
        public const string IsActive = "ApiClients.IsActive";
        public const string Scopes = "ApiClients.Scopes";
        public const string SearchPlaceholder = "ApiClients.Search.Placeholder";
        public const string LoadFailed = "ApiClients.LoadFailed";
        public const string CreateSuccess = "ApiClients.Create.Success";
        public const string CreateFailed = "ApiClients.Create.Failed";
        public const string UpdateSuccess = "ApiClients.Update.Success";
        public const string UpdateFailed = "ApiClients.Update.Failed";
        public const string DeleteSuccess = "ApiClients.Delete.Success";
        public const string DeleteFailed = "ApiClients.Delete.Failed";
        public const string ConfirmDelete = "ApiClients.ConfirmDelete";
        public const string ActivateSuccess = "ApiClients.Activate.Success";
        public const string ActivateFailed = "ApiClients.Activate.Failed";
        public const string DeactivateSuccess = "ApiClients.Deactivate.Success";
        public const string DeactivateFailed = "ApiClients.Deactivate.Failed";
        public const string RegenerateSecretSuccess = "ApiClients.RegenerateSecret.Success";
        public const string RegenerateSecretFailed = "ApiClients.RegenerateSecret.Failed";
        public const string ConfirmRegenerateSecret = "ApiClients.ConfirmRegenerateSecret";
        public const string SecretNote = "ApiClients.Secret.Note";
        public const string CopySuccess = "ApiClients.Copy.Success";
        public const string CopyFailed = "ApiClients.Copy.Failed";
        public const string ScopesUpdatedSuccess = "ApiClients.Scopes.Updated.Success";
        public const string ScopesUpdatedFailed = "ApiClients.Scopes.Updated.Failed";
        public const string Active = "ApiClients.Active";
        public const string Inactive = "ApiClients.Inactive";
        public const string Actions = "ApiClients.Actions";
        public const string NoData = "ApiClients.NoData";
        public const string ActionEdit = "ApiClients.Action.Edit";
        public const string ActionActivate = "ApiClients.Action.Activate";
        public const string ActionDeactivate = "ApiClients.Action.Deactivate";
        public const string ActionRegenerateSecret = "ApiClients.Action.RegenerateSecret";
        public const string ActionManageScopes = "ApiClients.Action.ManageScopes";
        public const string AvailableScopes = "ApiClients.AvailableScopes";
    }

    public static class Notes
    {
        public const string Title = "Notes.Title";
        public const string CreateNote = "Notes.CreateNote";
        public const string EditNote = "Notes.EditNote";
        public const string TitlePlaceholder = "Notes.TitlePlaceholder";
        public const string ContentPlaceholder = "Notes.ContentPlaceholder";
        public const string TakeNote = "Notes.TakeNote";
        public const string Pin = "Notes.Pin";
        public const string Unpin = "Notes.Unpin";
        public const string Archive = "Notes.Archive";
        public const string Unarchive = "Notes.Unarchive";
        public const string Delete = "Notes.Delete";
        public const string Restore = "Notes.Restore";
        public const string FilterAll = "Notes.FilterAll";
        public const string FilterPinned = "Notes.FilterPinned";
        public const string FilterArchived = "Notes.FilterArchived";
        public const string FilterTrash = "Notes.FilterTrash";
        public const string NoNotes = "Notes.NoNotes";
        public const string ConfirmDelete = "Notes.ConfirmDelete";
        public const string CreateSuccess = "Notes.Create.Success";
        public const string UpdateSuccess = "Notes.Update.Success";
        public const string DeleteSuccess = "Notes.Delete.Success";
        public const string PinSuccess = "Notes.Pin.Success";
        public const string UnpinSuccess = "Notes.Unpin.Success";
        public const string ArchiveSuccess = "Notes.Archive.Success";
        public const string UnarchiveSuccess = "Notes.Unarchive.Success";
        public const string RestoreSuccess = "Notes.Restore.Success";
    }

    public static class NotificationSchedules
    {
        public const string Title = "NotificationSchedules.Title";
        public const string NewSchedule = "NotificationSchedules.NewSchedule";
        public const string Loading = "NotificationSchedules.Loading";
        public const string LoadFailed = "NotificationSchedules.LoadFailed";
        public const string Retry = "NotificationSchedules.Retry";

        public static class Column
        {
            public const string Name = "NotificationSchedules.Column.Name";
            public const string Type = "NotificationSchedules.Column.Type";
            public const string Status = "NotificationSchedules.Column.Status";
            public const string NextRun = "NotificationSchedules.Column.NextRun";
            public const string Runs = "NotificationSchedules.Column.Runs";
            public const string Failures = "NotificationSchedules.Column.Failures";
            public const string Actions = "NotificationSchedules.Column.Actions";
        }

        public static class Status
        {
            public const string All = "NotificationSchedules.Status.All";
            public const string Active = "NotificationSchedules.Status.Active";
            public const string Paused = "NotificationSchedules.Status.Paused";
            public const string Disabled = "NotificationSchedules.Status.Disabled";
            public const string Completed = "NotificationSchedules.Status.Completed";
        }

        public static class Action
        {
            public const string Create = "NotificationSchedules.Action.Create";
            public const string Update = "NotificationSchedules.Action.Update";
            public const string Edit = "NotificationSchedules.Action.Edit";
            public const string RunNow = "NotificationSchedules.Action.RunNow";
            public const string Pause = "NotificationSchedules.Action.Pause";
            public const string Activate = "NotificationSchedules.Action.Activate";
        }

        public static class Form
        {
            public const string Name = "NotificationSchedules.Form.Name";
            public const string TemplateKey = "NotificationSchedules.Form.TemplateKey";
            public const string Subject = "NotificationSchedules.Form.Subject";
            public const string Body = "NotificationSchedules.Form.Body";
            public const string ScheduleType = "NotificationSchedules.Form.ScheduleType";
            public const string ExecuteTimeLocal = "NotificationSchedules.Form.ExecuteTimeLocal";
            public const string TimeZoneId = "NotificationSchedules.Form.TimeZoneId";
            public const string IntervalDays = "NotificationSchedules.Form.IntervalDays";
            public const string DayOfWeek = "NotificationSchedules.Form.DayOfWeek";
            public const string DayOfMonth = "NotificationSchedules.Form.DayOfMonth";
            public const string MonthlyOverflowPolicy = "NotificationSchedules.Form.MonthlyOverflowPolicy";
            public const string StartDateLocal = "NotificationSchedules.Form.StartDateLocal";
            public const string OneTimeAtLocal = "NotificationSchedules.Form.OneTimeAtLocal";

            public static class Option
            {
                public const string OneTime = "NotificationSchedules.Form.Option.OneTime";
                public const string EveryXDays = "NotificationSchedules.Form.Option.EveryXDays";
                public const string Weekly = "NotificationSchedules.Form.Option.Weekly";
                public const string Monthly = "NotificationSchedules.Form.Option.Monthly";
                public const string Monday = "NotificationSchedules.Form.Option.Monday";
                public const string Tuesday = "NotificationSchedules.Form.Option.Tuesday";
                public const string Wednesday = "NotificationSchedules.Form.Option.Wednesday";
                public const string Thursday = "NotificationSchedules.Form.Option.Thursday";
                public const string Friday = "NotificationSchedules.Form.Option.Friday";
                public const string Saturday = "NotificationSchedules.Form.Option.Saturday";
                public const string Sunday = "NotificationSchedules.Form.Option.Sunday";
                public const string SkipMonth = "NotificationSchedules.Form.Option.SkipMonth";
                public const string RunOnLastDay = "NotificationSchedules.Form.Option.RunOnLastDay";
            }
        }

        public const string RunNowSuccess = "NotificationSchedules.RunNow.Success";
        public const string RunNowMessage = "NotificationSchedules.RunNow.Message";
        public const string RunNowFailed = "NotificationSchedules.RunNow.Failed";
        public const string ConfirmDelete = "NotificationSchedules.ConfirmDelete";
        public const string DeleteSuccess = "NotificationSchedules.Delete.Success";
        public const string CreateSuccess = "NotificationSchedules.Create.Success";
        public const string CreateFailed = "NotificationSchedules.Create.Failed";
        public const string UpdateSuccess = "NotificationSchedules.Update.Success";
        public const string UpdateFailed = "NotificationSchedules.Update.Failed";
        public const string ValidationFixErrors = "NotificationSchedules.Validation.FixErrors";
    }

    public static class Resume
    {
        public const string AdminTitle = "Resume.AdminTitle";
        public const string BasicInfo = "Resume.BasicInfo";
        public const string Slug = "Resume.Slug";
        public const string FullName = "Resume.FullName";
        public const string Headline = "Resume.Headline";
        public const string MetaDescription = "Resume.MetaDescription";
        public const string Published = "Resume.Published";
        public const string Summary = "Resume.Summary";
        public const string Skills = "Resume.Skills";
        public const string SkillsPlaceholder = "Resume.SkillsPlaceholder";
        public const string Experience = "Resume.Experience";
        public const string Projects = "Resume.Projects";
        public const string Education = "Resume.Education";
        public const string Links = "Resume.Links";
        public const string AddRow = "Resume.AddRow";
        public const string Remove = "Resume.Remove";
        public const string Company = "Resume.Company";
        public const string Position = "Resume.Position";
        public const string StartDate = "Resume.StartDate";
        public const string EndDate = "Resume.EndDate";
        public const string Present = "Resume.Present";
        public const string PageTitleFallback = "Resume.PageTitleFallback";
        public const string ProjectLinkPlaceholder = "Resume.ProjectLinkPlaceholder";
        public const string ProfileLinkPlaceholder = "Resume.ProfileLinkPlaceholder";
        public const string Description = "Resume.Description";
        public const string ProjectName = "Resume.ProjectName";
        public const string TechStack = "Resume.TechStack";
        public const string LinkUrl = "Resume.LinkUrl";
        public const string School = "Resume.School";
        public const string Degree = "Resume.Degree";
        public const string LinkLabel = "Resume.LinkLabel";
        public const string Save = "Resume.Save";
        public const string SaveSuccess = "Resume.SaveSuccess";
        public const string SaveFailed = "Resume.SaveFailed";
        public const string LoadFailed = "Resume.LoadFailed";
        public const string ViewPublic = "Resume.ViewPublic";
        public const string NotFoundTitle = "Resume.NotFoundTitle";
        public const string NotFoundMessage = "Resume.NotFoundMessage";
        public const string DownloadButton = "Resume.DownloadButton";
        public const string Preview = "Resume.Preview";
        public const string PreviewTitle = "Resume.PreviewTitle";
        public const string OptimizeForJob = "Resume.OptimizeForJob";
        public const string CheckConsistency = "Resume.CheckConsistency";
        public const string ConsistencyReportTitle = "Resume.ConsistencyReportTitle";
        public const string Polish = "Resume.Polish";
        public const string AutoGenerate = "Resume.AutoGenerate";
        public const string AiRequestFailed = "Resume.AiRequestFailed";
        public const string JobDescription = "Resume.JobDescription";
        public const string Analyze = "Resume.Analyze";
    }

    public static class Power655
    {
        public const string Title = "Power655.Title";
        public const string SyncButton = "Power655.SyncButton";
        public const string PredictButton = "Power655.PredictButton";
        public const string SyncConfirmTitle = "Power655.SyncConfirmTitle";
        public const string SyncConfirmMessage = "Power655.SyncConfirmMessage";
        public const string SyncSuccess = "Power655.SyncSuccess";
        public const string SyncFailed = "Power655.SyncFailed";
        public const string PredictSuccess = "Power655.PredictSuccess";
        public const string PredictFailed = "Power655.PredictFailed";
        public const string LoadFailed = "Power655.LoadFailed";
        public const string MostFrequent = "Power655.MostFrequent";
        public const string LeastFrequent = "Power655.LeastFrequent";
        public const string HotLabel = "Power655.HotLabel";
        public const string ColdLabel = "Power655.ColdLabel";
        public const string Times = "Power655.Times";
        public const string Predictions = "Power655.Predictions";
        public const string CopySuccess = "Power655.CopySuccess";
        public const string CopyFailed = "Power655.CopyFailed";
        public const string Type = "Power655.Type";
        public const string TupleFreq = "Power655.TupleFreq";
        public const string Reasoning = "Power655.Reasoning";
        public const string MatchedNumbers = "Power655.MatchedNumbers";
        public const string DrawHistory = "Power655.DrawHistory";
        public const string DrawDate = "Power655.DrawDate";
        public const string DayOfWeek = "Power655.DayOfWeek";
        public const string Numbers = "Power655.Numbers";
        public const string BonusNum = "Power655.BonusNum";
        public const string Jackpot1 = "Power655.Jackpot1";
        public const string Jackpot2 = "Power655.Jackpot2";

        public static string DayOfWeekLabel(string dayOfWeek) => dayOfWeek switch
        {
            "Monday" => "DayOfWeek.Monday",
            "Tuesday" => "DayOfWeek.Tuesday",
            "Wednesday" => "DayOfWeek.Wednesday",
            "Thursday" => "DayOfWeek.Thursday",
            "Friday" => "DayOfWeek.Friday",
            "Saturday" => "DayOfWeek.Saturday",
            "Sunday" => "DayOfWeek.Sunday",
            _ => dayOfWeek
        };
    }

    public static class Blog
    {
        public const string PublicFallbackTitle = "Blog.PublicFallbackTitle";
        public const string PublicTitleFormat = "Blog.PublicTitleFormat";

        public static class Editor
        {
            public const string InsertImage = "Blog.Editor.InsertImage";
            public const string ImageTooLarge = "Blog.Editor.ImageTooLarge";
            public const string ImageUploadFailed = "Blog.Editor.ImageUploadFailed";
        }

        public static class Posts
        {
            public const string Title = "Blog.Posts.Title";
            public const string New = "Blog.Posts.New";
            public const string ColumnTitle = "Blog.Posts.ColumnTitle";
            public const string ColumnStatus = "Blog.Posts.ColumnStatus";
            public const string ColumnCategory = "Blog.Posts.ColumnCategory";
            public const string ColumnPublishedAt = "Blog.Posts.ColumnPublishedAt";
            public const string ColumnReadTime = "Blog.Posts.ColumnReadTime";
            public const string StatusDraft = "Blog.Posts.StatusDraft";
            public const string StatusScheduled = "Blog.Posts.StatusScheduled";
            public const string StatusPublished = "Blog.Posts.StatusPublished";
            public const string StatusArchived = "Blog.Posts.StatusArchived";
            public const string FilterAllStatuses = "Blog.Posts.FilterAllStatuses";
            public const string SearchPlaceholder = "Blog.Posts.SearchPlaceholder";
            public const string ActionDelete = "Blog.Posts.ActionDelete";
            public const string ActionPublish = "Blog.Posts.ActionPublish";
            public const string ActionUnpublish = "Blog.Posts.ActionUnpublish";
            public const string ActionArchive = "Blog.Posts.ActionArchive";
            public const string ConfirmDeleteMessage = "Blog.Posts.ConfirmDeleteMessage";
            public const string LoadFailed = "Blog.Posts.LoadFailed";
            public const string SaveSuccess = "Blog.Posts.SaveSuccess";
            public const string SaveFailed = "Blog.Posts.SaveFailed";
            public const string ActionSuccess = "Blog.Posts.ActionSuccess";
            public const string ActionFailed = "Blog.Posts.ActionFailed";
            public const string EditorTitleLabel = "Blog.Posts.EditorTitleLabel";
            public const string SlugLabel = "Blog.Posts.SlugLabel";
            public const string SummaryLabel = "Blog.Posts.SummaryLabel";
            public const string ContentLabel = "Blog.Posts.ContentLabel";
            public const string CategoryLabel = "Blog.Posts.CategoryLabel";
            public const string TagsLabel = "Blog.Posts.TagsLabel";
            public const string FeaturedImageLabel = "Blog.Posts.FeaturedImageLabel";
            public const string MetaTitleLabel = "Blog.Posts.MetaTitleLabel";
            public const string MetaDescriptionLabel = "Blog.Posts.MetaDescriptionLabel";
            public const string SchedulePublishAt = "Blog.Posts.SchedulePublishAt";
            public const string ScheduleConfirm = "Blog.Posts.ScheduleConfirm";
            public const string NewPostTitle = "Blog.Posts.NewPostTitle";
            public const string EditPostTitle = "Blog.Posts.EditPostTitle";
            public const string SectionPublish = "Blog.Posts.SectionPublish";
            public const string SectionOrganize = "Blog.Posts.SectionOrganize";
            public const string SectionSeo = "Blog.Posts.SectionSeo";
        }

        public static class Ai
        {
            public const string GenerateTitle = "Blog.Ai.GenerateTitle";
            public const string GenerateSummary = "Blog.Ai.GenerateSummary";
            public const string Rewrite = "Blog.Ai.Rewrite";
            public const string ImproveGrammar = "Blog.Ai.ImproveGrammar";
            public const string GenerateTags = "Blog.Ai.GenerateTags";
            public const string GenerateSeo = "Blog.Ai.GenerateSeo";
            public const string Review = "Blog.Ai.Review";
            public const string ActionFailed = "Blog.Ai.ActionFailed";
        }

        public static class Categories
        {
            public const string Title = "Blog.Categories.Title";
            public const string New = "Blog.Categories.New";
            public const string NameLabel = "Blog.Categories.NameLabel";
            public const string SlugLabel = "Blog.Categories.SlugLabel";
            public const string DescriptionLabel = "Blog.Categories.DescriptionLabel";
            public const string IconLabel = "Blog.Categories.IconLabel";
            public const string ColorLabel = "Blog.Categories.ColorLabel";
            public const string DeleteConfirm = "Blog.Categories.DeleteConfirm";
            public const string SaveSuccess = "Blog.Categories.SaveSuccess";
            public const string SaveFailed = "Blog.Categories.SaveFailed";
            public const string DeleteSuccess = "Blog.Categories.DeleteSuccess";
            public const string DeleteFailed = "Blog.Categories.DeleteFailed";
            public const string LoadFailed = "Blog.Categories.LoadFailed";
            public const string SearchPlaceholder = "Blog.Categories.SearchPlaceholder";
        }

        public static class Tags
        {
            public const string Title = "Blog.Tags.Title";
            public const string SearchPlaceholder = "Blog.Tags.SearchPlaceholder";
            public const string New = "Blog.Tags.New";
            public const string DeleteConfirm = "Blog.Tags.DeleteConfirm";
            public const string SaveFailed = "Blog.Tags.SaveFailed";
            public const string DeleteFailed = "Blog.Tags.DeleteFailed";
        }

        public static class Public
        {
            public const string ListTitle = "Blog.Public.ListTitle";
            public const string ReadMore = "Blog.Public.ReadMore";
            public const string NoPostsFound = "Blog.Public.NoPostsFound";
            public const string MinRead = "Blog.Public.MinRead";
            public const string LoadFailed = "Blog.Public.LoadFailed";
            public const string NotFoundTitle = "Blog.Public.NotFoundTitle";
            public const string NotFoundMessage = "Blog.Public.NotFoundMessage";
        }
    }

    public static class JsonBins
    {
        public const string Title = "JsonBins.Title";
        public const string ListTitle = "JsonBins.ListTitle";
        public const string CreateTitle = "JsonBins.Create.Title";
        public const string EditTitle = "JsonBins.Edit.Title";
        public const string Create = "JsonBins.Create";
        public const string Edit = "JsonBins.Edit";
        public const string ConfirmDelete = "JsonBins.ConfirmDelete";
        public const string SearchPlaceholder = "JsonBins.Search.Placeholder";
        public const string Code = "JsonBins.Code";
        public const string CodeOptionalHint = "JsonBins.Code.OptionalHint";
        public const string CodeAutoPlaceholder = "JsonBins.Code.AutoPlaceholder";
        public const string CodeWillAutoGenerate = "JsonBins.Code.WillAutoGenerate";
        public const string CodeKeepExisting = "JsonBins.Code.KeepExisting";
        public const string CodeExists = "JsonBins.Code.Exists";
        public const string CodeAvailable = "JsonBins.Code.Available";
        public const string Share = "JsonBins.Share";
        public const string RevokeShare = "JsonBins.RevokeShare";
        public const string ShareLink = "JsonBins.ShareLink";
        public const string SharedTitle = "JsonBins.Shared.Title";
        public const string ShareNotFound = "JsonBins.Share.NotFound";
        public const string ShareExpires = "JsonBins.Share.Expires";
        public const string Name = "JsonBins.Name";
        public const string Category = "JsonBins.Category";
        public const string Size = "JsonBins.Size";
        public const string Expires = "JsonBins.Expires";
        public const string Actions = "JsonBins.Actions";
        public const string Content = "JsonBins.Content";
        public const string Compress = "JsonBins.Compress";
        public const string Expire = "JsonBins.Expire";
        public const string NotFound = "JsonBins.NotFound";
        public const string NameRequired = "JsonBins.Name.Required";
        public const string ContentRequired = "JsonBins.Content.Required";
        public const string LoadFailed = "JsonBins.LoadFailed";
        public const string CreateSuccess = "JsonBins.Create.Success";
        public const string CreateFailed = "JsonBins.Create.Failed";
        public const string UpdateSuccess = "JsonBins.Update.Success";
        public const string UpdateFailed = "JsonBins.Update.Failed";
        public const string DeleteSuccess = "JsonBins.Delete.Success";
        public const string DeleteFailed = "JsonBins.Delete.Failed";
        public const string ExpireSuccess = "JsonBins.Expire.Success";
        public const string ExpireFailed = "JsonBins.Expire.Failed";
        public const string ShareSuccess = "JsonBins.Share.Success";
        public const string ShareFailed = "JsonBins.Share.Failed";
        public const string RevokeShareSuccess = "JsonBins.RevokeShare.Success";
        public const string RevokeShareFailed = "JsonBins.RevokeShare.Failed";
        public const string CopySuccess = "JsonBins.Copy.Success";
        public const string CopyFailed = "JsonBins.Copy.Failed";
        public const string ValidateCodeFailed = "JsonBins.ValidateCode.Failed";
    }

    public static class Tools
    {
        public const string HubTitle = "Tools.Hub.Title";
        public const string HubDescription = "Tools.Hub.Description";
        public const string SearchPlaceholder = "Tools.Search.Placeholder";
        public const string Save = "Tools.Save";
        public const string Share = "Tools.Share";
        public const string ClearDraft = "Tools.ClearDraft";
        public const string ShareLink = "Tools.ShareLink";
        public const string Input = "Tools.Input";
        public const string Output = "Tools.Output";
        public const string QrDownload = "Tools.QrDownload";
        public const string NotFound = "Tools.NotFound";
        public const string DraftRestored = "Tools.Draft.Restored";
        public const string SaveSuccess = "Tools.Save.Success";
        public const string SaveFailed = "Tools.Save.Failed";
        public const string ShareSuccess = "Tools.Share.Success";
        public const string ShareFailed = "Tools.Share.Failed";
        public const string ClearSuccess = "Tools.Clear.Success";
        public const string SignInRequired = "Tools.SignInRequired";
        public const string CloudBinNamePrefix = "Tools.CloudBinNamePrefix";
        public const string PrivacyLocalOnly = "Tools.Privacy.LocalOnly";
        public const string PrivacyCloudSync = "Tools.Privacy.CloudSync";
        public const string ValidationTitle = "Tools.Validation.Title";
        public const string ValidationGeneric = "Tools.Validation.Generic";

        public static class Category
        {
            public const string Developer = "Tools.Category.Developer";
            public const string Trading = "Tools.Category.Trading";
            public const string Risk = "Tools.Category.Risk";
            public const string Crypto = "Tools.Category.Crypto";
            public const string Data = "Tools.Category.Data";
            public const string CodeGen = "Tools.Category.CodeGen";
            public const string Format = "Tools.Category.Format";
            public const string Encode = "Tools.Category.Encode";
            public const string Security = "Tools.Category.Security";
            public const string Generators = "Tools.Category.Generators";
            public const string TextTime = "Tools.Category.TextTime";
        }

        public static class PositionSize
        {
            public const string Title = "Tools.PositionSize.Title";
            public const string Description = "Tools.PositionSize.Description";
        }

        public static class RiskReward
        {
            public const string Title = "Tools.RiskReward.Title";
            public const string Description = "Tools.RiskReward.Description";
        }

        public static class FuturesPnl
        {
            public const string Title = "Tools.FuturesPnl.Title";
            public const string Description = "Tools.FuturesPnl.Description";
        }

        public static class Liquidation
        {
            public const string Title = "Tools.Liquidation.Title";
            public const string Description = "Tools.Liquidation.Description";
        }

        public static class AverageEntry
        {
            public const string Title = "Tools.AverageEntry.Title";
            public const string Description = "Tools.AverageEntry.Description";
        }

        public static class Dca
        {
            public const string Title = "Tools.Dca.Title";
            public const string Description = "Tools.Dca.Description";
        }

        public static class BreakEven
        {
            public const string Title = "Tools.BreakEven.Title";
            public const string Description = "Tools.BreakEven.Description";
        }

        public static class TradingFee
        {
            public const string Title = "Tools.TradingFee.Title";
            public const string Description = "Tools.TradingFee.Description";
        }

        public static class Funding
        {
            public const string Title = "Tools.Funding.Title";
            public const string Description = "Tools.Funding.Description";
        }

        public static class Leverage
        {
            public const string Title = "Tools.Leverage.Title";
            public const string Description = "Tools.Leverage.Description";
        }

        public static class StopLoss
        {
            public const string Title = "Tools.StopLoss.Title";
            public const string Description = "Tools.StopLoss.Description";
        }

        public static class TakeProfit
        {
            public const string Title = "Tools.TakeProfit.Title";
            public const string Description = "Tools.TakeProfit.Description";
        }

        public static class RiskOfRuin
        {
            public const string Title = "Tools.RiskOfRuin.Title";
            public const string Description = "Tools.RiskOfRuin.Description";
        }

        public static class MaxDrawdown
        {
            public const string Title = "Tools.MaxDrawdown.Title";
            public const string Description = "Tools.MaxDrawdown.Description";
        }

        public static class LossRecovery
        {
            public const string Title = "Tools.LossRecovery.Title";
            public const string Description = "Tools.LossRecovery.Description";
        }

        public static class Expectancy
        {
            public const string Title = "Tools.Expectancy.Title";
            public const string Description = "Tools.Expectancy.Description";
        }

        public static class KellyCriterion
        {
            public const string Title = "Tools.KellyCriterion.Title";
            public const string Description = "Tools.KellyCriterion.Description";
        }

        public static class ConsecutiveLoss
        {
            public const string Title = "Tools.ConsecutiveLoss.Title";
            public const string Description = "Tools.ConsecutiveLoss.Description";
        }

        public static class MarketCap
        {
            public const string Title = "Tools.MarketCap.Title";
            public const string Description = "Tools.MarketCap.Description";
        }

        public static class MarketCapCompare
        {
            public const string Title = "Tools.MarketCapCompare.Title";
            public const string Description = "Tools.MarketCapCompare.Description";
        }

        public static class AthDrawdown
        {
            public const string Title = "Tools.AthDrawdown.Title";
            public const string Description = "Tools.AthDrawdown.Description";
        }

        public static class JsonFormatter
        {
            public const string Title = "Tools.JsonFormatter.Title";
            public const string Description = "Tools.JsonFormatter.Description";
        }

        public static class JsonCompare
        {
            public const string Title = "Tools.JsonCompare.Title";
            public const string Description = "Tools.JsonCompare.Description";
        }

        public static class YamlValidator
        {
            public const string Title = "Tools.YamlValidator.Title";
            public const string Description = "Tools.YamlValidator.Description";
        }

        public static class YamlJson
        {
            public const string Title = "Tools.YamlJson.Title";
            public const string Description = "Tools.YamlJson.Description";
        }

        public static class XmlFormatter
        {
            public const string Title = "Tools.XmlFormatter.Title";
            public const string Description = "Tools.XmlFormatter.Description";
        }

        public static class CsvJson
        {
            public const string Title = "Tools.CsvJson.Title";
            public const string Description = "Tools.CsvJson.Description";
        }

        public static class JsonToCsharp
        {
            public const string Title = "Tools.JsonToCsharp.Title";
            public const string Description = "Tools.JsonToCsharp.Description";
        }

        public static class JsonToTypescript
        {
            public const string Title = "Tools.JsonToTypescript.Title";
            public const string Description = "Tools.JsonToTypescript.Description";
        }

        public static class SqlFormatter
        {
            public const string Title = "Tools.SqlFormatter.Title";
            public const string Description = "Tools.SqlFormatter.Description";
        }

        public static class HtmlFormatter
        {
            public const string Title = "Tools.HtmlFormatter.Title";
            public const string Description = "Tools.HtmlFormatter.Description";
        }

        public static class CssFormatter
        {
            public const string Title = "Tools.CssFormatter.Title";
            public const string Description = "Tools.CssFormatter.Description";
        }

        public static class JsFormatter
        {
            public const string Title = "Tools.JsFormatter.Title";
            public const string Description = "Tools.JsFormatter.Description";
        }

        public static class MarkdownPreview
        {
            public const string Title = "Tools.MarkdownPreview.Title";
            public const string Description = "Tools.MarkdownPreview.Description";
            public const string Preview = "Tools.MarkdownPreview.Preview";
            public const string ShowEditor = "Tools.MarkdownPreview.ShowEditor";
            public const string Toolbar = "Tools.MarkdownPreview.Toolbar";
            public const string Heading = "Tools.MarkdownPreview.Heading";
            public const string Bold = "Tools.MarkdownPreview.Bold";
            public const string Italic = "Tools.MarkdownPreview.Italic";
            public const string Strike = "Tools.MarkdownPreview.Strike";
            public const string Quote = "Tools.MarkdownPreview.Quote";
            public const string BulletList = "Tools.MarkdownPreview.BulletList";
            public const string NumberedList = "Tools.MarkdownPreview.NumberedList";
            public const string InlineCode = "Tools.MarkdownPreview.InlineCode";
            public const string CodeBlock = "Tools.MarkdownPreview.CodeBlock";
            public const string Link = "Tools.MarkdownPreview.Link";
            public const string HorizontalRule = "Tools.MarkdownPreview.HorizontalRule";
            public const string EditorPlaceholder = "Tools.MarkdownPreview.EditorPlaceholder";
            public const string CopyAll = "Tools.MarkdownPreview.CopyAll";
            public const string CopyHtml = "Tools.MarkdownPreview.CopyHtml";
            public const string CopyAllSuccess = "Tools.MarkdownPreview.CopyAllSuccess";
            public const string CopyHtmlSuccess = "Tools.MarkdownPreview.CopyHtmlSuccess";
            public const string CopyFailed = "Tools.MarkdownPreview.CopyFailed";
        }

        public static class HtmlMarkdown
        {
            public const string Title = "Tools.HtmlMarkdown.Title";
            public const string Description = "Tools.HtmlMarkdown.Description";
        }

        public static class Base64
        {
            public const string Title = "Tools.Base64.Title";
            public const string Description = "Tools.Base64.Description";
        }

        public static class UrlCodec
        {
            public const string Title = "Tools.UrlCodec.Title";
            public const string Description = "Tools.UrlCodec.Description";
        }

        public static class Sha256
        {
            public const string Title = "Tools.Sha256.Title";
            public const string Description = "Tools.Sha256.Description";
        }

        public static class JwtDecoder
        {
            public const string Title = "Tools.JwtDecoder.Title";
            public const string Description = "Tools.JwtDecoder.Description";
        }

        public static class PasswordGenerator
        {
            public const string Title = "Tools.PasswordGenerator.Title";
            public const string Description = "Tools.PasswordGenerator.Description";
        }

        public static class PasswordStrength
        {
            public const string Title = "Tools.PasswordStrength.Title";
            public const string Description = "Tools.PasswordStrength.Description";
        }

        public static class Uuid
        {
            public const string Title = "Tools.Uuid.Title";
            public const string Description = "Tools.Uuid.Description";
        }

        public static class Cron
        {
            public const string Title = "Tools.Cron.Title";
            public const string Description = "Tools.Cron.Description";
        }

        public static class Qr
        {
            public const string Title = "Tools.Qr.Title";
            public const string Description = "Tools.Qr.Description";
        }

        public static class Curl
        {
            public const string Title = "Tools.Curl.Title";
            public const string Description = "Tools.Curl.Description";
        }

        public static class Gitignore
        {
            public const string Title = "Tools.Gitignore.Title";
            public const string Description = "Tools.Gitignore.Description";
        }

        public static class ConventionalCommit
        {
            public const string Title = "Tools.ConventionalCommit.Title";
            public const string Description = "Tools.ConventionalCommit.Description";
        }

        public static class Regex
        {
            public const string Title = "Tools.Regex.Title";
            public const string Description = "Tools.Regex.Description";
        }

        public static class UnixTimestamp
        {
            public const string Title = "Tools.UnixTimestamp.Title";
            public const string Description = "Tools.UnixTimestamp.Description";
        }

        public static class Diff
        {
            public const string Title = "Tools.Diff.Title";
            public const string Description = "Tools.Diff.Description";
        }
    }

    public static class ExpenseTracker
    {
        public const string Title = "ExpenseTracker.Title";
        public const string Wallets = "ExpenseTracker.Wallets";
        public const string Categories = "ExpenseTracker.Categories";
        public const string Transactions = "ExpenseTracker.Transactions";
        public const string Transfers = "ExpenseTracker.Transfers";
        public const string Budgets = "ExpenseTracker.Budgets";
        public const string SavingGoals = "ExpenseTracker.SavingGoals";
        public const string Tags = "ExpenseTracker.Tags";
        public const string TransactionTags = "ExpenseTracker.TransactionTags";
        public const string RecurringTransactions = "ExpenseTracker.RecurringTransactions";
        public const string TotalAsset = "ExpenseTracker.TotalAsset";
        public const string IncomeToday = "ExpenseTracker.IncomeToday";
        public const string ExpenseToday = "ExpenseTracker.ExpenseToday";
        public const string CashFlow = "ExpenseTracker.CashFlow";
        public const string Name = "ExpenseTracker.Name";
        public const string Type = "ExpenseTracker.Type";
        public const string Currency = "ExpenseTracker.Currency";
        public const string Balance = "ExpenseTracker.Balance";
        public const string Amount = "ExpenseTracker.Amount";
        public const string Note = "ExpenseTracker.Note";
        public const string Period = "ExpenseTracker.Period";
        public const string StartDate = "ExpenseTracker.StartDate";
        public const string EndDate = "ExpenseTracker.EndDate";
        public const string TargetAmount = "ExpenseTracker.TargetAmount";
        public const string CurrentAmount = "ExpenseTracker.CurrentAmount";
        public const string Color = "ExpenseTracker.Color";
        public const string Transaction = "ExpenseTracker.Transaction";
        public const string Tag = "ExpenseTracker.Tag";
        public const string Frequency = "ExpenseTracker.Frequency";
        public const string NextRun = "ExpenseTracker.NextRun";
        public const string CreateTitle = "ExpenseTracker.Create.Title";
        public const string EditTitle = "ExpenseTracker.Edit.Title";
        public const string ConfirmDelete = "ExpenseTracker.ConfirmDelete";
        public const string SaveSuccess = "ExpenseTracker.Save.Success";
        public const string SaveFailed = "ExpenseTracker.Save.Failed";
        public const string DeleteSuccess = "ExpenseTracker.Delete.Success";
        public const string DeleteFailed = "ExpenseTracker.Delete.Failed";
        public const string SourceWallet = "ExpenseTracker.SourceWallet";
        public const string TargetWallet = "ExpenseTracker.TargetWallet";
        public const string TargetDate = "ExpenseTracker.TargetDate";
        public const string InitialBalance = "ExpenseTracker.InitialBalance";
    }

    public static class Calc
    {
        public const string Calculate = "Calc.Calculate";
        public const string Reset = "Calc.Reset";
        public const string Results = "Calc.Results";
        public const string Formula = "Calc.Formula";
        public const string Interpretation = "Calc.Interpretation";
        public const string RiskDisclaimer = "Calc.RiskDisclaimer";
        public const string Yes = "Calc.Yes";
        public const string No = "Calc.No";
        public const string SideLong = "Calc.Side.Long";
        public const string SideShort = "Calc.Side.Short";
        public const string FeeTaker = "Calc.Fee.Taker";
        public const string FeeMaker = "Calc.Fee.Maker";
        public const string FeeCustom = "Calc.Fee.Custom";
        public const string CurrencyUsdt = "Calc.Currency.USDT";
        public const string CurrencyUsd = "Calc.Currency.USD";
        public const string CurrencyVnd = "Calc.Currency.VND";
        public const string CurrencyBtc = "Calc.Currency.BTC";
        public const string MarginIsolated = "Calc.Margin.Isolated";

        public static class Field
        {
            public const string Currency = "Calc.Field.Currency";
            public const string Side = "Calc.Field.Side";
            public const string FeeMode = "Calc.Field.FeeMode";
            public const string Account = "Calc.Field.Account";
            public const string RiskPct = "Calc.Field.RiskPct";
            public const string Entry = "Calc.Field.Entry";
            public const string Exit = "Calc.Field.Exit";
            public const string StopLoss = "Calc.Field.StopLoss";
            public const string TakeProfit = "Calc.Field.TakeProfit";
            public const string Size = "Calc.Field.Size";
            public const string Leverage = "Calc.Field.Leverage";
            public const string MakerPct = "Calc.Field.MakerPct";
            public const string TakerPct = "Calc.Field.TakerPct";
            public const string OpenFeePct = "Calc.Field.OpenFeePct";
            public const string CloseFeePct = "Calc.Field.CloseFeePct";
            public const string FeePct = "Calc.Field.FeePct";
            public const string MmrPct = "Calc.Field.MmrPct";
            public const string Notional = "Calc.Field.Notional";
            public const string Margin = "Calc.Field.Margin";
            public const string FundingRatePct = "Calc.Field.FundingRatePct";
            public const string Periods = "Calc.Field.Periods";
            public const string TargetPct = "Calc.Field.TargetPct";
            public const string TargetRr = "Calc.Field.TargetRr";
            public const string LegsPriceQty = "Calc.Field.LegsPriceQty";
            public const string LegsPriceAmount = "Calc.Field.LegsPriceAmount";
            public const string WinRatePct = "Calc.Field.WinRatePct";
            public const string RiskReward = "Calc.Field.RiskReward";
            public const string RiskPerTradePct = "Calc.Field.RiskPerTradePct";
            public const string Peak = "Calc.Field.Peak";
            public const string Trough = "Calc.Field.Trough";
            public const string EquitySeries = "Calc.Field.EquitySeries";
            public const string LossPct = "Calc.Field.LossPct";
            public const string AvgWin = "Calc.Field.AvgWin";
            public const string AvgLoss = "Calc.Field.AvgLoss";
            public const string PayoffRatio = "Calc.Field.PayoffRatio";
            public const string Equity = "Calc.Field.Equity";
            public const string Losses = "Calc.Field.Losses";
            public const string Price = "Calc.Field.Price";
            public const string Supply = "Calc.Field.Supply";
            public const string TargetMcap = "Calc.Field.TargetMcap";
            public const string Ath = "Calc.Field.Ath";
            public const string Current = "Calc.Field.Current";
        }

        public static class Placeholder
        {
            public const string LegsPriceQty = "Calc.Placeholder.LegsPriceQty";
            public const string LegsPriceAmount = "Calc.Placeholder.LegsPriceAmount";
            public const string EquitySeries = "Calc.Placeholder.EquitySeries";
        }
    }
}

