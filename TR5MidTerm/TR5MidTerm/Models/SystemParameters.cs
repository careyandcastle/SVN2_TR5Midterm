using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class SystemParameters
    {
        [Key]
        [StringLength(50)]
        public string ParameterName { get; set; }
        [Required]
        [StringLength(50)]
        public string ParameterValue { get; set; }
        [Required]
        [StringLength(5)]
        public string UPD_USER { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UPD_DATE { get; set; }
    }
}
