/*Script on ValidateCard*/
$(document).ready(function () {
    $('#clear-text-index').click(function () {
        $('#idPIN').val($('#idPIN').val().slice(0, -1));
    });
});

$(document).ready(function () {
    $('#removeText').click(function () {
        $('#idPIN').val($('#idPIN').val().slice(0, -1));
    });
});

function Clear() {
    document.getElementById("idPIN").value = "";
}

function Add(n) {
    var st = document.getElementById("idPIN").value;
    if (st.length < 13) {
        document.getElementById("idPIN").value += n;
    }
}

function displayTable() {
    var change = document.getElementById("keyboard");
    change.style.display = "block";
}

function disableTable() {
    var text = document.getElementById("idPIN").value;
    var change = document.getElementById("keyboard");
    if (text.length == 0) {
        change.style.display = "none";
    }
}

function select(n) {
    return "you are choosing to view the history " + n;
}