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
    public partial class 承租人檔CreateViewModel:承租人檔BaseViewModel
    {
        //[CTRequired]
        [DisplayName("刪除註記")]
        public string 刪除註記顯示 { get; set; }
        [CTRequired]
        [NameRegex(NameType.NaturalPerson)]
        [DisplayName("承租人姓名")]
        public string 承租人明文 { get; set; }
        [CTRequired]
        [DisplayName("統一編號")]
        public string 統一編號明文 { get; set; }
        [TenDigitCode]
        [DisplayName("行動電話")]
        public string 行動電話明文 { get; set; }
        [EmailRegex]
        [DisplayName("電子郵件")]
        public string 電子郵件明文 { get; set; }
 

    }
}
