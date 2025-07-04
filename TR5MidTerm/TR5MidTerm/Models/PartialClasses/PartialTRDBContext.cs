using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TscLibCore.BaseObject;

namespace TR5MidTerm.Models
{
    public partial class TRDBContext : BaseDbContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            CustomSqlFunctions.Register(modelBuilder);
        }

        
    }
}
