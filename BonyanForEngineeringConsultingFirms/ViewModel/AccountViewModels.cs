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
	public class ChangePasswordViewModel
	{
		[Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
		[DataType(DataType.Password)]
		public string CurrentPassword { get; set; }

		[Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
		[DataType(DataType.Password)]
		[MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
		public string NewPassword { get; set; }

		[Compare("NewPassword", ErrorMessage = "كلمات المرور غير متطابقتين")]
		[DataType(DataType.Password)]
		public string ConfirmNewPassword { get; set; }
	}
	public class ForgotPasswordViewModel
	{
		[Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
		[EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
		public string Email { get; set; }
	}

	public class ResetPasswordViewModel
	{
		[Required]
		public string Email { get; set; }

		[Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
		[MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[Compare("NewPassword", ErrorMessage = "كلمات المرور غير متطابقتين")]
		[DataType(DataType.Password)]
		public string ConfirmNewPassword { get; set; }
	}
}