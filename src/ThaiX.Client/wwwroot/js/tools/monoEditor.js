/**
 * Keep MonoTextArea line gutter scrollTop in sync with the textarea.
 * Returns { dispose() } so Blazor can unbind on re-render/dispose.
 */
window.ThaiXBindMonoEditorScroll = function (textarea, gutter) {
    if (!textarea || !gutter) {
        return { dispose: function () { } };
    }

    const sync = function () {
        gutter.scrollTop = textarea.scrollTop;
    };

    textarea.addEventListener('scroll', sync, { passive: true });
    sync();

    return {
        dispose: function () {
            textarea.removeEventListener('scroll', sync);
        }
    };
};
