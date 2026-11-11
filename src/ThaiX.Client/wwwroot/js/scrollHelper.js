window.downloadFileFromBytes = function (fileName, contentType, base64Content) {
    var anchor = document.createElement('a');
    anchor.href = 'data:' + contentType + ';base64,' + base64Content;
    anchor.download = fileName;
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
};

window.printResume = function () {
    window.print();
};

window.copyTextToClipboard = async function (text) {
    if (navigator.clipboard && window.isSecureContext) {
        await navigator.clipboard.writeText(text);
        return true;
    }

    var textArea = document.createElement('textarea');
    textArea.value = text || '';
    textArea.setAttribute('readonly', '');
    textArea.style.position = 'fixed';
    textArea.style.top = '0';
    textArea.style.left = '0';
    textArea.style.opacity = '0';

    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();
    textArea.setSelectionRange(0, textArea.value.length);

    try {
        return document.execCommand('copy');
    } finally {
        document.body.removeChild(textArea);
    }
};

// Keep arrow keys and space from scrolling the page while a game board has focus.
// Blazor cannot do this per key: @onkeydown:preventDefault would also swallow Tab.
document.addEventListener('keydown', function (event) {
    var target = event.target;
    if ((event.key === ' ' || event.key.indexOf('Arrow') === 0)
        && target.closest && target.closest('[data-game-keys]')
        && !target.matches('input, textarea, button')) {
        event.preventDefault();
    }
});
