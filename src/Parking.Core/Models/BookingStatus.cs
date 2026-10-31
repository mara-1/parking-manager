namespace Parking.Core.Models;

public enum BookingStatus
{
    // Ожидается: место удерживается до времени прибытия + 15 минут.
    Waiting,

    // Автомобиль заехал по брони.
    Arrived,

    // Бронь отменена оператором.
    Cancelled,

    // Автомобиль не приехал за 15 минут — бронь снята автоматически.
    NoShow,
}
