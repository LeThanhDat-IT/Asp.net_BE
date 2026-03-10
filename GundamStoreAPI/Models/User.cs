using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GundamStoreAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Avatar { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Role { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    [JsonIgnore] // Thêm dòng này để Swagger không hiện nó ra nữa
    public virtual ICollection<Cart>? Carts { get; set; }

    public virtual ICollection<Follow> FollowFollowers { get; set; } = new List<Follow>();

    public virtual ICollection<Follow> FollowFollowings { get; set; } = new List<Follow>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();

    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    public virtual ICollection<PostShare> PostShares { get; set; } = new List<PostShare>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
