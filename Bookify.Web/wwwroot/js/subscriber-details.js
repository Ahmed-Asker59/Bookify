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
                            var activeIcon = $('#ActiveStatusIcon');
                            activeIcon.removeClass('d-none'); 
                            activeIcon.siblings('svg').remove();
                            activeIcon.parents('.card').removeClass('bg-warning').addClass('bg-success');
                            $('#RentalButton').removeClass('d-none');
                            $('#CardStatus').text('Active Subscriber');
                            $('#StatusBadge').removeClass('badge-light-warning').addClass('badge-light-success').text('Active Subscriber');
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

    $('.js-cancel-rental').on('click', function () {
        var btn = $(this);
        bootbox.confirm({
            message: 'Are you sure you want to cancel this rental?',
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
                        url: `/Rentals/MarkAsDeleted/${btn.data('id')}`,
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (copiesCount) {
                            btn.parents('tr').remove();
                            if ($('#RentalsTable tbody tr').length === 0) {
                                $('#RentalsTable').fadeOut(function () {
                                    $('#Alert').fadeIn();
                                });   
                            }
                            showSuccessMessage();
                            var totalCount = $('#TotalCopies');
                            var currentCount = parseInt(totalCount.text());
                            totalCount.text(currentCount - copiesCount);
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