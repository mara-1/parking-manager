namespace Parking.Core.Models;

// Категория парковочного места. Оператор не отмечает конкретное место,
// система учитывает только количество занятых мест каждой категории.
public enum SpotCategory
{
    Regular,
    Disabled,
}
