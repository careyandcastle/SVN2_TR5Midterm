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
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using TR5MidTerm.PC;

namespace TR5MidTerm.Controllers
{
    [ProcUseRang(ProcNo, ProcUseRang.Menu)]
    [TypeFilter(typeof(BaseActionFilter))]
    public class TR5MidTerm03Controller : Controller
    {
        #region 初始化
        private readonly TRDBContext _context;
        private const string ProcNo = "TR5MidTerm";

        private static IConfigurationProvider _config;
        private static IMapper _mapper;

        public TR5MidTerm03Controller(TRDBContext context)
        {
            _context = context;
            _config ??= new MapperConfiguration(cfg =>
            {
                cfg.CreateProjection<商品檔, 商品檔DisplayViewModel>();
                cfg.CreateMap<商品檔DisplayViewModel, 商品檔>();
                cfg.CreateMap<商品檔, 商品檔DisplayViewModel>();

                cfg.CreateMap<商品檔CreateViewModel, 商品檔>();
                cfg.CreateMap<商品檔, 商品檔CreateViewModel>();

                cfg.CreateMap<商品檔EditViewModel, 商品檔>();
                cfg.CreateMap<商品檔, 商品檔EditViewModel>();

            });

            _mapper ??= _config.CreateMapper();
        }
        #endregion
        #region 首頁
        public async Task<IActionResult> Index()
        {
            ViewBag.TableFieldDescDict = new CreateTableFieldsDescription()
                   .Create<商品檔DisplayViewModel>();

            #region query下拉式清單 
            var 已使用事業代碼 = await _context.商品檔
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

            ViewBag.事業選單 = 事業清單;
            ViewBag.單位選單 = 單位清單;
            ViewBag.部門選單 = 部門清單;
            ViewBag.分部選單 = 分部清單;
            #endregion

            return View();
        }

