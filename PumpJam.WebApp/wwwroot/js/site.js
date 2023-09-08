function Load() {
    $.ajax({
        type: "GET",
        url: ajaxUrl,
        dataType: "json",
        success: function (result) {
            //alert(result[0]["contest"]);
            try {
                FillRating(result);
            }
            finally {

            }
        },
        error: function (err) {

        }
    });
}
function FillRating(data) {
    console.log(data[0])
    $('h1.group').text(`Заезд №${data[0]['raceNum']} группы ${data[0]['contest']}`);
    //$('h1.current').text(`Заезд №${data[0]['raceNum']} группы ${data[0]['contest']}`);
    var qual = data[0]['qual'];
    if (qual) {
        var tableHead = `<tr>
                <th>Место</th>
                <th>Номер участника</th>
                <th>Имя участника</th>
                <th>Середина круга 1</th>
                <th>Общ. время круга 1</th>
                <th>Середина круга 2</th>
                <th>Общ. время круга 2</th>
            </tr>`;
        $('thead').html(tableHead);
    }
    else {
        var tableHead = `<tr>
                <th>Место</th>
                <th>Номер участника</th>
                <th>Имя участника</th>
                <th>Середина круга</th>
                <th>Общ. время круга</th>                
            </tr>`;
        $('thead').html(tableHead);
    }
    var table = '';
    for (racerKey in data) {
        //console.log(data[racerKey])
        var tableData = `<td>${data[racerKey]["rank"]}</td><td>${data[racerKey]["bib"]}</td><td>${data[racerKey]["name"]}</td><td>${data[racerKey]["ht"]}<span class="ms-2 ${data[racerKey]["hbClass"]}">${data[racerKey]["hb"]}</span></td><td>${data[racerKey]["raceT"]}</td>`;
        if (qual) {
            tableData += `<td>${data[racerKey]["h2t"]}<span class="ms-2 ${data[racerKey]["h2bClass"]}">${data[racerKey]["h2b"]}</span></td><td>${data[racerKey]["race2T"]}</td>`;
        }
        var tableRow = `<tr>${tableData}</tr>`;
        table += tableRow;
        $('tbody').html(table);
    }
}

$(document).ready(() => {
    if (window.location.href.includes("current")) {
        let interval = setInterval(function () { Load() }, 1000);
    }
    //Load();
});