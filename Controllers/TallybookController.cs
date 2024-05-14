using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.IdentityModel.Tokens;
using AspNet8Mvc_Tallybook.Models;

namespace AspNet8Mvc_Tallybook.Controllers
{
    public class TallybookController : Controller
    {
        private readonly TallybookDBContext db; // 宣告TallybookDBContext物件變數db

        #region  TallybookController類別的建構式    
        public TallybookController(TallybookDBContext context)
        {
            // 將TallybookDBContext物件傳入的參數值指定給db變數 (依賴注入DI)
            db = context;

            int count = db.ExpenseTypes.Count(); // 取得消費種類資料表的紀錄數

            //===== 若消費種類資料表(ExpenseTypes)沒有任何紀錄，則將9個預先設好的消費種類存入消費種類資料表中
            if (count == 0) 
            {
                //初始消費種類陣列
                string[] initialExpenseTypes = { "餐飲", "購物", "交通", "娛樂", "醫療", "投資", "保險", "教育", "社交" };
                int length = initialExpenseTypes.Length; // 取得初始消費種類陣列的長度

                // 建立消費種類紀錄陣列物件
                List<ExpenseType> expenseTypeList = new List<ExpenseType>(length);
                //若沒有任何消費種類，則利用迴圈方式，將初始消費種類逐一存到ExpenseTypes資料表中
                for (int i = 0; i < length; i++)
                {
                    // 將initialExpenseTypes陣列第i個元素存入expenseTypeArray物件第i個元素的expenseType屬性
                    ExpenseType expenseTypeObject = new ExpenseType(); // 建立消費種類物件
                    expenseTypeObject.expenseType = initialExpenseTypes[i];
                    expenseTypeList.Add(expenseTypeObject);
                }
                // 將expenseType物件新增到ExpenseTypes資料表中
                db.ExpenseTypes.AddRange(expenseTypeList.ToArray());
                // 儲存變更到資料庫 
                db.SaveChanges();
            }
        }
        #endregion

        #region 顯示儲存消費紀錄頁面的動作方法
        // 沒有HTTP動詞標示，預設只接受GET請求
        // 路由：/Tallybook/SaveExpense
        public IActionResult SaveExpense()
        {
            // 建立一個SaveExpenseViewModel物件
            SaveExpenseViewModel saveExpenseViewModel = new SaveExpenseViewModel();
            // 利用LINQ語法取出所有的消費種類紀錄
            var result = from a in db.ExpenseTypes
                         select a;
            // 建立一個字串清單物件expenseTypeList
            List<SelectListItem> expenseTypeList = new List<SelectListItem>();
            // 將回傳的消費種類一一取出，然後存入字串清單expenseTypeList
            int length = result.Count(); // 取得回傳的消費種類紀錄數
            for (int i = 0; i < length; i++)
            {
                string? text = result.AsEnumerable().ElementAt(i).expenseType;
                expenseTypeList.Add(new SelectListItem { Text = text});
            }
            // 將expenseTypeList存入saveExpenseViewModel物件的expenseTypeList屬性中
            saveExpenseViewModel.expenseTypeList = expenseTypeList;

            // 回傳攜帶saveExpenseViewModel物件的SaveExpense()的View Razor Page
            return View(saveExpenseViewModel);
        }
        #endregion

