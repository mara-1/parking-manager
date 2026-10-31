using System.Diagnostics.CodeAnalysis;
using Microsoft.Win32;

namespace ParkingManager.Services;

// Стандартное окно Windows «Сохранить как».
[ExcludeFromCodeCoverage(Justification = "Открывает стандартное окно Windows, проверяется вручную.")]
public sealed class SaveFileDialogService : ISaveFileDialog
{
    public string? AskPath(string defaultFileName, string filter)
    {
        var dialog = new SaveFileDialog
        {
            FileName = defaultFileName,
            Filter = filter,
            AddExtension = true,
            OverwritePrompt = true,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
