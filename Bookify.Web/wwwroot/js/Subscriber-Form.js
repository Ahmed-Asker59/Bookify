$(document).ready(function () {
    //select the governorate select list
    $('#GovernorateId').on('change', function () {
        //select the selected governorate
        var governorateId = $(this).val();
        var areaList = $('#AreaId');
        areaList.empty();
        areaList.append('<option></option>');
        if (governorateId !== '') {
            $.ajax({
                url: '/Subscribers/GetAreas?governorateId=' + governorateId,
                success: function (areas) {
                    $.each(areas, function (i, area) {
                        var areaToAdd = $('<option></option>').attr("value", area.value).text(area.text);
                        areaList.append(areaToAdd);
                    });
                },
                error: function () {
                    showErrorMessage();
                }
            });
        }

    })
}
);