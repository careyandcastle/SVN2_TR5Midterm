using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TR5MidTerm.Attributes
{
    public class CTRequiredAttribute : RequiredAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return !String.IsNullOrEmpty(ErrorMessage)
                ? ErrorMessage
                : $"{name}為必填欄位";
        }
    }
}
