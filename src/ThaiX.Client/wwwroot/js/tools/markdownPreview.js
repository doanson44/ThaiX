window.ThaiXIsDarkUi = function () {
    const root = document.documentElement;
    const body = document.body;
    const attr = (root.getAttribute('data-bs-theme')
        || body?.getAttribute('data-bs-theme')
        || '').toLowerCase();
    if (attr === 'light') return false;
    if (attr === 'dark') return true;
    const theme = (root.className || body?.className || '').toLowerCase();
    if (theme.includes('ThaiX-theme-light')) return false;
    if (theme.includes('dark')) return true;
    const bg = getComputedStyle(body || root).backgroundColor;
    const m = /rgba?\((\d+),\s*(\d+),\s*(\d+)/.exec(bg || '');
    if (!m) return true;
    const luma = (0.2126 * +m[1] + 0.7152 * +m[2] + 0.0722 * +m[3]) / 255;
    return luma < 0.45;
};

window.ThaiXEnsureMermaid = async function () {
    if (!window.mermaid) {
        await new Promise((resolve, reject) => {
            const s = document.createElement('script');
            s.src = 'https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.min.js';
            s.onload = resolve;
            s.onerror = reject;
            document.head.appendChild(s);
        });
    }

    const dark = window.ThaiXIsDarkUi();
    window.mermaid.initialize({
        startOnLoad: false,
        securityLevel: 'loose',
        theme: dark ? 'dark' : 'default',
        themeVariables: dark ? {
            darkMode: true,
            background: '#1c1917',
            primaryColor: '#292524',
            primaryTextColor: '#e7e5e4',
            primaryBorderColor: '#57534e',
            lineColor: '#a8a29e',
            secondaryColor: '#44403c',
            tertiaryColor: '#292524',
            mainBkg: '#292524',
            nodeBorder: '#78716c',
            clusterBkg: '#1c1917',
            titleColor: '#f59e0b',
            edgeLabelBackground: '#1c1917'
        } : undefined,
        er: { useMaxWidth: true }
    });
    return true;
};

window.ThaiXMermaidStartRe = /^(erDiagram|flowchart|graph|sequenceDiagram|classDiagram|stateDiagram(?:-v2)?|pie|gantt|mindmap|timeline|journey)\b/i;

window.ThaiXIsMermaidSource = function (text) {
    const head = (text || '').trim().split(/\r?\n/, 1)[0] || '';
    return window.ThaiXMermaidStartRe.test(head);
};

/**
 * Mermaid sequenceDiagram treats ";" as end-of-statement — quotes do NOT help
 * (verified Mermaid 11). Docs often write:
 *   A->>B: Status Released; start volume capture
 * Replace ";" inside messages with an em dash so the diagram still renders.
 */
window.ThaiXSanitizeMermaidSource = function (source) {
    let text = (source || '').replace(/\r\n/g, '\n').trim();
    if (!text) return text;

    // Trailing markdown HR sometimes glued onto bare blocks before fencing fix.
    text = text.replace(/\n(?:---|\*\*\*|___)\s*$/g, '');

    if (!/^\s*sequenceDiagram\b/im.test(text)) return text;

    return text.split('\n').map((line) => {
        const arrow = line.match(/^(\s*[A-Za-z0-9_]+\s*(?:-{1,2}>{1,2}|-->>)\s*[A-Za-z0-9_]+\s*:\s*)(.*)$/);
        if (arrow) return arrow[1] + window.ThaiXSafeMermaidMessage(arrow[2]);

        const note = line.match(/^(\s*Note(?:\s+(?:left|right)\s+of|\s+over)\s+[^:]+:\s*)(.*)$/i);
        if (note) return note[1] + window.ThaiXSafeMermaidMessage(note[2]);

        return line;
    }).join('\n');
};

window.ThaiXSafeMermaidMessage = function (message) {
    let msg = message ?? '';
    if (!msg) return msg;
    // Strip wrapping quotes from a previous sanitizer attempt / hand-authored quotes.
    const trimmed = msg.trim();
    if ((trimmed.startsWith('"') && trimmed.endsWith('"') && trimmed.length >= 2)
        || (trimmed.startsWith("'") && trimmed.endsWith("'") && trimmed.length >= 2)) {
        msg = trimmed.slice(1, -1);
    }
    // "#" starts a Mermaid comment for the rest of the line.
    msg = msg.replace(/;/g, ' — ').replace(/#/g, '＃');
    return msg.replace(/\s+—\s+/g, ' — ').replace(/\s{2,}/g, ' ');
};

/**
 * Markdig AdvancedExtensions emits: <pre class="mermaid">source</pre>
 * Classic fenced code emits: <pre><code class="language-mermaid">source</code></pre>
 */
window.ThaiXCollectMermaidBlocks = function (container) {
    const found = [];

    // 1) Markdig diagram style
    container.querySelectorAll('pre.mermaid').forEach((pre) => {
        if (pre.querySelector('code')) return; // handled below
        found.push({ el: pre, source: (pre.textContent || '').replace(/\u00a0/g, ' ').replace(/\r\n/g, '\n').trim() });
    });

    // 2) Classic / other renderers
    container.querySelectorAll('pre code').forEach((code) => {
        const cls = (code.className || '').toLowerCase();
        const source = (code.textContent || '').replace(/\u00a0/g, ' ').replace(/\r\n/g, '\n').trim();
        const isMermaid = cls.includes('language-mermaid')
            || cls.split(/\s+/).includes('mermaid')
            || window.ThaiXIsMermaidSource(source);
        if (!isMermaid) return;
        const pre = code.closest('pre') || code;
        found.push({ el: pre, source });
    });

    return found;
};

window.ThaiXClonePreviewForCopy = function (container) {
    if (!container) return null;
    const clone = container.cloneNode(true);
    clone.querySelectorAll('.ThaiX-md-block-actions').forEach((el) => el.remove());
    return clone;
};

window.ThaiXBlobToDataUrl = function (blob) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = () => reject(new Error('FileReader failed'));
        reader.readAsDataURL(blob);
    });
};

