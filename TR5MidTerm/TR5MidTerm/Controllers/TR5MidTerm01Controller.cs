using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TscLibCore.Commons;
using TR5MidTerm.Models;
using TR5MidTerm.Models.TR5MidTermViewModels;
using TscLibCore.BaseObject;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using System.Data;
using TscLibCore.FileTool;
using System.IO;
using TscLibCore.Modules;
using TscLibCore.Authority;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using TR5MidTerm.PC;
using System.Text;

namespace TR5MidTerm.Controllers
{
    [ProcUseRang(ProcNo, ProcUseRang.Menu)]
    [TypeFilter(typeof(BaseActionFilter))]

    public class TR5MidTerm01Controller : Controller
    {
        private readonly TRDBContext _context;
        private const string ProcNo = "TR5MidTerm";

        private static IConfigurationProvider _config;
        private static IMapper _mapper;

        public TR5MidTerm01Controller(TRDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            Debug.WriteLine($"[InitInventoryDefaultValues] ✅ 從 Session 取得使用者：UserNo={ua.UserNo}, BusinessNo={ua.BusinessNo}, DepartmentNo={ua.DepartmentNo}, DivisionNo={ua.DivisionNo}, BranchNo={ua.BranchNo}"); ;
            ViewBag.TableFieldDescDict = new CreateTableFieldsDescription()
                   .Create<承租人檔DisplayViewModel>();

            return View();
        }

        [HttpPost, ActionName("GetDataPost")]
        [ValidateAntiForgeryToken]
        [NeglectActionFilter]
        public async Task<IActionResult> GetData([FromBody] QueryConditions qc)
        { 
            IQueryable<承租人檔DisplayViewModel> sql = GetBaseQuery();

            PaginatedList<承租人檔DisplayViewModel> queryedData = null;
            queryedData = await PaginatedList<承租人檔DisplayViewModel>.CreateAsync(sql, qc);

            return Ok(new
            {
                data = queryedData,
                total = queryedData.TotalCount
            });
        }

