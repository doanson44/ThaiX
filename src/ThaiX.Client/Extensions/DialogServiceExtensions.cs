using Microsoft.Extensions.Localization;
using Radzen;
using ThaiX.Client.Constants;

namespace ThaiX.Client.Extensions;

public static class DialogServiceExtensions
{
    public static Task<bool?> ConfirmDeleteAsync(
        this DialogService dialogService,
        IStringLocalizer<Localization.SharedResource> localizer,
        string message)
    {
        return dialogService.Confirm(
            message,
            localizer[ResourceKeys.Common.Delete],
            new ConfirmOptions
            {
                OkButtonText = localizer[ResourceKeys.Common.Delete],
                CancelButtonText = localizer[ResourceKeys.Common.Cancel]
            });
    }
}