window.ThaiXUnwrapPreviewCopyBlocks = function (root) {
    root.querySelectorAll('.ThaiX-md-block').forEach((wrap) => {
        const parent = wrap.parentNode;
        if (!parent) return;
        while (wrap.firstChild) parent.insertBefore(wrap.firstChild, wrap);
        parent.removeChild(wrap);
    });
};

/** Build Word-friendly HTML document (inline styles; semantic tags from Markdig). */
window.ThaiXBuildRichPreviewHtml = function (clone) {
    const body = clone.innerHTML || '';
    return `<!DOCTYPE html><html><head><meta charset="utf-8"><style>
body{font-family:Calibri,Arial,sans-serif;font-size:11pt;line-height:1.5;color:#1c1917;background:#ffffff;}
h1,h2,h3,h4,h5,h6{font-family:Georgia,'Times New Roman',serif;color:#1c1917;margin:0.8em 0 0.35em;}
p{margin:0.45em 0;}
a{color:#b45309;}
pre{background:#f5f5f4;border:1px solid #d6d3d1;padding:8pt;white-space:pre-wrap;font-family:Consolas,'Courier New',monospace;font-size:9pt;color:#1c1917;}
code{font-family:Consolas,'Courier New',monospace;background:#f5f5f4;color:#1c1917;}
blockquote{border-left:3pt solid #d6d3d1;margin:0.5em 0;padding:0.15em 0 0.15em 10pt;color:#57534e;}
table{border-collapse:collapse;margin:0.5em 0;width:100%;}
th,td{border:1px solid #d6d3d1;padding:4pt 8pt;text-align:left;}
th{background:#f5f5f4;}
img{max-width:100%;height:auto;}
ul,ol{margin:0.4em 0 0.4em 1.4em;}
hr{border:none;border-top:1px solid #d6d3d1;margin:1em 0;}
</style></head><body>${body}</body></html>`;
};

