using StockAccountDomain.Entities;
using StockAccountDomain.Models;

namespace StockAccountDomain.Services
{
    public interface IActTransDomainService 
    {
        Task<ActTrans> CreateActTransAsync(ActTransModel actTransModel);
    }
}