        #region 儲存消費紀錄的動作方法
        [HttpPost]  // 只接受POST請求
        [ValidateAntiForgeryToken] // 設定防止CSRF攻擊的標註
        // 路由：/Tallybook/SaveExpense
        // 傳入參數：SaveExpenseViewModel物件
        // 只接受(繫結)price, expenseType, comment, payDate屬性
        public async Task<IActionResult> SaveExpense([Bind("price,expenseType,comment,payDate")] SaveExpenseViewModel expenseData)
        {
            if (ModelState.IsValid) // 假如傳入的SaveExpenseViewModel物件的模型繫結狀態正確，則執行以下程式碼
            {
                // 建立消費紀錄物件，並存入從前端網頁傳過來的消費紀錄資料
                ExpenseRecord expenseRecord = new ExpenseRecord();
                expenseRecord.price = expenseData.price;
                expenseRecord.expenseType = expenseData.expenseType;
                expenseRecord.comment = expenseData.comment;
                expenseRecord.payDate = expenseData.payDate;

                db.ExpenseRecords.Add(expenseRecord); // 將消費紀錄物件新增到ExpenseRecords資料表中
                await db.SaveChangesAsync(); // 儲存變更到資料庫
                // 將成功儲存1比消費紀錄訊息存入result屬性中
                TempData["ResultOfSaveExpense"] = "已成功儲存一筆解題紀錄!\n";// 儲存消費紀錄結果的訊息

                //重新導向儲存消費資料頁面
                return RedirectToAction("SaveExpense");
            }
            else
            {
                // 若模型繫結狀態不正確，則回傳攜帶SaveExpenseViewModel物件的SaveExpense()的View Razor Page
                return View(expenseData);
            }
        }
        #endregion

        #region  顯示查詢消費紀錄頁面的動作方法
        // 沒有HTTP動詞標示，預設只接受GET請求
        // 路由：/Tallybook/QueryExpense
        public IActionResult QueryExpense()
        {
            // 回傳QueryExpense()的View Razor Page
            return View();
        }
        #endregion

