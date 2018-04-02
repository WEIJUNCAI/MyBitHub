// Write your Javascript code.

// disable the sign in button after the form is submitted
$("form.disable-btn-on-submit").submit(function () {
    var button = $(this).find("button[type='submit']");
    var disableMsg = button.data("disable-with");
    button.prop('disabled', true).html(disableMsg);
});