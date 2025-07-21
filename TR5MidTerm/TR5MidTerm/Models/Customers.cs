using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class Customers
    {
        public Customers()
        {
            Business = new HashSet<Business>();
        }

        [Key]
        [StringLength(10)]
        public string CustomerID { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        public byte[] EncryptName { get; set; }
        [Required]
        [StringLength(50)]
        public string AlwaysEncryptName { get; set; }
        [Required]
        [StringLength(5)]
        public string UPD_USR { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UPD_DATE { get; set; }

        [InverseProperty("Customer")]
        public virtual ICollection<Business> Business { get; set; }
    }
}
