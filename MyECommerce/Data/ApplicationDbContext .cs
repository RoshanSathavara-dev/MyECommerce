using MyECommerce.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MyECommerce.Data
{
    public class ApplicationDbContext : DbContext  // Removed IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Admin> Admins { get; set; }
       
        public DbSet<User> Users { get; set; }
        public DbSet<CustomZula> CustomZulas { get; set; }

        public DbSet<ShoppingCartItem> ShoppingCartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ShoppingCartItem>() // ✅ Ensure decimal precision for cart items
              .Property(s => s.Price)
              .HasColumnType("decimal(18,2)");



            base.OnModelCreating(modelBuilder);
        }
    }


}
