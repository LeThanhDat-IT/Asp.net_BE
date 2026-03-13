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

    // Chặn đứng các quan hệ để Swagger không bị rối và tránh lỗi 400
    [JsonIgnore]
    public virtual ICollection<Cart>? Carts { get; set; }

    [JsonIgnore]
    public virtual ICollection<Follow> FollowFollowers { get; set; } = new List<Follow>();

    [JsonIgnore]
    public virtual ICollection<Follow> FollowFollowings { get; set; } = new List<Follow>();

    [JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [JsonIgnore]
    public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();

    [JsonIgnore]
    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    [JsonIgnore]
    public virtual ICollection<PostShare> PostShares { get; set; } = new List<PostShare>();

    [JsonIgnore]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}