window.ThaiXCopyHtmlAndText = async function (html, plain) {
    if (navigator.clipboard && typeof ClipboardItem !== 'undefined' && window.isSecureContext) {
        try {
            const item = new ClipboardItem({
                'text/html': new Blob([html], { type: 'text/html' }),
                'text/plain': new Blob([plain || ''], { type: 'text/plain' })
            });
            await navigator.clipboard.write([item]);
            return true;
        } catch (err) {
            console.warn('Rich clipboard write failed, falling back.', err);
        }
    }

    // Fallback: select a hidden contenteditable host (browsers put text/html on the clipboard).
    const host = document.createElement('div');
    host.contentEditable = 'true';
    host.style.cssText = 'position:fixed;left:-9999px;top:0;width:1px;height:1px;opacity:0;';
    host.innerHTML = html;
    document.body.appendChild(host);
    const selection = window.getSelection();
    const range = document.createRange();
    range.selectNodeContents(host);
    selection.removeAllRanges();
    selection.addRange(range);
    let ok = false;
    try {
        ok = document.execCommand('copy');
    } finally {
        selection.removeAllRanges();
        document.body.removeChild(host);
    }
    if (ok) return true;
    if (window.copyTextToClipboard) return await window.copyTextToClipboard(plain || '');
    return false;
};

window.ThaiXPreparePreviewCloneForRichCopy = async function (container) {
    const clone = window.ThaiXClonePreviewForCopy(container);
    if (!clone) return null;

    window.ThaiXUnwrapPreviewCopyBlocks(clone);

    // Rasterize live Mermaid SVGs (accurate size) → PNG data URLs for Word.
    const liveSvgs = [...container.querySelectorAll('.ThaiX-mermaid:not(.ThaiX-mermaid-error) svg')];
    const cloneHolders = [...clone.querySelectorAll('.ThaiX-mermaid:not(.ThaiX-mermaid-error)')];
    for (let i = 0; i < cloneHolders.length; i++) {
        const liveSvg = liveSvgs[i];
        const holder = cloneHolders[i];
        if (!liveSvg || !holder) continue;
        try {
            const blob = await window.ThaiXSvgToPngBlob(liveSvg, { forceLight: true });
            const dataUrl = await window.ThaiXBlobToDataUrl(blob);
            holder.innerHTML = '';
            const img = document.createElement('img');
            img.src = dataUrl;
            img.alt = 'Diagram';
            img.style.maxWidth = '100%';
            img.style.height = 'auto';
            holder.appendChild(img);
        } catch {
            /* keep SVG if rasterize fails */
        }
    }

    clone.querySelectorAll('pre, code').forEach((el) => {
        el.style.background = '#f5f5f4';
        el.style.color = '#1c1917';
    });

    return clone;
};

/** Copy all — rich HTML + plain text so Word / Outlook paste keeps formatting. */
window.ThaiXCopyMarkdownPreviewText = async function (container) {
    const clone = await window.ThaiXPreparePreviewCloneForRichCopy(container);
    if (!clone) return false;
    const plain = (clone.innerText || '').replace(/\n{3,}/g, '\n\n').trim();
    const html = window.ThaiXBuildRichPreviewHtml(clone);
    if (!plain && !(clone.innerHTML || '').trim()) return false;
    return await window.ThaiXCopyHtmlAndText(html, plain || ' ');
};

/** Copy HTML source as plain text (for editors / email HTML fields). */
window.ThaiXCopyMarkdownPreviewHtml = async function (container) {
    const clone = await window.ThaiXPreparePreviewCloneForRichCopy(container);
    if (!clone) return false;
    const html = (clone.innerHTML || '').trim();
    if (!html) return false;
    if (window.copyTextToClipboard) return await window.copyTextToClipboard(html);
    return false;
};

