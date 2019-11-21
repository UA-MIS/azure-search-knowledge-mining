// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

$('#next-control').click(function () {
    var idx = parseInt($('#docID').val());

    if (idx < results.length) {
        ShowDocument(idx + 1);

    }
});

$('#prev-control').click(function () {
    var idx = parseInt($('#docID').val());

    if (idx > 0) {
        ShowDocument(idx - 1);
        
    }
});

function getTextAnnotations() {

    div = document.getElementById("createAnn");
    div.style.display = "none";

    var items = document.getElementsByName("saveInfo");
    var id = items[0].value;

    $.ajax({
        type: "POST",
        url: "/Home/getArrayOfChars",
        data: { id: id },
        success: function (data) {
            startChars = data.textStartChars;
            endChars = data.textEndChars;
            annotations = data.textAnnotations;
            if (annotations.length != 0) {
                textRange(startChars, endChars, annotations);
            }
            else {
                $('#classification-loading-indicator').hide();
                alert("There are no text classification annotations.");
            }
        }
    });
}
function getEntityAnnotations() {

    div = document.getElementById("createAnn");
    div.style.display = "none";

    var items = document.getElementsByName("saveInfo");
    var id = items[0].value;

    $.ajax({
        type: "POST",
        url: "/Home/getArrayOfChars",
        data: { id: id },
        success: function (data) {
            startChars = data.entityStartChars;
            endChars = data.entityEndChars;
            annotations = data.entityAnnotations;
            if (annotations.length != 0) {
                entityRange(startChars, endChars, annotations);
            }
            else {
                $('#classification-loading-indicator').hide();
                alert("There are no entity classification annotations.");
            }
        }
    });
}


// Details
function ShowDocument(id) {
    $.post('/home/getdocumentbyid',
        {
            id: id
        },
        function (data) {
            result = data.result;
            chars = data.chars;

            var pivotLinksHTML = "";

            $('#details-pivot-content').html("");
            $('#Highlight-Buttons').html("");
            $('#Save-Annotation').html("");
            $('#reference-viewer').html("");
            $('#tag-viewer').html("");
            $('#details-viewer').html("").css("display", "none");

            $('#result-id').val(id);

            var fileContainerHTML = GetFileHTML(result);
            var transcriptContainerHTML = htmlDecode(result.content.trim());
            var fileName = "File";

            


            $('#Highlight-Buttons').html(`<div style="text-align:center;"><input type="button" style="color: #fff; margin: 5px; background-color: #0078d7; border: 1px solid transparent;  font-size: 13px;" onclick="loading(); getTextAnnotations(); refreshTranscript();" value="Text Classification Annotations">
                                            &nbsp <input type="button" style="color: #fff; margin: 5px; background-color: #0078d7; border: 1px solid transparent;  font-size: 13px;" onclick="loading(); getEntityAnnotations(); refreshTranscript();" value="Entity Classification Annotations"></div></br>                                          
                                            <div style="text-align:center;"> 
                                            <span style="background-color: yellow"> &nbsp &nbsp &nbsp &nbsp </span> &nbsp Text 
                                            &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp 
                                            <iframe src="https://giphy.com/embed/sSgvbe1m3n93G" id="classification-loading-indicator" style="display:none;" width="15" height="15" frameBorder="0" class="giphy-embed"></iframe><a href="https://giphy.com/gifs/juan-gabriel-sSgvbe1m3n93G"></a>
                                            &nbsp &nbsp &nbsp &nbsp &nbsp &nbsp 
                                            <span style="background-color: chartreuse"> &nbsp &nbsp &nbsp &nbsp </span> &nbsp Entity</div>
                                            <div><input type="hidden" id="docID" name="saveInfo" value= ${id}></div>
                                            <span id="myPopup"></span>`);

            $('#details-pivot-content').html(`<div id="file-pivot" class="ms-Pivot-content" data-content="file">
                                            <div id="file-viewer" style="height: 100%;"></div>
                                        </div>
                                        <div id="transcript-pivot" class="ms-Pivot-content" data-content="transcript">
                                            <div id="transcript-viewer" style="height: 100%;">
                                                <div class="panel-body context" id='transcript-div'>
                                                    <pre id="transcript-viewer-pre"></pre>
                                                </div>
                                            </div>
                                        </div>
                                        </div>`);

            $('#file-viewer').html(fileContainerHTML);
            $('#transcript-viewer-pre').html(transcriptContainerHTML);

            pivotLinksHTML += `<li id="file-pivot-link" class="ms-Pivot-link is-selected" data-content="file" title="File" tabindex="1">${fileName}</li>
                       <li id="transcript-pivot-link" class="ms-Pivot-link " data-content="transcript" title="Transcript" tabindex="1">Transcript</li>`;

            var tagContainerHTML = GetTagsHTML(result);

            $('#details-pivot-links').html(pivotLinksHTML);
            $('#tag-viewer').html(tagContainerHTML);
            $('#details-modal').modal('show');

            var PivotElements = document.querySelectorAll(".ms-Pivot");
            for (var i = 0; i < PivotElements.length; i++) {
                new fabric['Pivot'](PivotElements[i]);
            }

            //Log Click Events
            LogClickAnalytics(result.metadata_storage_name, 0);
           
        });
    $.ajax({
        type: "POST",
        url: "/Home/getDocClass",
        data: { id: id },
        success: function (data) {
            
                docClassDisplay(data.classID, data.docClassification);
           
        }
    });
   
}

