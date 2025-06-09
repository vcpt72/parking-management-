using System.ComponentModel.DataAnnotations;

namespace Parking_projekt.Models
{


    public enum UserRole
    {
        Admin = 0,   
        ReadOnly = 1  
    }
    public class AppUser
    {
        public int Id { get; set; }


        [RegularExpression(@"^[a-zA-Z0-9]{4,20}$", ErrorMessage = "Uživatelské jméno musí mít 3–20 znaků bez speciálních znaků.")]
        public string Username { get; set; }

        [RegularExpression(@"^[a-zA-Z0-9!$]{4,20}$", ErrorMessage = "Heslo musí mít alespoň 4 znaky a může obsahovat !$.")]
        public string Password { get; set; }
        public UserRole Role { get; set; } 
    }
}
