using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PricelistMailReader.DataBase
{
    public class AppContext : DbContext
    {
        public DbSet<PriceItem> PriceItems { get; set; }

        public AppContext() : base("DbConnection")
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<AppContext>());
        }
    }
}
