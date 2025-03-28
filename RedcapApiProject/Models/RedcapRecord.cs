using System.ComponentModel.DataAnnotations;

namespace RedcapApiProject.Models
{
    public class RedcapRecord
    {
        [Required]
        public string Id { get; set; } = string.Empty; // Default to empty string
    }
}
