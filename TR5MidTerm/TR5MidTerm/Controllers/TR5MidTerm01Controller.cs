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
            //_config ??= new MapperConfiguration(cfg =>
            //{
            //    cfg.CreateProjection<承租人檔, 承租人檔VM>();
            //    cfg.CreateMap<承租人檔VM, 承租人檔>();
            //    cfg.CreateMap<承租人檔, 承租人檔VM>();
            //});

            //_mapper ??= _config.CreateMapper();
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
            //var sql = (from s in _context.承租人檔
            //          select s).AsNoTracking().ProjectTo<承租人檔VM>(_config);
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            //IQueryable<承租人檔VM> sql = GetBaseQuery(ua.BusinessNo, ua.DepartmentNo);
            IQueryable<承租人檔DisplayViewModel> sql = GetBaseQuery(ua.BusinessNo);

            PaginatedList<承租人檔DisplayViewModel> queryedData = null;
            queryedData = await PaginatedList<承租人檔DisplayViewModel>.CreateAsync(sql, qc);

            return Ok(new
            {
                data = queryedData,
                total = queryedData.TotalCount
            });
        }

        private IQueryable<承租人檔DisplayViewModel> GetBaseQuery(string BusinessNo)
        {
            return (from m in _context.承租人檔
                        .Include(m => m.身分別編號Navigation)
                    //join u in _context.修改人 on m.修改人 equals u.修改人1 into ujoin
                    //from _u in ujoin.DefaultIfEmpty()
                    where m.事業 == BusinessNo
                    select new 承租人檔DisplayViewModel
                    {
                        事業 = m.事業,
                        單位 = m.單位,
                        部門 = m.部門,
                        分部 = m.分部,
                        承租人編號 = m.承租人編號,

                        承租人 = m.承租人,
                        承租人顯示 = CustomSqlFunctions.DecryptToString(m.承租人),

                        身分別編號 = m.身分別編號,
                        身分別名稱 = m.身分別編號Navigation.身分別,

                        統一編號 = m.統一編號,
                        統一編號顯示 = CustomSqlFunctions.DecryptToString(m.統一編號),

                        行動電話 = m.行動電話,
                        行動電話顯示 = CustomSqlFunctions.DecryptToString(m.行動電話),

                        電子郵件 = m.電子郵件,
                        電子郵件顯示 = CustomSqlFunctions.DecryptToString(m.電子郵件),

                        刪除註記 = m.刪除註記,
                        刪除註記顯示 = m.刪除註記 ? "是" : "否",

                        修改人 = m.修改人,
                        //修改人姓名 = CustomSqlFunctions.ConcatCodeAndName(m.修改人, CustomSqlFunctions.DecryptToString(_u.姓名)),
                        修改時間 = m.修改時間
                    }).AsNoTracking();
        }


        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public async Task<IActionResult> Create()
        {
            // ✅ 假設從共用方法取得組織與登入者資訊
            //var (org, period, date, formattedDate, userNo, biz, dept, div, branch) = InitInventoryDefaultValues();
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));
            Debug.WriteLine($"[承租人 Create] ▶ 使用者={ua.UserNo}, 組織={ua.BusinessNo}/{ua.DepartmentNo}/{ua.DivisionNo}/{ua.DivisionNo}");

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
                //修改人 = ua.UserNo,
                修改時間 = DateTime.Now
            };

            return PartialView(viewModel); // 若為一般頁面則改 return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public async Task<IActionResult> Create(承租人檔CreateViewModel model)
        {
            var SymmKey = "WuYeahSymmKey";
            var ua = HttpContext.Session.GetObject<UserAccountForSession>(nameof(UserAccountForSession));


            if (!ModelState.IsValid)
                return ModelStateInvalidResult("Create", false);

            try
            {
                var entity = new 承租人檔
                {
                    事業 = model.事業,
                    單位 = model.單位,
                    部門 = model.部門,
                    分部 = model.分部,
                    承租人編號 = model.承租人編號,
                    身分別編號 = model.身分別編號,
                    // 🔐 明文欄位 → 加密欄位 
                    //承租人 = _context.承租人檔.Select(x => _context.EncryptByKey(_context.Key_Guid(SymmKey), model.承租人明文)).FirstOrDefault(),
                    //承租人 = _context.承租人檔.Select(x => _context.EncryptByKey(_context.Key_Guid(SymmKey), model.承租人明文)).FirstOrDefault(),
                    //統一編號 = _context.承租人檔.Select(x => _context.EncryptByKey(_context.Key_Guid(SymmKey), model.統一編號明文)).FirstOrDefault(),
                    //行動電話 = _context.承租人檔.Select(x => _context.EncryptByKey(_context.Key_Guid(SymmKey), model.行動電話明文)).FirstOrDefault(),
                    //電子郵件 = _context.承租人檔.Select(x => _context.EncryptByKey(_context.Key_Guid(SymmKey), model.電子郵件明文)).FirstOrDefault(),
                    刪除註記 = false,
                    修改人 = ua.UserNo,
                    修改時間 = DateTime.Now,

                    
                };
                Debug.WriteLine("entity:", entity);

                _context.承租人檔.Add(entity);
                int result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    var display = await GetBaseQuery(ua.BusinessNo)
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

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ProcUseRang(ProcNo, ProcUseRang.Add)]
        //public async Task<IActionResult> Create([Bind("事業,單位,部門,分部,承租人編號,承租人,身分別編號,統一編號,行動電話,電子郵件,刪除註記,修改人,修改時間")] 承租人檔VM postData)
        //{
        //    //以下不驗證欄位值是否正確，請視欄位自行刪減
        //    ModelState.Remove($"欄位1");
        //    ModelState.Remove($"欄位2");
        //    ModelState.Remove($"欄位3");
        //    ModelState.Remove($"upd_usr");
        //    ModelState.Remove($"upd_dt");

        //    if (ModelState.IsValid == false)
        //        return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));

        //    /*
        //     *  Put Your Code Here.
        //     */

        //    承租人檔 filledData = _mapper.Map<承租人檔VM, 承租人檔>(postData);
        //    _context.Add(filledData);

        //    try
        //    {
        //        var opCount = await _context.SaveChangesAsync();
        //        if (opCount > 0)
        //            return Ok(new ReturnData(ReturnState.ReturnCode.OK)
        //            {
        //                data = postData
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtAction(nameof(Create), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR)
        //        {
        //            message = ex.Message
        //        });
        //    }

        //    return CreatedAtAction(nameof(Create), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));
        //}

        [ProcUseRang(ProcNo, ProcUseRang.Add)]
        public IActionResult CreateMulti()
        {
            return PartialView();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ProcUseRang(ProcNo, ProcUseRang.Add)]
        //public async Task<IActionResult> CreateMulti(List<承租人檔VM> postData)
        //{
        //    //以下不驗證欄位值是否正確，請視欄位自行刪減
        //    for (int idx = 0; idx < postData.Count; idx++)
        //    {
        //        ModelState.Remove($"postData[{idx}].欄位1");
        //        ModelState.Remove($"postData[{idx}].欄位2");
        //        ModelState.Remove($"postData[{idx}].欄位3");
                
        //        //.....
        //        //...

        //        ModelState.Remove($"postData[{idx}].upd_usr");
        //        ModelState.Remove($"postData[{idx}].upd_dt");
        //    }
            
        //    if (ModelState.IsValid == false)
        //        return BadRequest(new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));

        //    foreach (var item in postData)
        //    {
        //        /*
        //         *  Put Your Code Here.
        //         */
        //        承租人檔 filledData = _mapper.Map<承租人檔VM, 承租人檔>(item);
        //        _context.Add(filledData);
        //    }

        //    try
        //    {
        //        var opCount = await _context.SaveChangesAsync();
        //        if (opCount > 0)
        //            return CreatedAtAction(nameof(CreateMulti), new ReturnData(ReturnState.ReturnCode.OK)
        //            {
        //                data = postData
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtAction(nameof(CreateMulti), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR)
        //        {
        //            message = ex.Message
        //        });
        //    }


        //    return CreatedAtAction(nameof(CreateMulti), new ReturnData(ReturnState.ReturnCode.CREATE_ERROR));
        //}

        [ProcUseRang(ProcNo, ProcUseRang.Update)]
        public async Task<IActionResult> Edit() 
        {
            if (false)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            var result = await _context.承租人檔.FindAsync();
            if (result == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
            }

            return PartialView(result);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //[ProcUseRang(ProcNo, ProcUseRang.Update)]
        //public async Task<IActionResult> Edit( [Bind("事業,單位,部門,分部,承租人編號,承租人,身分別編號,統一編號,行動電話,電子郵件,刪除註記,修改人,修改時間")] 承租人檔VM postData)
        //{
        //    if (false)
        //    {
        //        return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
        //    }

        //    if (false)
        //    {
        //        return NotFound(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
        //    }

        //    if (ModelState.IsValid == false)
        //        return BadRequest(new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));

        //    try
        //    {
        //        /*
        //        *  Put Your Code Here.
        //        */

        //        承租人檔 filledData = _mapper.Map<承租人檔VM, 承租人檔>(postData);
        //        _context.Update(filledData);
        //        var opCount = await _context.SaveChangesAsync();

        //        if (opCount > 0)
        //            return Ok(new ReturnData(ReturnState.ReturnCode.OK)
        //            {
        //                data = postData
        //            });
        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtAction(nameof(Edit), new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
        //    }

        //    return CreatedAtAction(nameof(Edit), new ReturnData(ReturnState.ReturnCode.EDIT_ERROR));
        //}

        [ProcUseRang(ProcNo, ProcUseRang.Delete)]
        public async Task<IActionResult> Delete()
        {
            if (false)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
            }
            
            var result = await _context.承租人檔.FindAsync();
            if (result == null)
            {
                return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
            }

            return PartialView(result);
        }

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //[ProcUseRang(ProcNo, ProcUseRang.Delete)]
        //public async Task<IActionResult> DeleteConfirmed()
        //{
        //    if (ModelState.IsValid == false)
        //        return BadRequest(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));

        //    var result = await _context.承租人檔.FindAsync();
        //    if (result == null)
        //        return NotFound(new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));

        //    _context.承租人檔.Remove(result);
            
        //    try
        //    {
        //        var opCount = await _context.SaveChangesAsync();
        //        if (opCount > 0)
        //            return Ok(new ReturnData(ReturnState.ReturnCode.OK));
        //    }
        //    catch (Exception ex)
        //    {
        //        return CreatedAtAction(nameof(DeleteConfirmed), new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
        //    }

        //    return CreatedAtAction(nameof(DeleteConfirmed), new ReturnData(ReturnState.ReturnCode.DELETE_ERROR));
        //}


        [ProcUseRang(ProcNo, ProcUseRang.Export)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Export([FromBody] QueryConditions qc)
        {
            var sql = (from s in _context.承租人檔
                      select s).AsNoTracking().ProjectTo<承租人檔DisplayViewModel>(_config);;

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

        public bool isMasterKeyExist()
        {
            //return (_context.承租人檔.Any(m => ) == false);
            return true;
        }

    }
}