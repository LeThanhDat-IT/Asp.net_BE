using System;
using System.Collections.Generic;

namespace GundamStoreAPI.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public int SubcategoryId { get; set; }

    public string? Location { get; set; }

    public string? Tag { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int Stock { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual Subcategory Subcategory { get; set; } = null!;
}
