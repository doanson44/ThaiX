// Audio playback for the Frontline Tower Defense port. The game simulation is C#, so the
// browser Audio elements are the only scripted part: they are driven exclusively through
// FrontlineAudioService interop calls.
const masterVolume = 0.5;
const poolSize = 4;
const pools = new Map();
let muted = false;

function getPool(file) {
    let entry = pools.get(file);
    if (!entry) {
        entry = [];
        for (let index = 0; index < poolSize; index++) {
            const audio = new Audio(file);
            audio.preload = "none";
            entry.push(audio);
        }
        pools.set(file, entry);
    }
    return entry;
}

function start(audio, volume) {
    audio.volume = Math.min(1, masterVolume * (volume ?? 1));
    const promise = audio.play();
    if (promise && promise.catch) {
        promise.catch(() => { });
    }
}

export function play(file, volume) {
    if (muted || !file)
        return;

    try {
        const entry = getPool(file);
        const audio = entry.find(item => item.paused || item.ended) ?? entry[0];
        audio.currentTime = 0;
        start(audio, volume);
    } catch {
        // Audio is best effort: a blocked or unsupported element must not break the game.
    }
}

export function loop(file, volume) {
    if (!file)
        return;

    try {
        const audio = getPool(file)[0];
        audio.loop = true;
        if (!muted)
            start(audio, volume);
    } catch {
    }
}

export function stopLoop(file) {
    const entry = pools.get(file);
    if (!entry)
        return;

    for (const audio of entry) {
        audio.pause();
        audio.currentTime = 0;
    }
}

export function stopAll() {
    for (const entry of pools.values()) {
        for (const audio of entry) {
            audio.pause();
            audio.currentTime = 0;
        }
    }
}

export function setMuted(value) {
    muted = !!value;
    if (muted)
        stopAll();
}
