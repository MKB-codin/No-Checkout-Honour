using Microsoft.EntityFrameworkCore;
using HonourSelfCheckoutServer.Models;


namespace HonourSelfCheckoutServer.Data
{
    public class DatabaseContext :DbContext
    {

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<ReceiptItem> ReceiptItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<ReceiptItem>()
                .HasKey(ri => ri.ItemId);

            modelBuilder.Entity<ReceiptItem>()
                .HasOne<Receipt>()  
                .WithMany()         
                .HasForeignKey(ri => ri.ReceiptId);

            modelBuilder.Entity<ReceiptItem>()
                .HasOne<Product>()  
                .WithMany()         
                .HasForeignKey(ri => ri.ProductId);
        }
    }
}
