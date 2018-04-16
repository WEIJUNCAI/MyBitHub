// Write your Javascript code.

// ****** TODO ******
// listen for the "turbolinks:before-cache" event to 
// to properly reset the dropdowns

$('.error-span').bind('DOMNodeInserted', toggleFormErrorState);
$('.error-span').bind('DOMNodeRemoved', toggleFormErrorState);

function toggleFormErrorState() {
    // alert('node inserted');
    var parentForm = $(this).parent();
    parentForm.toggleClass("errored");
    $(this).toggleClass("error");
    parentForm.children("small").toggleClass("d-none");
}