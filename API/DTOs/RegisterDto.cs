using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
        //Required: Bu öznitelik, belirli bir model özelliğinin (property) boş bırakılmaması gerektiğini belirtir. Yani, bu özellik zorunlu olup bir değer içermelidir.
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [RegularExpression("(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{4,8}$", ErrorMessage = "Password must be complex")] // Şifrenin nasıl olması gerektiğini belirliyoruz örneğin 4 - 8 karakter aralığında olmalı dedik.
        public string Password { get; set; }
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public string Username { get; set; }
    }
}