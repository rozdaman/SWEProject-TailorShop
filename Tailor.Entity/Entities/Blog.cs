using System;
using System.Collections.Generic;

namespace Tailor.Entity.Entities
{
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ImageUrl { get; set; }
        
        public ICollection<BlogProductAssignment> ProductAssignments { get; set; }
        // public ICollection<BlogCategoryAssignment> CategoryAssignments { get; set; }
    }

    public class BlogProductAssignment
    {
        public int BlogId { get; set; }
        public Blog Blog { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
    
    // Minimal placeholders for context mapping
    public class BlogCategory { public int Id { get; set; } }
    public class BlogCategoryAssignment { 
        public int BlogId { get; set; }
        // public Blog Blog { get; set; }
        public int BlogCategoryId { get; set; }
        // public BlogCategory BlogCategory { get; set; }
    }
}
