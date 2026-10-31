using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Threading;
using ParkingManager.ViewModels;

namespace ParkingManager.Views;

[ExcludeFromCodeCoverage(Justification = "Окно: проверяется UI-тестами (FlaUI), покрытие которых не собирается.")]
public partial class ParkingWindow : Window
{
    private readonly DispatcherTimer _refreshTimer;

    public ParkingWindow(MainViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        DataContext = viewModel;

        // Раз в 30 секунд обновляются время стоянок и суммы к оплате.
        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _refreshTimer.Tick += (sender, args) => viewModel.Refresh(null);
        _refreshTimer.Start();
        Closed += (sender, args) => _refreshTimer.Stop();
    }
}
