using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class ProductTypes
    {
        public ProductTypes()
        {
            Products = new HashSet<Products>();
        }

        [Key]
        [StringLength(2)]
        public string ProductType { get; set; }
        [Required]
        [StringLength(20)]
        public string ProductTypeName { get; set; }
        [Required]
        [StringLength(5)]
        public string UPD_USR { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UPD_DATE { get; set; }

        [InverseProperty("ProductTypeNavigation")]
        public virtual ICollection<Products> Products { get; set; }
    }
}
