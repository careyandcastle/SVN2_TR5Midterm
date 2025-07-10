using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TR5MidTerm.Attributes;
using TscLibCore.Attribute;

#nullable disable

namespace TR5MidTerm.Models.TR5MidTermViewModels
{
    public partial class 水電分表檔BaseViewModel
    {
        [Key]
        [StringLength(2)]
        public string 事業 { get; set; }
        [Key]
        [StringLength(2)]
        public string 單位 { get; set; }
        [Key]
        [StringLength(2)]
        public string 部門 { get; set; }
        [Key]
        [StringLength(2)]
        public string 分部 { get; set; }
        [Key]
        [StringLength(20)]
        public string 總表號 { get; set; }
        [Key]
        [CTRequired]
        public int 分表號 { get; set; }
        [StringLength(20)]
        public string 備註 { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal 上期度數 { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        [CTRequired]
        public decimal 本期度數 { get; set; }
    }
}