/** Latest preview generation — stale async enhance/render must no-op. */
window._ThaiXMdPreviewEpoch = 0;

window.ThaiXMdPreviewIsCurrent = function (epoch) {
    return epoch == null || epoch === window._ThaiXMdPreviewEpoch;
};

/**
 * Blazor must NOT own preview innerHTML (MarkupString + JS DOM edits race → reload).
 * Set HTML here, then enhance Mermaid/hljs/copy buttons.
 */
window.ThaiXSetMarkdownPreview = async function (container, html, epoch) {
    window._ThaiXMdPreviewEpoch = epoch;
    if (!container) return;
    try {
        container.innerHTML = html || '';
    } catch {
        return;
    }
    if (!window.ThaiXMdPreviewIsCurrent(epoch) || !container.isConnected) return;
    await window.ThaiXEnhanceMarkdownPreview(container, epoch);
};

window.ThaiXEnhanceMarkdownPreview = async function (container, epoch) {
    const alive = () =>
        !!container
        && container.isConnected
        && window.ThaiXMdPreviewIsCurrent(epoch);

    if (!alive()) return;

    const blocks = window.ThaiXCollectMermaidBlocks(container);
    const mermaidSources = [];

    for (const { el, source } of blocks) {
        if (!alive() || !el || !el.parentNode) continue;
        const holder = document.createElement('div');
        holder.className = 'ThaiX-mermaid';
        holder.setAttribute('data-ThaiX-mermaid', '1');
        try {
            el.parentNode.replaceChild(holder, el);
            mermaidSources.push({ holder, source });
        } catch {
            /* node already replaced by a newer preview */
        }
    }

    if (!alive()) return;

    if (window.hljs) {
        container.querySelectorAll('pre code').forEach((block) => {
            try { window.hljs.highlightElement(block); } catch { /* ignore */ }
        });
    }

    if (mermaidSources.length === 0) {
        if (alive()) window.ThaiXAttachPreviewCopyActions(container);
        return;
    }

    await window.ThaiXEnsureMermaid();
    if (!alive()) return;

    for (let i = 0; i < mermaidSources.length; i++) {
        if (!alive()) return;
        const { holder, source } = mermaidSources[i];
        if (!holder.isConnected) continue;
        if (!source) {
            holder.className = 'ThaiX-mermaid-error';
            holder.textContent = 'Empty mermaid block.';
            continue;
        }
        const sanitized = window.ThaiXSanitizeMermaidSource(source);
        const id = `ThaiXMmd${Date.now()}${i}${Math.random().toString(36).slice(2, 8)}`;
        try {
            const { svg, bindFunctions } = await window.mermaid.render(id, sanitized);
            if (!alive() || !holder.isConnected) return;
            holder.innerHTML = svg;
            if (typeof bindFunctions === 'function') bindFunctions(holder);
        } catch (err) {
            if (!holder.isConnected) continue;
            holder.className = 'ThaiX-mermaid-error';
            const msg = (err && (err.message || err.str || String(err))) || 'Mermaid render failed';
            holder.textContent = msg;
            console.warn('Mermaid render failed:', msg, '\nSource:\n', sanitized);
        }
    }

    if (alive()) window.ThaiXAttachPreviewCopyActions(container);
};

