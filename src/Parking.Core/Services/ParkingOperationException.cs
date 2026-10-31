namespace Parking.Core.Services;

// Нарушение правила учёта: нет свободных мест, автомобиль уже на парковке,
// неверный номер и т. п. Текст сообщения показывается оператору как есть.
public sealed class ParkingOperationException : Exception
{
    public ParkingOperationException()
    {
    }

    public ParkingOperationException(string message)
        : base(message)
    {
    }

    public ParkingOperationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
