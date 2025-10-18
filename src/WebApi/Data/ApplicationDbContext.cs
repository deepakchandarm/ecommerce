using Microsoft.EntityFrameworkCore;
using WebApi.Dao;

namespace WebApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Dao.User> Users { get; set; }
        public DbSet<Dao.Cart> Carts { get; set; }
        public DbSet<Dao.CartItem> CartItems { get; set; }
        public DbSet<Dao.Order> Orders { get; set; }
        public DbSet<Dao.PaymentDetails> PaymentDetails { get; set; }
        public DbSet<Dao.Product> Products { get; set; }
        public DbSet<Dao.OrderItem> OrderItems { get; set; }
        public DbSet<Dao.Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            BuildUsersTable(modelBuilder);
            BuildProductsTable(modelBuilder);
            BuildCategoriesTable(modelBuilder);
            BuildCartsTable(modelBuilder);
            BuildCartItemsTable(modelBuilder);
            BuildOrdersTable(modelBuilder);
            BuildOrderItemsTable(modelBuilder);
            BuildPaymentDetailsTable(modelBuilder);

            AddIndexes(modelBuilder);
        }

        private static void AddIndexes(ModelBuilder modelBuilder)
        {
            // User indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_User_Email");

            // Product indexes
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_Product_CategoryId");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .HasDatabaseName("IX_Product_Name");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Brand)
                .HasDatabaseName("IX_Product_Brand");

            // Cart indexes
            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Cart_UserId");

            // CartItem indexes
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.CartId)
                .HasDatabaseName("IX_CartItem_CartId");

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.ProductId)
                .HasDatabaseName("IX_CartItem_ProductId");

            // Order indexes
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId)
                .HasDatabaseName("IX_Order_UserId");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CreatedDate)
                .HasDatabaseName("IX_Order_CreatedDate");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderStatus)
                .HasDatabaseName("IX_Order_Status");

            // OrderItem indexes
            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.OrderId)
                .HasDatabaseName("IX_OrderItem_OrderId");

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.ProductId)
                .HasDatabaseName("IX_OrderItem_ProductId");
        }

        private static void BuildUsersTable(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Dao.User>(entity =>
            {
                entity.ToTable("user");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Password).HasColumnName("password").IsRequired().HasMaxLength(50);
                // User to Cart relationship (one-to-many)
                entity.HasMany(u => u.Carts)
                    .WithOne(c => c.User)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // User to Order relationship (one-to-many)
                entity.HasMany(u => u.Orders)
                    .WithOne(o => o.User)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void BuildProductsTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.Product>(entity =>
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
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void BuildCategoriesTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.Category>(entity =>
            {
                entity.ToTable("category");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            });
        }

        private static void BuildCartsTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.Cart>(entity =>
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
                entity.HasMany(c => c.Items)
                      .WithOne(ci => ci.Cart)
                      .HasForeignKey(ci => ci.CartId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void BuildCartItemsTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.CartItem>(entity =>
            {
                entity.ToTable("cart_item");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.CartId).HasColumnName("cart_id").IsRequired();
                entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
                entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
                entity.Property(e => e.Amount).HasColumnName("amount").IsRequired().HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Cart)
                      .WithMany(c => c.Items)
                      .HasForeignKey(e => e.CartId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void BuildOrdersTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.Order>(entity =>
            {
                entity.ToTable("order");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.TotalAmount).HasColumnName("total_amount").IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Date).HasColumnName("date").IsRequired();
                entity.Property(e => e.OrderStatus).HasColumnName("order_status").IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(o => o.Items)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void BuildOrderItemsTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.OrderItem>(entity =>
            {
                entity.ToTable("order_item");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
                entity.Property(e => e.OrderId).HasColumnName("order_id").IsRequired();
                entity.Property(e => e.ProductId).HasColumnName("product_id").IsRequired();
                entity.Property(e => e.Quantity).HasColumnName("quantity").IsRequired();
                entity.Property(e => e.Amount).HasColumnName("amount").IsRequired().HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void BuildPaymentDetailsTable(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dao.PaymentDetails>(entity =>
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
