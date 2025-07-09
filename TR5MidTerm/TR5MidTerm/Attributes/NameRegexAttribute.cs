using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.ComponentModel.DataAnnotations;
namespace TR5MidTerm.Attributes
{
    public class NameRegexAttribute : RegularExpressionAttribute
    {
        private readonly NameType _nameType;

        public NameRegexAttribute(NameType nameType)
            : base(GeneratePattern(nameType))
        {
            _nameType = nameType;
        }

        private static string GeneratePattern(NameType nameType)
        {
            string pattern = nameType switch
            {
                NameType.NaturalPerson => @"^[A-Za-z\u4e00-\u9fa5]{2,10}$",
                NameType.VirtualHost => @"^[\u4e00-\u9fa5]{2,10}$",
                _ => throw new ArgumentOutOfRangeException()
            };

            return pattern;
        }

        public override string FormatErrorMessage(string name)
        {
            return name + (_nameType == NameType.NaturalPerson
                ? "必須為中英文字，長度 2~10"
                : "必須為中文，長度 2~10");
        }
    }
    // 🧩 可放同一檔案內的 enum
    public enum NameType
    {
        NaturalPerson,
        VirtualHost
    }
}
