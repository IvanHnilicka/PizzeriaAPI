using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace API_Pizzeria.Validaciones
{
    public class PasswordAttribute: ValidationAttribute
    {
        protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
        {
            string contraseña = value.ToString();
            bool isValidPassword = IsStrongPassword(contraseña);

            return isValidPassword ? ValidationResult.Success : new ValidationResult("Error. Contraseña no segura");
        }

        private bool IsStrongPassword(string contraseña)
        {
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*_])[A-Za-z\d!@#$%^&*_]+$";
            return Regex.IsMatch(contraseña, pattern);
        }
    }
}
