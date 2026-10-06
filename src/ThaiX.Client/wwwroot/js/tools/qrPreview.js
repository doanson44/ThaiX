/**
 * Download the QR SVG as a PNG file. Blazor WASM cannot rasterize SVG in C#
 * (the QrCodeGenerator PNG API needs System.Drawing), so draw it on a canvas
 * with a white background for scanner contrast and trigger a download.
 */
window.ThaiXDownloadQrPng = function (svgString, fileName) {
    return new Promise(function (resolve, reject) {
        var blob;
        var url;
        try {
            blob = new Blob([svgString || ''], { type: 'image/svg+xml;charset=utf-8' });
            url = URL.createObjectURL(blob);
        } catch (e) {
            reject(e);
            return;
        }

        var img = new Image();
        img.onload = function () {
            try {
                // The QR viewBox is square; read it to export at ~1024px.
                var vw = 1024;
                var vh = 1024;
                var vb = /viewBox="([^"]+)"/.exec(svgString);
                if (vb && vb[1]) {
                    var parts = vb[1].trim().split(/[\s,]+/).map(Number);
                    if (parts.length === 4 && parts[2] > 0 && parts[3] > 0) {
                        vw = parts[2];
                        vh = parts[3];
                    }
                }
                var scale = 1024 / Math.max(vw, vh);
                var w = Math.round(vw * scale);
                var h = Math.round(vh * scale);

                var canvas = document.createElement('canvas');
                canvas.width = w;
                canvas.height = h;
                var ctx = canvas.getContext('2d');
                ctx.fillStyle = '#ffffff';
                ctx.fillRect(0, 0, w, h);
                ctx.drawImage(img, 0, 0, w, h);
                URL.revokeObjectURL(url);

                canvas.toBlob(function (pngBlob) {
                    if (!pngBlob) {
                        reject(new Error('PNG encoding failed'));
                        return;
                    }
                    var dlUrl = URL.createObjectURL(pngBlob);
                    var a = document.createElement('a');
                    a.href = dlUrl;
                    a.download = fileName || 'qr.png';
                    document.body.appendChild(a);
                    a.click();
                    setTimeout(function () {
                        URL.revokeObjectURL(dlUrl);
                        a.remove();
                    }, 200);
                    resolve(true);
                }, 'image/png');
            } catch (e) {
                URL.revokeObjectURL(url);
                reject(e);
            }
        };
        img.onerror = function () {
            URL.revokeObjectURL(url);
            reject(new Error('QR SVG could not be loaded'));
        };
        img.src = url;
    });
};
