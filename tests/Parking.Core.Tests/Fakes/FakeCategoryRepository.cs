using Parking.Core.DataAccess;
using Parking.Core.Models;

namespace Parking.Core.Tests.Fakes;

// Вместимость в памяти: тесты задают маленькие числа, чтобы легко заполнить парковку.
public class FakeCategoryRepository : ICategoryRepository
{
    private readonly List<CategoryCapacity> _categories = new List<CategoryCapacity>();

    public FakeCategoryRepository(int regular, int disabled)
    {
        _categories.Add(new CategoryCapacity { Category = SpotCategory.Regular, Capacity = regular });
        _categories.Add(new CategoryCapacity { Category = SpotCategory.Disabled, Capacity = disabled });
    }

    public IReadOnlyList<CategoryCapacity> GetAll()
    {
        return _categories;
    }

    public CategoryCapacity? Get(SpotCategory category)
    {
        return _categories.Find(c => c.Category == category);
    }

    public void Update(CategoryCapacity capacity)
    {
        // Объекты хранятся по ссылке, изменения уже применены.
    }
}
