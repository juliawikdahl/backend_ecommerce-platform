using System.ComponentModel.DataAnnotations;

namespace OnlineShop_API.Dto
{
    public class UserDto
    {

        [Required]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string Name { get; set; } // Namnet på användaren

        [Required]
        public string Address { get; set; } // Användarens adress

        [Required]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string Mobile { get; set; } // Telefonnummer

        [Required]
        public string City { get; set; } // Stad

        [Required]
        [StringLength(10, ErrorMessage = "Zipcode cannot exceed 10 characters.")]
        public string Zipcode { get; set; } // Postnummer
    }
}
