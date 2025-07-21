using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace TR5MidTerm.Models
{
    public partial class 修改人
    {
        [Key]
        [Column("修改人")]
        [StringLength(5)]
        public string 修改人1 { get; set; }
        [Required]
        public byte[] 姓名 { get; set; }
    }
}
