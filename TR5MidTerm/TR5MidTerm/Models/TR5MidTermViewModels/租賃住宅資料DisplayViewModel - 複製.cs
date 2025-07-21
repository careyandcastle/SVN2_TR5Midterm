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
    public interface IHasOrgNameDisplay
    {
        string 事業顯示 { get; set; }
        string 單位顯示 { get; set; }
        string 部門顯示 { get; set; }
        string 分部顯示 { get; set; }
    }
}
