$(document).on("keyup", "#searchInput", function () {
    let key = $("#searchInput").val();

    $.ajax({
        url: "/Admin/Categories/Search",
        type: "GET",
        data: {
            "key": key
        },
        success: function (res) {
            $("#Categories").html(res);

        }
    });
});