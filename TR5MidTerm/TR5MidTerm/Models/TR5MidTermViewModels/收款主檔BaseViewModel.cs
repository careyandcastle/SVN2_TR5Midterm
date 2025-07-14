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
    public partial class 收款主檔BaseViewModel
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
        //[StringLength(5)]
        public string 案號 { get; set; }
        //[Required]
        //[StringLength(10)]
        //public string 修改人 { get; set; }
        //[Column(TypeName = "datetime")]
        //public DateTime 修改時間 { get; set; }
    }
}
