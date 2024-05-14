using System.ComponentModel.DataAnnotations.Schema;

namespace AspNet8Mvc_Tallybook.Models
{
    [Table("TallybookExpenseRecords")]
    public class ExpenseRecord
    {
        // 定義記帳簿資料表紀錄之欄位名稱與資料型態
        public int id { get; set; }             // 定義[消費紀錄id]欄位
        public int price { get; set; }          // 定義[消費金額]欄位
        public string? expenseType { get; set; } //  定義[消費種類]欄位
        public string? comment { get; set; }     // 定義[消費說明]欄位
        public DateTime payDate { get; set; }   // 定義[付款日期]欄位
    }
}
