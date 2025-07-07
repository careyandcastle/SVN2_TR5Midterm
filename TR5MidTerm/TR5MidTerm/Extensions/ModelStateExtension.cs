using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR5MidTerm.PC
{
    public static class ModelStateExtension
    {
        /// <summary>
        ///   將MoelState轉換前台可讀取的格式
        ///   [field1 : [error1, error2, ...]],[field2 : [error1, error2, ...]],...
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="keyPrefix">會加在key值前面, 用於轉換驗證陣列的結果. 因為陣列的key值是[index].property, 但前台的name會是 varname[index].property</param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, string[]>> ToErrorInfos(this ModelStateDictionary modelState, string keyPrefix = "")
        {
            return modelState.ToDictionary(kvp => keyPrefix + kvp.Key,
                kvp => kvp.Value.Errors
                .Select(e => e.ErrorMessage).ToArray())
                .Where(m => m.Value.Any());
        }

    }
}
