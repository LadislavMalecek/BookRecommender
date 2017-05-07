$('img.dynamic').each(function() {
    var item = this;
    $.ajax({
        type: "GET",
        url: "/Ajax/DynamicImage?entityUri=" + $(this).data('uri'),
        datatype: "html",
        success: function(data) {
            $(item).attr('src', data);
        },
        error: function() {
            $(item).attr('src', "images/imgna.png");
        }
    })
});

$('.recommendation').each(function() {
    var item = this;
    var type = $(this).data('type');
    var data = $(this).data('data');
    var ajaxUrl = "/Ajax/Recommendation?type=" + type + "&data=" + data;

    $.ajax({
        type: "GET",
        url: ajaxUrl,
        datatype: "html",
        success: function(data) {
            $(item).html(data);
        },
        error: function() {
            $(item).html();
        }
    })
});

$('#additionalData').each(function() {
    var item = this;
    $.ajax({
        type: "GET",
        url: "/Ajax/SparqlData?entityUri=" + $(this).data('uri'),
        datatype: "html",
        success: function(data) {
            $(item).html(data);
        }
    })
});

$(function() {
    $('#rateButton button').click(function() {
        $('#rateButton').hide();
        $('#rateForm').fadeIn("slow");
    });
});

var lastValue = '';
$("#searchBar").on('change keyup paste mouseup', function() {
    if ($(this).val() != lastValue) {
        lastValue = $(this).val();
        var item = this;
        $.ajax({
            type: "GET",
            url: "/Ajax/QueryAutoComplete?query=" + $(this).val(),
            dataType: "json",
            success: function(data) {
                $('#options').empty();
                for (var i = 0; i < data.length; i++) {
                    $('#options').append("<option value='" + data[i] + "'>");
                }
            }
        })
    }
});