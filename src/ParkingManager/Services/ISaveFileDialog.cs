namespace ParkingManager.Services;

// Выбор файла для сохранения. Вынесен в интерфейс, чтобы модель
// представления не открывала окна и проверялась модульными тестами.
public interface ISaveFileDialog
{
    // Путь к файлу или null, если пользователь отменил выбор.
    string? AskPath(string defaultFileName, string filter);
}
