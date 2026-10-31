using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public interface ICategoryRepository
{
    IReadOnlyList<CategoryCapacity> GetAll();

    CategoryCapacity? Get(SpotCategory category);

    void Update(CategoryCapacity capacity);
}
