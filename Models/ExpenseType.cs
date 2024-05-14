using System.ComponentModel.DataAnnotations.Schema;

namespace AspNet8Mvc_Tallybook.Models
{
    [Table("TallybookExpenseTypes")]
    public class ExpenseType
    {
        // 定義消費種類資料表紀錄之欄位名稱與資料型態
        public int id { get; set; }    // 定義[消費種類紀錄]的id欄位
        public string? expenseType { get; set; } // 定義[消費種類]欄位 
    }
}
