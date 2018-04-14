// Write your Javascript code.

$('.error-span').bind('DOMNodeInserted', toggleFormErrorState);
$('.error-span').bind('DOMNodeRemoved', toggleFormErrorState);

function toggleFormErrorState(){
    // alert('node inserted');
    var parentForm = $(this).parent();
    parentForm.toggleClass("errored");
    $(this).toggleClass("error");
    parentForm.children("small").toggleClass("d-none");
}