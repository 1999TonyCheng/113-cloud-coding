using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AspNet8Mvc_Tallybook.Models
{
    public class DeleteExpenseViewModel // 刪除消費資料頁面的ViewModel類別
    {
        [DisplayName("起始日期：")]
        [Required(ErrorMessage = "請選擇起始日期!")]
        [DataType(DataType.Date)]
        public DateTime startDate { get; set; } // 起始日期屬性
        [DisplayName("結束日期：")]
        [Required(ErrorMessage = "請選擇結束日期!")]
        [DataType(DataType.Date)]
        public DateTime endDate { get; set; } // 結束日期屬性
        public string? result { get; set; }  // 執行結果屬性
    }
}
