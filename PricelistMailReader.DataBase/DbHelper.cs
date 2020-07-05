using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PricelistMailReader.DataBase
{
    public class DbHelper
    {

        public void WritePriceLists(List<PriceItem> priceLists, IProgress<ProgressOptions> progress)
        {
            using (var db = new AppContext())
            {
                progress?.Report(new ProgressOptions{Message = "Запись позиций прайс-листов в базу данных..."});
                db.PriceItems.AddRange(priceLists); 
                var count = db.SaveChanges();
                progress?.Report(new ProgressOptions { Message = $"Записано {count} позиций прайс-листов." });
            }
        }

        public async Task ClearPriceListsTable(IProgress<ProgressOptions> progress)
        {
            using (var db = new AppContext())
            {
                progress?.Report(new ProgressOptions{ Message = "Очистка таблицы..."});
                db.PriceItems.RemoveRange(db.PriceItems.Select(x => x));
                await db.SaveChangesAsync();
                progress?.Report(new ProgressOptions { Message = "Таблица очищена." });
            }

        }
    }
}