window.ThaiXAttachPreviewCopyActions = function (container) {
    if (!container || !container.isConnected) return;

    const targets = [
        ...container.querySelectorAll('.ThaiX-mermaid'),
        ...container.querySelectorAll('pre'),
    ];
    const btnClass = window.ThaiXIsDarkUi() ? 'btn btn-sm btn-outline-light' : 'btn btn-sm btn-light';

    for (const el of targets) {
        if (!el || !el.parentNode) continue;
        if (el.parentElement && el.parentElement.classList.contains('ThaiX-md-block')) continue;
        if (el.classList.contains('ThaiX-mermaid-error')) continue;

        try {
            const wrap = document.createElement('div');
            wrap.className = 'ThaiX-md-block';
            el.parentNode.insertBefore(wrap, el);
            wrap.appendChild(el);

            const bar = document.createElement('div');
            bar.className = 'ThaiX-md-block-actions';
            const copyImg = document.createElement('button');
            copyImg.type = 'button';
            const canCopyImage = !!(navigator.clipboard && typeof ClipboardItem !== 'undefined' && window.isSecureContext);
            copyImg.textContent = canCopyImage ? 'Copy image' : 'Download image';
            copyImg.title = canCopyImage
                ? 'Copy diagram as PNG'
                : 'Image clipboard needs HTTPS (or localhost). Downloads PNG on HTTP.';
            copyImg.className = btnClass;
            copyImg.onclick = () => {
                window.ThaiXCopyElementAsPng(el, copyImg);
            };
            bar.appendChild(copyImg);

            if (el.tagName === 'PRE') {
                const copyCode = document.createElement('button');
                copyCode.type = 'button';
                copyCode.textContent = 'Copy code';
                copyCode.className = btnClass;
                copyCode.onclick = async () => {
                    const text = el.innerText || '';
                    if (window.copyTextToClipboard) await window.copyTextToClipboard(text);
                };
                bar.appendChild(copyCode);
            }
            wrap.appendChild(bar);
        } catch {
            /* ignore stale nodes */
        }
    }
};

window.ThaiXDownloadBlob = function (blob, fileName) {
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = fileName || 'diagram.png';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    setTimeout(() => URL.revokeObjectURL(a.href), 2000);
};

/** Best-effort image copy without ClipboardItem (may work for HTML paste targets). */
window.ThaiXTryCopyPngViaDom = async function (blob) {
    const url = URL.createObjectURL(blob);
    try {
        const img = new Image();
        img.src = url;
        await img.decode();

        const host = document.createElement('div');
        host.contentEditable = 'true';
        host.style.cssText = 'position:fixed;left:-9999px;top:0;opacity:0;';
        host.appendChild(img);
        document.body.appendChild(host);

        const selection = window.getSelection();
        const range = document.createRange();
        range.selectNode(img);
        selection.removeAllRanges();
        selection.addRange(range);

        let ok = false;
        try {
            ok = document.execCommand('copy');
        } finally {
            selection.removeAllRanges();
            document.body.removeChild(host);
        }
        return ok;
    } finally {
        URL.revokeObjectURL(url);
    }
};

window.ThaiXCanCopyImageToClipboard = function () {
    return !!(navigator.clipboard && typeof ClipboardItem !== 'undefined' && window.isSecureContext);
};

