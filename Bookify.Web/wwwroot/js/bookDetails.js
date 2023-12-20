function onAddCopySuccess(row) {
    showSuccessMessage();
    $('#Modal').modal('hide');

    //add new row to the table
    $('tbody').prepend(row);
    KTMenu.createInstances();

    //update the number of copies in the view
    //select the div that holds the count
    var count = $('#CopiesCount');
    var newCount = parseInt(count.text()) + 1;
    count.text(newCount);

    //if we are going to add copy and there is no copy yet
    //we have to hide the alert
    $('.js-alert').addClass('d-none');
    //show the table
    $('table').removeClass('d-none');

}

function onEditCopySuccess(row) {
    showSuccessMessage();
    $('#Modal').modal('hide');
    $(updatedRow).replaceWith(row);
    KTMenu.createInstances();
}