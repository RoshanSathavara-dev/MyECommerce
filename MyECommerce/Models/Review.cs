﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyECommerce.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public Product? Product { get; set; } // ✅ Make nullable

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty; // ✅ Use `int` instead of `string`

        public User? User { get; set; } // ✅ Make nullable

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(500)]
        public string Comment { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = "Anonymous";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
