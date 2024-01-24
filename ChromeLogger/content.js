var numMinutes = 2;

// get the url
setInterval(function() {
    var moveContent = confirm("You should take a break! Move content to the chair?");
    if (moveContent){
        var url = getUrl();
        saveText("recent.txt", url);
    }
}, (numMinutes * 60000));

function getUrl() {
    var url = window.location;
    console.log("Current url: ", url);
    return url;
} 

// write to a file - TODO silent?
function saveText(filename, text) {
    var tempElem = document.createElement('a');
    tempElem.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
    tempElem.setAttribute('download', filename);
    tempElem.click();
}