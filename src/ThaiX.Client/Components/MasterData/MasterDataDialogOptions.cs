using Radzen;

namespace ThaiX.Client.Components.MasterData;

public static class MasterDataDialogOptions
{
    public static DialogOptions Build() => new()
    {
        Width = "min(720px, 96vw)",
        Height = "auto",
        Resizable = false,
        Draggable = false,
        CloseDialogOnEsc = true,
        ShowClose = true,
        CloseDialogOnOverlayClick = false
    };
}