        private IQueryable<承租人檔DisplayViewModel> GetBaseQuery()
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));

            return (from m in _context.承租人檔
                        .Include(m => m.身分別編號Navigation)
                    join biz in _context.事業 on m.事業 equals biz.事業1
                    join dep in _context.單位 on m.單位 equals dep.單位1
                    join sec in _context.部門 on new { m.單位, m.部門 } equals new { sec.單位, 部門 = sec.部門1 }
                    join sub in _context.分部 on new { m.單位, m.部門, m.分部 } equals new { sub.單位, sub.部門, 分部 = sub.分部1 }
                    where m.事業 == ua.BusinessNo && m.單位 == ua.DepartmentNo
                    select new 承租人檔DisplayViewModel
                    {
                        #region 組織
                        事業 = m.事業,
                        事業顯示 = biz.事業名稱,

                        單位 = m.單位,
                        單位顯示 = dep.單位名稱,

                        部門 = m.部門,
                        部門顯示 = sec.部門名稱,
                        分部 = m.分部,

                        分部顯示 = sub.分部名稱,
                        承租人編號 = m.承租人編號,
                        #endregion
                        身分別編號 = m.身分別編號,
                        身分別名稱 = m.身分別編號Navigation.身分別,
                        #region 解密

                        承租人 = m.承租人,
                        承租人明文 = CustomSqlFunctions.DecryptToString(m.承租人),

                        統一編號 = m.統一編號,
                        統一編號明文 = CustomSqlFunctions.DecryptToString(m.統一編號),

                        行動電話 = m.行動電話,
                        行動電話明文 = CustomSqlFunctions.DecryptToString(m.行動電話),

                        電子郵件 = m.電子郵件,
                        電子郵件明文 = CustomSqlFunctions.DecryptToString(m.電子郵件),
                        #endregion
                        #region 註記
                        刪除註記 = m.刪除註記,
                        刪除註記顯示 = m.刪除註記 ? "是" : "否",
                        #endregion
                        #region 修改人與修改時間
                        修改人 = m.修改人,
                        修改時間 = m.修改時間
                        #endregion
                    }).AsNoTracking();
        }

        #region 增
        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public async Task<IActionResult> Create()
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
             
            // 📌 載入「身分別下拉選項」
            var 身分別選項 = await _context.身分別檔
                .Select(s => new SelectListItem
                {
                    Text = s.身分別編號 + "_" + s.身分別,
                    Value = s.身分別編號
                }).ToListAsync();

            if (!身分別選項.Any())
                Debug.WriteLine("[Create] ⚠️ 無身分別選項");

            身分別選項.Insert(0, new SelectListItem { Text = "--請選擇--", Value = "" });
            ViewBag.身分別選項 = 身分別選項;

            // ✅ 初始化 ViewModel
            var viewModel = new 承租人檔CreateViewModel
            {
                事業 = ua.BusinessNo,
                單位 = ua.DepartmentNo,
                部門 = ua.DivisionNo,
                分部 = ua.DivisionNo,
                 
                // 👉 預設值（若有）
                刪除註記 = false,
                刪除註記顯示 = "否",
                 
            };

            return PartialView(viewModel); // 若為一般頁面則改 return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public async Task<IActionResult> Create(承租人檔CreateViewModel model)
        {
            #region 驗證
            if (!ModelState.IsValid)
                return ModelStateInvalidResult("Create", false);
            #endregion


            #region 資料解密
            var SymmKey = _context.SymmetricKeyName;
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            var 承租人資料 = _context.承租人檔
                .Select(x => new {
                    承租人 = _context.EncryptByKey(SymmKey, model.承租人明文),
                    統一編號 = _context.EncryptByKey(SymmKey, model.統一編號明文),
                    行動電話 = _context.EncryptByKey(SymmKey, model.行動電話明文),
                    電子郵件 = _context.EncryptByKey(SymmKey, model.電子郵件明文),
                })
                .First();
            #endregion
            #region 寫入
            try
            {
                model.承租人編號 = await GetNext承租人編號Async(
    model.事業, model.單位, model.部門, model.分部);

                var entity = new 承租人檔
                {
                    事業 = model.事業,
                    單位 = model.單位,
                    部門 = model.部門,
                    分部 = model.分部,
                    承租人編號 = model.承租人編號,
                    身分別編號 = model.身分別編號,
                    承租人 = 承租人資料.承租人,
                    統一編號 = 承租人資料.統一編號,
                    行動電話 = 承租人資料.行動電話,
                    電子郵件 = 承租人資料.電子郵件,
                    刪除註記 = false,
                    修改人 = ua.UserNo + '_' + ua.UserName,
                    修改時間 = DateTime.Now,

                    
                };
                Debug.WriteLine("entity:", entity);

                _context.承租人檔.Add(entity);
                int result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    var display = await GetBaseQuery()
                        .Where(x =>
                            x.事業 == entity.事業 &&
                            x.單位 == entity.單位 &&
                            x.部門 == entity.部門 &&
                            x.分部 == entity.分部 &&
                            x.承租人編號 == entity.承租人編號
                        ).SingleOrDefaultAsync();

                    return Ok(new ReturnData(ReturnState.ReturnCode.OK) { data = display });
                }

                
            }
            #endregion
            #region 報錯

            catch (Exception ex)
            {
                // 處理例外
                Exception realEx = ex.GetOriginalException();

                return CreatedAtAction(nameof(Create), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR)
                {
                    message = realEx.ToMeaningfulMessage()
                });
            }

            return CreatedAtAction(nameof(Create), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR)
            {
                message = "發生未知錯誤，請聯絡管理員"
            });

            #endregion
        }

        private async Task<string> GetNext承租人編號Async(string 事業, string 單位, string 部門, string 分部)
        {
            var max編號 = await _context.承租人檔
                .Where(x => x.事業 == 事業 &&
                            x.單位 == 單位 &&
                            x.部門 == 部門 &&
                            x.分部 == 分部)
                .Select(x => x.承租人編號)
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync();

            int next;

            if (!string.IsNullOrEmpty(max編號) && int.TryParse(max編號, out int last))
            {
                next = last + 1;
            }
            else
            {
                next = 1;
            }

            return next.ToString("D5"); // 補滿五位數：00001、00002、...、12345
        }
        #endregion
        #region 自訂義驗證
        private IActionResult ModelStateInvalidResult(string context, bool 驗證前)
        {
            string sourceLabel = 驗證前 ? "ViewModel驗證" : "Validator驗證";
            Debug.WriteLine($"[{context}] [ERROR] ModelState 無效（{sourceLabel}）");

            foreach (var kv in ModelState.ToErrorInfos())
            {
                foreach (var msg in kv.Value)
                    Debug.WriteLine($"        ↳ 欄位：{kv.Key}，錯誤：{msg}");
            }

            // 根據 context 自動選對 ReturnCode
            var code = context.ToLower() switch
            {
                "Create" => ReturnState.ReturnCode.CREATE_ERROR,
                "Edit" => ReturnState.ReturnCode.EDIT_ERROR,
                "Delete" => ReturnState.ReturnCode.DELETE_ERROR,
                "ApproveConfirmed" => ReturnState.ReturnCode.EDIT_ERROR,
                "CreateDetail" => ReturnState.ReturnCode.CREATE_ERROR,
                "EditDetail" => ReturnState.ReturnCode.EDIT_ERROR,
                "DeleteDetail" => ReturnState.ReturnCode.DELETE_ERROR,
                _ => ReturnState.ReturnCode.ERROR
            };

            return Ok(new ReturnData(code)
            {
                data = ModelState.ToErrorInfos()
            });
        }
        #endregion
        #region 修
        public async Task<IActionResult> Edit(string 事業, string 單位, string 部門, string 分部, string 承租人編號)
        {
            //Debug.WriteLine('hello');
            // 檢查主鍵是否完整
            if (string.IsNullOrEmpty(事業) || string.IsNullOrEmpty(單位) ||
                string.IsNullOrEmpty(部門) || string.IsNullOrEmpty(分部) || string.IsNullOrEmpty(承租人編號))
            {
                return NotFound();
            }

            // 查詢資料（含身分別關聯）
            var model = await _context.承租人檔
                .Include(x => x.身分別編號Navigation)
                .FirstOrDefaultAsync(x =>
                    x.事業 == 事業 &&
                    x.單位 == 單位 &&
                    x.部門 == 部門 &&
                    x.分部 == 分部 &&
                    x.承租人編號 == 承租人編號);

            if (model == null)
            {
                return NotFound();
            }

            // 使用 AutoMapper 轉成 ViewModel
            var viewModel = _mapper.Map<承租人檔, 承租人檔EditViewModel>(model);

            // 解密欄位處理（假設你有自訂解密方法）
            var SymmKey = _context.SymmetricKeyName;
            var 承租人資料 = _context.承租人檔
                .Select(x => new {
                    承租人明文 = Encoding.Unicode.GetString(_context.DecryptByKey(model.承租人)),
                    統一編號明文 = Encoding.Unicode.GetString(_context.DecryptByKey(model.統一編號)),
                    行動電話明文 = Encoding.Unicode.GetString(_context.DecryptByKey(model.行動電話)),
                    電子郵件明文 = Encoding.Unicode.GetString(_context.DecryptByKey(model.電子郵件)),
                    //電子郵件 = _context.DecryptByKey(SymmKey, model.電子郵件明文),
                })
                .First();
            //Debug.WriteLine(CustomSqlFunctions.ConvertDecryptedByteArrayToString(model.承租人));
            viewModel.承租人明文 = 承租人資料.承租人明文;
            viewModel.統一編號明文 = 承租人資料.統一編號明文;
            //viewModel.行動電話明文 = 承租人資料.行動電話明文 != null
            //    ? CustomSqlFunctions.DecryptToString(model.行動電話)
            //    : string.Empty;
            viewModel.行動電話明文 = 承租人資料.行動電話明文;
            viewModel.電子郵件明文 = 承租人資料.電子郵件明文;

            // 傳入 ViewBag 下拉選單資料（身分別選項）
            ViewBag.身分別選項 = await _context.身分別檔
                .Select(x => new SelectListItem
                {
                    Value = x.身分別編號,
                    Text = x.身分別
                }).ToListAsync();

            return PartialView(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> Edit([Bind("事業,單位,部門,分部,承租人編號,承租人明文,身分別編號,統一編號明文,行動電話明文,電子郵件明文")] 承租人檔EditViewModel postData)
        {

            if (!ModelState.IsValid)
                return ModelStateInvalidResult("Edit", false);

            if (!ModelState.IsValid)
            {
                return Ok(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR)
                {
                    data = ModelState.ToErrorInfos()
                });
            }

            try
            {
                #region Put Your Code Here

                // 查原始資料
                var entity = await _context.承租人檔.FindAsync(postData.事業, postData.單位, postData.部門, postData.分部, postData.承租人編號);
                if (entity == null) return NotFound();

                // 更新欄位（使用 AutoMapper 更新非加密欄位）
                _mapper.Map(postData, entity); // ← 只更新內容，不更換主鍵

                // 加密欄位手動處理
                var SymmKey = "WuYeahSymmKey";
                //var key = _context.Key_Guid(SymmKey);

                var 承租人密文 = _context.承租人檔
                .Select(x => new {
                    承租人密文 = _context.EncryptByKey(SymmKey, postData.承租人明文),
                    統一編號密文 = _context.EncryptByKey(SymmKey, postData.統一編號明文),
                    行動電話密文 = _context.EncryptByKey(SymmKey, postData.行動電話明文),
                    電子郵件密文 = _context.EncryptByKey(SymmKey, postData.電子郵件明文),
                })
                .First();
                entity.承租人 = 承租人密文.承租人密文;
                entity.統一編號 = 承租人密文.統一編號密文;
                entity.行動電話 = 承租人密文.行動電話密文;
                entity.電子郵件 = 承租人密文.電子郵件密文;

                //設定系統欄位（如修改人、修改時間）
                var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
                entity.修改人 = ua.UserNo + '_' + ua.UserName;
                entity.修改時間 = DateTime.Now;

                _context.Update(entity);
                #endregion

                var opCount = await _context.SaveChangesAsync();

                if (opCount > 0)
                    return Ok(new ReturnData(ReturnState.ReturnCode.OK)
                    {
                        data = postData
                    });
            }
            catch (Exception ex)
            {
                return CreatedAtAction(nameof(Edit), new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            return CreatedAtAction(nameof(Edit), new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
        }
        #endregion
        #region 刪
        public async Task<IActionResult> Delete(string 事業, string 單位, string 部門, string 分部, string 承租人編號)
        {
            if (string.IsNullOrEmpty(事業) || string.IsNullOrEmpty(單位) ||
        string.IsNullOrEmpty(部門) || string.IsNullOrEmpty(分部) || string.IsNullOrEmpty(承租人編號))
            {
                return BadRequest("必要的主鍵參數缺漏：請確認事業、單位、部門、分部與承租人編號均已提供。");
            }

            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            var viewModel = await GetBaseQuery()
                .Where(x =>
             x.事業 == 事業 &&
             x.單位 == 單位 &&
             x.部門 == 部門 &&
             x.分部 == 分部 &&
             x.承租人編號 == 承租人編號)
                .SingleOrDefaultAsync();

            if (viewModel == null)
            {
                return NotFound($"找不到符合條件的承租人資料（事業={事業}, 單位={單位}, 部門={部門}, 分部={分部}, 編號={承租人編號}）。");
            }

            var 承租人資料明文 = _context.承租人檔
                .Select(x => new {
                    承租人明文 = Encoding.Unicode.GetString(_context.DecryptByKey(viewModel.承租人)),
                    統一編號明文 = Encoding.Unicode.GetString(_context.DecryptByKey(viewModel.統一編號)),
                    行動電話明文 = Encoding.Unicode.GetString(_context.DecryptByKey(viewModel.行動電話)),
                    電子郵件明文 = Encoding.Unicode.GetString(_context.DecryptByKey(viewModel.電子郵件))
                }).First(); ;
            viewModel.承租人明文 = 承租人資料明文.承租人明文;
            viewModel.統一編號明文 = 承租人資料明文.統一編號明文;
            viewModel.行動電話明文 = 承租人資料明文.行動電話明文;
            viewModel.電子郵件明文 = 承租人資料明文.電子郵件明文;
            viewModel.刪除註記顯示 = viewModel.刪除註記 ? "是" : "否";
            viewModel.修改人 = ua.UserNo + '_' + ua.UserName; 
            viewModel.修改時間 = DateTime.Now;
 

            return PartialView(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> DeleteConfirmed([Bind("事業,單位,部門,分部,承租人編號")] 承租人檔DisplayViewModel postData)
        {
            if (ModelState.IsValid == false)
                return BadRequest(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));

            //var result = await _context.承租人檔.FindAsync();3
            var result = await _context.承租人檔
                .Where(x =>
                    x.事業 == postData.事業 &&
                    x.單位 == postData.單位 &&
                    x.部門 == postData.部門 &&
                    x.分部 == postData.分部 &&
                    x.承租人編號 == postData.承租人編號
                    )
                .SingleOrDefaultAsync();
            if (result == null)
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));

            _context.承租人檔.Remove(result);

            try
            {
                var opCount = await _context.SaveChangesAsync();
                if (opCount > 0)
                    return Ok(new ReturnData(ReturnState.ReturnCode.OK));
            }
            catch (Exception ex)
            {
                return CreatedAtAction(nameof(DeleteConfirmed), new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
            }

            return CreatedAtAction(nameof(DeleteConfirmed), new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
        }
        #endregion

        public bool isMasterKeyExist()
        {
            //return (_context.承租人檔.Any(m => ) == false);
            return true;
        }

    }
}