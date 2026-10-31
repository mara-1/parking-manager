using ParkingManager.ViewModels;

namespace ParkingManager.Tests;

[Trait("Category", "Unit")]
[Trait("Task", "4")]
public class RelayCommandTests
{
    [Fact]
    public void CanExecute_WithoutCondition_IsAlwaysTrue()
    {
        var executed = 0;
        var command = new RelayCommand(() => executed++);

        command.Execute(null);

        Assert.True(command.CanExecute(null));
        Assert.Equal(1, executed);
    }

    [Fact]
    public void CanExecute_WithCondition_UsesCondition()
    {
        var allowed = false;
        var command = new RelayCommand(() => { }, () => allowed);

        Assert.False(command.CanExecute(null));
        allowed = true;
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public void CanExecuteChanged_SubscribeAndUnsubscribe_DoesNotThrow()
    {
        var command = new RelayCommand(() => { });
        EventHandler handler = (sender, args) => { };

        var error = Record.Exception(() =>
        {
            command.CanExecuteChanged += handler;
            command.CanExecuteChanged -= handler;
        });

        Assert.Null(error);
    }
}
