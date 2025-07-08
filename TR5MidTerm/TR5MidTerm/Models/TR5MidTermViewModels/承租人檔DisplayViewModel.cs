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
    public partial class 承租人檔DisplayViewModel
    {

        [Key]
        [HiddenForView]
        //[StringLength(2)]
        public string 事業 { get; set; }
        [Key]
        [HiddenForView]
        //[StringLength(2)]
        public string 單位 { get; set; }
        [Key]
        [HiddenForView]
        //[StringLength(2)]
        public string 部門 { get; set; }
        [Key]
        [HiddenForView]
        //[StringLength(2)]
        public string 分部 { get; set; }
        [Key]
        [HiddenForView]
        //[StringLength(5)]
        public string 承租人編號 { get; set; }
        //[Required]
        [HiddenForView]
        public byte[] 承租人 { get; set; }
        //[Required]
        //[StringLength(2)]
        [HiddenForView]
        public string 身分別編號 { get; set; }
        //[Required]
        [HiddenForView]
        public byte[] 統一編號 { get; set; }
        [HiddenForView]
        public byte[] 行動電話 { get; set; }
        [HiddenForView]
        public byte[] 電子郵件 { get; set; }
        [HiddenForView]
        public bool 刪除註記 { get; set; }
        //[Required]
        //[StringLength(10)]
         
        public string 修改人 { get; set; }
        //[Column(TypeName = "datetime")]
        public DateTime 修改時間 { get; set; }

        // 0703註解掉
        //[ForeignKey(nameof(身分別編號))]
        //[InverseProperty(nameof(身分別檔.承租人檔))]
        //public virtual 身分別檔 身分別編號Navigation { get; set; }
        //[InverseProperty("承租人檔")]
        //public virtual ICollection<租約主檔> 租約主檔 { get; set; }

        // 顯示用欄位（解密後的字串）0704
        [DisplayName("承租人")]
        public string 承租人明文 { get; set; }
        [DisplayName("統一編號")]
        public string 統一編號明文 { get; set; }
        [DisplayName("行動電話")]
        public string 行動電話明文 { get; set; }
        [DisplayName("電子郵件")]
        public string 電子郵件明文 { get; set; }
        [DisplayName("刪除註記")]
        public string 刪除註記顯示 { get; set; }

        //導覽屬性顯示用 0704
        [DisplayName("身分別")]
        public string 身分別名稱 { get; set; }

    }
}
