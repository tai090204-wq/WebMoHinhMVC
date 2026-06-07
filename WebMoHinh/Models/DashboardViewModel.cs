namespace WebMoHinh.Models
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }

        public int TotalOrders { get; set; }

        public decimal TotalRevenue { get; set; }

        public List<string> Labels { get; set; } = new();

        public List<int> OrderCounts { get; set; } = new();

        public List<decimal> Revenues { get; set; } = new();
    }
}