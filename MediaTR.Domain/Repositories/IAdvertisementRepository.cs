using MediaTR.Domain.Entities;
using MediaTR.Domain.Enums;
using MediaTR.SharedKernel.Data;

namespace MediaTR.Domain.Repositories;

public interface IAdvertisementRepository : IRepository<Advertisement>
{
    Task<IEnumerable<Advertisement>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Advertisement>> GetByStatusAsync(AdvertisementStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Advertisement>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Advertisement>> GetApprovedAdvertisementsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Advertisement>> GetPendingAdvertisementsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Advertisement>> GetExpiredAdvertisementsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Advertisement>> GetFeaturedAdvertisementsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<Advertisement>> SearchAdvertisementsAsync(string searchTerm, CancellationToken cancellationToken = default);
}