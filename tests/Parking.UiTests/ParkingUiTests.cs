using FlaUI.Core.Definitions;

namespace Parking.UiTests;

[Trait("Category", "UI")]
[Trait("Task", "4")]
public class ParkingUiTests
{
    public const string CarNumber = "А123ВС73";

    [Fact]
    [Trait("Task", "6")]
    public void ParkingWindow_ShowsAllDemoSpotsAsFree()
    {
        using var session = new AppSession();
        var parking = session.LoginAsDemoOperator();

        var spots = UiHelpers.Find(parking, "SpotsList").FindAllChildren(cf => cf.ByControlType(ControlType.ListItem));

        Assert.Equal(AppSession.DemoSpotCount, spots.Length);
        Assert.All(spots, spot => Assert.Equal("Свободно", spot.HelpText));
        UiHelpers.WaitForText(parking, "OccupiedCountText", "0");
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterEntryAndExit_UpdatesSpotAndCounters()
    {
        using var session = new AppSession();
        var parking = session.LoginAsDemoOperator();

        // Въезд: оператор выбирает место, вводит номер и регистрирует въезд.
        UiHelpers.Select(parking, "Spot_A-01");
        UiHelpers.WaitForText(parking, "SelectedSpotNumberText", "A-01");
        UiHelpers.SetText(parking, "CarNumberBox", CarNumber);
        UiHelpers.ClickWhenEnabled(parking, "EntryButton");

        UiHelpers.WaitForText(parking, "StatusMessageText", "Въезд зарегистрирован: место A-01.");
        UiHelpers.WaitForText(parking, "FreeCountText", "19");
        UiHelpers.WaitForText(parking, "OccupiedCountText", "1");
        UiHelpers.WaitForText(parking, "SelectedSpotStatusText", "Занято");
        Assert.StartsWith(CarNumber, UiHelpers.Find(parking, "Spot_A-01").HelpText, StringComparison.Ordinal);
        UiHelpers.Screenshot(parking, "04-after-entry");

        // Выезд: место освобождается, счётчики возвращаются к исходным.
        UiHelpers.ClickWhenEnabled(parking, "ExitButton");

        UiHelpers.WaitForText(parking, "StatusMessageText", "Выезд зарегистрирован: место A-01 освобождено.");
        UiHelpers.WaitForText(parking, "FreeCountText", "20");
        UiHelpers.WaitForHelpText(parking, "Spot_A-01", "Свободно");
        UiHelpers.Screenshot(parking, "05-after-exit");
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterEntry_SameCarTwice_ShowsErrorAndKeepsSpotFree()
    {
        using var session = new AppSession();
        var parking = session.LoginAsDemoOperator();

        UiHelpers.Select(parking, "Spot_A-01");
        UiHelpers.SetText(parking, "CarNumberBox", CarNumber);
        UiHelpers.ClickWhenEnabled(parking, "EntryButton");
        UiHelpers.WaitForText(parking, "FreeCountText", "19");

        UiHelpers.Select(parking, "Spot_A-02");
        UiHelpers.WaitForText(parking, "SelectedSpotNumberText", "A-02");
        UiHelpers.SetText(parking, "CarNumberBox", CarNumber);
        UiHelpers.ClickWhenEnabled(parking, "EntryButton");

        UiHelpers.WaitForText(parking, "StatusMessageText", $"Автомобиль {CarNumber} уже находится на парковке.");
        UiHelpers.WaitForText(parking, "FreeCountText", "19");
        UiHelpers.WaitForHelpText(parking, "Spot_A-02", "Свободно");
        UiHelpers.Screenshot(parking, "06-duplicate-car");
    }
}
