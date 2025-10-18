using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface ICartService
    {
        Task<Cart> GetCartAsync(long cartId);
        Task ClearCartAsync(long cartId);
        Task<decimal> GetTotalPriceAsync(long cartId);
    }
}