// Function to get the Selected Text
function annotate() {

    var selectedText = '';

    // window.getSelection
    if (window.getSelection) {
        selectedText = window.getSelection();
    }
    // document.getSelection
    else if (document.getSelection) {
        selectedText = document.getSelection();
    }
    // document.selection
    else if (document.selection) {
        selectedText = document.selection.createRange().text;
    }
    // To write the selected text into the textarea
    document.annotateForm.selectedtext.value = selectedText;

    var element = document.getElementById("transcript-viewer-pre");
    var start = 0, end = 0;
    var sel, range, priorRange;
    if (typeof window.getSelection != "undefined") {
        range = window.getSelection().getRangeAt(0);
        priorRange = range.cloneRange();
        priorRange.selectNodeContents(element);
        priorRange.setEnd(range.startContainer, range.startOffset);
        start = priorRange.toString().length;
        end = start + range.toString().length;
    }
    else if (typeof document.selection != "undefined" && (sel = document.selection).type != "Control") {
        range = sel.createRange();
        priorRange = document.body.createTextRange();
        priorRange.moveToElementText(element);
        priorRange.setEndPoint("EndToStart", range);
        start = priorRange.text.length - 1;
        end = start + range.text.length;
    }
    document.annotateCharForm.startSelectedTextChar.value = start.toString();
    document.annotateCharForm.endSelectedTextChar.value = end.toString();
}

// Function to save everything to table
function save() {

    var items = document.getElementsByName("saveInfo");
    var id = items[0].value;
    var annotate = items[1].value;
    var comment = items[2].value;
    var start = items[3].value;
    var end = items[4].value;

    var dropDownitems = document.getElementsByName("dropDowns");
    var docClassID = dropDownitems[0].value;
    var entityClassID = dropDownitems[1].value;
    var textClassID = dropDownitems[2].value;

    if (annotate != "" && (entityClassID != "null" || textClassID != "null")) {
        $.ajax({
            type: "POST",
            url: "/Home/SaveAnnotations",
            data: { sText: annotate.toString(), id: id.toString(), commentText: comment, docClassID: docClassID, entityClassID: entityClassID, textClassID: textClassID, start: start, end: end },
            success: function () {
                alert("Annotation has been saved.");
                //then reload the page.
                var items = document.getElementsByName("saveInfo");
                //annotate box
                items[1].value = "";
                //comment box
                items[2].value = "";
                //hidden start char box
                items[3].value = "";
                //hidden end char box
                items[4].value = "";
                //popup
                popup = document.getElementById("myPopup");
                popup.innerHTML = "";
                //text and entity classification dropdowns
                var dropDownDivs = document.getElementsByName("dropDownDivs");
                var dropDowns = document.getElementsByName("dropDowns");
                var entityType = dropDownDivs[0];
                var textType = dropDownDivs[1];
                var entity = dropDowns[1];
                var text = dropDowns[2];
                entity.selectedIndex = 0;
                text.selectedIndex = 0;
                entityType.style.display = "none";
                textType.style.display = "none";
                if (entityType.style.display === "block") {

                    document.getElementById("btn1").disabled = false;
                }
                else if (entityType.style.display === "none") {

                    document.getElementById("btn1").disabled = false;
                }
                if (textType.style.display === "block") {

                    document.getElementById("btn2").disabled = false;
                }
                else if (textType.style.display === "none") {

                    document.getElementById("btn2").disabled = false;
                }
            }
        });
    } else if (entityClassID == "null" && textClassID == "null") {
        alert("Must have a text or entity classification to be saved.");
    }
    else if (annotate == "") {
        alert("Must have an annotation to be saved.");
    }
    else {
        alert("Make sure all fields are complete.");
    }

}

