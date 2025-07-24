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
using TR5MidTerm.PC;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TR5MidTerm.Controllers
{
    [ProcUseRang(ProcNo, ProcUseRang.Menu)]
    [TypeFilter(typeof(BaseActionFilter))]
    public class TR5MidTerm04Controller : Controller
    {
        #region 初始化
        private readonly TRDBContext _context;
        private const string ProcNo = "TR5MidTerm";

        private static IConfigurationProvider _config;
        private static IMapper _mapper;

        public TR5MidTerm04Controller(TRDBContext context)
        {
            _context = context;
            _config ??= new MapperConfiguration(cfg =>
            {
                cfg.CreateProjection<租約主檔, 租約主檔DisplayViewModel>();
                cfg.CreateMap<租約主檔DisplayViewModel, 租約主檔>();
                cfg.CreateMap<租約主檔, 租約主檔DisplayViewModel>();
                cfg.CreateMap<租約主檔EditViewModel, 租約主檔>();
                cfg.CreateMap<租約主檔, 租約主檔EditViewModel>();
                cfg.CreateMap<租約主檔CreateViewModel, 租約主檔>();
                cfg.CreateMap<租約主檔, 租約主檔CreateViewModel>();


                cfg.CreateMap<租約明細檔EditViewModel, 租約明細檔>();
                cfg.CreateMap<租約明細檔, 租約明細檔EditViewModel>();
                cfg.CreateMap<租約明細檔CreateViewModel, 租約明細檔>();
                cfg.CreateMap<租約明細檔, 租約明細檔CreateViewModel>();
                cfg.CreateMap<租約明細檔DisplayViewModel, 租約明細檔>();
                cfg.CreateMap<租約明細檔, 租約明細檔DisplayViewModel>();

            });

            _mapper ??= _config.CreateMapper();
        }
        #endregion
        #region index
        public async Task<IActionResult> Index()
        {
            ViewBag.TableFieldDescDict = new CreateTableFieldsDescription()
                   .Create<租約主檔DisplayViewModel, 租約明細檔DisplayViewModel>();
            #region query下拉式清單 
            var 已使用事業代碼 = await _context.租約主檔
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
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
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
            IQueryable<租約主檔DisplayViewModel> sql = GetBaseQuery().AsNoTracking();

            PaginatedList<租約主檔DisplayViewModel> queryedData = null;
            queryedData = await PaginatedList<租約主檔DisplayViewModel>.CreateAsync(sql, qc);

            foreach (var item in queryedData)
            {
                // 每期租金含稅
                var 每期租金含稅 = (
                    from d in _context.租約明細檔
                    join p in _context.商品檔
                      on new { d.事業, d.單位, d.部門, d.分部, d.商品編號 }
                      equals new { p.事業, p.單位, p.部門, p.分部, p.商品編號 }
                    where d.事業 == item.事業 && d.單位 == item.單位 &&
                          d.部門 == item.部門 && d.分部 == item.分部 && d.案號 == item.案號
                    select d.數量 * p.單價 * 1.05m
                ).Sum();

                item.每期租金含稅 = 每期租金含稅;

                // 總期數（至今累計應收幾期）
                int totalMonthsPassed = (DateTime.Today.Year - item.租約起始日期.Year) * 12
                                      + (DateTime.Today.Month - item.租約起始日期.Month);

                int monthsPassed = Math.Min(totalMonthsPassed, item.租期月數);
                int 累計應收期數 = monthsPassed / Math.Max(1, item.計租週期月數);
                item.累計月數 = monthsPassed;

                // 取已收款的最新年月
                var latestYm = _context.收款明細檔
                    .Where(x => x.事業 == item.事業 && x.單位 == item.單位 &&
                                x.部門 == item.部門 && x.分部 == item.分部 && x.案號 == item.案號)
                    .OrderByDescending(x => x.計租年月)
                    .Select(x => x.計租年月)
                    .FirstOrDefault();

                int paidPeriod = 0;
                DateTime? 下次收租日 = null;

                if (latestYm != default)
                {
                    int paidMonths = (latestYm.Year - item.租約起始日期.Year) * 12
                                   + (latestYm.Month - item.租約起始日期.Month);
                    paidPeriod = paidMonths / Math.Max(1, item.計租週期月數);

                    // 推估下次收租日
                    var latestDate = new DateTime(latestYm.Year, latestYm.Month, item.租約起始日期.Day);
                    下次收租日 = latestDate.AddMonths(item.計租週期月數);
                }
                else
                {
                    paidPeriod = 0;
                    下次收租日 = item.租約起始日期.AddMonths(item.計租週期月數);
                }

                // 補上欄位
                item.未繳期數 = Math.Max(0, 累計應收期數 - paidPeriod);
                item.下次收租日期 = 下次收租日;
                item.累計應收租金含稅 = item.每期租金含稅 * item.未繳期數;
                item.可收租 = 下次收租日 < DateTime.Now && item.未繳期數 != 0 ;
                item.超過收租期限 = (下次收租日 < DateTime.Now.AddDays(item.繳款期限天數)) && (item.未繳期數 != 0);
                item.租約終止日期 = item.租約起始日期.AddMonths(item.租期月數);
            }


            return Ok(new
            {
                data = queryedData,
                total = queryedData.TotalCount
            });
        }
        private IQueryable<租約主檔DisplayViewModel> GetBaseQuery()
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            var today = DateTime.Today; // ✅ 提前轉換為可轉譯參數

            return (from m in _context.租約主檔
                    join biz in _context.事業 on m.事業 equals biz.事業1
                    join dep in _context.單位 on m.單位 equals dep.單位1
                    join sec in _context.部門 on new { m.單位, m.部門 } equals new { sec.單位, 部門 = sec.部門1 }
                    join sub in _context.分部 on new { m.單位, m.部門, m.分部 } equals new { sub.單位, sub.部門, 分部 = sub.分部1 }
                    //join man in _context.承租人檔 on new { m.事業, m.單位, m.部門, m.分部, m.承租人編號 } equals new { man.事業, man.單位, man.部門, man.分部, man.承租人編號 }
                    select new 租約主檔DisplayViewModel
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
                        案名 = m.案名,
                        承租人編號 = m.承租人編號,
                        //租賃方式編號 = m.租賃方式編號,
                        租賃方式編號 = m.租賃方式編號,  // 要 Include 或 join 對應資料
                        租賃用途 = m.租賃用途,
                        租約起始日期 = m.租約起始日期,
                        租期月數 = m.租期月數,
                        計租週期月數 = m.計租週期月數,
                        繳款期限天數 = m.繳款期限天數,
                        //租約終止日期 = m.租約終止日期,
                        備註 = m.備註,
                        #endregion
                        #region naviagtion
                        // 📌 顯示用欄位（從 Navigation 或對照表取）
                        租賃方式顯示 = CustomSqlFunctions.ConcatCodeAndName(m.租賃方式編號Navigation.租賃方式編號, m.租賃方式編號Navigation.租賃方式),
                        //m.租賃方式編號Navigation.租賃方式編號  // 要 Include 或 join 對應資料
                        #endregion
                        //承租人顯示 = CustomSqlFunctions.ConcatCodeAndName(m.承租人編號, CustomSqlFunctions.DecryptToString(man.承租人)),

                        #region 修改人與修改時間
                        修改人 = m.修改人,
                        修改時間 = m.修改時間,
                        #endregion
                        #region 明細按鈕控制
                        可否新增明細 = (ua.BusinessNo == m.事業 && ua.DepartmentNo == m.單位 && ua.DivisionNo == m.部門 && ua.BranchNo == m.分部),
                        可否展開明細 = _context.租約明細檔.Any(s => s.事業 == m.事業 && s.單位 == m.單位 && s.部門 == m.部門 && s.分部 == m.分部 && s.案號 == m.案號),
                        可否編輯刪除 = (m.事業 == ua.BusinessNo && m.單位 == ua.DepartmentNo && m.部門 == ua.DivisionNo && m.分部 == ua.BranchNo)
                        #endregion
                    }
                );
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
            // 承租人選項
            ViewBag.承租人選項 = _context.承租人檔
                .OrderBy(x => x.承租人編號)
                .Select(x => new SelectListItem
                {
                    Value = x.承租人編號,
                    Text = $"{x.承租人編號} - {CustomSqlFunctions.DecryptToString(x.承租人)}"
                }).ToList();

            // 租賃方式選項
            ViewBag.租賃方式選項 = _context.租賃方式檔
                .OrderBy(x => x.租賃方式編號)
                .Select(x => new SelectListItem
                {
                    Value = x.租賃方式編號,
                    Text = $"{x.租賃方式編號} - {x.租賃方式}"
                }).ToList();

            // 租賃用途選項（例如從設定檔或共用表）
            ViewBag.租賃用途選項 = _context.租賃用途檔
            .OrderBy(x => x.租賃用途編號)
            .Select(x => new SelectListItem
            {
                Value = x.租賃用途,
                Text = x.租賃用途編號 + "_" + x.租賃用途
            })
            .ToList();


            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            var viewModel = new 租約主檔CreateViewModel
            {
                事業 = ua.BusinessNo,
                單位 = ua.DepartmentNo,
                部門 = ua.DivisionNo,
                分部 = ua.BranchNo,
                租約起始日期 = DateTime.Today
            };
            return PartialView(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public async Task<IActionResult> Create([Bind("事業,單位,部門,分部,案號,案名,承租人編號,租賃方式編號,租賃用途,租約起始日期,租期月數,計租週期月數,繳款期限天數,租約終止日期,備註")] 租約主檔CreateViewModel postData)
        {
            //return new EmptyResult(); // 不回傳任何內容，悄悄中止
            //return NoContent(); // HTTP 204
            if (ModelState.IsValid == false)
                return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));

            /*
             *  Put Your Code Here.
             */

            租約主檔 filledData = _mapper.Map<租約主檔CreateViewModel, 租約主檔>(postData);

            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));

            filledData.備註 = string.IsNullOrWhiteSpace(postData.備註) ? "" : postData.備註;
            filledData.修改人 = CombineCodeAndName(ua.UserNo, ua.UserName);
            filledData.修改時間 = DateTime.Now;
            _context.Add(filledData);
             

            try
            {
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
        public async Task<IActionResult> CreateMulti(List<租約主檔DisplayViewModel> postData)
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
                租約主檔 filledData = _mapper.Map<租約主檔DisplayViewModel, 租約主檔>(item);
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
        public async Task<IActionResult> Edit(string 事業, string 單位, string 部門, string 分部, string 案號)
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 案號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }




            var result = await _context.租約主檔.FindAsync(事業, 單位, 部門, 分部, 案號);
            var viewModel = _mapper.Map<租約主檔, 租約主檔EditViewModel>(result);
            if (viewModel == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }


            // 承租人選項
            ViewBag.承租人選項 = _context.承租人檔
                .OrderBy(x => x.承租人編號)
                .Select(x => new SelectListItem
                {
                    Value = x.承租人編號,
                    Text = $"{x.承租人編號} - {CustomSqlFunctions.DecryptToString(x.承租人)}"
                }).ToList();

            // 租賃方式選項
            ViewBag.租賃方式選項 = _context.租賃方式檔
                .OrderBy(x => x.租賃方式編號)
                .Select(x => new SelectListItem
                {
                    Value = x.租賃方式編號,
                    Text = $"{x.租賃方式編號} - {x.租賃方式}"
                }).ToList();

            ViewBag.租賃用途選項 = _context.租賃用途檔
            .OrderBy(x => x.租賃用途編號)
            .Select(x => new SelectListItem
            {
                Value = x.租賃用途,
                Text = x.租賃用途編號 + "_" + x.租賃用途
            })
            .ToList();


            return PartialView(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> Edit(string 事業, string 單位, string 部門, string 分部, string 案號, [Bind("事業,單位,部門,分部,案號,案名,承租人編號,租賃方式編號,租賃用途,租約起始日期,租期月數,計租週期月數,繳款期限天數,租約終止日期,備註")] 租約主檔EditViewModel postData)
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 案號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            if (事業 != postData.事業 || 單位 != postData.單位 || 部門 != postData.部門 || 分部 != postData.分部 || 案號 != postData.案號)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            if (ModelState.IsValid == false)
                return BadRequest(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));

            try
            {
                /*
                *  Put Your Code Here.(ua.UserNo, ua.UserName);
                */

                租約主檔 filledData = _mapper.Map<租約主檔EditViewModel, 租約主檔>(postData);
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
        #region Delete
        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> Delete(string 事業, string 單位, string 部門, string 分部, string 案號)
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 案號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
            }

            var result = await GetBaseQuery()
               .Where(x =>
            x.事業 == 事業 &&
            x.單位 == 單位 &&
            x.部門 == 部門 &&
            x.分部 == 分部 &&
            x.案號 == 案號)
               .SingleOrDefaultAsync();

            //var viewModel = _mapper.Map<租約主檔DisplayViewModel, 租約主檔>(result);
            if (result == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
            }

            return PartialView(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> DeleteConfirmed([Bind("事業,單位,部門,分部,案號")] 租約主檔DisplayViewModel postData)
        {
            await ValidateForDelete(postData);
            if (ModelState.IsValid == false)
                ModelStateInvalidResult("Delete", false);
            //return BadRequest(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));



            // 取得要刪除的主檔資料
            var master = await _context.租約主檔.FindAsync(postData.事業, postData.單位, postData.部門, postData.分部, postData.案號);
            if (master == null)
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR) { message = "查無主檔資料" });

            // 開啟交易（確保主從刪除的一致性）
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 刪除明細檔（全部找出來再 Remove）
                var details = await _context.租約明細檔
                    .Where(x =>
                        x.事業 == postData.事業 &&
                        x.單位 == postData.單位 &&
                        x.部門 == postData.部門 &&
                        x.分部 == postData.分部 &&
                        x.案號 == postData.案號)
                    .ToListAsync();

                _context.租約明細檔.RemoveRange(details);

                // 刪除主檔
                _context.租約主檔.Remove(master);

                // 實際執行刪除
                var count = await _context.SaveChangesAsync();

                // 提交交易
                await transaction.CommitAsync();

                if (count > 0)
                    return Ok(new ReturnData(ReturnState.ReturnCode.OK));
                else
                    return CreatedAtAction(nameof(DeleteConfirmed), new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                //_logger.LogError(ex, $"[DeleteConfirmed] 刪除主從資料失敗：{ex.Message}");

                return StatusCode(500, new ReturnData(ReturnState.ReturnCode.DELETE_ERROR)
                {
                    message = "刪除失敗，請稍後再試"
                });
            }
        }
        #endregion
        #region Export

        [ProcUseRang(ProcNo, ProcUseRang.Export)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Export([FromBody] QueryConditions qc)
        {
            var sql = (from s in _context.租約主檔
                       select s).AsNoTracking().ProjectTo<租約主檔DisplayViewModel>(_config); ;

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
        #region Charge
        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> Charge(string 事業, string 單位, string 部門, string 分部, string 案號)
        {
            if (string.IsNullOrWhiteSpace(事業) || string.IsNullOrWhiteSpace(單位) ||
                string.IsNullOrWhiteSpace(部門) || string.IsNullOrWhiteSpace(分部) || string.IsNullOrWhiteSpace(案號))
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            #region ✅ 檢查租約主檔是否存在
            var rentMaster = await _context.租約主檔.FindAsync(事業, 單位, 部門, 分部, 案號);
            if (rentMaster == null)
            {
                ViewBag.無租約主檔 = true;
                return PartialView();
            }
            else
            {
                ViewBag.無租約主檔 = false;
            }
            #endregion

            #region ✅ 檢查是否已建立收款主檔（不然不可收租）
            var chargeMaster = await _context.收款主檔.FindAsync(事業, 單位, 部門, 分部, 案號);
            if (chargeMaster == null)
            {
                ViewBag.無收款主檔 = true;
                return PartialView();
            }
            else{
                ViewBag.無收款主檔 = false;
            }
            #endregion

            #region ✅ 推算起算年月（下次收租年月）
            var lastYm = await _context.收款明細檔
                .Where(x => x.事業 == 事業 && x.單位 == 單位 &&
                            x.部門 == 部門 && x.分部 == 分部 && x.案號 == 案號)
                .OrderByDescending(x => x.計租年月)
                .Select(x => (DateTime?)x.計租年月)
                .FirstOrDefaultAsync();



            #region ✅ 推算剩餘月數與可收期數上限
            var 租期月數 = rentMaster.租期月數;
            var 終止日 = rentMaster.租約終止日期 ?? rentMaster.租約起始日期.AddMonths(租期月數);
            //var 終止年月 = 終止日;


            var 計租週期 = Math.Max(1, rentMaster.計租週期月數);
            DateTime nextStartYm;

            if (lastYm.HasValue)
            {
                // ✅ 取 .Value 才能使用 .Year 和 .Month
                var 剩餘 = ((終止日.Year - lastYm.Value.Year) * 12 + (終止日.Month - lastYm.Value.Month) + 1);
                if (剩餘 < rentMaster.計租週期月數){
                    nextStartYm = lastYm.Value.AddMonths(剩餘);
                }
                else
                {
                    nextStartYm = lastYm.Value.AddMonths(計租週期);
                }
            }
            else
            {
                var 起始日 = rentMaster.租約起始日期;
                //nextStartYm = new DateTime(起始日.Year, 起始日.Month, 1);
                nextStartYm = 起始日;
            }
            #endregion

            #region ✅ 計算每月租金
            var 每月租金 = await (
                from d in _context.租約明細檔
                join p in _context.商品檔
                    on new { d.事業, d.單位, d.部門, d.分部, d.商品編號 }
                    equals new { p.事業, p.單位, p.部門, p.分部, p.商品編號 }
                where d.事業 == 事業 && d.單位 == 單位 &&
                      d.部門 == 部門 && d.分部 == 分部 && d.案號 == 案號
                select d.數量 * p.單價 * 1.05m
            ).SumAsync();

            var 每期租金 = 每月租金 * 計租週期;
            #endregion


            var 剩餘月數 = ((終止日.Year - nextStartYm.Year) * 12) + (終止日.Month - nextStartYm.Month);
            var 可收期數上限 = (int)Math.Ceiling((decimal)剩餘月數 / 計租週期);
            #endregion

            #region ✅ 組 ViewModel
            var viewModel = new 收款明細檔CreateViewModel
            {
                事業 = 事業,
                單位 = 單位,
                部門 = 部門,
                分部 = 分部,
                案號 = 案號,
                計租年月 = nextStartYm,
                計租年月_判斷殘月用 = nextStartYm,
                案號名顯示 = $"{案號}_{rentMaster.案名}",
                每期月數 = 計租週期,
                每月租金含稅 = 每月租金,
                每期租金含稅 = 每期租金,
                剩餘可收月數 = 剩餘月數,
                可收期數上限 = 可收期數上限
            };
            #endregion

            return PartialView(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> Charge([Bind("事業,單位,部門,分部,案號,計租年月, 計租年月_判斷殘月用,本次收幾期")] 收款明細檔CreateViewModel postData)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));

            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));

            // ✅ 查租約主檔，取得週期、終止年月
            var rent = await _context.租約主檔.FindAsync(postData.事業, postData.單位, postData.部門, postData.分部, postData.案號);
            if (rent == null)
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR) { message = "找不到租約主檔" });

            #region 更新收款主檔

            var 每期月數 = Math.Max(1, rent.計租週期月數);
            var 終止日 = rent.租約終止日期 ?? rent.租約起始日期.AddMonths(rent.租期月數 - 1);
            var 終止年月 = new DateTime(終止日.Year, 終止日.Month, 1);

            // ✅ 查每月租金（JOIN 商品檔）
            var 每月金額 = await (
                from d in _context.租約明細檔
                join p in _context.商品檔
                    on new { d.事業, d.單位, d.部門, d.分部, d.商品編號 }
                    equals new { p.事業, p.單位, p.部門, p.分部, p.商品編號 }
                where d.事業 == postData.事業 && d.單位 == postData.單位 &&
                      d.部門 == postData.部門 && d.分部 == postData.分部 &&
                      d.案號 == postData.案號
                select d.數量 * p.單價 * 1.05m
            ).SumAsync();

            var 起算年月 = postData.計租年月;
            var 起算年月_判斷殘月用 = postData.計租年月_判斷殘月用;
            var maxSerial = await _context.收款明細檔
                .Where(x =>
                    x.事業 == postData.事業 && x.單位 == postData.單位 &&
                    x.部門 == postData.部門 && x.分部 == postData.分部 && x.案號 == postData.案號)
                .MaxAsync(x => (int?)x.流水號) ?? 0;

            var 新增筆數 = 0;
            //var currentYm_殘月計算 = new DateTime(起算年月_判斷殘月用.Year, 起算年月_判斷殘月用.Month, 1);
            var currentYm_殘月計算 = 起算年月_判斷殘月用;
            //var currentYm_計租年月標記 = new DateTime(起算年月.Year, 起算年月.Month, 1);
            var currentYm_計租年月標記 = 起算年月;
            var 實際月數總計 = 0;

            for (int i = 0; i < postData.本次收幾期; i++)
            {
                int 本期月數;

                // 判斷是否為尾期
                var 剩餘月數 = ((終止年月.Year - currentYm_殘月計算.Year) * 12) + (終止年月.Month - currentYm_殘月計算.Month) + 1;
                if (剩餘月數 <= 0)
                    break;
               
                if (剩餘月數 < 每期月數)
                {
                    本期月數 = 剩餘月數; // 尾期
                }
                else
                {
                    本期月數 = 每期月數;
                }

                var 本期金額 = 每月金額 * 本期月數;


                // 跳下期
                currentYm_殘月計算 = currentYm_殘月計算.AddMonths(本期月數);
                currentYm_計租年月標記 = currentYm_計租年月標記.AddMonths(本期月數);

                var entity = new 收款明細檔
                {
                    事業 = postData.事業,
                    單位 = postData.單位,
                    部門 = postData.部門,
                    分部 = postData.分部,
                    案號 = postData.案號,
                    計租年月 = currentYm_計租年月標記,
                    金額 = 本期金額,
                    流水號 = ++maxSerial,
                    修改人 = CombineCodeAndName(ua.UserNo, ua.UserName),
                    修改時間 = DateTime.Now
                };


                _context.收款明細檔.Add(entity);
                新增筆數++;
                實際月數總計 += 本期月數;

                
            }

            // ✅ 更新收款主檔 修改時間
            var chargeMaster = await _context.收款主檔.FindAsync(postData.事業, postData.單位, postData.部門, postData.分部, postData.案號);
            if (chargeMaster != null)
            {
                chargeMaster.修改人 = CombineCodeAndName(ua.UserNo, ua.UserName);
                chargeMaster.修改時間 = DateTime.Now;
            }
            #endregion
             
            #region 更新水電表

            // Step 1️⃣ 查出該案號所有的「總表號 + 分表號」組合（先存在記憶體中）
            var 租約水電清單 = await _context.租約水電檔
                .Where(x =>
                    x.事業 == postData.事業 &&
                    x.單位 == postData.單位 &&
                    x.部門 == postData.部門 &&
                    x.分部 == postData.分部 &&
                    x.案號 == postData.案號)
                .Select(x => new { x.總表號, x.分表號 })
                .ToListAsync();

            // Step 2️⃣ 查出所有符合事業/單位/部門/分部 的水電分表檔
            var 全部水電分表 = await _context.水電分表檔
                .Where(x =>
                    x.事業 == postData.事業 &&
                    x.單位 == postData.單位 &&
                    x.部門 == postData.部門 &&
                    x.分部 == postData.分部)
                .ToListAsync();

            // Step 3️⃣ 在記憶體中做 Join
            var 水電分表清單 = (
                from 水電 in 全部水電分表
                join 租水 in 租約水電清單
                  on new { 水電.總表號, 水電.分表號 } equals new { 租水.總表號, 租水.分表號 }
                select 水電
            ).ToList();

            // Step 4️⃣ 同步上期度數
            foreach (var item in 水電分表清單)
            {
                item.上期度數 = item.本期度數;
                item.修改人 = CombineCodeAndName(ua.UserNo, ua.UserName);
                item.修改時間 = DateTime.Now;
            }
            #endregion

            // ✅ 儲存異動
            try
            {
                var opCount = await _context.SaveChangesAsync();
                return Ok(new ReturnData(ReturnState.ReturnCode.OK)
                {
                    message = $"已成功新增 {新增筆數} 筆收租紀錄，共 {實際月數總計} 月",
                    data = new
                    {
                        實際月數 = 實際月數總計,
                        最後截止年月 = currentYm_計租年月標記.AddMonths(-1),
                        起始年月 = postData.計租年月
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ReturnData(ReturnState.ReturnCode.CREATE_ERROR)
                {
                    message = "儲存失敗: " + ex.Message
                });
            }

            return StatusCode(500, new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));
        }

        #endregion
        #region other
        public bool isMasterKeyExist(string 事業, string 單位, string 部門, string 分部, string 案號)
        {
            return (_context.租約主檔.Any(m => m.事業 == 事業 && m.單位 == 單位 && m.部門 == 部門 && m.分部 == 分部 && m.案號 == 案號) == false);
        }

        #endregion
        //=================================================================================================//

        /*
         * Details
         */
        #region Detail
        [HttpPost, ActionName("GetDetailDataPost")]
        [ValidateAntiForgeryToken]
        [NeglectActionFilter]
        public async Task<IActionResult> GetDetails([FromBody] 租約主檔 keys)
        {
            if (keys.事業 == null || keys.單位 == null || keys.部門 == null || keys.分部 == null || keys.案號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.ERROR));
            }

            //var query = from s in _context.租約明細檔
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

        private IQueryable<租約明細檔DisplayViewModel> GetDetailsBaseQuery()
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            return (from m in _context.租約明細檔
                    join biz in _context.事業 on m.事業 equals biz.事業1
                    join dep in _context.單位 on m.單位 equals dep.單位1
                    join sec in _context.部門 on new { m.單位, m.部門 } equals new { sec.單位, 部門 = sec.部門1 }
                    join sub in _context.分部 on new { m.單位, m.部門, m.分部 } equals new { sub.單位, sub.部門, 分部 = sub.分部1 }
                    join p in _context.商品檔
    on new { m.事業, m.單位, m.部門, m.分部, m.商品編號 }
    equals new { p.事業, p.單位, p.部門, p.分部, p.商品編號 }
                    select new 租約明細檔DisplayViewModel
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
                        商品編號 = m.商品編號,
                        商品名稱顯示 = CustomSqlFunctions.ConcatCodeAndName(p.商品編號, p.商品名稱),
                        數量 = m.數量,
                        總金額 = m.數量 * p.單價,
                        #endregion
                        #region 修改人與修改時間
                        修改人 = m.修改人,
                        修改時間 = m.修改時間,
                        #endregion

                        可否修改明細 = (ua.BusinessNo == m.事業 && ua.DepartmentNo == m.單位 && ua.DivisionNo == m.部門 && ua.BranchNo == m.分部),
                        可否刪除明細 = (ua.BusinessNo == m.事業 && ua.DepartmentNo == m.單位 && ua.DivisionNo == m.部門 && ua.BranchNo == m.分部)
                    }
                ) ;
        }
        #endregion
        #region CreateDetail

        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public async Task<IActionResult> CreateDetail(string 事業, string 單位, string 部門, string 分部, string 案號)
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 案號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.ERROR));
            }
            //var item = await _context.租約主檔.FindAsync(事業, 單位, 部門, 分部, 案號);
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));



            #region 避免主檔過期時可以新增
            // ✅ 查詢租約主檔
            var 租約 = await _context.租約主檔
                .Where(x => x.事業 == 事業 && x.單位 == 單位 &&
                            x.部門 == 部門 && x.分部 == 分部 && x.案號 == 案號)
                .Select(x => new
                {
                    x.租約起始日期,
                    x.租期月數,
                    x.租約終止日期
                })
                .FirstOrDefaultAsync();

            if (租約 == null)
                return NotFound(new ReturnData(ReturnState.ReturnCode.ERROR));

            // ✅ 計算終止日期（如未填則用 起始日 + 月數）
            var 租約結束日 = 租約.租約終止日期 ?? 租約.租約起始日期.AddMonths(租約.租期月數);

            // ✅ 若租約已過期，禁止新增
            if (租約結束日 < DateTime.Today)
            {
                ViewBag.過期主檔 = true;
                return PartialView();
                //return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR)
                //{
                //    message = $"此租約已於 {租約結束日:yyyy/MM/dd} 結束，不可新增明細。"
                //});
            }
            ViewBag.過期主檔 = false;

            #endregion
            var viewModel = new 租約明細檔CreateViewModel
            {
                事業 = 事業,
                單位 = 單位,
                部門 = 部門,
                分部 = 分部,
                案號 = 案號
            };

            // ✅ 查詢所有租期未結束的商品（可自行改用 today > 租約起始日期）
            var 租用中商品 = _context.租約明細檔
    .Join(_context.租約主檔,
        m => new { m.事業, m.單位, m.部門, m.分部, m.案號 },
        h => new { h.事業, h.單位, h.部門, h.分部, h.案號 },
        (m, h) => new
        {
            m.商品編號,
            h.租約起始日期,
            h.租期月數,
            租約終止日期 = h.租約終止日期 ?? h.租約起始日期.AddMonths(h.租期月數)
        })
    .Where(x =>
        x.租約起始日期 <= DateTime.Now &&
        x.租約終止日期 >= DateTime.Now)
    .AsEnumerable() // ← 關鍵：讓後面 GroupBy 用 C# 執行
    .GroupBy(x => x.商品編號)
    .ToDictionary(g => g.Key, g => g.Max(x => x.租約終止日期));


            ViewBag.商品選單 = _context.商品檔
                .Where(x =>
                x.事業 == 事業 &&
                x.單位 == 單位 &&
                x.部門 == 部門 &&
                x.分部 == 分部)
                .Select(x => new SelectListItem
                {
                    Value = x.商品編號,
                    Text = 租用中商品.ContainsKey(x.商品編號)
            ? $"{x.商品編號} - {x.商品名稱} ⚠已租至 {租用中商品[x.商品編號]:yyyy/MM/dd}"
            : $"{x.商品編號} - {x.商品名稱}",
                    Disabled = 租用中商品.ContainsKey(x.商品編號) // ← 如不想禁止選擇請改為 false
                })
    .ToList();
            if (viewModel == null)
            {
                return NotFound();
            }

            return PartialView(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public async Task<IActionResult> CreateDetail([Bind("事業,單位,部門,分部,案號,商品編號,數量,修改人,修改時間")] 租約明細檔CreateViewModel postData)
        {
            if (ModelState.IsValid == false)
                return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));

            /*
             *  Put Your Code Here.
             */

            租約明細檔 filledData = _mapper.Map<租約明細檔CreateViewModel, 租約明細檔>(postData);
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
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
        public async Task<IActionResult> CreateMultiDetails(List<租約明細檔DisplayViewModel> postData)
        {
            if (ModelState.IsValid == false)
                return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));

            foreach (var item in postData)
            {
                /*
                 *  Put Your Code Here.
                 */
                租約明細檔 filledData = _mapper.Map<租約明細檔DisplayViewModel, 租約明細檔>(item);
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
        public async Task<IActionResult> EditDetail(string 事業, string 單位, string 部門, string 分部, string 案號, string 商品編號)
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 案號 == null || 商品編號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }



            var result = await _context.租約明細檔.FindAsync(事業, 單位, 部門, 分部, 案號, 商品編號);
            var viewModel = _mapper.Map<租約明細檔, 租約明細檔EditViewModel>(result);
            if (viewModel == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            // ✅ 查詢所有租期未結束的商品（可自行改用 today > 租約起始日期）
            var 租用中商品 = _context.租約明細檔
    .Join(_context.租約主檔,
        m => new { m.事業, m.單位, m.部門, m.分部, m.案號 },
        h => new { h.事業, h.單位, h.部門, h.分部, h.案號 },
        (m, h) => new
        {
            m.商品編號,
            h.租約起始日期,
            h.租期月數,
            租約終止日期 = h.租約終止日期 ?? h.租約起始日期.AddMonths(h.租期月數)
        })
    .Where(x =>
        x.租約起始日期 <= DateTime.Now &&
        x.租約終止日期 >= DateTime.Now)
    .AsEnumerable() // ← 關鍵：讓後面 GroupBy 用 C# 執行
    .GroupBy(x => x.商品編號)
    .ToDictionary(g => g.Key, g => g.Max(x => x.租約終止日期));


            ViewBag.商品選單 = _context.商品檔
                .Where(x =>
                x.事業 == 事業 &&
                x.單位 == 單位 &&
                x.部門 == 部門 &&
                x.分部 == 分部)
                .Select(x => new SelectListItem
                {
                    Value = x.商品編號,
                    Text = 租用中商品.ContainsKey(x.商品編號)
            ? $"{x.商品編號} - {x.商品名稱} ⚠已租至 {租用中商品[x.商品編號]:yyyy/MM/dd}"
            : $"{x.商品編號} - {x.商品名稱}",
                    Disabled = 租用中商品.ContainsKey(x.商品編號) // ← 如不想禁止選擇請改為 false
                })
    .ToList();

            return PartialView(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> EditDetail(string 事業, string 單位, string 部門, string 分部, string 案號, string 商品編號, [Bind("事業,單位,部門,分部,案號,商品編號,數量")] 租約明細檔EditViewModel postData)
        {
            if (ModelState.IsValid == false)
                return CreatedAtAction(nameof(EditDetail), new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));

            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 案號 == null || 商品編號 == null)
                return CreatedAtAction(nameof(EditDetail), new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));

            /*
             *  Put Your Code Here.
             */

            租約明細檔 filledData = _mapper.Map<租約明細檔EditViewModel, 租約明細檔>(postData);
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
        #region DeleteDetail

        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> DeleteDetail(string 事業, string 單位, string 部門, string 分部, string 案號, string 商品編號)
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 案號 == null || 商品編號 == null)
            {
                return NotFound();
            }

            //var result = await _context.租約明細檔.FindAsync(事業, 單位, 部門, 分部, 案號, 商品編號);
            var result = GetDetailsBaseQuery().Where(m => m.事業 == 事業 && m.單位 == 單位 && m.部門 == 部門 && m.分部 == 分部).FirstOrDefault();
            if (result == null)
            {
                return NotFound();
            }

            return PartialView(result);
        }

        //[HttpPost, ActionName("DeleteDetail")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> DeleteDetailConfirmed(string 事業, string 單位, string 部門, string 分部, string 案號, string 商品編號, [Bind("事業,單位,部門,分部,案號,商品編號")] 租約明細檔DisplayViewModel postData)
        {
            
            if (ModelState.IsValid == false)
                return CreatedAtAction(nameof(DeleteDetailConfirmed), new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));

            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 案號 == null || 商品編號 == null)
                return CreatedAtAction(nameof(DeleteDetailConfirmed), new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));


            var result = await _context.租約明細檔.FindAsync(事業, 單位, 部門, 分部, 案號, 商品編號);
            if (result == null)
                return CreatedAtAction(nameof(DeleteDetailConfirmed), new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));

            _context.租約明細檔.Remove(result);

            try
            {
                var opCount = await _context.SaveChangesAsync();
                if (opCount > 0)
                    return CreatedAtAction(nameof(DeleteDetailConfirmed), new ReturnData(ReturnState.ReturnCode.OK));
            }
            catch (Exception ex)
            {
                return CreatedAtAction(nameof(DeleteDetailConfirmed), new ReturnData(ReturnState.ReturnCode.DELETE_ERROR)
                {
                    message = ex.Message
                });
            }

            return CreatedAtAction(nameof(DeleteDetailConfirmed), new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
        }
        #endregion
        #region other
        public bool isDetailKeyExist(string 事業, string 單位, string 部門, string 分部, string 案號, string 商品編號)
        {
            return (_context.租約明細檔.Any(m => m.事業 == 事業 && m.單位 == 單位 && m.部門 == 部門 && m.分部 == 分部 && m.案號 == 案號 && m.商品編號 == 商品編號) == false);
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
        #region
        //concatCodeAndName只能由SQL使用，因此，我這裡設個這個
        public static string CombineCodeAndName(string code, string name)
        {
            return string.IsNullOrEmpty(name) ? code : $"{code}_{name}";
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
        public async Task<IActionResult> CheckButtonPermissions([FromBody] 租約主檔DisplayViewModel key)
        {
            var today = DateTime.Today;
            Debug.WriteLine("📌【CheckButtonPermissions】啟動");


            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));


            if (key.事業 != ua.BusinessNo || key.單位 != ua.DepartmentNo || key.部門 != ua.DivisionNo || key.分部 != ua.BranchNo)
            {
                Debug.WriteLine("❌ 不是登入者可操作之組織");
                return Ok(new
                {
                    //canCharge = false,
                    canClickEditOrDelete = false,
                    reasons = new
                    {
                        edit = "不是登入者可操作之組織",
                        delete = "不是登入者可操作之組織",
                    }
                });
            }
             

            return Ok(new
            {
                //canCharge = true,
                canClickEditOrDelete = true
            });
        }

        #endregion

        #region 驗證
        private async Task ValidateForCreate(租約主檔CreateViewModel model)
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
                ModelState.AddModelError(nameof(model.案號), "已有該案之承租檔");
            }
        }
        private async Task ValidateForDelete(租約主檔DisplayViewModel model)
        {
            // 1. 檢查是否已有相同案號的收租檔
            var exists = await _context.租約水電檔.AnyAsync(x =>
                x.事業 == model.事業 &&
                x.單位 == model.單位 &&
                x.部門 == model.部門 &&
                x.分部 == model.分部 &&
                x.案號 == model.案號
            );

            if (exists)
            {
                ModelState.AddModelError(nameof(model.案號), "該租約有租約水電紀錄，不可刪除");
            }
        }
        #endregion
    }
}