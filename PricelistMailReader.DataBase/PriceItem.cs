using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PricelistMailReader.DataBase
{
    [Table("PriceItems")]
    public class PriceItem
    {
        [Key]
        public int Id { get; set; }

        [Column("Vendor"), MaxLength(64)]
        public string Vendor { get; set; }

        [Column("Number"), MaxLength(64)]
        public string Number { get; set; }

        [Column("SearchVendor"), MaxLength(64)]
        public string SearchVendor { get; set; }

        [Column("SearchNumber"), MaxLength(64)]
        public string SearchNumber { get; set; }

        [Column("Description"), MaxLength(512)]
        public string Description { get; set; }

        [Column("Price")]
        public decimal Price { get; set; }

        [Column("Count")]
        public int Count { get; set; }
    }

    public static class PriceItemFieldsNames
    {
        public static string Vendor = "Vendor";
        public static string Number = "Number";
        public static string SearchVendor = "SearchVendor";
        public static string SearchNumber = "SearchNumber";
        public static string Description = "Description";
        public static string Price = "Price";
        public static string Count = "Count";
    }
}
