using Microsoft.AspNetCore.Identity;

namespace WebMoHinh.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        public int Age { get; set; }

        public string Gender { get; set; }

        public string Job { get; set; }

        public string Address { get; set; }
    }
}