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
    public partial class 承租人檔CreateViewModel:承租人檔BaseViewModel
    {

        [DisplayName("刪除註記")]
        public string 刪除註記顯示 { get; set; }
        //[HiddenForView]
        [DisplayName("承租人姓名")]
        public string 承租人明文 { get; set; }
        [DisplayName("統一編號")]
        public string 統一編號明文 { get; set; }
        [DisplayName("行動電話")]
        //[HiddenForView]
        public string 行動電話明文 { get; set; }
        [DisplayName("電子郵件")]
        //[HiddenForView]
        public string 電子郵件明文 { get; set; }
 

    }
}
