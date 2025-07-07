using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR5MidTerm.Controllers
{
    public static class ExceptionExtensions
    {
        public static Exception GetOriginalException(this Exception ex)
        {
            if (ex.InnerException == null) return ex;

            return ex.InnerException.GetOriginalException();
        }

        public static string ToMeaningfulMessage(this Exception ex)
        {
            if (ex is DbUpdateConcurrencyException)
            {
                return "同時操作導致異常，請稍後再試";
            }

            if (ex is SqlException)
            {
                SqlException sqlEx = ex as SqlException;

                switch (sqlEx.Number)
                {
                    case 515: ///資料表欄位不可null
                        return sqlEx.Message;
                    case 547: // Foreign Key violation
#if DEBUG
                        return $"外鍵關聯異常，{sqlEx.Message}";
#else
                        return $"外鍵關聯異常";
#endif
                    case 2627: // Primary key violation
#if DEBUG
                        return $"已有相同鍵值資料，{sqlEx.Message}";
#else
                        return $"已有相同鍵值資料";
#endif
                    default:
#if DEBUG
                        return $"未知的資料庫異常，狀態:{sqlEx.State}/錯誤代碼:{sqlEx.Number}，{sqlEx.Message}";
#else
                        return $"未知的資料庫異常";
#endif
                }
            }

#if DEBUG
            return ex.Message;
#else
            return "程式異常，請通知資訊人員";
#endif
        }
    }
}
