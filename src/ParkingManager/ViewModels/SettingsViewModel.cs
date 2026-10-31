using System.Globalization;
using System.Windows.Input;
using Parking.Core.Models;
using Parking.Core.Services;

namespace ParkingManager.ViewModels;

// Вкладка «Тарифы и места»: вместимость категорий и стоимость часа.
public sealed class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private string _regularCapacityText = string.Empty;
    private string _disabledCapacityText = string.Empty;
    private string _regularPriceText = string.Empty;
    private string _disabledPriceText = string.Empty;
    private string _inUseText = string.Empty;
    private string _message = string.Empty;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        SaveCommand = new RelayCommand(Save);
        Load();
    }

    public event EventHandler? Saved;

    public string RegularCapacityText
    {
        get => _regularCapacityText;
        set => SetProperty(ref _regularCapacityText, value ?? string.Empty);
    }

    public string DisabledCapacityText
    {
        get => _disabledCapacityText;
        set => SetProperty(ref _disabledCapacityText, value ?? string.Empty);
    }

    public string RegularPriceText
    {
        get => _regularPriceText;
        set => SetProperty(ref _regularPriceText, value ?? string.Empty);
    }

    public string DisabledPriceText
    {
        get => _disabledPriceText;
        set => SetProperty(ref _disabledPriceText, value ?? string.Empty);
    }

    public string InUseText
    {
        get => _inUseText;
        private set => SetProperty(ref _inUseText, value);
    }

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public ICommand SaveCommand { get; }

    public void Load()
    {
        var settings = _settingsService.GetSettings();
        var regular = settings.First(s => s.Category == SpotCategory.Regular);
        var disabled = settings.First(s => s.Category == SpotCategory.Disabled);

        RegularCapacityText = regular.Capacity.ToString(CultureInfo.InvariantCulture);
        DisabledCapacityText = disabled.Capacity.ToString(CultureInfo.InvariantCulture);
        RegularPriceText = regular.PricePerHour.ToString("0.##", CultureInfo.InvariantCulture);
        DisabledPriceText = disabled.PricePerHour.ToString("0.##", CultureInfo.InvariantCulture);
        InUseText = $"Сейчас занято: обычных {regular.InUse}, для инвалидов {disabled.InUse}.";
    }

    private static bool TryParseCount(string text, out int value)
    {
        return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    // Принимается и запятая, и точка: «75,5» и «75.5».
    private static bool TryParsePrice(string text, out decimal value)
    {
        return decimal.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private void Save()
    {
        if (!TryParseCount(RegularCapacityText, out var regularCapacity)
            || !TryParseCount(DisabledCapacityText, out var disabledCapacity))
        {
            Message = "Вместимость должна быть целым числом.";
            return;
        }

        if (!TryParsePrice(RegularPriceText, out var regularPrice)
            || !TryParsePrice(DisabledPriceText, out var disabledPrice))
        {
            Message = "Стоимость часа должна быть числом, например 60 или 75,5.";
            return;
        }

        try
        {
            _settingsService.UpdateCapacities(regularCapacity, disabledCapacity);
            _settingsService.UpdatePrice(SpotCategory.Regular, regularPrice);
            _settingsService.UpdatePrice(SpotCategory.Disabled, disabledPrice);
            Message = "Настройки сохранены.";
            Load();
            Saved?.Invoke(this, EventArgs.Empty);
        }
        catch (ParkingOperationException ex)
        {
            Message = ex.Message;
        }
    }
}
