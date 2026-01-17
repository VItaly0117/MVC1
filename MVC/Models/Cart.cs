namespace MVC.Models;

public class Cart
{
    public int Id { get; set; }
    public Guid UniqueId { get; set; } = Guid.NewGuid();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int? UserId { get; set; }
    public User? User { get; set; }

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}