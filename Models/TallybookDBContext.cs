using Microsoft.EntityFrameworkCore;
namespace AspNet8Mvc_Tallybook.Models
{
    public class TallybookDBContext : DbContext
    {
        // 資料庫內容類別TallybookDBContext之建構式
        public TallybookDBContext(DbContextOptions<TallybookDBContext> options)
            : base(options)
        {
        }
        // 定義ExpenseRecords資料表實體集，資料表中每一筆紀錄對應到ExpenseRecord類別建立的物件
        public DbSet<ExpenseRecord> ExpenseRecords { get; set; }

        // 定義ExpenseTypes資料表實體集，資料表中每一筆紀錄對應到ExpenseType類別建立的物件
        public DbSet<ExpenseType> ExpenseTypes { get; set; }
        
    }
}
