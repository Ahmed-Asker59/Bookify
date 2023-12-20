var updatedRow;
// Shared variables
var table;
var datatable;
var exportedCols = [];
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

function disableSubmitButton() {
    $('body :submit').attr('disabled', 'disabled').attr('data-kt-indicator', 'on');
}
//when edit or add request is fired
function onModalBegin() {
    disableSubmitButton();
}

//handle editing or adding a category
function onModalSuccess(row) {
    showSuccessMessage();
    $('#Modal').modal('hide');
   
    //if we are editing
    if (updatedRow !== undefined) {
        datatable.row(updatedRow).remove().draw();
        updatedRow = undefined;
    }
    var newRow = $(row);
    datatable.row.add(newRow).draw();
   
    KTMenu.init();
    KTMenu.initHandlers();
}

function onModalComplete() {
    $('body :submit').removeAttr('disabled').removeAttr('data-kt-indicator');
}
//DataTables Exporting
var headers = $('th');
$.each(headers, function (index) {
    var column = $(this);
    if (!column.hasClass('js-no-export')) {
        exportedCols.push(index);
    }
});

//datatables
var KTDatatables = function () {
    // Private functions
    var initDatatable = function () {
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            'info': false,
            'pageLength': 10,
            'drawCallback': function () {
                KTMenu.createInstances();
            }
        });
    }

    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = $('js-datatables').data('document-title');
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                }
            ]
        }).container().appendTo($('#kt_datatable_example_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Public methods
    return {
        init: function () {
            table = document.querySelector('.js-datatables');

            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            handleSearchDatatable();
        }
    };
}();

$(document).ready(function () {
    $('form').on('submit', function () {
        if ($('.js-tinymce').length > 0) {
            $('.js-tinymce').each(function () {
                var input = $(this);
                var content = tinyMCE.get(input.attr('id')).getContent();
                input.val(content);
            });

            }
            
    
        var isValid = $(this).valid();
        if (isValid) disableSubmitButton();

    });
    if ($('.js-tinymce').length > 0) {
        var options = { selector: ".js-tinymce", height: "430" };

        if (KTThemeMode.getMode() === "dark") {
            options["skin"] = "oxide-dark";
            options["content_css"] = "dark";
        }
        tinymce.init(options);
    }

    //handle select2
    $('.js-select2').select2();
    //fire live validtion of the select item
    $('.js-select2').on('select2:select', function (e) {
        var selectList = $(this);
        $('form').validate().element('#' + selectList.attr('id'));
    });

    //end select2
    //datepicker
    $('.js-datepicker').daterangepicker({
        singleDatePicker: true,
        autoApply: true,
        drops: 'up',
       maxDate: new Date()
    });

    //show seet alert
    var message = $('#Message').text();
    if (message !== "") {
        showSuccessMessage();
    }

    //datatables
 KTUtil.onDOMContentLoaded(function () {
            KTDatatables.init();
        });

   

    //Handle Bootstrap model while editing or adding
    $('body').delegate('.js-render-modal', 'click', function () {
        var anchorTag = $(this);
        var modal = $('#Modal');
        //change the title of the modal wheter its Edit or Add
        modal.find('#ModalLabel').text(anchorTag.data('title'));

        //if we are editing select the row 
        if (anchorTag.data('update') !== undefined) {
            updatedRow = anchorTag.parents('tr');
        }
        $.get({
            url: anchorTag.data('url'),
            success: function (form) {
                //render form inside the modal
                modal.find('.modal-body').html(form);
                $.validator.unobtrusive.parse(modal);
            },
            error: function () {
                showErrorMessage();
            }


        });
        modal.modal('show');
    });

    //Handle toggle status
    $('body').delegate('.js-toggle-status', 'click', function () {
        //select the button [anchor tag]
        var btn = $(this);
        //show confirmation box of bootbox library
        bootbox.confirm({
            message: 'Are you sure you want to toggle the status?',
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                //fire the post request to toggle status
                if (result) {
                    $.post({
                        url: btn.data('url'),
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (LastUpdatedOn) {
                            //change the badge content and color
                            var row = btn.parents('tr');
                            //select the span of the badge
                            var badgeSpan = row.find('.js-status');
                            //get new the text content
                            var newStatus = badgeSpan.text().trim() === 'Deleted' ? 'Available' : 'Deleted';
                            //change the color
                            badgeSpan.text(newStatus).toggleClass('badge-light-success badge-light-danger');
                            //change the background
                            row.find('.js-updated-on').html(LastUpdatedOn);
                            //add flash animation to the toggled row
                            row.addClass('animate__animated animate__flash');
                            //success sweet alert
                            showSuccessMessage();

                        },
                        error: function () {

                            showErrorMessage();
                        }

                    });
                }
            }
        });


    });
});