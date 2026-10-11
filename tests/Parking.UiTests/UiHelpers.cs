using FlaUI.Core.AutomationElements;
using FlaUI.Core.Capturing;
using FlaUI.Core.Tools;

namespace Parking.UiTests;

// Действия оператора и ожидания результата, Каждое ожидание повторяется до тайм-аута
public static class UiHelpers
{
    public static AutomationElement Find(AutomationElement parent, string automationId)
    {
        ArgumentNullException.ThrowIfNull(parent);

        var result = Retry.WhileNull(
            () => parent.FindFirstDescendant(cf => cf.ByAutomationId(automationId)),
            UiTestSettings.Timeout,
            UiTestSettings.PollInterval,
            ignoreException: true);

        return result.Result ?? throw new InvalidOperationException($"Элемент «{automationId}» не найден на экране.");
    }

    public static void SetText(AutomationElement parent, string automationId, string text)
    {
        Find(parent, automationId).AsTextBox().Text = text;
    }

    public static void Select(AutomationElement parent, string automationId)
    {
        Find(parent, automationId).AsListBoxItem().Select();
    }

    public static void ClickWhenEnabled(AutomationElement parent, string automationId)
    {
        var button = Find(parent, automationId);
        var enabled = Retry.WhileFalse(
            () => button.IsEnabled,
            UiTestSettings.Timeout,
            UiTestSettings.PollInterval,
            ignoreException: true);

        Assert.True(enabled.Result, $"Кнопка «{automationId}» осталась недоступной.");
        button.AsButton().Invoke();
    }

    public static void WaitForText(AutomationElement parent, string automationId, string expected)
    {
        WaitFor(parent, automationId, element => element.Name, expected);
    }

    public static void WaitForHelpText(AutomationElement parent, string automationId, string expected)
    {
        WaitFor(parent, automationId, element => element.HelpText, expected);
    }

    public static void Screenshot(AutomationElement element, string name)
    {
        ArgumentNullException.ThrowIfNull(element);

        Directory.CreateDirectory(UiTestSettings.ScreenshotsDirectory);
        using var image = Capture.Element(element);
        image.ToFile(Path.Combine(UiTestSettings.ScreenshotsDirectory, $"{name}.png"));
    }

    private static void WaitFor(AutomationElement parent, string automationId, Func<AutomationElement, string> read, string expected)
    {
        var actual = string.Empty;
        var matched = Retry.WhileFalse(
            () =>
            {
                actual = read(Find(parent, automationId));
                return actual == expected;
            },
            UiTestSettings.Timeout,
            UiTestSettings.PollInterval,
            ignoreException: true);

        Assert.True(matched.Result, $"«{automationId}»: ожидалось «{expected}», на экране «{actual}».");
    }
}