//Functions to highlight all previous annotations
function getTextNodesIn(node) {
    var textNodes = [];
    if (node.nodeType == 3) {
        textNodes.push(node);
    } else {
        var children = node.childNodes;
        for (var i = 0, len = children.length; i < len; ++i) {
            textNodes.push.apply(textNodes, getTextNodesIn(children[i]));
        }
    }
    return textNodes;
}

function setSelectionRange(el, start, end) {
    if (document.createRange && window.getSelection) {
        var range = document.createRange();
        range.selectNodeContents(el);
        var textNodes = getTextNodesIn(el);
        var foundStart = false;
        var charCount = 0, endCharCount;

        for (var i = 0, textNode; textNode = textNodes[i++];) {
            endCharCount = charCount + textNode.length;
            if (!foundStart && start >= charCount && (start < endCharCount || (start == endCharCount && i <= textNodes.length))) {
                range.setStart(textNode, start - charCount);
                foundStart = true;
            }
            if (foundStart && end <= endCharCount) {
                range.setEnd(textNode, end - charCount);
                break;
            }
            charCount = endCharCount;
        }
        var sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range);
    } else if (document.selection && document.body.createTextRange) {
        var textRange = document.body.createTextRange();
        textRange.moveToElementText(el);
        textRange.collapse(true);
        textRange.moveEnd("character", end);
        textRange.moveStart("character", start);
        textRange.select();
    }
}

function makeEditableAndHighlightText(colour, annotation) {

    var sel = window.getSelection();
    if (sel.rangeCount && sel.getRangeAt) {
        var range = sel.getRangeAt(0);
    }
    document.designMode = "on";

    var id = annotation.annotationID;
    range.deleteContents();
    var el = document.createElement("span");
    el.innerHTML = '<span style="background-color:' + colour + '; cursor: pointer;" onclick="callPV(' + "'" + id + "'" + ')">' + annotation.highlightedText + '</span>';
    var frag = document.createDocumentFragment(), node, lastNode;
    while ((node = el.firstChild)) {
        lastNode = frag.appendChild(node);
    }
    range.insertNode(frag);

    $('#classification-loading-indicator').hide();
    document.designMode = "off";
}

function makeEditableAndHighlightEntity(colour, annotation) {
    var sel = window.getSelection();
    if (sel.rangeCount && sel.getRangeAt) {
        var range = sel.getRangeAt(0);
    }
    document.designMode = "on";

    var id = annotation.annotationID;
    range.deleteContents();
    var el = document.createElement("span");
    el.innerHTML = '<span style="background-color:' + colour + '; cursor: pointer;" onclick="callPV(' + "'" + id + "'" + ')">' + annotation.highlightedText + '</span>';
    var frag = document.createDocumentFragment(), node, lastNode;
    while ((node = el.firstChild)) {
        lastNode = frag.appendChild(node);
    }
    range.insertNode(frag);

    $('#classification-loading-indicator').hide();
    document.designMode = "off";
}

function textRange(startChars, endChars, annotations) {

    for (var i = 0; i < startChars.length; i++) {
        selectTextRange(document.getElementById("transcript-viewer-pre"), startChars[i], endChars[i], annotations[i])
    }
}

function entityRange(startChars, endChars, annotations) {

    for (var i = 0; i < startChars.length; i++) {
        selectEntityRange(document.getElementById("transcript-viewer-pre"), startChars[i], endChars[i], annotations[i])
    }
}

