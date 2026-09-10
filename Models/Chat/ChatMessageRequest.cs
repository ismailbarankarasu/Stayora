using System.ComponentModel.DataAnnotations;

namespace Stayora.Models.Chat
{
    public class ChatMessageRequest
    {
        [Required(ErrorMessage = "Lütfen bir mesaj yazın.")]
        [StringLength(
            2000,
            ErrorMessage = "Mesaj en fazla 2000 karakter olabilir.")]
        public string Message { get; set; } = string.Empty;
    }
}