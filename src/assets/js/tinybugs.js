function qs() {
    var query = window.location.search.substring(1);
    var pl = /\+/g;  // Regex for replacing addition symbol with a space
    var split = /([^&=]+)=?([^&]*)/g;
    var decode = function (s) { return decodeURIComponent(s.replace(pl, " ")); };
    var match;

    var urlParams = {};
    while (match = split.exec(query)) {
        urlParams[decode(match[1]).toLowerCase()] = decode(match[2]);
    }

    return urlParams;
}

$(function () {
    // support for closing alerts cleanly.
    $(".alert").find(".close").on("click", function (e) {
        e.stopPropagation();
        e.preventDefault();
        $(this).closest(".alert").slideUp();
    });

    // support for modal login form.
    $('.login.modal').submit(function (e) {
        var dlg = $(this);
        var submit = dlg.find(':submit').button('loading');
        var alert = dlg.find('.alert').slideUp('fast');

        e.preventDefault();
        $.post('/api/session/', dlg.find('form').serialize())
            .done(function () {
                dlg.modal('hide');
            }).fail(function (jqXHR, textStatus, errorThrown) {
                alert.slideDown();
            }).always(function () {
                submit.button('reset');
            });
    });

    $('.login.modal').on('hide', function () {
        $(this).find('.alert').hide();
    });
});
