using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public class CategoryRepository : ICategoryRepository
{
    private readonly ParkingDbContext _dbContext;

    public CategoryRepository(ParkingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IReadOnlyList<CategoryCapacity> GetAll()
    {
        // Категория хранится строкой, поэтому порядок задаётся после загрузки.
        return _dbContext.Categories.AsEnumerable().OrderBy(c => c.Category).ToList();
    }

    public CategoryCapacity? Find(SpotCategory category)
    {
        return _dbContext.Categories.Find(category);
    }

    public void Update(CategoryCapacity capacity)
    {
        ArgumentNullException.ThrowIfNull(capacity);

        _dbContext.Categories.Update(capacity);
        _dbContext.SaveChanges();
    }
}
