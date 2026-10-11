using System.Windows;
using ParkingManager.ViewModels;

namespace ParkingManager.Views;

public partial class ParkingWindow : Window
{
    public ParkingWindow(MainViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        DataContext = viewModel;
    }
}
