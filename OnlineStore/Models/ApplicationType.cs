using System.ComponentModel.DataAnnotations;

namespace OnlineStore.Models;

public class ApplicationType
{
    [Key]
    public int Id { get; set; }
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; }
}