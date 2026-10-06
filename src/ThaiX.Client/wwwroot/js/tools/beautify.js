window.ThaiXBeautify = async function (type, text) {
    const src = text ?? '';
    const t = (type || '').toLowerCase();
    if (t === 'json') {
        try { return JSON.stringify(JSON.parse(src), null, 2); } catch { return src; }
    }
    // Lightweight indent for html/css/js/sql without extra deps
    if (t === 'html' || t === 'xml') {
        return src
            .replace(/>\s*</g, '>\n<')
            .split('\n')
            .map(l => l.trim())
            .filter(Boolean)
            .join('\n');
    }
    if (t === 'css' || t === 'js' || t === 'javascript') {
        return src
            .replace(/;\s*/g, ';\n')
            .replace(/\{\s*/g, '{\n')
            .replace(/\}\s*/g, '\n}\n');
    }
    return src;
};
