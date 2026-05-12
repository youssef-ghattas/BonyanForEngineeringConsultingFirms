using System.ComponentModel.DataAnnotations;
using Bonyan.DAL.Models;
using Bonyan.DAL.Enums;


namespace Bonyan.PL.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "الاسم الأخير مطلوب")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب"), EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password), MinLength(6)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "كلمات المرور غير متطابقتين")]
        public string ConfirmPassword { get; set; }

        public Specialization Specialization { get; set; }
        public string PhoneNum { get; set; }
        public string SSN { get; set; }
        public Gender Gender { get; set; }
        public DateTime HireDate { get; set; } = DateTime.Now;
        public decimal Salary { get; set; }
    }
}