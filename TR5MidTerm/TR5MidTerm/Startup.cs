using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TscLibCore.BaseObject;
using TscLibCore.DB;

namespace TR5MidTerm
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment _hostEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = _hostEnvironment;
        }

        public IConfiguration Configuration { get; }
        private IHostEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            const string syskind = "TR5MidTerm";
            ConnectionStrings cs = ConnectionStrings.CreateInstance(syskind);

            TscLibCore.Startup.ConfigureServices(
              services,
              new TscLibCore.ProjEnvironments()
              {
                  sysKind = syskind,
                  outterServiceColl = services,
                  outterConfig = Configuration,
                  hostEnvironment = HostingEnvironment,
                  isTW = true,
                  shouldRecordWebServiceLog = false, //暫不寫入WSDB //呼叫要不要寫log? 透過web? 透過api? 讀Json回來
                  webServiceLogDbName = "WSDB" //提供服務給別人呼叫
              });

            services.AddDbContext<Models.TRDBContext>(b => //0528 10:15 修正底層，確保 User @@@11
            {
                var DB_Name = "TRDB";

                var connStr = cs.GetDbConnectionString(DB_Name);

                b.UseSqlServer(connStr)
                .LogTo(Console.WriteLine, LogLevel.Information)  // <== 這行會印出 SQL
           .EnableSensitiveDataLogging()
           ;// <== 顯示參數值，方便除錯;

                SymmetricKey key = cs.GetDbSymmetricKey(DB_Name);

                b.AddInterceptors(new BaseDbCommandInterceptor(DB_Name, key.Name, key.PWD));
            });

        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();//專門顯示錯誤訊息
            }

            app.UseRouting();//中介軟體

            TscLibCore.Startup.Configure(app);

            app.UseEndpoints(endpoints =>
            {
                // 🛠️ 如果現在是開發機（例如：localhost 或 測試環境），才套用這段路由
                if (env.IsDevelopment())
                {
                    //👉 這是「預設路由」，意思是：網址長得像 / Home / Index 就會進到 HomeController 的 Index 方法
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}"

                    );
                }
 
                endpoints.MapControllerRoute(
                    name: "TR5MidTerm_Route",
                    pattern: "TR5MidTerm/{controller=Home}/{action=Index}"
                );
            });

        }
    }
    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

}
