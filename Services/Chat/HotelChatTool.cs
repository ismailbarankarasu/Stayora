using System.Text.Json.Nodes;

namespace Stayora.Services.Chat
{
    public static class HotelChatTool
    {
        public static JsonObject CreateDeclaration()
        {
            return JsonNode.Parse(
                """
                {
                  "name": "search_hotels",
                  "description": "Kullanıcının açıkça belirttiği şehir, tarihler, yetişkin ve oda sayısıyla gerçek otelleri arar. Eksik zorunlu bilgileri kullanıcıdan almadan çağırma. Fiyatlar TRY para birimindedir.",
                  "parameters": {
                    "type": "OBJECT",
                    "properties": {
                      "City": {
                        "type": "STRING",
                        "description": "Aranacak şehir adı. Belirsizse kullanıcıya sor."
                      },
                      "CheckIn": {
                        "type": "STRING",
                        "description": "Giriş tarihi, yyyy-MM-dd biçiminde."
                      },
                      "CheckOut": {
                        "type": "STRING",
                        "description": "Çıkış tarihi, yyyy-MM-dd biçiminde."
                      },
                      "Adults": {
                        "type": "INTEGER",
                        "description": "Yetişkin sayısı. Desteklenen aralık 1-8."
                      },
                      "Rooms": {
                        "type": "INTEGER",
                        "description": "Oda sayısı. Desteklenen aralık 1-4."
                      },
                      "MinPrice": {
                        "type": "NUMBER",
                        "description": "Kullanıcının belirttiği minimum fiyat, TRY. Belirtilmediyse alanı gönderme."
                      },
                      "MaxPrice": {
                        "type": "NUMBER",
                        "description": "Kullanıcının belirttiği maksimum fiyat, TRY. Belirtilmediyse alanı gönderme."
                      },
                      "CategoryFilter": {
                        "type": "STRING",
                        "description": "Tek kategori filtresi. Kullanıcı seçmediyse alanı gönderme.",
                        "enum": [
                          "class::3",
                          "class::4",
                          "class::5",
                          "reviewscorebuckets::80",
                          "reviewscorebuckets::90",
                          "facility::107",
                          "facility::46",
                          "facility::433",
                          "facility::4",
                          "mealplan::breakfast_included",
                          "free_cancellation::1"
                        ]
                      },
                      "SortBy": {
                        "type": "STRING",
                        "description": "Sıralama. Belirtilmediyse popularity kullan.",
                        "enum": [
                          "popularity",
                          "price",
                          "price_from_high_to_low",
                          "bayesian_review_score",
                          "distance",
                          "class_descending",
                          "class_ascending",
                          "upsort_bh"
                        ]
                      }
                    },
                    "required": [
                      "City",
                      "CheckIn",
                      "CheckOut",
                      "Adults",
                      "Rooms"
                    ]
                  }
                }
                """)!.AsObject();
        }
    }
}