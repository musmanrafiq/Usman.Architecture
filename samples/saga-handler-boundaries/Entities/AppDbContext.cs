// ADR-0030 -- Demo DbContext
using Microsoft.EntityFrameworkCore;

namespace SagaHandlerBoundaries.Entities;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set(typeof(Order).Name!) as DbSet<Order>;
    public DbSet<Cart> Carts => Set(typeof(Cart).Name!) as DbSet<Cart>;

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.Entity<Order>().HasKey(o => o.OrderId);
        mb.Entity<Cart>().HasKey(c => c.CartId);
    }
}

public class Order
{
    public Guid OrderId { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Created";
}

public class Cart
{
    public string CartId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; } = new();
}

public class CartItem
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
