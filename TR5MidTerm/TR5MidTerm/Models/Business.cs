using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    [Index(nameof(DepartmentNo), Name = "IX_Business")]
    public partial class Business
    {
        public Business()
        {
            BusinessDetail = new HashSet<BusinessDetail>();
        }

        [Key]
        [Column(TypeName = "decimal(6, 0)")]
        public decimal BusinessMonth { get; set; }
        [Key]
        public int BusinessID { get; set; }
        [Required]
        [StringLength(2)]
        public string DepartmentNo { get; set; }
        [StringLength(10)]
        public string CustomerID { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal Total { get; set; }
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }
        [Required]
        [StringLength(5)]
        public string UPD_USR { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UPD_DATE { get; set; }

        [ForeignKey(nameof(CustomerID))]
        [InverseProperty(nameof(Customers.Business))]
        public virtual Customers Customer { get; set; }
        [InverseProperty("Business")]
        public virtual ICollection<BusinessDetail> BusinessDetail { get; set; }
    }
}
