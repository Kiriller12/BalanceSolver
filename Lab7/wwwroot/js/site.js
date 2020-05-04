// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function resizeToFitContent(element) {
    element.height = element.contentWindow.document.body.scrollHeight + 16;
}

function resetIframe() {
    let element = document.getElementsByName("output");
    if (element[0]) {
        element[0].height = 128;
    }
}