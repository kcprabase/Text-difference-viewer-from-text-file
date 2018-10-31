<%@ Page EnableEventValidation="false" Language="C#"  AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="TextDiffViewer.Home"  %>

<%@Import Namespace="System.IO" %>
<%@Import Namespace="my.utils" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="Content-Type" content="text/html;charset=utf-8" />
    <script type="text/javascript">
        var timerStart = Date.now();
        </script>
    <title>Text Diff Viewer</title>
    <link rel="stylesheet" type="text/css" href="~/StyleSheet.css" />
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js" ></script>
    <script src="ClientScript.js"></script>
    
</head>
<body>
    <form id="myForm" runat="server">
        
    <div id ="wrapper">
        <div id="header">
           Text Diff Viewer
        </div>
        <div id="upload">
        <%--    <ul class="tab">
  <li><a href="#" class="tablinks" onclick="openTab(event, 'BrowseIndividualFiles')">Browse Individual Files</a></li>
  <li><a href="#" class="tablinks" onclick="openTab(event, 'LoadMultipleFiles')">Load Multiple Files</a></li>
</ul>--%>
            <%--<asp:RadioButtonList  ID="radioFileMode" runat="server" RepeatDirection="Horizontal" onChange="openTab()" >
                <asp:ListItem Value="individual" Selected="True">Browse Individual Files</asp:ListItem>            
                <asp:ListItem Value="multiple">Load Multiple Files</asp:ListItem>
                </asp:RadioButtonList>--%>

            <table  style="width: 100%;">
            <tr id="BrowseIndividualFiles" class="tabcontent" style="display:table-row" runat="server">
                <td>Old File: </td>
                <td>
                    <asp:TextBox ID="txtSourceOld" runat="server" ReadOnly="true"></asp:TextBox>
                    <asp:FileUpload ID="fileUploadOld" runat="server" Width="293px" ViewStateMode="Enabled" Style="display: none;" onchange="setfilename(this.value, 'txtSourceOld');"/>
                    <asp:Button ID="btnOld" type="button" runat="server" Text="Browse" OnClientClick="return showOldBrowseDialog()" />
                </td>
               
                <td>New File: </td>
                <td>
                    <asp:TextBox ID="txtSourceNew" ReadOnly="true" runat="server"></asp:TextBox>
                    <asp:FileUpload ID="fileUploadNew" runat="server" Width="293px" Height="24px" style="margin-left: 0px; display: none;" ViewStateMode="Enabled" onchange="setfilename(this.value, 'txtSourceNew');"/>
                    <asp:Button ID="btnNew" type="button" runat="server" Text="Browse" OnClientClick="return showNewBrowseDialog()" />
                </td>
                <td>       
                <asp:Button ID="btnCompare" runat="server" Text="Compare" Width="183px" OnClientClick="return validation()"  OnClick="btnCompare_Click" /> 
                <asp:Label ID="lblResult" runat="server" Text=""></asp:Label>
            </td>
            </tr>
            <tr id="LoadMultipleFiles" class="tabcontent" style="display:none" runat="server">
                <td>Load File: </td>
                <td>
                    <asp:TextBox ID="txtSourceLoadFile" runat="server" ReadOnly="true"></asp:TextBox>
                    <asp:FileUpload ID="fileUploadLoad" runat="server" Width="293px" ViewStateMode="Enabled" Style="display: none;" onchange="setFileNameAndLineNumber(this.value, 'txtSourceLoadFile', event);"/>
                    <asp:Button ID="btnLoad" type="button" runat="server" Text="Browse" OnClientClick="return showLoadBrowseDialog()" />
                </td>
                <td><asp:Button ID="btnPrevious" runat="server" Text="Previous" Width="183px" OnClientClick="return PreviousBtnStuffs()" OnClick="btnContinue_Click" />   
                <asp:Button ID="btnContinue" runat="server" Text="Next" Width="183px" OnClientClick="return ContinueBtnStuffs()" OnClick="btnContinue_Click" /> 
               <asp:Label ID="lblResult1" runat="server" Text=""></asp:Label>
            </td>
            </tr>
        </table>
            <input id="hiddenSkipNum" type="hidden" runat="server"  />
            <span id="spnFirstName" runat="server" style="color:red"></span>                       

        </div>
        <div id="radio">  
            <div style="float:left;"><table><tr><td>       
                <asp:RadioButtonList ID="radioBtnViewMode" runat="server" RepeatDirection="Horizontal" onChange="return validation()" OnTextChanged ="OnChangeRadio"   AutoPostBack="True">
                <asp:ListItem Value="inline" Selected="True">In Line Difference</asp:ListItem>            
                <asp:ListItem Value="side">Side by Side Difference</asp:ListItem>
                </asp:RadioButtonList></td>
                <td><asp:CheckBox ID="cbDIffOnly" runat="server" text="Difference Only" onChange="return validation()" OnCheckedChanged="cbDiffOnly_CheckedChanged" Checked="True" AutoPostBack="True" /></td>
            </tr></table></div><div style="float:right;"><div id="load"></div></div>                    
           </div>
                
        <div id ="container" runat ="server">
          <div id="diffBody" runat="server" class="diffBody"><table id="myTab" >
                    <%Compare();%>
            </table></div>
        </div>
       <table><tr><td><div id="footer" runat="server">
           </div></td>
           <td>
               <a id="firstLink" class="linkButton" onclick="getIdZero()"> << </a></td>
           <td>
               <a id="prevLink" class="linkButton" onclick="getCurrentId('prevLink');"> < </a></td>
           <td>
               <a id="nextLink" class="linkButton" onclick="getCurrentId('nextLink');"> > </a></td>
           <td>
               <a id="lastLink" class="linkButton" onclick="getCurrentId('lastLink')"> >> </a></td>
                         </tr></table>
    </div>
    </form>

    <script>
        var pageLoadTime;
        //for progress bar display
        document.onreadystatechange = function () {
            var state = document.readyState
            if (state == 'complete') {
                document.getElementById('interactive');
                document.getElementById('load').style.visibility = "hidden";
            }
        }

    // for showing page load time console
         $(document).ready(function () {
             console.log("Time until DOMready: ", Date.now() - timerStart), "ms";
         });
         $(window).load(function () {
             pageLoadTime = Date.now() - timerStart;
             console.log("Time until everything loaded: ", pageLoadTime, "ms");
             save();
         });

        //for saving pageload time in log file at server
         function save(){
             var param1 = msToTime(pageLoadTime);
             $.ajax({
                 type: "POST",
                 url: "AddLog.asmx/Save",
                 data: "{param1: '"+ param1  +"'}",
                 contentType: "application/json; charset=utf-8",
                 dataType: "json",
            //     async: false,
            //     cache: false,
                 success: function(result){
                     //Successfully gone to the server and returned with the string result of the server side function do what you want with the result  
                     console.log("page load time saved");
                 },
                 error: function(er){
                     console.log("Error");
                 }
         });
         }

        //for displaying the hidden texts and removing the expand button after click
         function ToggleVisibility(rowName, btnId, startRow) {
             var els = document.getElementsByName(rowName);
             var i;
             var j;
             for (i = els.length - startRow-1, j = 0; i >= 0 && j<50 ; i--, j++) {
                 els[i].style.display = "table-row";
             }
             startRow += 50;
             var btns = document.getElementsByName(btnId);
             for (var x = 0 ; x < btns.length; x++)
             btns[x].setAttribute("onClick", "javascript: ToggleVisibility('"+ rowName +"','"+ btnId +"',"+ startRow +");");
             if (i <= 0)
                 document.getElementById(btnId).style.display = "none";
             //    for (var x = 0 ; x < btns.length; x++)
             //        btns[x].style.display = "none";
             //}
         }
         
        //conversion of time from millisecond to h:m:s later to be saved in log file
         function msToTime(duration) {
             var milliseconds = parseFloat((duration % 1000))
                 , seconds = parseInt((duration / 1000) % 60)
                 , minutes = parseInt((duration / (1000 * 60)) % 60)
                 , hours = parseInt((duration / (1000 * 60 * 60)));

             //hours = (hours < 10) ? "0" + hours : hours;
             //minutes = (minutes < 60) ? "0" + minutes : minutes;
             //seconds = (seconds < 60) ? "0" + seconds : seconds;

             return hours + ":" + minutes + ":" + seconds + "." + milliseconds;
         }


        //navigation button tasks.
         var currentId = 0;
         var lastId = '<%=lastId%>';
        var diffOnly = '<%=diffOnly%>'
        var firstLink = document.getElementById("firstLink");
        var prevLink = document.getElementById("prevLink");
        var nextLink = document.getElementById("nextLink");
        var lastLink = document.getElementById("lastLink");
        if (diffOnly)
           firstLink.setAttribute("href", "#1");
        else
            firstLink.setAttribute("href", "#0");
            nextLink.setAttribute("href", "#1");
            prevLink.setAttribute("href", "#0");
            lastLink.setAttribute("href","#" + lastId);
                 

        //sets the id of next difference row to be shown after a navigation button is clicked
         function getCurrentId(linkId) {
         //    document.getElementById(linkId).className = "highlighted";
            if (linkId == "lastLink") {
                currentId = lastId;
                prevLink.setAttribute("href", "#" + (currentId - 1));
                nextLink.setAttribute("href", "#" + lastId);
            } else {
                 nextLink.setAttribute("href", "#" + (currentId + 1));
                 prevLink.setAttribute("href", "#" + (currentId - 1));
                 if (($(".diffBody").height() - $("#" + currentId).height() - $("#" + currentId).offset().top) < -100) {
                     nextLink.setAttribute("href", "#" + (currentId));;
                 }
                 if (linkId == "nextLink" && currentId < lastId)
                     currentId++;
                 else if (linkId == "prevLink" && currentId > 0)
                     currentId--;
            }
             //highlight the difference navigated using button
            $(function () {
             //   $(document.getElementById(currentId)).addClass('highlighted');
                $(document.getElementById(currentId)).children().addClass('highlighted');
                    $(document.getElementById(currentId)).siblings().children().removeClass('highlighted');
            });
                 return false;
         }

        //highlight the row in webpage when clicked
         $(function () {
             $('#myTab td').click(function () {
                 $('tr').removeClass('highlighted');
                 $('td').removeClass('highlighted');
                 $(this).siblings().addClass('highlighted');
                 $(this).addClass('highlighted');

             });
         });

        //to synchronize scroll bar and navigation buttons for id of next diff to be shown after button click
        function getUtlCurrentId(linkId) {
            if (linkId <= lastId)
                currentId = linkId;
        }

//set current id to zero
         function getIdZero() {
             currentId = 0;
             return false;
         }

        //to get the row id of current difference just showing on the webpage
         var rows = $("#myTab tr");
         $(".diffBody").scroll(function (event) {
             //  debugger;
             var t = $(".diffBody").scrollTop() + 50;
             var highlight_row;
             rows.each(function () {
                 var row = $(this);
                 if (row.offset().top < 100 && row.attr('id') != undefined) {
                     highlight_row = row;
                     var testId = highlight_row.attr('id');
                     if (!($("#cbDIffOnly").is(":checked")))
                     currentId = parseInt(testId) + 1;
                     //nextLink.setAttribute("href", "#" + (currentId + 1));
                     //prevLink.setAttribute("href", "#" + (currentId - 1));
                 }
             });
         });

        </script>

</body>
    
</html>