window.ThaiXSvgToPngBlob = async function (svgEl, options) {
    const forceLight = !!(options && options.forceLight);
    const dark = forceLight ? false : window.ThaiXIsDarkUi();
    const fill = dark ? '#1c1917' : '#ffffff';
    const clone = svgEl.cloneNode(true);
    if (!clone.getAttribute('xmlns')) {
        clone.setAttribute('xmlns', 'http://www.w3.org/2000/svg');
    }
    if (!clone.getAttribute('xmlns:xlink')) {
        clone.setAttribute('xmlns:xlink', 'http://www.w3.org/1999/xlink');
    }

    let w = parseFloat(clone.getAttribute('width'));
    let h = parseFloat(clone.getAttribute('height'));
    const vb = (clone.viewBox && clone.viewBox.baseVal && clone.viewBox.baseVal.width)
        ? clone.viewBox.baseVal
        : (svgEl.viewBox && svgEl.viewBox.baseVal);
    if ((!w || !h || Number.isNaN(w) || Number.isNaN(h)) && vb && vb.width && vb.height) {
        w = vb.width;
        h = vb.height;
    }
    if (!w || !h || Number.isNaN(w) || Number.isNaN(h)) {
        const r = svgEl.getBoundingClientRect();
        w = Math.max(Math.ceil(r.width), 1);
        h = Math.max(Math.ceil(r.height), 1);
    }
    w = Math.max(Math.ceil(w), 1);
    h = Math.max(Math.ceil(h), 1);
    clone.setAttribute('width', String(w));
    clone.setAttribute('height', String(h));

    // Opaque background so PNG is not transparent-on-dark chat apps
    const bg = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
    bg.setAttribute('width', '100%');
    bg.setAttribute('height', '100%');
    bg.setAttribute('fill', fill);
    clone.insertBefore(bg, clone.firstChild);

    const xml = new XMLSerializer().serializeToString(clone);
    const dataUrl = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(xml);

    const img = new Image();
    img.decoding = 'async';
    await new Promise((res, rej) => {
        img.onload = () => res();
        img.onerror = () => rej(new Error('SVG image load failed'));
        img.src = dataUrl;
    });

    const scale = 2;
    const canvas = document.createElement('canvas');
    canvas.width = Math.max(img.naturalWidth || w, w) * scale;
    canvas.height = Math.max(img.naturalHeight || h, h) * scale;
    const ctx = canvas.getContext('2d');
    ctx.fillStyle = fill;
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

    const blob = await new Promise((res, rej) => {
        canvas.toBlob((b) => (b ? res(b) : rej(new Error('PNG encode failed'))), 'image/png');
    });
    return blob;
};

window.ThaiXCopyElementAsPng = function (el, buttonEl) {
    const svg = el && (el.tagName === 'svg' ? el : el.querySelector('svg'));
    const canCopy = window.ThaiXCanCopyImageToClipboard();
    const idleLabel = canCopy ? 'Copy image' : 'Download image';
    if (!svg) {
        if (buttonEl) {
            buttonEl.textContent = 'No SVG';
            setTimeout(() => { buttonEl.textContent = idleLabel; }, 1500);
        }
        return;
    }

    if (buttonEl) {
        buttonEl.disabled = true;
        buttonEl.textContent = canCopy ? 'Copying…' : 'Downloading…';
    }

    const pngPromise = window.ThaiXSvgToPngBlob(svg);

    const done = (ok, downloaded) => {
        if (!buttonEl) return;
        buttonEl.disabled = false;
        buttonEl.textContent = ok ? (downloaded ? 'Downloaded' : 'Copied!') : 'Failed';
        setTimeout(() => { buttonEl.textContent = idleLabel; }, 1600);
    };

    const fallbackDownload = (blob) => {
        window.ThaiXDownloadBlob(blob, 'diagram.png');
        done(true, true);
    };

    // ClipboardItem image write requires a secure context (HTTPS or localhost).
    if (canCopy) {
        try {
            const item = new ClipboardItem({ 'image/png': pngPromise });
            navigator.clipboard.write([item]).then(() => done(true, false)).catch(async () => {
                try {
                    const blob = await pngPromise;
                    if (await window.ThaiXTryCopyPngViaDom(blob)) {
                        done(true, false);
                        return;
                    }
                    fallbackDownload(blob);
                } catch {
                    done(false, false);
                }
            });
            return;
        } catch (e) {
            console.warn('ClipboardItem sync write failed', e);
        }
    }

    // HTTP / non-secure: try DOM copy first, then download (browser blocks image clipboard API).
    pngPromise.then(async (blob) => {
        try {
            if (await window.ThaiXTryCopyPngViaDom(blob)) {
                done(true, false);
                return;
            }
        } catch (e) {
            console.warn('DOM image copy failed', e);
        }
        fallbackDownload(blob);
    }).catch((e) => {
        console.error(e);
        done(false, false);
    });
};
