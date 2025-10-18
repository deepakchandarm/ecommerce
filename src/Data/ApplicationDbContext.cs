using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        internal DbSet<Dao.UserDao> Users { get; set; }
        internal DbSet<Dao.CartDao> Carts { get; set; }
        internal DbSet<Dao.CartItemDao> CartItems { get; set; }
        internal DbSet<Dao.OrderDao> Orders { get; set; }
        internal DbSet<Dao.PaymentDetailsDao> PaymentDetails { get; set; }
        internal DbSet<Dao.ProductDao> Products { get; set; }
        internal DbSet<Dao.OrderItemDao> OrderItems { get; set; }
        internal DbSet<Dao.CategoryDao> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            BuildUsersTable(modelBuilder);
            BuildProductsTable(modelBuilder);
            BuildCategoriesTable(modelBuilder);
            BuildCartsTable(modelBuilder);
            BuildCartItemsTable(modelBuilder);
            BuildOrdersTable(modelBuilder);
            BuildOrderItemsTable(modelBuilder);
            BuildPaymentDetailsTable(modelBuilder);
        }

        private static void BuildUsersTable(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Dao.UserDao>(entity =>
            {
                entity.ToTable("user");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).HasColumnName("password").IsRequired().HasMaxLength(50);
            });
        }

        private static void BuildProductsTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.ProductDao>(entity =>
            {
                entity.ToTable("product");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnName("price").IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
                entity.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired();
                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void BuildCategoriesTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.CategoryDao>(entity =>
            {
                entity.ToTable("category");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            });
        }

        private static void BuildCartsTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.CartDao>(entity =>
            {
                entity.ToTable("cart");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.TotalAmount).HasColumnName("total_amount").IsRequired().HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void BuildCartItemsTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.CartItemDao>(entity =>
            {
                entity.ToTable("cart_item");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.CartId).HasColumnName("cart_id").IsRequired();
                entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
                entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
                entity.Property(e => e.Amount).HasColumnName("amount").IsRequired().HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Cart)
                      .WithMany()
                      .HasForeignKey(e => e.CartId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void BuildOrdersTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.OrderDao>(entity =>
            {
                entity.ToTable("order");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.TotalAmount).HasColumnName("total_amount").IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Date).HasColumnName("date").IsRequired();
                entity.Property(e => e.OrderStatus).HasColumnName("order_status").IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void BuildOrderItemsTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.OrderItemDao>(entity =>
            {
                entity.ToTable("order_item");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
                entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
                entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
                entity.Property(e => e.Amount).HasColumnName("amount").IsRequired().HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Order)
                      .WithMany()
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void BuildPaymentDetailsTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.PaymentDetailsDao>(entity =>
            {
                entity.ToTable("payment_details");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Amount).HasColumnName("amount").IsRequired();
                entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
                entity.Property(e => e.PaymentType).HasColumnName("payment_type").IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.Order)
                      .WithMany()
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
