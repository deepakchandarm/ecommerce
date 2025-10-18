using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Dao
{
    internal class PaymentDetailsDao
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; }
        public int OrderId { get; set; }
        public string PaymentId { get; set; }
        public string Status { get; set; }
        public OrderDao Order { get; set; }
    }
}
