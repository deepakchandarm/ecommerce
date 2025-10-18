using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Dao
{
    internal class OrderItemDao
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public int OrderId { get; set; }
        public ProductDao Product { get; set; }
        public OrderDao Order { get; set; }
    }
}
