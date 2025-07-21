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
    public class TR5MidTerm05Controller : Controller
    {
        #region 初始化
        private readonly TRDBContext _context;
        private const string ProcNo = "TR5MidTerm";

        private static IConfigurationProvider _config;
        private static IMapper _mapper;

        public TR5MidTerm05Controller(TRDBContext context)
        {
            _context = context;
            _config ??= new MapperConfiguration(cfg =>
            {
                cfg.CreateProjection<收款主檔, 收款主檔DisplayViewModel>();
                cfg.CreateMap<收款主檔DisplayViewModel, 收款主檔>();
                cfg.CreateMap<收款主檔, 收款主檔DisplayViewModel>();

                cfg.CreateMap<收款主檔CreateViewModel, 收款主檔>();
                cfg.CreateMap<收款主檔, 收款主檔CreateViewModel>();

                cfg.CreateMap<收款主檔EditViewModel, 收款主檔>();
                cfg.CreateMap<收款主檔, 收款主檔EditViewModel>();
                 

                cfg.CreateMap<收款明細檔DisplayViewModel, 收款明細檔>();
                cfg.CreateMap<收款明細檔, 收款明細檔DisplayViewModel>();

                cfg.CreateMap<收款明細檔CreateViewModel, 收款明細檔>();
                cfg.CreateMap<收款明細檔, 收款明細檔CreateViewModel>();

                cfg.CreateMap<收款明細檔EditViewModel, 收款明細檔>();
                cfg.CreateMap<收款明細檔, 收款明細檔EditViewModel>();

            });

            _mapper ??= _config.CreateMapper();
        }
        #endregion
        #region index

        public async Task<IActionResult> Index()
        {
            ViewBag.TableFieldDescDict = new CreateTableFieldsDescription()
                   .Create<收款主檔DisplayViewModel, 收款明細檔DisplayViewModel>();
            #region query下拉式清單 
            var 已使用事業代碼 = await _context.收款主檔
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
            #endregion
            return View();
        }

        [HttpPost, ActionName("GetDataPost")]
        [ValidateAntiForgeryToken]
        [NeglectActionFilter]
        public async Task<IActionResult> GetData([FromBody] QueryConditions qc)
        {
            //var sql = (from s in _context.收款主檔
            //          select s).AsNoTracking().ProjectTo<收款主檔DisplayViewModel>(_config);
            IQueryable<收款主檔DisplayViewModel> sql = GetBaseQuery().AsNoTracking();
            PaginatedList<收款主檔DisplayViewModel> queryedData = null;
            queryedData = await PaginatedList<收款主檔DisplayViewModel>.CreateAsync(sql, qc);
            foreach (var item in queryedData)
            {
                // 取已收款的最新年月
                var latestYm = _context.收款明細檔
                    .Where(x => x.事業 == item.事業 && x.單位 == item.單位 &&
                                x.部門 == item.部門 && x.分部 == item.分部 && x.案號 == item.案號)
                    .OrderByDescending(x => x.計租年月)
                    .Select(x => x.計租年月)
                    .FirstOrDefault();
                 item.收款日期 = latestYm;
            }

            //收款日期
            return Ok(new
            {
                data = queryedData,
                total = queryedData.TotalCount
            });
        }
        private IQueryable<收款主檔DisplayViewModel> GetBaseQuery()
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            Debug.WriteLine($"[Session] 事業={ua.BusinessNo}, 單位={ua.DepartmentNo}, 部門={ua.DivisionNo}, 分部={ua.BranchNo}");

            return (from m in _context.收款主檔
                    join biz in _context.事業 on m.事業 equals biz.事業1
                    join dep in _context.單位 on m.單位 equals dep.單位1
                    join sec in _context.部門 on new { m.單位, m.部門 } equals new { sec.單位, 部門 = sec.部門1 }
                    join sub in _context.分部 on new { m.單位, m.部門, m.分部 } equals new { sub.單位, sub.部門, 分部 = sub.分部1 }
                    select new 收款主檔DisplayViewModel
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
                        #region 主欄位
                        案號 = m.案號,
                        #endregion

                        #region 修改人與修改時間
                        修改人 = m.修改人,
                        修改時間 = m.修改時間,
                        #endregion
                        #region 明細按鈕控制
                        可否新增明細 = (ua.BusinessNo == m.事業 && ua.DepartmentNo == m.單位 && ua.DivisionNo == m.部門 && ua.BranchNo == m.分部),
                        可否展開明細 = _context.收款明細檔.Any(s => s.事業 == m.事業 && s.單位 == m.單位 && s.部門 == m.部門 && s.分部 == m.分部 && s.案號 == m.案號)
                        #endregion

                    }
                );
        }
        #endregion
        #region Create

        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public IActionResult Create()
        {

            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            #region 租賃用途選項（例如從設定檔或共用表）
            ViewBag.案號選項 = _context.租約主檔
                .Where(x => x.事業 == ua.BusinessNo && x.單位 == ua.DepartmentNo &&
                            x.部門 == ua.DivisionNo && x.分部 == ua.BranchNo)
            .OrderBy(x => x.案號)
            .Select(x => new SelectListItem
            {
                Value = x.案號,
                Text = x.案號 + "_" + x.案名
            })
            .ToList();
            #endregion
            #region viewModel初始化
            var viewModel = new 收款主檔CreateViewModel
            {
                事業 = ua.BusinessNo,
                單位 = ua.DepartmentNo,
                部門 = ua.DivisionNo,
                分部 = ua.BranchNo,
                //租約起始日期 = DateTime.Today
            };
            #endregion
            return PartialView(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public async Task<IActionResult> Create([Bind("事業,單位,部門,分部,案號")] 收款主檔CreateViewModel postData)
        {
            #region 驗證欄位
            await ValidateForCreate(postData);
            //if (ModelState.IsValid == false)
            //    return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));
            if (!ModelState.IsValid)
                return ModelStateInvalidResult("Create", true);
            #endregion
            #region model初始化
            收款主檔 filledData = _mapper.Map<收款主檔CreateViewModel, 收款主檔>(postData);
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            filledData.修改人 = CombineCodeAndName(ua.UserNo, ua.UserName);
            filledData.修改時間 = DateTime.Now;
            _context.Add(filledData);
            #endregion
            try
            {
                #region 寫入DB
                var opCount = await _context.SaveChangesAsync();
                if (opCount > 0)
                {
                    var newData = await GetBaseQuery()
                            .Where(x =>
                                x.事業 == postData.事業 &&
                                x.單位 == postData.單位 &&
                                x.部門 == postData.部門 &&
                                x.分部 == postData.分部 &&
                                x.案號 == postData.案號
                            ).SingleOrDefaultAsync();
                    return Ok(new ReturnData(ReturnState.ReturnCode.OK)
                    {
                        data = newData
                    });
                }
                    
                #endregion
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
        public async Task<IActionResult> CreateMulti(List<收款主檔DisplayViewModel> postData)
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
                收款主檔 filledData = _mapper.Map<收款主檔DisplayViewModel, 收款主檔>(item);
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
  
        #region Delete
        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> Delete(string 事業, string 單位, string 部門, string 分部, string 案號)
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 案號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
            }
            
            var result = await _context.收款主檔.FindAsync(事業, 單位, 部門, 分部, 案號);
            if (result == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
            }

            return PartialView(result);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> DeleteConfirmed(string 事業, string 單位, string 部門, string 分部, string 案號)
        {
            if (ModelState.IsValid == false)
                return BadRequest(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));

            var result = await _context.收款主檔.FindAsync(事業, 單位, 部門, 分部, 案號);
            if (result == null)
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));

            _context.收款主檔.Remove(result);
            
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
            var sql = (from s in _context.收款主檔
                      select s).AsNoTracking().ProjectTo<收款主檔DisplayViewModel>(_config);;

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
        #region 提供index使用
        public async Task<IActionResult> GetDepartmentSelectList(string Biz)
        {
            var 已使用單位代碼 = await _context.收款主檔
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
            var 已使用部門代碼 = await _context.收款主檔
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
        public async Task<IActionResult> GetBranchSelectList(string Biz, string DepNo, string DivNo)
        {
            var 已使用分部代碼 = await _context.收款主檔
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

        #region other
        public bool isMasterKeyExist(string 事業, string 單位, string 部門, string 分部, string 案號)
        {
            return (_context.收款主檔.Any(m => m.事業 == 事業 && m.單位 == 單位 && m.部門 == 部門 && m.分部 == 分部 && m.案號 == 案號) == false);
        }
        #endregion

        //=================================================================================================//

        /*
         * Details
         */
        #region GetDetails
        [HttpPost, ActionName("GetDetailDataPost")]
        [ValidateAntiForgeryToken]
        [NeglectActionFilter]
        public async Task<IActionResult> GetDetails([FromBody] 收款主檔 keys)
        {
            if (keys.事業 == null || keys.單位 == null || keys.部門 == null || keys.分部 == null || keys.案號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.ERROR));
            }

            //var query = from s in _context.收款明細檔
            //            where s.事業 == keys.事業 && s.單位 == keys.單位 && s.部門 == keys.部門 && s.分部 == keys.分部 && s.案號 == keys.案號
            //            select s;

            //var query = GetDetailsBaseQuery();
            var query = GetDetailsBaseQuery().Where(s =>
    s.事業 == keys.事業 &&
    s.單位 == keys.單位 &&
    s.部門 == keys.部門 &&
    s.分部 == keys.分部 &&
    s.案號 == keys.案號
);
            return CreatedAtAction(nameof(GetDetails), new ReturnData(ReturnState.ReturnCode.OK)
            {
                data = await query.ToListAsync()
            });
        }
        private IQueryable<收款明細檔DisplayViewModel> GetDetailsBaseQuery()
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            return (from m in _context.收款明細檔
                    join biz in _context.事業 on m.事業 equals biz.事業1
                    join dep in _context.單位 on m.單位 equals dep.單位1
                    join sec in _context.部門 on new { m.單位, m.部門 } equals new { sec.單位, 部門 = sec.部門1 }
                    join sub in _context.分部 on new { m.單位, m.部門, m.分部 } equals new { sub.單位, sub.部門, 分部 = sub.分部1 }
                    //join p in _context.商品檔 on m.商品編號 equals p.商品編號
                    select new 收款明細檔DisplayViewModel
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

                        #region 主欄位
                        案號 = m.案號,
                        流水號 = m.流水號,
                        金額 = m.金額,
                        計租年月 = m.計租年月,
                       
                        #endregion
                        #region 修改人與修改時間
                        修改人 = m.修改人,
                        修改時間 = m.修改時間,
                        #endregion

                        可否修改明細 = (ua.BusinessNo == m.事業 && ua.DepartmentNo == m.單位 && ua.DivisionNo == m.部門 && ua.BranchNo == m.分部),
                        可否刪除明細 = (ua.BusinessNo == m.事業 && ua.DepartmentNo == m.單位 && ua.DivisionNo == m.部門 && ua.BranchNo == m.分部)
                    }
                );
        }
        #endregion
        #region CreateDetail

        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public async Task<IActionResult> CreateDetail(string 事業, string 單位, string 部門, string 分部, string 案號)
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 案號 == null  )
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.ERROR));
            }
            var result = await _context.租約明細檔.FindAsync(事業, 單位, 部門, 分部, 案號);
 
            var viewModel = new 收款明細檔CreateViewModel
            {
                事業 = 事業,
                單位 = 單位,
                部門 = 部門,
                分部 = 分部,
                案號 = 案號,
                //金額 = result.
            };
            if (viewModel == null)
            {
                return NotFound();
            }

            return PartialView(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public async Task<IActionResult> CreateDetail([Bind("事業,單位,部門,分部,案號,計租年月,金額")] 收款明細檔CreateViewModel postData)
        {
            if (ModelState.IsValid == false)
                return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));

            /*
             *  Put Your Code Here.
             */

            收款明細檔 filledData = _mapper.Map<收款明細檔CreateViewModel, 收款明細檔>(postData);
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));

            // 建立新明細前，先找目前最大流水號
            var newSerialNo = _context.收款明細檔
                .Where(x =>
                    x.事業 == postData.事業 &&
                    x.單位 == postData.單位 &&
                    x.部門 == postData.部門 &&
                    x.分部 == postData.分部 &&
                    x.案號 == postData.案號)
                .Select(x => (int?)x.流水號)  // 轉成 nullable，避免資料為空時拋例外
                .Max() ?? 0;  // 若沒有資料則從 0 開始

            newSerialNo += 1;
            filledData.流水號 = newSerialNo;
            filledData.修改人 = CombineCodeAndName(ua.UserNo, ua.UserName);
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
                return CreatedAtAction(nameof(CreateDetail), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR)
                {
                    message = ex.Message
                });
            }

            return CreatedAtAction(nameof(CreateDetail), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));
        }
        
        public IActionResult CreateMultiDetails()
        {
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMultiDetails(List<收款明細檔DisplayViewModel> postData)
        {
            if (ModelState.IsValid == false)
                return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));

            foreach (var item in postData)
            {
                /*
                 *  Put Your Code Here.
                 */
                收款明細檔 filledData = _mapper.Map<收款明細檔DisplayViewModel, 收款明細檔>(item);
                _context.Add(filledData);
            }

            try
            {
                var opCount = await _context.SaveChangesAsync();
                if (opCount > 0)
                    return CreatedAtAction(nameof(CreateMultiDetails), new ReturnData(ReturnState.ReturnCode.OK)
                    {
                        data = postData
                    });
            }
            catch (Exception ex)
            {
                return CreatedAtAction(nameof(CreateMultiDetails), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR)
                {
                    message = ex.Message
                });
            }


            return CreatedAtAction(nameof(CreateMultiDetails), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));
        }
        #endregion
        #region EditDetail
        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> EditDetail(string 事業, string 單位, string 部門, string 分部, string 案號, int? 流水號,  DateTime? 計租年月) 
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 案號 == null || 計租年月== null || 流水號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            var result = await _context.收款明細檔.FindAsync(事業, 單位, 部門, 分部, 案號, 流水號, 計租年月);
            if (result == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            return PartialView(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> EditDetail(string 事業, string 單位, string 部門, string 分部, string 案號, int 流水號, DateTime? 計租年月, [Bind("事業,單位,部門,分部,案號,計租年月,金額,流水號")] 收款明細檔EditViewModel postData)
        {
            if (ModelState.IsValid == false)
                return CreatedAtAction(nameof(EditDetail), new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));

            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 案號 == null || 計租年月 == null)
                return CreatedAtAction(nameof(EditDetail), new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));

            /*
             *  Put Your Code Here.
             */

            收款明細檔 filledData = _mapper.Map<收款明細檔EditViewModel, 收款明細檔>(postData);
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            filledData.修改人 = CombineCodeAndName(ua.UserNo, ua.UserName);
            filledData.修改時間 = DateTime.Now;
            _context.Update(filledData);

            try
            {
                var opCount = await _context.SaveChangesAsync();
                if (opCount > 0)
                    return CreatedAtAction(nameof(EditDetail), new ReturnData(ReturnState.ReturnCode.OK)
                    {
                        data = postData
                    });
            }
            catch (Exception ex)
            {
                return CreatedAtAction(nameof(EditDetail), new ReturnData(ReturnState.ReturnCode.EDIT_ERROR)
                {
                    message = ex.Message
                });
            }

            return CreatedAtAction(nameof(EditDetail), new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
        }
        #endregion
        #region other2
        public bool isDetailKeyExist(string 事業, string 單位, string 部門, string 分部, string 案號, DateTime 計租年月)
        {
            return (_context.收款明細檔.Any(m => m.事業 == 事業 && m.單位 == 單位 && m.部門 == 部門 && m.分部 == 分部 && m.案號 == 案號 && m.計租年月 == 計租年月) == false);
        }
        #endregion

        #region concatCodeAndName
        //concatCodeAndName只能由SQL使用，因此，我這裡設個這個
        public static string CombineCodeAndName(string code, string name)
        {
            return string.IsNullOrEmpty(name) ? code : $"{code}_{name}";
        }
        #endregion

        #region 驗證
        private async Task ValidateForCreate(收款主檔CreateViewModel model)
        { 
            // 1. 檢查是否已有相同案號的收租檔
            var exists = await _context.收款主檔.AnyAsync(x =>
                x.事業 == model.事業 &&
                x.單位 == model.單位 &&
                x.部門 == model.部門 &&
                x.分部 == model.分部 &&
                x.案號 == model.案號
            );

            if (exists)
            {
                ModelState.AddModelError(nameof(model.案號), "已有該案之收租檔");
            }
        }

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
                "create" => ReturnState.ReturnCode.CREATE_ERROR,
                "edit" => ReturnState.ReturnCode.EDIT_ERROR,
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