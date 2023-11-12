function showSuccessMessage(message = "Saved Successfully!") {
    Swal.fire({
        icon: "success",
        title: "Success",
        text: message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}

function showErrorMessage(message = "Something Went Wrong!") {
    //error sweet alert
    Swal.fire({
        icon: "error",
        title: "Oops...",
        text: "message",
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}
$(document).ready(function () {
    var message = $('#Message').text();
    if (message !== "") {
        showSuccessMessage();
    }

    //Handle Bootstrap model
    $('.js-render-modal').on('click', function () {
        var modal = $('#Modal');
        modal.modal('show');
    })
});