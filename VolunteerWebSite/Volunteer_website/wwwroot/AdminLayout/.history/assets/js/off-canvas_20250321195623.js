(function ($) {
  "use strict";
  $(function () {
    $('[data-toggle="offcanvas"]').on("click", function () {
      $("body").toggleClass("sidebar-icon-only"); // Thêm/xóa class vào body
    });
  });
})(jQuery);
