$(document).ready(function () {
    $('.js-renew').on('click', function () {
        var subscriptionKey = $(this).data('key');
        bootbox.confirm({
            message: 'Are you sure you want to renew this subscription?',
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success'
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
                        url: `/Subscribers/RenewSubscription?skey=${subscriptionKey}`,
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (row) {
                            $('#SubscriptionsTable').find('tbody').append(row);
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