        #region 查詢消費紀錄的動作方法
        [HttpPost] // 設定只接受POST請求
        [ValidateAntiForgeryToken] // 設定防止CSRF攻擊的標註
        // 路由：/Tallybook/QueryExpense
        // 傳入參數： QueryExpenseViewModel物件
        // 只接受(繫結)startDate,endDate,queryMode屬性
        public IActionResult QueryExpense([Bind("startDate,endDate,queryMode")] QueryExpenseViewModel queryExpenseData)
        {
            if (ModelState.IsValid) // 假如傳入的QueryExpenseViewModel物件的模型繫結狀態正確，則執行以下程式碼
            {
                // 取出從前端網頁傳過來的起、訖日期及查詢模式
                DateTime sDate = queryExpenseData.startDate.Date;
                DateTime eDate = queryExpenseData.endDate.Date;
                int queryMode = queryExpenseData.queryMode;
                string str;      //用於紀錄查詢結果字串
                int totalAmount; // 用於紀錄消費總金額
                int rowCount;    // 用於儲存紀錄數
                int typeSum;     // 用於儲存個別消費種類的消費總金額

                if (queryMode == 0)  // queryMode==0，按照消類日期查詢
                {
                    str = "";
                    totalAmount = 0;
                    // 使用LINQ語法查詢起訖日期間的消費紀錄，並依照消費日期排序
                    var result = from a in db.ExpenseRecords 
                                    where ((a.payDate >= sDate) && (a.payDate <= eDate))
                                    orderby a.payDate
                                    select a;
                    rowCount = result.Count(); // 取得記錄數量
                    str = "在" + sDate.Date.ToString("yyyy-MM-dd") + "到"
                            + eDate.Date.ToString("yyyy-MM-dd") + "共有" + rowCount + "筆解題紀錄:\n";

                    foreach (var record in result) // 利用迴圈逐一讀取每一筆紀錄
                    { //讀取price欄位(即消費金額price)，並加總到消費總金額中
                        totalAmount += record.price;
                    }
                    // 將消費總金額串接到顯示字串(str)中
                    str += "共計消費 " + totalAmount + " 題.\n";

                    // 顯示消費紀錄之每一個欄位之抬頭，將每一個欄位的抬頭串接到顯示字串(str)中	
                    string[] colNames = { "編號", "解題數量", "題目類別", "解題日期", "解題說明" };
                    foreach (var name in colNames)
                    {
                        str += string.Format("{0}    ", name);
                    }

                    str += "\n";

                    // 利用迴圈逐一讀取每一筆紀錄
                    int i = 0;
                    foreach (var record in result)
                    {
                        // 串接記錄編號(索引值+1)
                        str += string.Format("{0:d4}  ", (i + 1));
                        // 串接price欄位值(消費金額)
                        str += string.Format("{0,8}   ", record.price);
                        // 串接expenseType欄位值(消費種類)
                        str += string.Format("{0,6}     ", record.expenseType);
                        // 串接payDate欄位值(消費日期)
                        str += string.Format("{0,10}    ", record.payDate.Date.ToString("d"));
                        // 串接comment欄位值(消費說明)
                        str += string.Format("{0,-8}", record.comment);
                        str += "\n";
                        i++;
                    }
                    // 將查詢結果字串存入QueryExpenseViewModel物件的result欄位中
                    queryExpenseData.result = str;

                    // 回傳攜帶QueryExpenseViewModel物件的QueryExpense()的View Razor Page
                    return View(queryExpenseData); 
                }
                else // queryMode==1，按照消類種類查詢
                {
                    str = "";
                    totalAmount = 0;

                    // 使用LINQ語法查詢起訖日期間的消費紀錄，並依照消費種類群組，
                    // 每種消費種類只取出第1筆紀錄 
                    var result = from a in db.ExpenseRecords
                                    where ((a.payDate >= sDate) && (a.payDate <= eDate))
                                    group a by a.expenseType into g
                                    select g.FirstOrDefault();

                    var records = result.ToArray<ExpenseRecord>(); // 將結果轉換成消費紀錄陣列

                    //
                    rowCount = result.Count(); // 取得紀錄數
                    if (rowCount == 0)  // 若記錄數為0，則回傳有消費紀錄之訊息
                    {
                        str = "在" + sDate.Date.ToString("yyyy-MM-dd") + "到" +
                                eDate.Date.ToString("yyyy-MM-dd") + "並沒有消費紀錄";
                        
                        // 將查詢結果字串存入QueryExpenseViewModel物件的result欄位中
                        queryExpenseData.result = str;
                        // 回傳攜帶QueryExpenseViewModel物件的QueryExpenseIndex的View Razor Page
                        return View(queryExpenseData);
                    }
                    else // 
                    {
                        str = "在" + sDate.Date.ToString("yyyy-MM-dd") + "到" +
                        eDate.Date.ToString("yyyy-MM-dd") + "之消費統計如下:\n";
                        foreach (var record in records) // 逐一取出消費資料的每一筆紀錄record
                        {
                            // 使用LINQ語法查詢起訖日期間消費種類與record消費種類相同的所有消費紀錄
                            // use LINQ to query data 
                            var result1 = from b in db.ExpenseRecords
                                            where ((b.payDate >= sDate) && (b.payDate <= eDate) &&
                                                    (b.expenseType == record.expenseType))
                                            select b;

                            // 計算該消費種類的總金額
                            typeSum = 0;
                            foreach (var record1 in result1)
                            {
                                typeSum += record1.price;
                            }
                            //格式化串接該消費種類之總金額
                            str +=
                                string.Format("{0}: {1}元\n", record.expenseType, typeSum);
                            totalAmount += typeSum; //  累加消費總金額

                        }
                        str += "消費金額總計" + totalAmount + "元\n";
                        
                        // 將查詢結果字串存入QueryExpenseViewModel物件的result欄位中
                        queryExpenseData.result = str;
                        // 回傳攜帶QueryExpenseViewModel物件的QueryExpense()的View Razor Page
                        return View(queryExpenseData);
                    }
                }
            }
            else // 若模型繫結狀態不正確，則回傳攜帶QueryExpenseViewModel物件的QueryExpense()的View Razor Page
            {
                // 將警示訊息存入QueryExpenseViewModel物件的result欄位中
                queryExpenseData.result = "請輸入查詢日期"; 
                // 回傳攜帶QueryExpenseViewModel物件的QueryExpense()的View Razor Page
                return View(queryExpenseData);
            }
        }
        #endregion

        #region 顯示刪除消費紀錄頁面的動作方法
        // 沒有HTTP動詞標示，預設只接受GET請求
        // 路由：/Tallybook/DeleteExpense
        public IActionResult DeleteExpense()
        {
            // 回傳DeleteExpense()的View Razor Page
            return View();
        }
        #endregion

