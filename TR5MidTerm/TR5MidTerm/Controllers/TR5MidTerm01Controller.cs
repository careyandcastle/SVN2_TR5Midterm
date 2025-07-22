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
using TscLibCore.WebAPI;
using TR5WebAPI_namespace;

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
        private readonly TscHttpClientService _tscHttpClentService;

        public TR5MidTerm01Controller(TscHttpClientService tscHttpClientService, TRDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _tscHttpClentService = tscHttpClientService;
        }

        #region 首頁
        public async Task<IActionResult> Index()
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            ViewBag.TableFieldDescDict = new CreateTableFieldsDescription()
                   .Create<承租人檔DisplayViewModel>();

            #region query下拉式清單 
            var 已使用事業代碼 = await _context.承租人檔
       .Select(x => x.事業)
       .Distinct()
       .ToListAsync();

            var 事業清單 = await _context.事業
        .Where(d => 已使用事業代碼.Contains(d.事業1))
        .Select(d => new SelectListItem
        {
            Value = d.事業1,
            Text = d.事業1 + "_" + d.事業名稱
        }).ToListAsync();
             
            var 單位清單 = new List<SelectListItem>();
            var 部門清單 = new List<SelectListItem>();
            var 分部清單 = new List<SelectListItem>();
            事業清單.Insert(0, new SelectListItem
            {
                Text = "--不篩選事業--",
                Value = ""
            });
            單位清單.Insert(0, new SelectListItem
            {
                Text = "--不篩選單位--",
                Value = ""
            });
            部門清單.Insert(0, new SelectListItem
            {
                Text = "--不篩選部門--",
                Value = ""
            });
            分部清單.Insert(0, new SelectListItem
            {
                Text = "--不篩選分部--",
                Value = ""
            });
            ViewBag.事業選單 = 事業清單;
            ViewBag.單位選單 = 單位清單;
            ViewBag.部門選單 = 部門清單;
            ViewBag.分部選單 = 分部清單;
            ViewBag.事業 = ua.BusinessName;
            ViewBag.單位 = ua.DepartmentName;
            ViewBag.部門 = ua.DivisionName;
            ViewBag.分部 = ua.BranchName;
            #endregion

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
                    //where m.事業 == ua.BusinessNo && m.單位 == ua.DepartmentNo
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
                        修改時間 = m.修改時間,
                        #endregion
                        #region 權限
                        可否編輯刪除 = (m.事業 == ua.BusinessNo && m.單位 == ua.DepartmentNo && m.部門 == ua.DivisionNo && m.分部 == ua.BranchNo),
                        #endregion
                    }).AsNoTracking();
        }
        #endregion
        #region 新增主檔
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
                分部 = ua.BranchNo,
                 
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
            // 🔐 權限檢查
            ValidateUserHasOrgPermission(model.事業, model.單位, model.部門, model.分部);

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
            try
            {
                
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
        #region 修改主檔
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

            // 🔐 權限檢查
            ValidateUserHasOrgPermission(postData.事業, postData.單位, postData.部門, postData.分部);

            //if (!ModelState.IsValid)
            //    return ModelStateInvalidResult("Edit", false);

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
        #region 刪除主檔
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
            // 🔐 權限檢查
            ValidateUserHasOrgPermission(postData.事業, postData.單位, postData.部門, postData.分部);
            await ValidateForDelete(postData);
            if (ModelState.IsValid == false)
            {
                //TempData["Error"] = "驗證失敗，請確認輸入資料。";
                //return RedirectToAction("Index");
                return ModelStateInvalidResult("Delete", false);
            }

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
        #region 其它
        public bool isMasterKeyExist()
        {
            //return (_context.承租人檔.Any(m => ) == false);
            return true;
        }
        #endregion
        #region 提供index使用
        //[HttpGet]
        public async Task<IActionResult> GetDepartmentSelectList(string Biz)
        {
            var 已使用單位代碼 = await _context.承租人檔
        .Where(x => x.事業 == Biz)
        .Select(x => x.單位)
        .Distinct()
        .ToListAsync();

            var 單位清單 = await _context.單位
                .Where(d => 已使用單位代碼.Contains(d.單位1))
                .Select(d => new SelectListItem
                {
                    Value = d.單位1,
                    Text = d.單位1 + "_" + d.單位名稱
                }).ToListAsync();

            return Json(單位清單);
        }
        public async Task<IActionResult> GetDivisionSelectList(string Biz, string DepNo)
        {
            var 已使用部門代碼 = await _context.承租人檔
        .Where(x => x.事業 == Biz && x.單位 == DepNo)
        .Select(x => x.部門)
        .Distinct()
        .ToListAsync();

            var 部門清單 = await _context.部門
                .Where(d => d.單位 == DepNo && 已使用部門代碼.Contains(d.部門1))
                .Select(d => new SelectListItem
                {
                    Value = d.部門1,
                    Text = d.部門1 + "_" + d.部門名稱
                }).ToListAsync();

            return Json(部門清單);
        }

        //[HttpGet]
        public async Task<IActionResult> GetBranchSelectList(string Biz, string DepNo, string DivNo)
        { 
            var 已使用分部代碼 = await _context.承租人檔
        .Where(x => x.事業 == Biz && x.單位 == DepNo && x.部門 == DivNo)
        .Select(x => x.分部)
        .Distinct()
        .ToListAsync();

            var 分部清單 = await _context.分部
                .Where(d => d.單位 == DepNo && d.部門 == DivNo && 已使用分部代碼.Contains(d.分部1))
                .Select(d => new SelectListItem
                {
                    Value = d.分部1,
                    Text = d.分部1 + "_" + d.分部名稱
                }).ToListAsync();

            return Json(分部清單);
        }


        #endregion
        #region 檢驗
        private void ValidateUserHasOrgPermission(string 主檔事業, string 主檔單位, string 主檔部門, string 主檔分部)
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            if (主檔事業 != ua.BusinessNo)
            {
                ModelState.AddModelError("", $"無權操作該事業資料（登入事業為 {ua.BusinessName}）");
            }
            else if(主檔單位 != ua.DepartmentNo)
            {
                ModelState.AddModelError("", $"無權操作該單位資料（登入單位為 {ua.DepartmentName}）");
            }
            else if (主檔部門 != ua.DivisionNo)
            {
                ModelState.AddModelError("", $"無權操作該部門資料（登入部門為 {ua.DivisionName}）");
            }
            else if (主檔分部 != ua.BranchNo)
            {
                ModelState.AddModelError("", $"無權操作該分部資料（登入分部為 {ua.BranchNo}）");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckButtonPermissions([FromBody] 承租人檔DisplayViewModel key) 
        {
            var today = DateTime.Today;
            Debug.WriteLine("📌【CheckButtonPermissions】啟動");


            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
 
            
            if (key.事業 != ua.BusinessNo || key.單位 != ua.DepartmentNo || key.部門 != ua.DivisionNo || key.分部 != ua.BranchNo)  
            {
                Debug.WriteLine("❌ 不是登入者可操作之組織");
                return Ok(new
                {
                    canClickEditOrDelete = false,
                    //canClickEditOrDelete = false,
                    reasons = new
                    {
                        edit = "不是登入者可操作之組織",
                        delete = "不是登入者可操作之組織",
                    }
                }) ;
            }
 
            //bool canClickEditOrDelete = canEditOrDelete;
             

            return Ok(new
            {
                canClickEditOrDelete = true
            });
        }
        #endregion
        #region 驗證
        private async Task ValidateForCreate(承租人檔CreateViewModel model)
        {
            // 1. 檢查是否已有相同案號的收租檔
            var exists = await _context.承租人檔.AnyAsync(x =>
                x.事業 == model.事業 &&
                x.單位 == model.單位 &&
                x.部門 == model.部門 &&
                x.分部 == model.分部 &&
                x.承租人編號 == model.承租人編號
            );

            if (exists)
            {
                ModelState.AddModelError(nameof(model.承租人編號), "已有該承租人");
            }
        }
        private async Task ValidateForDelete(承租人檔DisplayViewModel model)
        {
            // 1. 檢查是否已有相同案號的收租檔
            var exists = await _context.承租人檔.AnyAsync(x =>
                x.事業 == model.事業 &&
                x.單位 == model.單位 &&
                x.部門 == model.部門 &&
                x.分部 == model.分部 &&
                x.承租人編號 == model.承租人編號
            );

            if (exists)
            {
                ModelState.AddModelError(nameof(model.承租人編號), "該承租人具有承租紀錄，故不可刪除該承租人");
            }
        }

        #endregion
    }
}