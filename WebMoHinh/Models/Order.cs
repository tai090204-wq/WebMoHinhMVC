namespace WebMoHinh.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}