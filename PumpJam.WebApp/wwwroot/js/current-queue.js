function Load() {
    $.ajax({
        type: "GET",
        url: ajaxUrl,
        dataType: "json",
        success: function (result) {
            //alert(result[0]["contest"]);
            try {
                FillRating(result);
            } finally {

            }
        },
        error: function (err) {

        }
    });
}

function FillRating(data) {
    console.log(data[0])
    $('h1.group').text(`Очередь заезда группы ${data[0]['contest']}`);
    // $('h1.group').text(`Очередь заезда №${data[0]['raceNum']} группы ${data[0]['contest']}`);
    //$('h1.current').text(`Заезд №${data[0]['raceNum']} группы ${data[0]['contest']}`);
    var qual = data[0]['qual'];
    if (qual) {
        var tableHead = `<tr>
                <th>Номер участника</th>
                <th>Имя участника</th>
            </tr>`;
        $('thead').html(tableHead);
    } else {
        var tableHead = `<tr>
                <th>Номер участника</th>
                <th>Имя участника</th>                
            </tr>`;
        $('thead').html(tableHead);
    }
    var table = '';
    for (racerKey in data) {
        var tableData = `<td><strong>${data[racerKey]["bib"]}</strong></td><td>${data[racerKey]["name"]}</td>`;
        var tableRow = `<tr>${tableData}</tr>`;
        table += tableRow;
        $('tbody').html(table);
    }
}

$(document).ready(() => {
    let interval = setInterval(function () {
        Load()
    }, 3000);
});