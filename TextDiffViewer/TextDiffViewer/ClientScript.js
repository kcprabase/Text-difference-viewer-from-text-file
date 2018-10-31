//<script src="https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js" ;></script>

//$(document).ready(function () {
    
//    $(".oldFileView").scroll(function () {
//        $(".newFileView").scrollLeft($(this).scrollLeft());
//    });

//    $(".newFileView").scroll(function () {
//        $(".oldFileView").scrollLeft($(this).scrollLeft());
//    });
//    $(".oldFileView").scroll(function () {
//        $(".newFileView").scrollTop($(this).scrollTop());
//    });

//    $(".newFileView").scroll(function () {
//        $(".oldFileView").scrollTop($(this).scrollTop());
//    });
//});


var radioIndividualSelection = true;//true for individual file upload mode

//validation of buttons, checkbox and radio buttons when all files are not uploaded!!!
function validation() {
    
    if (radioIndividualSelection) {
        var valfirst = document.getElementById("txtSourceOld").value;
        var valsecond = document.getElementById("txtSourceNew").value;
        var valfirstUpload = document.getElementById("fileUploadOld").value;
        var valSecondUpload = document.getElementById("fileUploadNew").value;

        if ((valfirst == null || valfirst == "") && (valfirstUpload == null || valfirstUpload == "")) {
            document.getElementById("spnFirstName").innerHTML = "* Please select two files for comparision!!!";
            //      alert(   "A file is required to compare");
            return false;

        }
        else if ((valsecond == null || valsecond == "") && (valSecondUpload == null || valSecondUpload == "")) {
            document.getElementById("spnFirstName").innerHTML = "* Please select two files for comparision!!!";
            //      alert(   "A file is required to compare");
            return false;
        }
        else
            return true;
    } else {
        var valLaodFile = document.getElementById("txtSourceLoadFile").value;
        var valLoadFileUpload = document.getElementById("fileUploadLoad").value;
        if ((valLoadFileUpload == null || valLoadFileUpload == "") && (valLoadFileUpload == null || valLoadFileUpload == "")) {
            document.getElementById("spnFirstName").innerHTML = "* Please select a file with address of text files to compare!!!";
            //      alert(   "A file is required to compare");
            return false;
        } else
            return true;
    }
}

//click of browse button opens file upload control which is hidden due to inconsistent appears from browser to browser
function showOldBrowseDialog(filename) {
    var fileuploadctrl = document.getElementById("fileUploadOld");
    fileuploadctrl.click();
    return false;
}
function showNewBrowseDialog(filename) {
    var fileuploadctrl = document.getElementById("fileUploadNew");
    fileuploadctrl.click();
    return false;
}
function showLoadBrowseDialog(filename) {
    localStorage.clear();
    var fileuploadctrl = document.getElementById("fileUploadLoad");
    fileuploadctrl.click();
    return false;
}

//set the text with uploaded filename in the textbox with browse button
function setfilename(val, name) {
    var fileName = val.substr(val.lastIndexOf("\\") + 1, val.length);
    document.getElementById(name).value = fileName;
}

//set filename of loadfile and number of comparisions included in load file
function setFileNameAndLineNumber(val, name, event) {

    var fileName = val.substr(val.lastIndexOf("\\") + 1, val.length);
    document.getElementById(name).value = fileName;

    var input = event.target;

    var reader = new FileReader();
    reader.onload = function () {
        var text = reader.result;
        var textLines = text.split('\n');
        var count = textLines.length;
  //      var fileCount = count;
        localStorage.setItem('fileCount', count);
    };
    reader.readAsText(input.files[0]);
}

//function openTab(evt, tabName) {
//    var i, tabcontent, tablinks;
//    tabcontent = document.getElementsByClassName("tabcontent");
//    for (i = 0; i < tabcontent.length; i++) {
//        tabcontent[i].style.display = "none";
//    }
//    tablinks = document.getElementsByClassName("tablinks");
//    for (i = 0; i < tablinks.length; i++) {
//        tablinks[i].className = tablinks[i].className.replace(" active", "");
//    }
//    document.getElementById(tabName).style.display = "table-row";
//    evt.currentTarget.className += " active";
//}

//changes the file selection mode for comparision (individual or multiple)
function openTab() {
    var list = document.getElementById("radioFileMode"); //Client ID of the radiolist
    var inputs = list.getElementsByTagName("input");
    var selected;
    for (var i = 0; i < inputs.length; i++) {
        if (inputs[i].checked) {
            selected = inputs[i];
            break;
        }
    }
    if (selected) {
    if (selected.value == "individual") {
        document.getElementById("BrowseIndividualFiles").style.display = "table-row";
        document.getElementById("LoadMultipleFiles").style.display = "none";
        radioIndividualSelection = true;
    }
    else if (selected.value == "multiple") {
        document.getElementById("BrowseIndividualFiles").style.display = "none";
        document.getElementById("LoadMultipleFiles").style.display = "table-row";
        radioIndividualSelection = false;
    }
    document.getElementById("spnFirstName").innerText = "";
    }
}

//var skipNum = 2;
//localStorage.setItem('skipNum', 2);
//
//stores the count of comparision being made with load file
function ContinueBtnStuffs() {
    var skipNum = localStorage.getItem('skipNum');

    if (skipNum === null) {
        skipNum = 2;
    }
    var lineCount = localStorage.getItem('fileCount');
    document.getElementById("hiddenSkipNum").value = skipNum;
        skipNum++;
        if (skipNum >= lineCount)
        skipNum = 2;
        localStorage.setItem('skipNum', skipNum);
}

//stores the count of comparision being made with load file
function PreviousBtnStuffs(){
    var skipNum = localStorage.getItem('skipNum');

    if (skipNum === null) {
        skipNum = 2;
    }
    var lineCount = localStorage.getItem('fileCount');
    document.getElementById("hiddenSkipNum").value = skipNum;
    skipNum--;
    if (skipNum < 2)
        skipNum = lineCount-1;
    localStorage.setItem('skipNum', skipNum);
}