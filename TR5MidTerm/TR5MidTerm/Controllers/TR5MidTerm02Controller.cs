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
using Microsoft.AspNetCore.Mvc.Rendering;
using TscLibCore.Authority;
using TR5MidTerm.PC;
using System.Diagnostics;

namespace TR5MidTerm.Controllers
{
    [ProcUseRang(ProcNo, ProcUseRang.Menu)]
    [TypeFilter(typeof(BaseActionFilter))]
    public class TR5MidTerm02Controller : Controller
    {
        #region 初始化
        private readonly TRDBContext _context;
        private const string ProcNo = "TR5MidTerm";

        private static IConfigurationProvider _config;
        private static IMapper _mapper;

        public TR5MidTerm02Controller(TRDBContext context)
        {
            _context = context;
            _config ??= new MapperConfiguration(cfg =>
            {
                cfg.CreateProjection<水電總表檔, 水電總表檔DisplayViewModel>();
                cfg.CreateMap<水電總表檔DisplayViewModel, 水電總表檔>();
                cfg.CreateMap<水電總表檔, 水電總表檔DisplayViewModel>();

                cfg.CreateMap<水電總表檔, 水電總表檔CreateViewModel>();
                cfg.CreateMap<水電總表檔CreateViewModel, 水電總表檔>();


                cfg.CreateMap<水電總表檔EditViewModel, 水電總表檔>();
                cfg.CreateMap<水電總表檔, 水電總表檔EditViewModel>();

                cfg.CreateMap<水電分表檔CreateViewModel, 水電分表檔>();
                cfg.CreateMap<水電分表檔, 水電分表檔CreateViewModel>();

                cfg.CreateMap<水電分表檔DisplayViewModel, 水電分表檔>();
                cfg.CreateMap<水電分表檔, 水電分表檔DisplayViewModel>();

                cfg.CreateMap<水電分表檔EditViewModel, 水電分表檔>();
                cfg.CreateMap<水電分表檔, 水電分表檔EditViewModel>();

            });

            _mapper ??= _config.CreateMapper();
        }
        #endregion
        #region index
        public async Task<IActionResult> Index()
        {
            ViewBag.TableFieldDescDict = new CreateTableFieldsDescription()
                   .Create<水電總表檔DisplayViewModel, 水電分表檔DisplayViewModel>();
            #region query下拉式清單 
            var 已使用事業代碼 = await _context.水電總表檔
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
            //var sql = (from s in _context.水電總表檔
            //          select s).AsNoTracking().ProjectTo<水電總表檔DisplayViewModel>(_config);

            IQueryable<水電總表檔DisplayViewModel> sql = GetBaseQuery();

            PaginatedList<水電總表檔DisplayViewModel> queryedData = null;
            queryedData = await PaginatedList<水電總表檔DisplayViewModel>.CreateAsync(sql, qc);

            return Ok(new
            {
                data = queryedData,
                total = queryedData.TotalCount
            });
        }
        private IQueryable<水電總表檔DisplayViewModel> GetBaseQuery()
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));

            return (from m in _context.水電總表檔
                        .Include(m => m.計量表種類編號Navigation)
                    join biz in _context.事業 on m.事業 equals biz.事業1
                    join dep in _context.單位 on m.單位 equals dep.單位1
                    join sec in _context.部門 on new { m.單位, m.部門 } equals new { sec.單位, 部門 = sec.部門1 }
                    join sub in _context.分部 on new { m.單位, m.部門, m.分部 } equals new { sub.單位, sub.部門, 分部 = sub.分部1 }
                    //where m.事業 == ua.BusinessNo && m.單位 == ua.DepartmentNo
                    select new 水電總表檔DisplayViewModel
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
                        #endregion
                        #region 資料
                        總表號 = m.總表號,
                        案號 = m.案號,
                        計量對象 = m.計量對象,

                        計量表種類編號 = m.計量表種類編號,
                        計量表種類 = m.計量表種類編號 + '_' + m.計量表種類編號Navigation.計量表種類,
                        #endregion
                        #region 修改人與修改時間
                        修改人 = m.修改人,
                        修改時間 = m.修改時間,
                        #endregion
                        #region
                        可否展開 = _context.水電分表檔.Any(d =>
    d.事業 == m.事業 &&
    d.單位 == m.單位 &&
    d.部門 == m.部門 &&
    d.分部 == m.分部 &&
    d.總表號 == m.總表號)
                        ,

                        可否新增 = (ua.BusinessNo == m.事業 && ua.DepartmentNo == m.單位 && ua.DivisionNo == m.部門 && ua.BranchNo == m.分部),
                        #endregion
                    }).AsNoTracking();
        }

        #endregion
        #region 新增主檔

        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public IActionResult Create()
        {
            #region 載入下拉選單（計量表種類）
            ViewBag.計量表種類選項 = _context.計量表種類檔
                .OrderBy(x => x.計量表種類編號)
                .Select(x => new SelectListItem
                {
                    Value = x.計量表種類編號,
                    Text = $"{x.計量表種類編號} - {x.計量表種類}"
                }).ToList();
            #endregion
            #region 初始化表單預設資料
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            var viewModel = new 水電總表檔CreateViewModel
            {
                事業 = ua.BusinessNo,
                單位 = ua.DepartmentNo,
                部門 = ua.DivisionNo,
                分部 = ua.BranchNo,
            };
            #endregion
            return PartialView(viewModel);
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public async Task<IActionResult> Create([Bind("事業,單位,部門,分部,總表號,案號,計量表種類編號,計量對象")] 水電總表檔CreateViewModel postData)
        {
            #region 驗證
            if (!ModelState.IsValid)
            {
                return ModelStateInvalidResult("Create", false);
            }
            #endregion
            #region 資料準備+加入追蹤區     
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            水電總表檔 filledData = _mapper.Map<水電總表檔CreateViewModel, 水電總表檔>(postData);

            filledData.修改人 = ua.UserNo + '_' + ua.UserName;
            filledData.修改時間 = DateTime.Now;

            _context.Add(filledData);
            #endregion
            #region	資料庫儲存+回傳處理
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
                                x.總表號 == postData.總表號
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
            #endregion
            return CreatedAtAction(nameof(Create), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));
        }
        #endregion
 
        #region 編輯主檔
        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> Edit(string 事業, string 單位, string 部門, string 分部, string 總表號)
        {
            #region 依主鍵查詢資料model+轉為viewModel
            //var result = await _context.水電總表檔.FindAsync(事業, 單位, 部門, 分部, 總表號);
            var result = await _context.水電總表檔
    .Include(x => x.計量表種類編號Navigation)
    .FirstOrDefaultAsync(x =>
        x.事業 == 事業 &&
        x.單位 == 單位 &&
        x.部門 == 部門 &&
        x.分部 == 分部 &&
        x.總表號 == 總表號);
            if (result == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }
            var viewModel = _mapper.Map<水電總表檔, 水電總表檔EditViewModel>(result);
            viewModel.計量表種類 = result.計量表種類編號 + '_' + result.計量表種類編號Navigation.計量表種類;
            #endregion
            #region 載入下拉選單（計量表種類）
            ViewBag.計量表種類選項 = _context.計量表種類檔
                .OrderBy(x => x.計量表種類編號)
                .Select(x => new SelectListItem
                {
                    Value = x.計量表種類編號,
                    Text = $"{x.計量表種類編號} - {x.計量表種類}"
                }).ToList();
            #endregion

            return PartialView(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> Edit(string 事業, string 單位, string 部門, string 分部, string 總表號, [Bind("事業,單位,部門,分部,總表號,案號,計量表種類編號,計量對象")] 水電總表檔EditViewModel postData)
        {
            #region 驗證
            if (!ModelState.IsValid)
            {
                return ModelStateInvalidResult("Edit", false);
            }
            #endregion
            #region 資料準備+加入追蹤區     
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            水電總表檔 filledData = _mapper.Map<水電總表檔EditViewModel, 水電總表檔>(postData);

            filledData.修改人 = ua.UserNo + '_' + ua.UserName;
            filledData.修改時間 = DateTime.Now;

            _context.Update(filledData);
            #endregion
            #region	資料庫儲存+回傳處理
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
            #endregion
            return CreatedAtAction(nameof(Edit), new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
        }
        #endregion
        #region 刪除主檔
        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> Delete(string 事業, string 單位, string 部門, string 分部, string 總表號)
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 總表號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
            }

            //var result = await _context.水電總表檔.FindAsync(事業, 單位, 部門, 分部, 總表號);
            var result = await GetBaseQuery()
               .Where(x =>
            x.事業 == 事業 &&
            x.單位 == 單位 &&
            x.部門 == 部門 &&
            x.分部 == 分部 &&
            x.總表號 == 總表號)
               .SingleOrDefaultAsync();
            //var viewModel = _mapper.Map<水電總表檔, 水電總表檔DisplayViewModel>(result);

            return PartialView(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> DeleteConfirmed([Bind("事業,單位,部門,分部,總表號")] 水電總表檔DisplayViewModel postData)
        {
            ValidateUserHasOrgPermission(postData.事業, postData.單位, postData.部門, postData.分部);
            #region 驗證
            if (!ModelState.IsValid)
            {
                return ModelStateInvalidResult("Delete", false);
            }
            #endregion
            var result = await _context.水電總表檔.FindAsync(postData.事業, postData.單位, postData.部門, postData.分部, postData.總表號);
            if (result == null)
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));

            _context.水電總表檔.Remove(result);
            
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
        #region 匯出主檔

        [ProcUseRang(ProcNo, ProcUseRang.Export)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Export([FromBody] QueryConditions qc)
        {
            var sql = (from s in _context.水電總表檔
                      select s).AsNoTracking().ProjectTo<水電總表檔DisplayViewModel>(_config);;

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
            var 已使用單位代碼 = await _context.水電總表檔
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
            var 已使用部門代碼 = await _context.水電總表檔
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
            var 已使用分部代碼 = await _context.水電總表檔
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



        //=================================================================================================//

        /*
         * Details
         */

        #region 查詢明細

        [HttpPost, ActionName("GetDetailDataPost")]
        [ValidateAntiForgeryToken]
        [NeglectActionFilter]
        public async Task<IActionResult> GetDetails([FromBody] 水電總表檔 keys)
        {
            if (keys.事業 == null || keys.單位 == null || keys.部門 == null || keys.分部 == null || keys.總表號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.ERROR));
            }

            var query = GetDetailsBaseQuery(keys.事業, keys.單位, keys.部門, keys.分部, keys.總表號);
            return CreatedAtAction(nameof(GetDetails), new ReturnData(ReturnState.ReturnCode.OK)
            {
                data = await query.ToListAsync()
            });
        }
        private IQueryable<水電分表檔DisplayViewModel> GetDetailsBaseQuery(string 主檔事業, string 主檔單位, string 主檔部門, string 主檔分部, string 主檔總表號)
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            return (from s in _context.水電分表檔
                    join biz in _context.事業 on s.事業 equals biz.事業1
                    join dep in _context.單位 on s.單位 equals dep.單位1
                    join sec in _context.部門 on new { s.單位, s.部門 } equals new { sec.單位, 部門 = sec.部門1 }
                    join sub in _context.分部 on new { s.單位, s.部門, s.分部 } equals new { sub.單位, sub.部門, 分部 = sub.分部1 }
                    where s.事業 == 主檔事業 &&
                          s.單位 == 主檔單位 &&
                          s.部門 == 主檔部門 &&
                          s.分部 == 主檔分部 &&
                          s.總表號 == 主檔總表號
                    select new 水電分表檔DisplayViewModel
                    {
                        事業 = s.事業,
                        事業顯示 = biz.事業名稱,
                        單位 = s.單位,
                        單位顯示 = dep.單位名稱,
                        部門 = s.部門,
                        部門顯示 = sec.部門名稱,
                        分部 = s.分部,
                        分部顯示 = sub.分部名稱,
                        總表號 = s.總表號,
                        分表號 = s.分表號,
                        備註 = s.備註,
                        上期度數 = s.上期度數,
                        本期度數 = s.本期度數,
                        修改人 = s.修改人,
                        修改時間 = s.修改時間,
                        目前使用度數 = s.本期度數 - s.上期度數,
                        可否刪除明細 = (ua.BusinessNo == s.事業 && ua.DepartmentNo == s.單位 && ua.DivisionNo == s.部門 && ua.BranchNo == s.分部),
                        可否修改明細 = (ua.BusinessNo == s.事業 && ua.DepartmentNo == s.單位 && ua.DivisionNo == s.部門 && ua.BranchNo == s.分部)
                    }).AsNoTracking();
        }

        #endregion
        #region 建立明細
        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        //public async Task<IActionResult> CreateDetail(string 事業, string 單位, string 部門, string 分部, string 總表號, int? 分表號)
        public async Task<IActionResult> CreateDetail(string 事業, string 單位, string 部門, string 分部, string 總表號, int? 分表號)
        {
            //if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 總表號 == null || 分表號== null)
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 總表號 == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.ERROR));
            }
            //var item = await _context.水電總表檔.FindAsync(事業, 單位, 部門, 分部, 總表號, 分表號);
            var item = await _context.水電總表檔.FindAsync(事業, 單位, 部門, 分部, 總表號);
            var viewModel = new 水電分表檔CreateViewModel
            {
                事業 = item.事業,
                單位 = item.單位,
                部門 = item.部門,
                分部 = item.分部,
                總表號 = item.總表號,
                上期度數 = 0m
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
        public async Task<IActionResult> CreateDetail([Bind("事業,單位,部門,分部,總表號,分表號,備註,上期度數 ,本期度數")] 水電分表檔CreateViewModel postData)
        {
            ValidateUserHasOrgPermission(postData.事業, postData.單位, postData.部門, postData.分部);

            if (!ModelState.IsValid)
            {
                return ModelStateInvalidResult("CreateDetail", false);
            }   //    return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));


            /*
             *  Put Your Code Here.
             */

            水電分表檔 filledData = _mapper.Map<水電分表檔CreateViewModel, 水電分表檔>(postData);
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            filledData.備註 ??= "";
            filledData.上期度數 = 0m;
            filledData.修改人 = ua.BranchNo + '_' + ua.UserName;
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
        public async Task<IActionResult> CreateMultiDetails(List<水電分表檔DisplayViewModel> postData)
        {
            if (ModelState.IsValid == false)
                return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));

            foreach (var item in postData)
            {
                /*
                 *  Put Your Code Here.
                 */
                水電分表檔 filledData = _mapper.Map<水電分表檔DisplayViewModel, 水電分表檔>(item);
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
        #region 編輯明細
        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> EditDetail(string 事業, string 單位, string 部門, string 分部, string 總表號, int? 分表號) 
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 總表號 == null || 分表號== null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            var result = await _context.水電分表檔.FindAsync(事業, 單位, 部門, 分部, 總表號, 分表號);
 
            if (result == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            var viewModel = _mapper.Map<水電分表檔, 水電分表檔EditViewModel>(result);

            return PartialView(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> EditDetail(string 事業, string 單位, string 部門, string 分部, string 總表號, int? 分表號, [Bind("事業,單位,部門,分部,總表號,分表號,備註, 上期度數, 本期度數")] 水電分表檔EditViewModel postData)
        {

            ValidateUserHasOrgPermission(postData.事業, postData.單位, postData.部門, postData.分部);
            if (!ModelState.IsValid)
            {
                return ModelStateInvalidResult("EditDetail", false);
            }

            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 總表號 == null || 分表號 == null)
                return CreatedAtAction(nameof(EditDetail), new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
              

            水電分表檔 filledData = _mapper.Map<水電分表檔EditViewModel, 水電分表檔>(postData);
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));

            filledData.備註 ??= "";
            filledData.修改人 = ua.UserNo + '_' + ua.UserName;
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
        #region 刪除明細
        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> DeleteDetail(string 事業, string 單位, string 部門, string 分部, string 總表號, int? 分表號)
        {
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 總表號 == null || 分表號== null)
            {
                return NotFound();
            }
             
            var viewModel = await GetDetailsBaseQuery(事業, 單位, 部門, 分部, 總表號)
               .Where(x =>
            x.事業 == 事業 &&
            x.單位 == 單位 &&
            x.部門 == 部門 &&
            x.分部 == 分部 &&
            x.總表號 == 總表號&&
            x.分表號 == 分表號)
               .SingleOrDefaultAsync();
            if (viewModel == null)
            {
                return NotFound();
            }

            return PartialView(viewModel);
        }

        [HttpPost, ActionName("DeleteDetailConfirmed")]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> DeleteDetailConfirmed(string 事業, string 單位, string 部門, string 分部, string 總表號, int? 分表號, [Bind("事業,單位,部門,分部,總表號,分表號,備註, 上期度數, 本期度數")] 水電分表檔DisplayViewModel postData)
        {
            ValidateUserHasOrgPermission(postData.事業, postData.單位, postData.部門, postData.分部);
            #region 驗證
            if (!ModelState.IsValid)
            {
                return ModelStateInvalidResult("DeleteDetail", false);
            }
            #endregion
            if (事業 == null || 單位 == null || 部門 == null || 分部 == null || 總表號 == null || 分表號 == null)
                return CreatedAtAction(nameof(DeleteDetailConfirmed), new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));


            var result = await _context.水電分表檔.FindAsync(事業, 單位, 部門, 分部, 總表號, 分表號);
            if (result == null)
                return CreatedAtAction(nameof(DeleteDetailConfirmed), new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));

            _context.水電分表檔.Remove(result);

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
         

        #region 檢驗
        private void ValidateUserHasOrgPermission(string 主檔事業, string 主檔單位, string 主檔部門, string 主檔分部)
        {
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            if (主檔事業 != ua.BusinessNo)
            {
                ModelState.AddModelError("欄位名稱", $"無權操作該事業資料（登入事業為 {ua.BusinessName}）");
            }
            else if (主檔單位 != ua.DepartmentNo)
            {
                ModelState.AddModelError("欄位名稱", $"無權操作該單位資料（登入單位為 {ua.DepartmentName}）");
            }
            else if (主檔部門 != ua.DivisionNo)
            {
                ModelState.AddModelError("欄位名稱", $"無權操作該部門資料（登入部門為 {ua.DivisionName}）");
            }
            else if (主檔分部 != ua.BranchNo)
            {
                ModelState.AddModelError("欄位名稱", $"無權操作該分部資料（登入分部為 {ua.BranchNo}）");
            }
        }

        [HttpPost]
        public IActionResult CheckButtonPermissions([FromBody] 水電總表檔DisplayViewModel key)
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

            return Ok(new
            {
                canClickEditOrDelete = true
            });
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