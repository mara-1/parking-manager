namespace Parking.UiTests;

[Trait("Category", "UI")]
[Trait("Task", "4")]
[Trait("Task", "5")]
public class LoginUiTests
{
    [Fact]
    public void Login_DemoOperator_OpensParkingWindow()
    {
        using var session = new AppSession();

        session.SubmitLogin(AppSession.DemoLogin, AppSession.DemoPassword, "01-login-filled");
        var parking = session.WaitForWindow(AppSession.ParkingWindowTitle);

        UiHelpers.WaitForText(parking, "OperatorNameText", AppSession.DemoOperatorName);
        Assert.True(session.WaitForWindowClosed(AppSession.LoginWindowTitle), "Окно входа не закрылось после входа.");
        UiHelpers.Screenshot(parking, "03-parking-window");
    }

    [Fact]
    public void Login_WrongPassword_ShowsErrorAndStaysOnLoginWindow()
    {
        using var session = new AppSession();

        var login = session.SubmitLogin(AppSession.DemoLogin, "wrong-password");

        UiHelpers.WaitForText(login, "ErrorText", "Неверный логин или пароль.");
        Assert.False(session.IsWindowOpen(AppSession.ParkingWindowTitle), "Главное окно открылось при неверном пароле.");
        UiHelpers.Screenshot(login, "02-login-error");
    }
}
