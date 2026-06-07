namespace WebMoHinh.Models
{
    public class ReportViewModel
    {
        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public decimal TotalRevenue { get; set; }

        public int TotalOrders { get; set; }

        public int TotalCustomers { get; set; }

        public List<ProductStatisticVM> TopBestProducts { get; set; }
            = new();

        public List<ProductStatisticVM> TopWorstProducts { get; set; }
            = new();

        public List<CategoryStatisticVM> TopBestCategories { get; set; }
            = new();

        public List<CategoryStatisticVM> TopWorstCategories { get; set; }
            = new();
    }
}