        #region 刪除消費紀錄的動作方法
        [HttpPost, ActionName("DeleteExpense")] // 設定只接受POST請求，並設定動作的名稱為DeleteExpense
        [ValidateAntiForgeryToken] // 設定防止CSRF攻擊的標註
        // 路由：/Tallybook/DeleteExpense
        // 傳入參數： DeleteExpenseViewModel物件
        // 只接受(繫結)startDate,endDate屬性
        public async Task<IActionResult> DeleteExpenseConfirmed([Bind("startDate,endDate")] DeleteExpenseViewModel deleteExpenseData)
        {
            if (ModelState.IsValid) // 假如傳入的DeleteExpenseViewModel物件的模型繫結狀態正確，則執行以下程式碼
            {
                // 取出從前端網頁傳過來的起、訖日期
                DateTime sDate = deleteExpenseData.startDate.Date;
                DateTime eDate = deleteExpenseData.endDate.Date;
                string str = "";
                // 使用 LINQ 取出起訖日期間的消費資料紀錄 (依照消費日期排序)
                var result = from a in db.ExpenseRecords
                             where ((a.payDate >= sDate) && (a.payDate <= eDate))
                             orderby a.payDate
                             select a;
                int count = result.Count(); // 取得紀錄數
                if (count == 0)  // 若記錄數為0，則回傳沒有消費紀錄之訊息
                {
                    str = "在" + sDate.Date.ToString("yyyy-MM-dd") + "到" +
                            eDate.Date.ToString("yyyy-MM-dd") + "沒有解題紀錄:!";
                    
                    // 將沒有消費紀錄的訊息存入DeleteExpenseViewModel物件的result欄位中
                    deleteExpenseData.result = str;
                    // 回傳攜帶DeleteExpenseViewModel物件的DeleteExpense()的View Razor Page
                    return View(deleteExpenseData);
                }

                db.ExpenseRecords.RemoveRange(result); // 從db物件刪除result中的消費資料紀錄
                await db.SaveChangesAsync();       // 將更新存入資料庫中

                // 將刪除成功的訊息存入DeleteExpenseViewModel物件的result欄位中
                deleteExpenseData.result = "已經成功刪除" + count + "筆紀錄!";
                // 回傳攜帶DeleteExpenseViewModel物件的DeleteExpense()的View Razor Page
                return View(deleteExpenseData);
            }
            else // 若模型繫結狀態不正確，則回傳攜帶DeleteExpenseViewModel物件的DeleteExpense()的View Razor Page
            {
                // 將警示訊息存入DeleteExpenseViewModel物件的result欄位中
                deleteExpenseData.result = "請輸入查詢日期";
                // 回傳攜帶DeleteExpenseViewModel物件的DeleteExpense()的View Razor Page
                return View(deleteExpenseData);
            }
        }
        #endregion

        #region 顯示管理消費種類頁面的動作方法
        // 沒有HTTP動詞標示，預設只接受GET請求
        // 路由：/Tallybook/ManageExpenseType
        public IActionResult ManageExpenseType()
        {
            // 建立ManageExpenseTypeViewModel物件
            ManageExpenseTypeViewModel manageExpenseTypeData = new ManageExpenseTypeViewModel();

            // 用於儲存結果之字串變數
            string str = "";
            str += TempData["result"];
            // 使用LINQ語法取出所有消費種類，並依照id排序
            var result = from a in db.ExpenseTypes
                            orderby a.id
                            select a;

            // 利用迴圈逐一讀取每一筆紀錄
            var i = 0;
            str += "現有消費資料種類如下：\n";
            foreach (var record in result)
            {
                str += string.Format("{0:d2}  ", (i + 1)); // 串接記錄編號(索引值+1)
                str += string.Format("{0}", record.expenseType);//串接price欄位值(消費金額)
                str += "\n";
                i++;
            }
            // 將結果字串存入ManageExpenseTypeViewModel物件的result欄位中
            manageExpenseTypeData.result = str;
            // 回傳攜帶ManageExpenseTypeViewModel物件的ManageExpenseType()的View Razor Page
            return View(manageExpenseTypeData);
        }
        #endregion

        #region 顯示新增消費種類局部頁面的動作方法
        public IActionResult AddNewExpenseType()
        {
            // 因為此頁面用於插入到管理消費種類頁面中，故使用部分檢視回傳
            return PartialView("AddNewExpenseType");
        }
        #endregion

