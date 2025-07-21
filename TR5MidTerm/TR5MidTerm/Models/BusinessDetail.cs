using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class BusinessDetail
    {
        [Key]
        [Column(TypeName = "decimal(6, 0)")]
        public decimal BusinessMonth { get; set; }
        [Key]
        public int BusinessID { get; set; }
        [Key]
        [StringLength(2)]
        public string ProductID { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal Count { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal Price { get; set; }
        [Required]
        [StringLength(5)]
        public string UPD_USR { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UPD_DATE { get; set; }

        [ForeignKey("BusinessMonth,BusinessID")]
        [InverseProperty("BusinessDetail")]
        public virtual Business Business { get; set; }
        [ForeignKey(nameof(ProductID))]
        [InverseProperty(nameof(Products.BusinessDetail))]
        public virtual Products Product { get; set; }
    }
}
