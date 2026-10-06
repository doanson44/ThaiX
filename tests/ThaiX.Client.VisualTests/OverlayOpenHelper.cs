using Microsoft.Playwright;

namespace ThaiX.Client.VisualTests;

internal static class OverlayOpenHelper
{
    public static async Task ExecuteAsync(IPage page, AuthFixture auth, OverlayCatalog.OverlayCase overlay)
    {
        await auth.GotoAsync(page, overlay.HostRoute, overlay.Width, overlay.Height, settleMs: 700);

        foreach (var step in overlay.Steps)
        {
            await ExecuteStepAsync(page, auth, step, overlay.Width, overlay.Height);
            await page.WaitForTimeoutAsync(200);
        }

        var overlayLocator = page.Locator(overlay.AssertSelector).First;
        await overlayLocator.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 60000
        });
    }

    private static async Task ExecuteStepAsync(
        IPage page,
        AuthFixture auth,
        OverlayCatalog.OverlayStep step,
        int width,
        int height)
    {
        switch (step.Kind)
        {
            case OverlayCatalog.OverlayStepKind.ClickAddIcon:
                await ClickIconAsync(page, "add");
                break;

            case OverlayCatalog.OverlayStepKind.ClickIcon:
                await ClickIconAsync(page, step.Arg ?? throw new InvalidOperationException("ClickIcon requires Arg."));
                break;

            case OverlayCatalog.OverlayStepKind.ClickCss:
                var clickTarget = page.Locator(step.Arg!).First;
                if (step.Force)
                {
                    // Force DOM click — needed on pages that re-render continuously (e.g. MEXC socket).
                    await clickTarget.WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Attached,
                        Timeout = 30000
                    });
                    await clickTarget.DispatchEventAsync("click");
                }
                else
                {
                    await clickTarget.ClickAsync(new LocatorClickOptions { Timeout = 30000 });
                }

                break;

            case OverlayCatalog.OverlayStepKind.FillCss:
                await page.Locator(step.Arg!).First.FillAsync(
                    step.Value ?? string.Empty,
                    new LocatorFillOptions { Timeout = 30000 });
                break;

            case OverlayCatalog.OverlayStepKind.WaitCss:
                await page.Locator(step.Arg!).First.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 60000
                });
                break;

            case OverlayCatalog.OverlayStepKind.OpenFirstPortfolioDetail:
                await auth.GotoAsync(page, "/portfolio/list", width, height, settleMs: 700);
                await ClickIconAsync(page, "visibility");
                await AuthFixture.WaitForPageReadyAsync(page, "/portfolio/");
                await page.WaitForTimeoutAsync(500);
                break;

            case OverlayCatalog.OverlayStepKind.EnableSwitchThenSettings:
                // MexcSocketPriceMoveAlertBar opens the settings dialog as soon as the switch is turned on.
                var toggle = page.Locator(".rz-switch:not(.rz-state-disabled), .rz-switch").First;
                await toggle.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000
                });

                // Wait until the switch is interactive (AlertBar sets _isReady after localStorage load).
                await page.WaitForFunctionAsync(
                    @"() => {
                        const el = document.querySelector('.rz-switch');
                        return !!el && !el.classList.contains('rz-state-disabled') && !el.hasAttribute('disabled');
                    }",
                    null,
                    new PageWaitForFunctionOptions { Timeout = 30000 });

                var alreadyEnabled = await page.Locator(".rz-switch.rz-switch-checked, .rz-switch[aria-checked='true']").CountAsync();
                if (alreadyEnabled > 0)
                {
                    await ClickIconAsync(page, "settings");
                }
                else
                {
                    await toggle.ClickAsync(new LocatorClickOptions { Timeout = 30000, Force = true });
                }

                break;

            default:
                throw new InvalidOperationException($"Unsupported overlay step: {step.Kind}");
        }
    }

    private static async Task ClickIconAsync(IPage page, string iconName)
    {
        var byClass = page.Locator($"button:has(.rzi-{iconName.Replace('_', '-')}), button:has(.rzi-{iconName})");
        if (await byClass.CountAsync() > 0)
        {
            await byClass.First.ClickAsync(new LocatorClickOptions { Timeout = 30000 });
            return;
        }

        var byText = page.Locator("button").Filter(new LocatorFilterOptions
        {
            Has = page.Locator(".rzi, i, span", new PageLocatorOptions { HasTextString = iconName })
        });

        await byText.First.ClickAsync(new LocatorClickOptions { Timeout = 30000 });
    }
}
