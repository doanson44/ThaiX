// Pointer tracking for the Frontline Tower Defense board. Blazor mouse events only report
// offsets relative to the clicked grid cell, but the legacy runtime aims and fires with the
// exact stage coordinates, so this module forwards the mapped 800x480 stage position.
const handlers = new WeakMap();

function mapEvent(element, event) {
    const rect = element.getBoundingClientRect();
    if (!rect.width || !rect.height)
        return null;

    return {
        x: (event.clientX - rect.left) / rect.width * 800,
        y: (event.clientY - rect.top) / rect.height * 480
    };
}

export function attach(element, dotnetReference) {
    detach(element);

    const move = event => {
        const point = mapEvent(element, event);
        if (point)
            dotnetReference.invokeMethodAsync("OnFrontlinePointerMove", point.x, point.y);
    };

    const click = event => {
        const point = mapEvent(element, event);
        if (point)
            dotnetReference.invokeMethodAsync("OnFrontlinePointerClick", point.x, point.y);
    };

    element.addEventListener("mousemove", move);
    element.addEventListener("click", click);
    handlers.set(element, { move, click });
}

export function detach(element) {
    const handler = handlers.get(element);
    if (!handler)
        return;

    element.removeEventListener("mousemove", handler.move);
    element.removeEventListener("click", handler.click);
    handlers.delete(element);
}
