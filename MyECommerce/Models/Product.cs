﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyECommerce.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Stock { get; set; }

        // Foreign Key for Category
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;

        public bool IsLimitedEdition { get; set; } = false;

        public bool ShowInHeroSection { get; set; } = false; // ✅ New property for Hero Section

        public bool IsFeatured { get; set; }


    }
}
