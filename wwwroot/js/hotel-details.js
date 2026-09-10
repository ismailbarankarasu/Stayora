(function ($) {
    "use strict";

    $(function () {
        const gallery = $("#hotel-photo-gallery");

        if (!gallery.length || !$.fn.magnificPopup) {
            return;
        }

        gallery.magnificPopup({
            delegate: ".hotel-gallery-link",
            type: "image",

            gallery: {
                enabled: true,
                preload: [0, 1],
                arrowMarkup:
                    '<button title="%title%" type="button" ' +
                    'class="mfp-arrow mfp-arrow-%dir%"></button>',
                tPrev: "Önceki fotoğraf",
                tNext: "Sonraki fotoğraf",
                tCounter: "%curr% / %total%"
            },

            image: {
                titleSrc: function (item) {
                    return $("<div>")
                        .text(item.el.attr("title") || "")
                        .html();
                },

                tError:
                    'Fotoğraf yüklenemedi. ' +
                    '<a href="%url%">Fotoğrafı doğrudan aç</a>.'
            },

            tClose: "Kapat (Esc)",
            tLoading: "Fotoğraf yükleniyor…",
            closeOnContentClick: false,
            closeOnBgClick: true,
            enableEscapeKey: true
        });
    });
})(jQuery);