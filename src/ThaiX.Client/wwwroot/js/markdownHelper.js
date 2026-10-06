/**
 * Highlights all <pre><code> blocks inside the given container using highlight.js
 * (loaded via CDN in index.html). Call after each preview re-render.
 * @param {HTMLElement} container
 */
window.highlightCodeBlocks = function (container) {
    if (!container || !window.hljs) return;
    container.querySelectorAll('pre code').forEach(function (block) {
        window.hljs.highlightElement(block);
    });
};

/**
 * Wraps the current selection (or inserts a placeholder if nothing is selected) with the
 * given before/after markdown syntax, directly mutating the textarea's DOM value and firing
 * an 'input' event so Blazor's two-way binding picks up the change. Returns the new full value
 * so the caller can also keep its own C# state in sync without waiting for a render round-trip.
 * @param {HTMLTextAreaElement} textarea
 * @param {string} before
 * @param {string} after
 * @param {string} placeholder
 * @returns {string}
 */
window.insertMarkdownSyntax = function (textarea, before, after, placeholder) {
    if (!textarea) return '';

    var start = textarea.selectionStart;
    var end = textarea.selectionEnd;
    var value = textarea.value;
    var selected = value.substring(start, end) || placeholder || '';
    var newValue = value.substring(0, start) + before + selected + after + value.substring(end);

    textarea.value = newValue;
    var cursorPos = start + before.length + selected.length;
    textarea.setSelectionRange(cursorPos, cursorPos);
    textarea.focus();
    textarea.dispatchEvent(new Event('input', { bubbles: true }));

    return newValue;
};

/**
 * Inserts plain text at the current cursor position (replacing any selection), used for
 * image-upload markdown snippets. Same DOM-mutation + synthetic 'input' event approach as
 * insertMarkdownSyntax.
 * @param {HTMLTextAreaElement} textarea
 * @param {string} text
 * @returns {string}
 */
window.insertTextAtCursor = function (textarea, text) {
    if (!textarea) return '';

    var start = textarea.selectionStart;
    var end = textarea.selectionEnd;
    var value = textarea.value;
    var newValue = value.substring(0, start) + text + value.substring(end);

    textarea.value = newValue;
    var cursorPos = start + text.length;
    textarea.setSelectionRange(cursorPos, cursorPos);
    textarea.focus();
    textarea.dispatchEvent(new Event('input', { bubbles: true }));

    return newValue;
};