        [HttpPost, ActionName("GetDataPost")]
        [ValidateAntiForgeryToken]
        [NeglectActionFilter]
        public async Task<IActionResult> GetData([FromBody] QueryConditions qc)
        {
            //var sql = (from s in _context.商品檔
            //           select s).AsNoTracking().ProjectTo<商品檔DisplayViewModel>(_config);
            IQueryable<商品檔DisplayViewModel> sql = GetBaseQuery();
            PaginatedList<商品檔DisplayViewModel> queryedData = null;
            queryedData = await PaginatedList<商品檔DisplayViewModel>.CreateAsync(sql, qc);

            return Ok(new
            {
                data = queryedData,
                total = queryedData.TotalCount
            });
        }
        private IQueryable<商品檔DisplayViewModel> GetBaseQuery()
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));

            return (from m in _context.商品檔
                        .Include(m => m.商品類別編號Navigation)
                    join biz in _context.事業 on m.事業 equals biz.事業1
                    join dep in _context.單位 on m.單位 equals dep.單位1
                    join sec in _context.部門 on new { m.單位, m.部門 } equals new { sec.單位, 部門 = sec.部門1 }
                    join sub in _context.分部 on new { m.單位, m.部門, m.分部 } equals new { sub.單位, sub.部門, 分部 = sub.分部1 }
                    //where m.事業 == ua.BusinessNo && m.單位 == ua.DepartmentNo
                    select new 商品檔DisplayViewModel
                    {
                        #region 組織資料
                        事業 = m.事業,
                        事業顯示 = CustomSqlFunctions.ConcatCodeAndName(m.事業, biz.事業名稱),

                        單位 = m.單位,
                        單位顯示 = CustomSqlFunctions.ConcatCodeAndName(m.單位, dep.單位名稱),

                        部門 = m.部門,
                        部門顯示 = CustomSqlFunctions.ConcatCodeAndName(m.部門, sec.部門名稱),

                        分部 = m.分部,
                        分部顯示 = CustomSqlFunctions.ConcatCodeAndName(m.分部, sub.分部名稱),
                        #endregion
                        #region 商品資料
                        商品類別編號 = m.商品類別編號,
                        商品名稱 = m.商品名稱,
                        商品編號 = m.商品編號,
                        商品類別 = m.商品類別編號Navigation.商品類別,
                        商品類別顯示 = CustomSqlFunctions.ConcatCodeAndName(m.商品類別編號, m.商品類別編號Navigation.商品類別),
                        物件編號 = m.物件編號,
                        單價 = m.單價,
                        #endregion
                        #region 修改人與修改時間
                        修改人 = m.修改人,
                        修改時間 = m.修改時間
                        #endregion
                    }); 
        }
        #endregion
        #region 提供index使用
        //[HttpGet]
        public async Task<IActionResult> GetDepartmentSelectList(string Biz)
        {
            var 已使用單位代碼 = await _context.商品檔
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
            var 已使用部門代碼 = await _context.商品檔
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
            var 已使用分部代碼 = await _context.商品檔
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
        #region Create
        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public IActionResult Create()
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            var viewModel = new 商品檔CreateViewModel
            {
                事業 = ua.BusinessNo,
                單位 = ua.DepartmentNo,
                部門 = ua.DivisionNo,
                分部 = ua.BranchNo,
                單價 = 0
            };
            ViewBag.商品類別選項 = _context.商品類別檔
                .OrderBy(x => x.商品類別編號)
                .Select(x => new SelectListItem
                {
                    Text = $"{x.商品類別編號}_{x.商品類別}",
                    Value = x.商品類別編號
                }).ToList();
            //            ViewBag.建物資料清單 = _context.建物主檔
            ////.Where(x => x.組織符合條件)
            //.Where(x =>
            //        x.事業 == ua.BusinessNo &&
            //        x.單位 == ua.DepartmentNo &&
            //        x.部門 == ua.DivisionNo &&
            //        x.分部 == ua.BranchName)
            //.Select(x => new SelectListItem
            //{
            //    Value = x.建物編號,
            //    Text = $"{x.建物編號} - {x.建物名稱}"
            //}).ToList();
            ViewBag.建物資料清單 = new List<SelectListItem>
{
    new SelectListItem { Value = "B001", Text = "建物：B001 - 台北大樓" },
    new SelectListItem { Value = "B002", Text = "建物：B002 - 新竹倉庫" },
    new SelectListItem { Value = "B003", Text = "建物：B003 - 高雄宿舍" },
    new SelectListItem { Value = "R101", Text = "租賃住宅：R101 - 台中公寓A" },
    new SelectListItem { Value = "R102", Text = "租賃住宅：R102 - 台中公寓B" },
};

            return PartialView(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public async Task<IActionResult> Create([Bind("事業,單位,部門,分部,商品編號,商品名稱,商品類別編號,物件編號,單價")] 商品檔CreateViewModel postData)
        {
            if (!ModelState.IsValid)
            {
                ModelStateInvalidResult("Create", false);
                //return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));
            } 

            商品檔 filledData = _mapper.Map<商品檔CreateViewModel, 商品檔>(postData);
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            filledData.修改人 = $"{ua.UserNo}_{ua.UserName}";
            filledData.修改時間 = DateTime.Now;
            _context.Add(filledData);

            try
            {
                var opCount = await _context.SaveChangesAsync();
                if (opCount > 0)
                    return Ok(new ReturnData(ReturnState.ReturnCode.OK)
                    {
                        data = postData
                    });
            }
            catch (Exception ex)
            {
                return CreatedAtAction(nameof(Create), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR)
                {
                    message = ex.Message
                });
            }

            return CreatedAtAction(nameof(Create), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));
        }

        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public IActionResult CreateMulti()
        {
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public async Task<IActionResult> CreateMulti(List<商品檔DisplayViewModel> postData)
        {
            //以下不驗證欄位值是否正確，請視欄位自行刪減
            for (int idx = 0; idx < postData.Count; idx++)
            {
                ModelState.Remove($"postData[{idx}].欄位1");
                ModelState.Remove($"postData[{idx}].欄位2");
                ModelState.Remove($"postData[{idx}].欄位3");
                
                //.....
                //...

                ModelState.Remove($"postData[{idx}].upd_usr");
                ModelState.Remove($"postData[{idx}].upd_dt");
            }
            
            if (ModelState.IsValid == false)
                return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));

            foreach (var item in postData)
            {
                /*
                 *  Put Your Code Here.
                 */
                商品檔 filledData = _mapper.Map<商品檔DisplayViewModel, 商品檔>(item);
                _context.Add(filledData);
            }

            try
            {
                var opCount = await _context.SaveChangesAsync();
                if (opCount > 0)
                    return CreatedAtAction(nameof(CreateMulti), new ReturnData(ReturnState.ReturnCode.OK)
                    {
                        data = postData
                    });
            }
            catch (Exception ex)
            {
                return CreatedAtAction(nameof(CreateMulti), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR)
                {
                    message = ex.Message
                });
            }


            return CreatedAtAction(nameof(CreateMulti), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));
        }
        #endregion
        #region Edit
        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> Edit(string 事業, string 單位, string 部門, string 分部, string 商品編號) 
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 商品編號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            var viewModel = new 商品檔EditViewModel
            {
                事業 = ua.BusinessNo,
                單位 = ua.DepartmentNo,
                部門 = ua.DivisionNo,
                分部 = ua.BranchNo,
                單價 = 0
            };
            ViewBag.商品類別選項 = _context.商品類別檔
                .OrderBy(x => x.商品類別編號)
                .Select(x => new SelectListItem
                {
                    Text = $"{x.商品類別編號}_{x.商品類別}",
                    Value = x.商品類別編號
                }).ToList();
            ViewBag.建物資料清單 = new List<SelectListItem>
{
    new SelectListItem { Value = "B001", Text = "建物：B001 - 台北大樓" },
    new SelectListItem { Value = "B002", Text = "建物：B002 - 新竹倉庫" },
    new SelectListItem { Value = "B003", Text = "建物：B003 - 高雄宿舍" },
    new SelectListItem { Value = "R101", Text = "租賃住宅：R101 - 台中公寓A" },
    new SelectListItem { Value = "R102", Text = "租賃住宅：R102 - 台中公寓B" },
};
            //var result = await _context.商品檔.FindAsync(事業, 單位, 部門, 分部, 商品編號);
            if (viewModel == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            return PartialView(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> Edit([Bind("事業,單位,部門,分部,商品編號,商品名稱,商品類別編號,物件編號,單價")] 商品檔EditViewModel postData)
        {
            if (postData.事業 == null || postData.單位 == null || postData.部門 == null || postData.分部 == null || postData.商品編號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            ValidateUserHasOrgPermission(postData.事業, postData.單位, postData.部門, postData.分部);

            if (ModelState.IsValid == false)
            {
                ModelStateInvalidResult("Edit", true);
                //return BadRequest(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            try
            {
                商品檔 filledData = _mapper.Map<商品檔EditViewModel, 商品檔>(postData);
                var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
                filledData.修改人 = CustomSqlFunctions.ConcatCodeAndName(ua.UserNo, ua.UserName);
                filledData.修改時間 = DateTime.Now;
                _context.Update(filledData);
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
        #region delete
        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> Delete(string 事業, string 單位, string 部門, string 分部, string 商品編號)
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 商品編號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
            }

            //var result = await _context.商品檔.FindAsync(事業, 單位, 部門, 分部, 商品編號);
            //var viewModel = _mapper.Map<商品檔, 商品檔DisplayViewModel>(result);
            var result = await GetBaseQuery()
               .Where(x =>
            x.事業 == 事業 &&
            x.單位 == 單位 &&
            x.部門 == 部門 &&
            x.分部 == 分部 &&
            x.商品編號 == 商品編號)
               .SingleOrDefaultAsync();
            if (result == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
            }

            return PartialView(result);
        }

        //[HttpPost, ActionName("Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> DeleteConfirmed( [Bind("事業,單位,部門,分部,商品編號")] 商品檔DisplayViewModel postData)
        {

            ValidateUserHasOrgPermission(postData.事業, postData.單位, postData.部門, postData.分部);

            if (ModelState.IsValid == false)
            {
                ModelStateInvalidResult("Delete", true);
                //return BadRequest(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
            } 
            var result = await _context.商品檔.FindAsync(postData.事業, postData.單位, postData.部門, postData.分部, postData.商品編號);
            if (result == null)
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));

            _context.商品檔.Remove(result);
            
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
        #region export
        [ProcUseRang(ProcNo, ProcUseRang.Export)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Export([FromBody] QueryConditions qc)
        {
            var sql = (from s in _context.商品檔
                      select s).AsNoTracking().ProjectTo<商品檔DisplayViewModel>(_config);;

            sql = sql.Where(qc.searchBy).PermissionFilter();

            bool hasSortKey = string.IsNullOrEmpty(qc.sortBy) == false;

            if (hasSortKey && qc.isDesc)
            {
                sql = sql.OrderByDescending(qc.sortBy);
            }
            else if (hasSortKey)
            {
                sql = sql.OrderBy(qc.sortBy);
            }

            DataTable dt = sql.ToDataTable();
            GenerateSheets gs = new GenerateSheets();
            MemoryStream memory = gs.DataTableToMemoryStream(dt);
            byte[] byteContent = memory.ToArray();
            memory.Close();

            return File(byteContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
        #endregion
        #region 其它
        public bool isMasterKeyExist(string 事業, string 單位, string 部門, string 分部, string 商品編號)
        {
            return (_context.商品檔.Any(m => m.事業 == 事業 && m.單位 == 單位 && m.部門 == 部門 && m.分部 == 分部 && m.商品編號 == 商品編號) == false);
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
            else if (主檔單位 != ua.DepartmentNo)
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
        public async Task<IActionResult> CheckButtonPermissions([FromBody] 商品檔DisplayViewModel key)
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
                });
            }

            //bool canClickEditOrDelete = canEditOrDelete;


            return Ok(new
            {
                canClickEditOrDelete = true
            });
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

    }
}