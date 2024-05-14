using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace AspNet8Mvc_Tallybook.Models
{
    public class QueryExpenseViewModel // 查詢消費資料頁面的ViewModel類別
    {
        [DisplayName("起始日期：")]
        [Required(ErrorMessage = "請選擇起始日期!")]
        [DataType(DataType.Date)]
        public DateTime startDate { get; set; } // 起始日期屬性
        [DisplayName("結束日期：")]
        [Required(ErrorMessage = "請選擇結束日期!")]
        [DataType(DataType.Date)]
        public DateTime endDate { get; set; } // 結束日期屬性
        [DisplayName("查詢方式：")]
        [Required(ErrorMessage = "請選擇查詢方式!")]
        public int queryMode { get; set; }  // 查詢方式屬性
        public string? result { get; set; }  // 執行結果屬性
    }
}
