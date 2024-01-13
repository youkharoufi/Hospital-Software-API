namespace Hospital_Software.Models
{
    public class RegisterUser
    {
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string Password { get; set; }

        public int? BadgeNumber { get; set; }

        public string RoleName { get; set; }

        public string? PictureUrl { get; set; }

        public IFormFile ImageFile { get; set; }
    }
}
