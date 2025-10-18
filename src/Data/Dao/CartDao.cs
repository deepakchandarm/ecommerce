using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Dao
{
    internal class CartDao
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public int UserId { get; set; }
        public UserDao User { get; set; }
    }
}
