using System;
using System.Collections.Generic;

namespace GundamStoreAPI.Models;

public partial class Post
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string? Caption { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? LikeCount { get; set; }

    public int? CommentCount { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<PostComment> PostComments { get; set; } = new List<PostComment>();

    public virtual ICollection<PostImage> PostImages { get; set; } = new List<PostImage>();

    public virtual ICollection<PostLike> PostLikes { get; set; } = new List<PostLike>();

    public virtual ICollection<PostShare> PostShares { get; set; } = new List<PostShare>();

    public virtual User User { get; set; } = null!;
}
