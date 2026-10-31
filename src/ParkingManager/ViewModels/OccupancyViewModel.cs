using Parking.Core.Models;

namespace ParkingManager.ViewModels;

// Занятость одной категории мест: карточка со счётчиками и упрощённая схема.
// На схеме квадратики заполняются по порядку: сначала занятые, затем
// забронированные, затем свободные — конкретное место не отслеживается.
public sealed class OccupancyViewModel
{
    public OccupancyViewModel(OccupancyInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        Category = info.Category;
        Title = info.Category.ToDisplayName();
        Capacity = info.Capacity;
        Occupied = info.Occupied;
        Booked = info.Booked;
        Free = info.Free;

        var cells = new List<CellState>(Capacity);
        for (var i = 0; i < Capacity; i++)
        {
            cells.Add(i < Occupied ? CellState.Occupied : i < Occupied + Booked ? CellState.Booked : CellState.Free);
        }

        Cells = cells;
    }

    public SpotCategory Category { get; }

    public string Title { get; }

    public int Capacity { get; }

    public int Occupied { get; }

    public int Booked { get; }

    public int Free { get; }

    public string Summary => $"занято {Occupied} · бронь {Booked} · свободно {Free} из {Capacity}";

    public IReadOnlyList<CellState> Cells { get; }

    // Обычные места — сетка 30 × 15, места для инвалидов — 10 × 5.
    public int CellColumns => Category == SpotCategory.Disabled ? 10 : 30;
}