function selectTextRange(id, start, end, annotation) {
    setSelectionRange(document.getElementById("transcript-viewer-pre"), start, end);
    makeEditableAndHighlightText("yellow", annotation);
}

function selectEntityRange(id, start, end, annotation) {
    setSelectionRange(document.getElementById("transcript-viewer-pre"), start, end);
    makeEditableAndHighlightEntity("chartreuse", annotation);
}

function callPV(id) {
    $.ajax({
        type: "POST",
        url: "/Home/AnnotationView",
        data: { id: id },
        success: function (data) {
            var annID = data.annotation.annotationID;
            var text = data.annotation.highlightedText;
            var comments = data.comments;
            var classification = data.classification;
            var deny = data.annotation.deny;
            var accept = data.annotation.accept;
            var popup = document.getElementById("myPopup");
            popup.innerHTML = "<br /><div style='text-align: center;'><strong><u>Details</u></strong></div>" + "<p><br /><strong>ID:</strong> " + annID + "<br />"
                + "<strong>Annotation:</strong> " + text + "<br />"
                + "<strong>Annotation Comment:</strong> " + comments + "<br />"
                + "<strong>Classification:</strong> " + classification + "</p>" + "<div style='text-align: center;'>" + "<div style='text-align: center;'>" + "<button id='accept-btn'  value = 'likes' onclick='thumbsUp(" + '"' + annID + '"' + ", " + '"' + accept + '"' + ")' style='background-color: #0078d7; border: 1px solid transparent; color:white;  margin: 15px; font-size: 13px;'>"
                + "<span class='glyphicon glyphicon-thumbs-up'> Accept </span></button>"
                + "<span id = 'likeCount'>" + accept + " </span>"
                + "<button id='deny-btn' onclick='thumbsDown(" + '"' + annID + '"' + ", " + '"' + deny + '"' + ")' style='background-color:  #0078d7; border: 1px solid transparent; color: white;  margin: 15px; font-size: 13px;'>"
                + "<span class='glyphicon glyphicon-thumbs-down'> Deny </span></button>"
                + "<span id = 'denyCount'>" + deny + " </span></div>";
            popup.classList.toggle("show");
        }
    });
}

function togglediv1() {
    var div = document.getElementById("item1");
    var dropDown = document.getElementById("textType");
    dropDown.selectedIndex = 0;
    div.style.display = div.style.display == "none" ? "block" : "none";

    if (div.style.display === "block") {

        document.getElementById("btn2").disabled = true;
    }
    else if (div.style.display === "none") {

        document.getElementById("btn2").disabled = false;
    }
}

function togglediv2() {
    var div = document.getElementById("item2");
    var dropDown = document.getElementById("entityType");
    dropDown.selectedIndex = 0;
    div.style.display = div.style.display == "none" ? "block" : "none";

    if (div.style.display === "block") {

        document.getElementById("btn1").disabled = true;
    }
    else if (div.style.display === "none") {
        document.getElementById("btn1").disabled = false;
    }
}

//redirects + Add Classification to new webpage
document.getElementById('entityType').onchange = function () {
    if (document.getElementById('entityType').value == "test1") {
        window.location.href = this.children[this.selectedIndex].getAttribute('href');
    }
}

document.getElementById('textType').onchange = function () {
    if (document.getElementById('textType').value == "test2") {
        window.location.href = this.children[this.selectedIndex].getAttribute('href');
    }
}

document.getElementById('docType').onchange = function () {
    if (document.getElementById('docType').value == "test3") {
        window.location.href = this.children[this.selectedIndex].getAttribute('href');
    }
}

function docClassDisplay(classID, docClassification) {
    var list = document.getElementById("option");
    list.value = classID;
    list.innerHTML = docClassification;
}

function thumbsUp(annID, accept) {
    let likes = accept;
    likes++;
    var id = annID;

    $.ajax({
        type: "POST",
        url: "/Home/SaveAcceptValue",
        data: { id: id, likes: likes },
        success: function () {
            callPV(id);
        }
    });

}

