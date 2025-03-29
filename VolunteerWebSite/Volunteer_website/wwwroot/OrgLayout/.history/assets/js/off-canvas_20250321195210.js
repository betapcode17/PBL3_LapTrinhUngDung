(function ($) {
  "use strict";
  $(function () {
    $('[data-toggle="offcanvas"]').on("click", function () {
      $(".sidebar-offcanvas").addClass("active");
    });
  });
})(jQuery);
