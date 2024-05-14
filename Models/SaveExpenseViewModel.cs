using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AspNet8Mvc_Tallybook.Models
{
    public class SaveExpenseViewModel // 儲存消費資料頁面的ViewModel類別
    {
        [DisplayName("解題數量：")]
        [Required(ErrorMessage = "請輸入解題數量!")]
        public int price { get; set; } // 消費金額屬性
        [DisplayName("題目種類：")]
        [Required(ErrorMessage = "請選擇題目種類!")]
        public string? expenseType { get; set; } // 消費種類屬性
        public List<SelectListItem>? expenseTypeList { get; set; } // 消費種類清單屬性
        [DisplayName("解題說明：")]
        [Required(ErrorMessage = "請輸入解題說明!")]
        public string? comment { get; set; } // 消費說明屬性
        [DisplayName("解題日期：")]
        [Required(ErrorMessage = "請選擇解題日期!")]
        [DataType(DataType.Date)]
        public DateTime payDate { get; set; } // 付款日期屬性
        public string? result { get; set; } // 執行結果屬性
    }
}