using System.ComponentModel.DataAnnotations;

namespace Stayora.Models
{
    public class HotelSearchRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Lütfen şehir giriniz.")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giriş tarihi seçiniz.")]
        public DateOnly? CheckIn { get; set; }

        [Required(ErrorMessage = "Çıkış tarihi seçiniz.")]
        public DateOnly? CheckOut { get; set; }

        [Range(1, 8, ErrorMessage = "Yetişkin sayısı 1 ile 8 arasında olmalı.")]
        public int Adults { get; set; } = 2;

        [Range(1, 4, ErrorMessage = "Oda sayısı 1 ile 4 arasında olmalı.")]
        public int Rooms { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (CheckIn.HasValue &&
                CheckIn.Value < DateOnly.FromDateTime(DateTime.Today))
            {
                yield return new ValidationResult(
                    "Giriş tarihi geçmişte olamaz.",
                    [nameof(CheckIn)]);
            }

            if (CheckIn.HasValue &&
                CheckOut.HasValue &&
                CheckOut.Value <= CheckIn.Value)
            {
                yield return new ValidationResult(
                    "Çıkış tarihi giriş tarihinden sonra olmalı.",
                    [nameof(CheckOut)]);
            }
            if (MinPrice.HasValue &&
                    MaxPrice.HasValue &&
                    MinPrice.Value > MaxPrice.Value)
            {
                yield return new ValidationResult(
                    "Maksimum fiyat minimum fiyattan küçük olamaz.", [nameof(MaxPrice)]);
            }
        }
        [RegularExpression("^(popularity|price|price_from_high_to_low|bayesian_review_score|distance|class_descending|class_ascending|upsort_bh)$", ErrorMessage = "Geçersiz sıralama seçimi.")]
        public string SortBy { get; set; } = "popularity";

        [RegularExpression(
            @"^(class::[0-5]|reviewscorebuckets::(50|60|70|80|90)|facility::[0-9]+|property_type::[0-9]+|privacy_type::3|free_cancellation::1|mealplan::(breakfast_included|breakfast_and_dinner|all_inclusive|full_board|breakfast_and_lunch|999))$",
            ErrorMessage = "Geçersiz filtre seçimi.")]
        public string? CategoryFilter { get; set; }

        [Range(typeof(decimal), "0", "100000000", ErrorMessage = "Minimum fiyat 0 ile 100.000.000 arasında olmalı.")]
        public decimal? MinPrice { get; set; }

        [Range(typeof(decimal), "0", "100000000", ErrorMessage = "Maksimum fiyat 0 ile 100.000.000 arasında olmalı.")]
        public decimal? MaxPrice { get; set; }
    }
}