namespace Parking.UiTests;

[Trait("Category", "UI")]
[Trait("Task", "4")]
public class ParkingUiTests
{
    public const string CarNumber = "А123ВС73";

    [Fact]
    [Trait("Task", "6")]
    public void MainWindow_AfterLogin_ShowsCapacityPerCategory()
    {
        using var session = new AppSession();
        var parking = session.LoginAsDemoOperator();

        UiHelpers.WaitForText(parking, "RegularFreeText", $"{AppSession.RegularCapacity}");
        UiHelpers.WaitForText(parking, "DisabledFreeText", $"{AppSession.DisabledCapacity}");
        UiHelpers.WaitForText(parking, "ActiveCountText", "0");
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterEntryAndExit_UpdatesCounters()
    {
        using var session = new AppSession();
        var parking = session.LoginAsDemoOperator();

        // Въезд: оператор вводит номер и регистрирует въезд на обычное место.
        UiHelpers.SetText(parking, "CarNumberBox", CarNumber);
        UiHelpers.ClickWhenEnabled(parking, "EntryButton");

        UiHelpers.WaitForText(parking, "StatusMessageText", $"Въезд зарегистрирован: {CarNumber}, место обычное.");
        UiHelpers.WaitForText(parking, "RegularFreeText", "449");
        UiHelpers.WaitForText(parking, "ActiveCountText", "1");
        UiHelpers.Screenshot(parking, "04-after-entry");

        // Выезд: только что въехавший автомобиль уже выделен в списке.
        UiHelpers.ClickWhenEnabled(parking, "ExitButton");

        UiHelpers.WaitForText(parking, "StatusMessageText", $"Выезд зарегистрирован: {CarNumber}, к оплате 60 ₽.");
        UiHelpers.WaitForText(parking, "RegularFreeText", "450");
        UiHelpers.WaitForText(parking, "ActiveCountText", "0");
        UiHelpers.Screenshot(parking, "05-after-exit");
    }

    [Fact]
    [Trait("Task", "6")]
    public void RegisterEntry_DisabledCategory_TakesDisabledPlace()
    {
        using var session = new AppSession();
        var parking = session.LoginAsDemoOperator();

        UiHelpers.Select(parking, "DisabledCategoryRadio");
        UiHelpers.SetText(parking, "CarNumberBox", "М808АА73");
        UiHelpers.ClickWhenEnabled(parking, "EntryButton");

        UiHelpers.WaitForText(parking, "StatusMessageText", "Въезд зарегистрирован: М808АА73, место для инвалидов.");
        UiHelpers.WaitForText(parking, "DisabledFreeText", "49");
        UiHelpers.WaitForText(parking, "RegularFreeText", "450");
    }

    [Fact]
    [Trait("Task", "7")]
    public void RegisterEntry_SameCarTwice_ShowsError()
    {
        using var session = new AppSession();
        var parking = session.LoginAsDemoOperator();

        UiHelpers.SetText(parking, "CarNumberBox", CarNumber);
        UiHelpers.ClickWhenEnabled(parking, "EntryButton");
        UiHelpers.WaitForText(parking, "ActiveCountText", "1");

        UiHelpers.SetText(parking, "CarNumberBox", CarNumber);
        UiHelpers.ClickWhenEnabled(parking, "EntryButton");

        UiHelpers.WaitForText(parking, "StatusMessageText", $"Автомобиль {CarNumber} уже находится на парковке.");
        UiHelpers.WaitForText(parking, "ActiveCountText", "1");
        UiHelpers.Screenshot(parking, "06-duplicate-car");
    }

    [Fact]
    [Trait("Task", "7")]
    public void Search_FindsCarAndRegistersItsExit()
    {
        using var session = new AppSession();
        var parking = session.LoginAsDemoOperator();
        UiHelpers.SetText(parking, "CarNumberBox", CarNumber);
        UiHelpers.ClickWhenEnabled(parking, "EntryButton");
        UiHelpers.WaitForText(parking, "ActiveCountText", "1");
        UiHelpers.SetText(parking, "CarNumberBox", "Х777ХХ73");
        UiHelpers.ClickWhenEnabled(parking, "EntryButton");
        UiHelpers.WaitForText(parking, "ActiveCountText", "2");

        // Поиск по части номера оставляет в списке одну машину и выделяет её.
        UiHelpers.SetText(parking, "SearchBox", "123");
        UiHelpers.ClickWhenEnabled(parking, "ExitButton");

        UiHelpers.WaitForText(parking, "StatusMessageText", $"Выезд зарегистрирован: {CarNumber}, к оплате 60 ₽.");
        UiHelpers.WaitForText(parking, "ActiveCountText", "1");
    }

    [Fact]
    [Trait("Task", "6")]
    public void SchemeTab_AfterEntry_OpensOccupancyScheme()
    {
        using var session = new AppSession();
        var parking = session.LoginAsDemoOperator();
        UiHelpers.SetText(parking, "CarNumberBox", CarNumber);
        UiHelpers.ClickWhenEnabled(parking, "EntryButton");
        UiHelpers.WaitForText(parking, "ActiveCountText", "1");

        UiHelpers.Select(parking, "SchemeTab");

        Assert.True(UiHelpers.Find(parking, "SchemeTab").Patterns.SelectionItem.Pattern.IsSelected.Value);
        UiHelpers.Screenshot(parking, "07-scheme");
    }

    [Fact]
    [Trait("Task", "7")]
    public void HistoryTab_AfterExit_ShowsFinishedSession()
    {
        using var session = new AppSession();
        var parking = session.LoginAsDemoOperator();
        UiHelpers.SetText(parking, "CarNumberBox", CarNumber);
        UiHelpers.ClickWhenEnabled(parking, "EntryButton");
        UiHelpers.WaitForText(parking, "ActiveCountText", "1");
        UiHelpers.ClickWhenEnabled(parking, "ExitButton");
        UiHelpers.WaitForText(parking, "ActiveCountText", "0");

        UiHelpers.Select(parking, "HistoryTab");
        UiHelpers.ClickWhenEnabled(parking, "ShowHistoryButton");

        UiHelpers.WaitForText(parking, "HistorySummaryText", "Найдено стоянок: 1 · сумма 60 ₽");
        UiHelpers.Screenshot(parking, "08-history");
    }

    [Fact]
    [Trait("Task", "8")]
    public void SettingsTab_ChecksDisabledShareAndSavesCapacity()
    {
        using var session = new AppSession();
        var parking = session.LoginAsDemoOperator();
        UiHelpers.Select(parking, "SettingsTab");

        // 450 + 10 мест: мест для инвалидов меньше 10 % — сохранение запрещено.
        UiHelpers.SetText(parking, "DisabledCapacityBox", "10");
        UiHelpers.ClickWhenEnabled(parking, "SaveSettingsButton");
        UiHelpers.WaitForText(parking, "SettingsMessageText", "Мест для инвалидов должно быть не менее 10 % от общего числа: не менее 46.");

        UiHelpers.SetText(parking, "DisabledCapacityBox", "60");
        UiHelpers.ClickWhenEnabled(parking, "SaveSettingsButton");
        UiHelpers.WaitForText(parking, "SettingsMessageText", "Настройки сохранены.");
        UiHelpers.WaitForText(parking, "DisabledFreeText", "60");
        UiHelpers.Screenshot(parking, "09-settings");
    }
}
