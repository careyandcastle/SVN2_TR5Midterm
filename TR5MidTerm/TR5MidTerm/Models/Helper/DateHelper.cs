using System;
using System.Globalization;

namespace TR5MidTerm.Helpers
{
    /// <summary>
    /// 提供將西元日期轉換為民國年格式字串的工具方法。
    /// </summary>
    public static class DateHelper
    {
        /// <summary>
        /// 將指定的 <see cref="DateTime"/> 轉換為民國格式的字串。
        /// </summary>
        /// <param name="date">欲轉換的西元日期。</param>
        /// <param name="format">民國格式顯示類型，例如僅年、年月或完整年月日。</param>
        /// <returns>轉換後的民國日期字串，例如 "114/07/25"。</returns>
        /// <example>
        /// DateHelper.ToTaiwanDateString(new DateTime(2025, 7, 25), TaiwanDateFormat.FullDate)
        /// // 輸出: "114/07/25"
        /// </example>
        /// <remarks>
        /// 使用 System.Globalization.TaiwanCalendar 進行轉換。
        /// 若格式為 <c>YearOnly</c> 則僅輸出民國年，不含斜線與其他資訊。
        /// </remarks>
        public static string ToTaiwanDateString(DateTime date, TaiwanDateFormat format = TaiwanDateFormat.FullDate)
        {
            if (date < new DateTime(1912, 1, 1))
                return string.Empty; // 或回傳 "無效日期"、"--" 等字串
            var taiwanCalendar = new TaiwanCalendar();
            int year = taiwanCalendar.GetYear(date);

            return format switch
            {
                TaiwanDateFormat.YearOnly => $"民國 {year:000} 年",
                TaiwanDateFormat.YearMonth => $"民國 {year:000} 年 {date.Month:00} 月",
                TaiwanDateFormat.FullDate => $"民國 {year:000} 年 {date.Month:00} 月 {date.Day:00} 日",
                _ => date.ToString("yyyy/MM/dd")
            };
        }
        /// <summary>
        /// 將可為 null 的日期轉換為民國格式字串。
        /// </summary>
        /// <param name="date">Nullable 的 <see cref="DateTime"/>。</param>
        /// <param name="format">民國格式顯示類型。</param>
        /// <returns>
        /// 若日期為 null 則回傳空字串，否則回傳轉換後的民國日期字串。
        /// </returns>
        public static string ToTaiwanDateString(DateTime? date, TaiwanDateFormat format = TaiwanDateFormat.FullDate)
        {
            //return date.HasValue ? ToTaiwanDateString(date.Value, format) : string.Empty;
            return date.HasValue ? ToTaiwanDateString(date.Value, format) : "租約已完成收租";
        }
    }
    /// <summary>
    /// 指定民國日期格式的顯示方式。
    /// </summary>
    public enum TaiwanDateFormat
    {
        /// <summary>
        /// 僅顯示年份（例如：114）
        /// </summary>
        YearOnly,

        /// <summary>
        /// 顯示年份與月份（例如：114/07）
        /// </summary>
        YearMonth,

        /// <summary>
        /// 顯示完整日期（例如：114/07/25）
        /// </summary>
        FullDate
    }
}
