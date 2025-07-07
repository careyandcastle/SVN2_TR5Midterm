using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TscLibCore.Attribute;

#nullable disable

namespace TR5MidTerm.Models.TR5MidTermViewModels
{
    public partial class 水電分表檔DisplayViewModel
    {
        [Key]
        //[StringLength(2)]
        public string 事業 { get; set; }
        [Key]
        //[StringLength(2)]
        public string 單位 { get; set; }
        [Key]
        //[StringLength(2)]
        public string 部門 { get; set; }
        [Key]
        //[StringLength(2)]
        public string 分部 { get; set; }
        [Key]
        //[StringLength(20)]
        public string 總表號 { get; set; }
        [Key]
        public int 分表號 { get; set; }
        [Required]
        //[StringLength(20)]
        public string 備註 { get; set; }
        //[Column(TypeName = "decimal(18, 0)")]
        public decimal 上期度數 { get; set; }
        //[Column(TypeName = "decimal(18, 0)")]
        public decimal 本期度數 { get; set; }
        //[Required]
        //[StringLength(10)]
        public string 修改人 { get; set; }
        //[Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }

        //[ForeignKey("事業,單位,部門,分部,總表號")]
        //[InverseProperty("水電分表檔")]
        //public virtual 水電總表檔 水電總表檔 { get; set; }
    }
}