function thumbsDown(annID, deny) {

    let dislikes = deny;
    dislikes++;
    var id = annID;

    $.ajax({
        type: "POST",
        url: "/Home/SaveDenyValue",
        data: { id: id, dislikes: dislikes },
        success: function () {
            if (dislikes >= 5) {
                showDeleted();
            } else {
                callPV(id);
            }
        }
    });
}

function showDeleted() {
    window.location.href = "/Home/SoftDelete";
}

function GetTheDocClass() {
    $.ajax({
        type: "POST",
        url: "/Home/getDocClass",
        data: { id: id },
        success: function (data) {

            docClassDisplay(data.classID, data.docClassification);

        }
    });
}

function loading() {
    $('#classification-loading-indicator').show();
}

function refreshTranscript() {
    var transcriptContainerHTML = htmlDecode(result.content.trim());
    $('#transcript-viewer-pre').html(transcriptContainerHTML);
    popup = document.getElementById("myPopup");
    popup.innerHTML = "";
}

function createAnn() {
    var transcriptContainerHTML = htmlDecode(result.content.trim());
    $('#transcript-viewer-pre').html(transcriptContainerHTML);
    div = document.getElementById("createAnn");
    div.style.display = "block";
}

function GetMatches(string, regex, index) {
    var matches = [];
    var match;
    while (match = regex.exec(string)) {
        matches.push(match[index]);
    }
    return matches;
}

function GetFileHTML(result) {

    var path = result.metadata_storage_path + token;

    if (path !== null) {

        var pathLower = path.toLowerCase();
        var fileContainherHTML;

        if (pathLower.includes(".pdf")) {
            fileContainerHTML =
                `<object class="file-container" data="${path}" type="application/pdf">
                    <iframe class="file-container" src="${path}" type="application/pdf">
                        This browser does not support PDFs. Please download the XML to view it: <a href="${path}">Download PDF</a>"
                    </iframe>
                </object>`;
        }
        else if (pathLower.includes(".jpg") || pathLower.includes(".jpeg") || pathLower.includes(".gif") || pathLower.includes(".png")) {
            fileContainerHTML =
                `<div class="file-container">
                    <img style='max-width:100%;' src="${path}"/>
                </div>`;
        }
        else if (pathLower.includes(".xml")) {
            fileContainerHTML =
                `<iframe class="file-container" src="${path}" type="text/xml">
                    This browser does not support XMLs. Please download the XML to view it: <a href="${path}">Download XML</a>"
                </iframe>`;
        }
        else if (pathLower.includes(".htm")) {
            var srcPrefixArr = result.metadata_storage_path.split('/');
            srcPrefixArr.splice(-1, 1);
            var srcPrefix = srcPrefixArr.join('/');

            var htmlContent = result.content.replace(/src=\"/gi, `src="${srcPrefix}/`);

            fileContainerHTML =
                `${htmlContent}`;
        }
        else if (pathLower.includes(".mp3")) {
            fileContainerHTML =
                `<audio controls>
                  <source src="${path}" type="audio/mp3">
                  Your browser does not support the audio tag.
                </audio>`;
        }
        else if (pathLower.includes(".mp4")) {
            fileContainerHTML =
                `<video controls class="video-result">
                        <source src="${path}" type="video/mp4">
                        Your browser does not support the video tag.
                    </video>`;
        }
        else if (pathLower.includes(".doc") || pathLower.includes(".ppt") || pathLower.includes(".xls")) {
            var src = "https://view.officeapps.live.com/op/view.aspx?src=" + encodeURIComponent(path);

            fileContainerHTML =
                `<iframe class="file-container" src="${src}"></iframe>`;
        }
        else {
            fileContainerHTML =
                `<div>This file cannot be previewed. Download it here to view: <a href="${path}">Download</a></div>`;
        }
    }
    else {
        fileContainerHTML =
            `<div>This file cannot be previewed or downloaded.`;
    }

    return fileContainerHTML;
}

function GetSearchReferences(q) {
    var copy = q;

    copy = copy.replace(/~\d+/gi, "");
    matches = GetMatches(copy, /\w+/gi, 0);

    matches.forEach(function (match) {
        GetReferences(match, true);
    });
}

function SearchTranscript(searchText) {
    $('#reference-viewer').html("");

    if (searchText !== "") {
        // get whole phrase
        GetReferences(searchText, false);
    }
}




	
	
	