        #region 新增消費種類的動作方法
        [HttpPost] //設定此Action只接受POST請求
        [ValidateAntiForgeryToken] // 設定防止CSRF攻擊的標註
        // 路由：/Tallybook/AddNewExpenseType
        // 傳入參數： ManageExpenseTypeViewModel物件
        // 只接受(繫結)expenseType屬性
        public async Task<IActionResult> AddNewExpenseType(string newExpenseType)
        {
            string str = ""; // 用來儲存結果的字串變數
            if (newExpenseType.IsNullOrEmpty())
            {
                str += "擬新增的消費種類不能為空白，請重新輸入!\n";
            }
            else
            {
                // 檢驗擬新增的消費種類是否已存在
                var result = db.ExpenseTypes.Where(x => x.expenseType == newExpenseType);
                int count = result.Count(); // 取得記錄數，若count!=0，代表已經存在
                if (count != 0) // 若 count != 0，代表擬新增的消費種類已經存在，在結果中加入警示訊息
                {
                    str += "擬新增的消費種類 " + newExpenseType + " 已經存在，請重新輸入!\n";
                }
                else // 若 count == 0，代表擬新增的消費種類還不存在，可存入消費種類資料表中
                {
                    // 建立ExpenseType物件
                    ExpenseType expenseTypeObject = new ExpenseType();
                    // 將新增的消費種類儲存到ExpenseType物件的expenseType屬性中
                    expenseTypeObject.expenseType = newExpenseType;
                    db.ExpenseTypes.Add(expenseTypeObject); // 新增消費種類
                    await db.SaveChangesAsync();          // 更新至資料庫
                    str += "已經新增消費種類： " + newExpenseType + "\n";
                }
            }
            // 將結果字串存入TempData["result"]中
            TempData["result"] = str;
            // 重新導向管理消費種類頁面
            return RedirectToAction("ManageExpenseType");
        }
        #endregion

        #region 顯示刪除消費種類局部頁面的動作方法
        public IActionResult DeleteExpenseType()
        {
            // 因為此頁面用於插入到管理消費種類頁面中，故使用部分檢視回傳
            return PartialView();
        }
        #endregion

        #region 刪除消費種類的動作方法
        [HttpPost] //設定此Action只接受POST請求
        [ValidateAntiForgeryToken] // 設定防止CSRF攻擊的標註
        // 路由：/Tallybook/DeleteExpenseType
        // 傳入參數： ManageExpenseTypeViewModel物件
        // 只接受(繫結)expenseTypeNumber屬性
        public async Task<IActionResult> DeleteExpenseType(string expenseTypeNumber)
        {
            string str = ""; // 用來儲存結果的字串變數
            if (expenseTypeNumber.IsNullOrEmpty())
            {
                str += "擬刪除的解題種類編號不能為空白，請重新輸入!\n";
            }
            else
            {
                int num;
                bool isNumber = int.TryParse(expenseTypeNumber, out num);
                // 若輸入的編號不是數字，在結果中加入警示訊息
                if (!isNumber)
                {
                    str += "擬刪除的解題種類編號必須為數字，請重新輸入!\n";
                }
                else
                {
                    // 利用LINQ語法取出所有的消費種類紀錄
                    var result = from a in db.ExpenseTypes
                                 orderby a.id
                                 select a;

                    int count = result.Count(); // 取出記錄數
                    if ((num < 1) || (num > count)) // 輸入的編號不在範圍內，則回傳提示訊息
                    {
                        str += "輸入的編號為" + num + "，不在 1~" + count + " 之間，請重新輸入!\n";
                    }
                    else
                    {

                        var record = result.ToArray()[num - 1]; //找出指定編號那筆紀錄
                        db.ExpenseTypes.Remove(record); // //將指定編號那筆紀錄刪除
                        await db.SaveChangesAsync(); // 更新資料庫
                        str += "已經刪除解題種類： " + record.expenseType + "\n";
                    }
                }
            }
            // 將結果字串存入TempData["result"]中
            TempData["result"] = str;
            // 重新導向管理消費種類頁面
            return RedirectToAction("ManageExpenseType");
        }
        #endregion
    }
}
