namespace Parking.UiTests;

[Trait("Category", "UI")]
[Trait("Task", "4")]
[Trait("Task", "5")]
public class LoginUiTests
{
    [Fact]
    public void Registration_FirstLaunch_OpensParkingWindowWithOperatorName()
    {
        using var session = new AppSession();

        session.SubmitRegistration(AppSession.OperatorPassword, "00-registration");
        var parking = session.WaitForParkingWindow();

        UiHelpers.WaitForText(parking, "OperatorNameText", AppSession.OperatorName);
        Assert.True(session.WaitForWindowClosed(AppSession.RegistrationWindowTitle), "Окно регистрации не закрылось.");
        UiHelpers.Screenshot(parking, "03-parking-window");
    }

    [Fact]
    public void Registration_PasswordsDiffer_ShowsErrorAndStaysOnRegistrationWindow()
    {
        using var session = new AppSession();

        var registration = session.SubmitRegistration("other-password");

        UiHelpers.WaitForText(registration, "RegistrationErrorText", "Пароли не совпадают.");
        Assert.False(session.IsWindowOpen(AppSession.ParkingWindowTitle), "Главное окно открылось при несовпадающих паролях.");
    }

    [Fact]
    public void Login_AfterRestart_RegisteredOperatorOpensParkingWindow()
    {
        using var session = new AppSession();
        session.RegisterOperator();
        session.Restart();

        session.SubmitLogin(AppSession.OperatorLogin, AppSession.OperatorPassword, "01-login-filled");
        var parking = session.WaitForWindow(AppSession.ParkingWindowTitle);

        UiHelpers.WaitForText(parking, "OperatorNameText", AppSession.OperatorName);
        Assert.True(session.WaitForWindowClosed(AppSession.LoginWindowTitle), "Окно входа не закрылось после входа.");
    }

    [Fact]
    public void Login_WrongPassword_ShowsErrorAndStaysOnLoginWindow()
    {
        using var session = new AppSession();
        session.RegisterOperator();
        session.Restart();

        var login = session.SubmitLogin(AppSession.OperatorLogin, "wrong-password");

        UiHelpers.WaitForText(login, "ErrorText", "Неверный логин или пароль.");
        Assert.False(session.IsWindowOpen(AppSession.ParkingWindowTitle), "Главное окно открылось при неверном пароле.");
        UiHelpers.Screenshot(login, "02-login-error");
    }
}
