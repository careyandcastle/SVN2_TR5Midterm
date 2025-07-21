using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    [Index(nameof(Cancelled), Name = "IX_StoreProducts")]
    public partial class StoreProducts
    {
        [Key]
        [StringLength(2)]
        public string DepartmentNo { get; set; }
        [Key]
        [StringLength(2)]
        public string ProductID { get; set; }
        public int Count { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal Price { get; set; }
        public bool Cancelled { get; set; }
        [Required]
        [StringLength(5)]
        public string UPD_USR { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UPD_DATE { get; set; }

        [ForeignKey(nameof(ProductID))]
        [InverseProperty(nameof(Products.StoreProducts))]
        public virtual Products Product { get; set; }
    }
}
