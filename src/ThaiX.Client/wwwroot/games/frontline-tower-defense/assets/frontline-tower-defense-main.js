let E;
const _t = /* @__PURE__ */ new Map(), Bt = /* @__PURE__ */ new Map();
let gt = null;
function L(s) {
  return E.chars[s];
}
function tt() {
  return gt || (gt = new AudioContext()), gt;
}
function Qt(s) {
  return new Promise((t, e) => {
    const i = new Image();
    i.onload = () => t(i), i.onerror = () => e(new Error("image " + s)), i.src = s;
  });
}
async function Zt(s, t) {
  E = await (await fetch(s + "library.json")).json();
  const e = [], i = [];
  for (const u in E.chars) {
    const d = E.chars[u];
    d.k === "bitmap" ? e.push(+u) : d.k === "sound" && i.push(+u);
  }
  const n = e.length + i.length;
  let a = 0;
  const r = () => t(++a, n), o = e.slice(), h = async () => {
    for (; o.length; ) {
      const u = o.shift();
      _t.set(u, await Qt(`${s}img/${u}.png`)), r();
    }
  }, l = tt(), c = i.map(async (u) => {
    const d = E.chars[u], g = await (await fetch(s + d.src)).arrayBuffer();
    try {
      Bt.set(u, await l.decodeAudioData(g));
    } catch (b) {
      console.warn("sound decode failed", u, b);
    }
    r();
  });
  await Promise.all([...Array.from({ length: 12 }, h), ...c]);
}
function Q(s, t = 1) {
  return s ? `rgba(${s[0]},${s[1]},${s[2]},${s[3] * t})` : "rgba(0,0,0,1)";
}
function G(s, t = 1) {
  const e = s >>> 16 & 255, i = s >>> 8 & 255, n = s & 255;
  return `rgba(${e},${i},${n},${t})`;
}
const St = /* @__PURE__ */ new WeakMap();
function ht(s) {
  let t = St.get(s);
  return t || (t = new Path2D(s.d), St.set(s, t)), t;
}
const J = 819.2, Pt = /* @__PURE__ */ new WeakMap();
function te(s, t, e) {
  const i = ht(e);
  if (t.t === "s") {
    s.fillStyle = Q(t.c), s.fill(i, "evenodd");
    return;
  }
  if (t.t === "b") {
    const o = _t.get(t.id);
    if (!o) return;
    const h = s.createPattern(o, t.rep ? "repeat" : "no-repeat");
    if (!h) return;
    h.setTransform(new DOMMatrix(t.m)), s.imageSmoothingEnabled = t.smooth !== !1, s.fillStyle = h, s.fill(i, "evenodd"), s.imageSmoothingEnabled = !0;
    return;
  }
  const n = new DOMMatrix(t.m);
  let a = Pt.get(e);
  a || (a = new Path2D(), a.addPath(i, n.inverse()), Pt.set(e, a));
  let r;
  t.t === "lg" ? r = s.createLinearGradient(-J, 0, J, 0) : t.t === "rg" ? r = s.createRadialGradient(0, 0, 0, 0, 0, J) : r = s.createRadialGradient((t.focal || 0) * J, 0, 0, 0, 0, J);
  for (const [o, h] of t.stops) r.addColorStop(Math.min(1, Math.max(0, o)), Q(h));
  s.save(), s.transform(t.m[0], t.m[1], t.m[2], t.m[3], t.m[4], t.m[5]), s.fillStyle = r, s.fill(a, "evenodd"), s.restore();
}
function ee(s, t) {
  if (t.img !== void 0) {
    const e = _t.get(t.img);
    if (!e) return;
    const i = t.m;
    s.save(), s.transform(i[0], i[1], i[2], i[3], i[4], i[5]), s.imageSmoothingEnabled = t.smooth !== !1, s.drawImage(e, 0, 0), s.restore();
    return;
  }
  if (t.f) for (const e of t.f) te(s, e.s, e);
  if (t.l)
    for (const e of t.l)
      s.lineWidth = Math.max(e.s.w, 1), s.strokeStyle = Q(e.s.c), s.lineCap = e.s.cap || "round", s.lineJoin = e.s.join || "round", e.s.miter && (s.miterLimit = e.s.miter), s.stroke(ht(e));
}
let mt = null;
function ie() {
  return mt || (mt = document.createElement("canvas").getContext("2d")), mt;
}
function se(s, t, e) {
  const i = s.b;
  if (t < i[0] || e < i[1] || t > i[2] || e > i[3]) return !1;
  if (s.img !== void 0) return !0;
  const n = ie();
  if (s.f) {
    for (const a of s.f) if (n.isPointInPath(ht(a), t, e, "evenodd")) return !0;
  }
  if (s.l) {
    for (const a of s.l)
      if (n.lineWidth = Math.max(a.s.w, 1), n.isPointInStroke(ht(a), t, e)) return !0;
  }
  return !1;
}
function $t(s, t) {
  s._paths || (s._paths = new Array(s.glyphs.length).fill(void 0));
  let e = s._paths[t];
  if (e === void 0) {
    const i = s.glyphs[t];
    e = i ? new Path2D(i) : null, s._paths[t] = e;
  }
  return e;
}
function jt(s, t) {
  s._map || (s._map = /* @__PURE__ */ new Map(), s.codes.forEach((i, n) => s._map.set(i, n)));
  const e = s._map.get(t);
  return e === void 0 ? -1 : e;
}
function ne(s, t) {
  const e = t.m;
  s.save(), s.transform(e[0], e[1], e[2], e[3], e[4], e[5]);
  for (const i of t.r) {
    const n = L(i.font);
    if (!n || n.k !== "font") continue;
    const a = i.h / 1024;
    s.fillStyle = Q(i.c);
    let r = i.x;
    for (const [o, h] of i.g) {
      const l = $t(n, o);
      l && (s.save(), s.translate(r, i.y), s.scale(a, a), s.fill(l, "evenodd"), s.restore()), r += h;
    }
  }
  s.restore();
}
function re(s) {
  return s.replace(/&(lt|gt|amp|quot|apos|nbsp|#\d+);/g, (t, e) => {
    switch (e) {
      case "lt":
        return "<";
      case "gt":
        return ">";
      case "amp":
        return "&";
      case "quot":
        return '"';
      case "apos":
        return "'";
      case "nbsp":
        return " ";
      default:
        return String.fromCharCode(+e.slice(1));
    }
  });
}
function Lt(s, t) {
  const e = /<p[^>]*align="(left|right|center|justify)"/i.exec(s);
  e && (t.align = { left: 0, right: 1, center: 2, justify: 3 }[e[1].toLowerCase()]);
  const i = /<font[^>]*color="#([0-9a-f]{6})"/i.exec(s);
  if (i) {
    const r = parseInt(i[1], 16);
    t.color = [r >> 16 & 255, r >> 8 & 255, r & 255, 1];
  }
  const n = /<font[^>]*size="(\d+)"/i.exec(s);
  n && (t.size = +n[1]);
  let a = s.replace(/<br\s*\/?>/gi, `
`).replace(/<\/p>/gi, `
`).replace(/<[^>]+>/g, "");
  return a = re(a), a.replace(/\n$/, "");
}
function ae(s, t) {
  const e = t.font !== void 0 ? L(t.font) : void 0;
  if (e && e.k === "font" && e.glyphs.length) {
    const i = t.size / 1024;
    return (n) => {
      let a = 0;
      for (let r = 0; r < n.length; r++) {
        const o = jt(e, n.charCodeAt(r));
        o >= 0 ? a += e.adv[o] * i : a += t.size * 0.3;
      }
      return a;
    };
  }
  return s.font = Ut(t), (i) => s.measureText(i).width;
}
function Ut(s) {
  return `${s.bold ? "bold " : ""}${s.size}px "${s.face || "Arial"}", Arial, sans-serif`;
}
function oe(s, t, e, i, n, a) {
  const r = ae(s, e), o = t.replace(/\r\n/g, `
`).replace(/\r/g, `
`).split(`
`), h = [], l = i - e.leftMargin - e.rightMargin;
  for (const c of a ? o : [o.join(" ")]) {
    if (!n) {
      h.push({ text: c, width: r(c) });
      continue;
    }
    const u = c.split(/(\s+)/);
    let d = "";
    for (const g of u) {
      const b = d + g;
      d && r(b.trimEnd()) > l - (h.length === 0 ? e.indent : 0) ? (h.push({ text: d.trimEnd(), width: r(d.trimEnd()) }), d = g.trimStart()) : d = b;
    }
    h.push({ text: d.trimEnd(), width: r(d.trimEnd()) });
  }
  return h;
}
function le(s) {
  const t = s.font !== void 0 ? L(s.font) : void 0;
  if (t && t.k === "font" && t.ascent) {
    const e = s.size / 1024;
    return { ascent: t.ascent * e, descent: t.descent * e };
  }
  return { ascent: s.size * 0.9, descent: s.size * 0.22 };
}
function he(s, t, e, i, n = 1) {
  const a = t.b, r = a[0] + 2, o = a[1] + 2, h = a[2] - a[0] - 4, l = oe(s, e, i, h, t.wordWrap, t.multiline), { ascent: c, descent: u } = le(i), d = c + u + i.leading, g = i.font !== void 0 ? L(i.font) : void 0, b = !!(g && g.k === "font" && g.glyphs.length && t.outlines);
  s.save(), s.beginPath(), s.rect(a[0], a[1], a[2] - a[0], a[3] - a[1]), s.clip(), s.fillStyle = Q(i.color, n), b || (s.font = Ut(i), s.textBaseline = "alphabetic");
  let D = o + c;
  l.forEach((m, j) => {
    let x = r + i.leftMargin + (j === 0 ? i.indent : 0);
    const v = h - i.leftMargin - i.rightMargin;
    if (i.align === 1 ? x += v - m.width : i.align === 2 && (x += (v - m.width) / 2), b) {
      const T = i.size / 1024;
      for (let R = 0; R < m.text.length; R++) {
        const f = jt(g, m.text.charCodeAt(R));
        if (f < 0) {
          x += i.size * 0.3;
          continue;
        }
        const w = $t(g, f);
        w && (s.save(), s.translate(x, D), s.scale(T, T), s.fill(w, "evenodd"), s.restore()), x += g.adv[f] * T;
      }
    } else
      s.fillText(m.text, x, D);
    D += d;
  }), s.restore(), t.border && (s.strokeStyle = "#000", s.lineWidth = 1, s.strokeRect(a[0], a[1], a[2] - a[0], a[3] - a[1]));
}
function Me(s) {
  return +s | 0;
}
function xe(s) {
  return +s >>> 0;
}
function Ee(s, t) {
  if (t == null) return null;
  if (t instanceof s) return t;
  throw new TypeError(`Type Coercion failed: cannot convert ${t?.constructor?.name} to ${s?.name}`);
}
function xt(s, t) {
  console.error(`[${s}]`, t);
}
const Rt = /* @__PURE__ */ new WeakMap();
function Wt(s) {
  let t = Rt.get(Object.getPrototypeOf(s));
  if (!t) {
    const e = /* @__PURE__ */ new Set();
    let i = Object.getPrototypeOf(s);
    for (; i && i !== Object.prototype && !Jt.has(i); ) {
      for (const n of Object.getOwnPropertyNames(i)) {
        if (n === "constructor" || n.startsWith("__fi")) continue;
        typeof Object.getOwnPropertyDescriptor(i, n).value == "function" && e.add(n);
      }
      i = Object.getPrototypeOf(i);
    }
    t = [...e], Rt.set(Object.getPrototypeOf(s), t);
  }
  for (const e of t) s[e] = s[e].bind(s);
}
function ce(s) {
  const t = [];
  let e = Object.getPrototypeOf(s);
  for (; e && e !== Object.prototype && !Jt.has(e); )
    Object.prototype.hasOwnProperty.call(e, "__fi") && t.push(e.__fi), e = Object.getPrototypeOf(e);
  for (let i = t.length - 1; i >= 0; i--) t[i].call(s);
}
const Et = /* @__PURE__ */ new Map(), zt = /* @__PURE__ */ new Map();
function Ce(s, t) {
  const e = E.symbols[t];
  e !== void 0 && (Et.set(s, e), zt.set(e, s));
}
function Ct(s) {
  let t = s;
  for (; t && t !== Function.prototype; ) {
    const e = Et.get(t);
    if (e !== void 0) return e;
    t = Object.getPrototypeOf(t);
  }
}
class P {
  constructor(t = 0, e = 0) {
    this.x = t, this.y = e;
  }
  get length() {
    return Math.sqrt(this.x * this.x + this.y * this.y);
  }
  add(t) {
    return new P(this.x + t.x, this.y + t.y);
  }
  subtract(t) {
    return new P(this.x - t.x, this.y - t.y);
  }
  clone() {
    return new P(this.x, this.y);
  }
  offset(t, e) {
    this.x += t, this.y += e;
  }
  equals(t) {
    return t.x === this.x && t.y === this.y;
  }
  normalize(t) {
    const e = this.length;
    e > 0 && (this.x *= t / e, this.y *= t / e);
  }
  static distance(t, e) {
    return Math.hypot(t.x - e.x, t.y - e.y);
  }
  static interpolate(t, e, i) {
    return new P(e.x + (t.x - e.x) * i, e.y + (t.y - e.y) * i);
  }
  toString() {
    return `(x=${this.x}, y=${this.y})`;
  }
}
class Z {
  constructor(t = 0, e = 0, i = 0, n = 0) {
    this.x = t, this.y = e, this.width = i, this.height = n;
  }
  get left() {
    return this.x;
  }
  get top() {
    return this.y;
  }
  get right() {
    return this.x + this.width;
  }
  get bottom() {
    return this.y + this.height;
  }
  clone() {
    return new Z(this.x, this.y, this.width, this.height);
  }
  contains(t, e) {
    return t >= this.x && e >= this.y && t < this.x + this.width && e < this.y + this.height;
  }
  containsPoint(t) {
    return this.contains(t.x, t.y);
  }
  intersects(t) {
    return t.x < this.right && t.right > this.x && t.y < this.bottom && t.bottom > this.y;
  }
  isEmpty() {
    return this.width <= 0 || this.height <= 0;
  }
  setTo(t, e, i, n) {
    this.x = t, this.y = e, this.width = i, this.height = n;
  }
}
function yt(s, t) {
  return [
    s[0] * t[0] + s[2] * t[1],
    s[1] * t[0] + s[3] * t[1],
    s[0] * t[2] + s[2] * t[3],
    s[1] * t[2] + s[3] * t[3],
    s[0] * t[4] + s[2] * t[5] + s[4],
    s[1] * t[4] + s[3] * t[5] + s[5]
  ];
}
function U(s) {
  const t = s[0] * s[3] - s[1] * s[2];
  return t ? [
    s[3] / t,
    -s[1] / t,
    -s[2] / t,
    s[0] / t,
    (s[2] * s[5] - s[3] * s[4]) / t,
    (s[1] * s[4] - s[0] * s[5]) / t
  ] : [0, 0, 0, 0, 0, 0];
}
function k(s, t, e) {
  return [s[0] * t + s[2] * e + s[4], s[1] * t + s[3] * e + s[5]];
}
function pt(s, t) {
  return s ? t ? [Math.min(s[0], t[0]), Math.min(s[1], t[1]), Math.max(s[2], t[2]), Math.max(s[3], t[3])] : s : t;
}
function N(s, t) {
  if (!t) return null;
  const e = [k(s, t[0], t[1]), k(s, t[2], t[1]), k(s, t[0], t[3]), k(s, t[2], t[3])];
  return [
    Math.min(...e.map((i) => i[0])),
    Math.min(...e.map((i) => i[1])),
    Math.max(...e.map((i) => i[0])),
    Math.max(...e.map((i) => i[1]))
  ];
}
class C {
  constructor(t, e = !1, i = !1) {
    this.type = t, this.bubbles = e, this.cancelable = i, this.target = null, this.currentTarget = null, this.eventPhase = 2, this._stop = !1, this._stopNow = !1;
  }
  static {
    this.ENTER_FRAME = "enterFrame";
  }
  static {
    this.EXIT_FRAME = "exitFrame";
  }
  static {
    this.FRAME_CONSTRUCTED = "frameConstructed";
  }
  static {
    this.ADDED = "added";
  }
  static {
    this.ADDED_TO_STAGE = "addedToStage";
  }
  static {
    this.REMOVED = "removed";
  }
  static {
    this.REMOVED_FROM_STAGE = "removedFromStage";
  }
  static {
    this.COMPLETE = "complete";
  }
  static {
    this.SOUND_COMPLETE = "soundComplete";
  }
  static {
    this.CHANGE = "change";
  }
  static {
    this.RESIZE = "resize";
  }
  static {
    this.ACTIVATE = "activate";
  }
  static {
    this.DEACTIVATE = "deactivate";
  }
  stopPropagation() {
    this._stop = !0;
  }
  stopImmediatePropagation() {
    this._stop = !0, this._stopNow = !0;
  }
  preventDefault() {
  }
  isDefaultPrevented() {
    return !1;
  }
  clone() {
    return new C(this.type, this.bubbles, this.cancelable);
  }
}
class O extends C {
  constructor() {
    super(...arguments), this.localX = 0, this.localY = 0, this.stageX = 0, this.stageY = 0, this.buttonDown = !1, this.shiftKey = !1, this.ctrlKey = !1, this.altKey = !1, this.delta = 0, this.relatedObject = null;
  }
  static {
    this.CLICK = "click";
  }
  static {
    this.DOUBLE_CLICK = "doubleClick";
  }
  static {
    this.MOUSE_DOWN = "mouseDown";
  }
  static {
    this.MOUSE_UP = "mouseUp";
  }
  static {
    this.MOUSE_MOVE = "mouseMove";
  }
  static {
    this.MOUSE_OVER = "mouseOver";
  }
  static {
    this.MOUSE_OUT = "mouseOut";
  }
  static {
    this.ROLL_OVER = "rollOver";
  }
  static {
    this.ROLL_OUT = "rollOut";
  }
  static {
    this.MOUSE_WHEEL = "mouseWheel";
  }
  updateAfterEvent() {
  }
}
class st extends C {
  static {
    this.TIMER = "timer";
  }
  static {
    this.TIMER_COMPLETE = "timerComplete";
  }
  updateAfterEvent() {
  }
}
class Te extends C {
  constructor() {
    super(...arguments), this.keyCode = 0, this.charCode = 0, this.shiftKey = !1, this.ctrlKey = !1, this.altKey = !1;
  }
  static {
    this.KEY_DOWN = "keyDown";
  }
  static {
    this.KEY_UP = "keyUp";
  }
}
const ct = /* @__PURE__ */ new Set();
class H {
  constructor() {
    this._listeners = /* @__PURE__ */ new Map();
  }
  addEventListener(t, e, i = !1, n = 0, a = !1) {
    if (!e) return;
    let r = this._listeners.get(t);
    r || this._listeners.set(t, r = []), !r.some((o) => o.fn === e) && (r.push({ fn: e, prio: n, weak: a }), r.sort((o, h) => h.prio - o.prio), t === C.ENTER_FRAME && this instanceof $ && ct.add(this));
  }
  removeEventListener(t, e, i = !1) {
    const n = this._listeners.get(t);
    if (!n) return;
    const a = n.findIndex((r) => r.fn === e);
    a >= 0 && n.splice(a, 1), t === C.ENTER_FRAME && !n.length && this instanceof $ && ct.delete(this);
  }
  hasEventListener(t) {
    const e = this._listeners.get(t);
    return !!e && e.length > 0;
  }
  willTrigger(t) {
    return this.hasEventListener(t);
  }
  _fire(t) {
    const e = this._listeners.get(t.type);
    if (!(!e || !e.length)) {
      t.currentTarget = this;
      for (const i of e.slice()) {
        try {
          i.fn.call(this, t);
        } catch (n) {
          xt(`${this.constructor.name}.${t.type}`, n);
        }
        if (t._stopNow) break;
      }
    }
  }
  dispatchEvent(t) {
    return t.target || (t.target = this), this._fire(t), !0;
  }
}
let B = null, ue = 0, Tt = 0, bt = null;
const At = {
  add: "lighter",
  multiply: "multiply",
  screen: "screen",
  lighten: "lighten",
  darken: "darken",
  difference: "difference",
  overlay: "overlay",
  hardlight: "hard-light"
};
class $ extends H {
  constructor() {
    super();
    const t = B;
    B = null, this._setup();
    let e;
    t ? (e = t.st.id, this._depth = t.depth, this._placeOp = t.st.create, this._applyPlace(t.st, !0), t.parent._insertTimelineChild(this)) : e = Ct(new.target), ce(this), Wt(this), this._init(e);
  }
  _setup() {
    this._parent = null, this._a = 1, this._b = 0, this._c = 0, this._d = 1, this._tx = 0, this._ty = 0, this._alpha = 1, this._cx = null, this._visible = !0, this._name = "instance" + ++ue, this._filters = null, this._blend = "normal", this._depth = void 0, this._placeOp = null, this._scriptMoved = !1, this._dirty = !0, this._born = Tt, this._layer = null, this._layerPool = null, this.cacheAsBitmap = !1, this.opaqueBackground = null, this.scrollRect = null, this.mask = null;
  }
  _init(t) {
  }
  _applyPlace(t, e) {
    if (t.m ? [this._a, this._b, this._c, this._d, this._tx, this._ty] = t.m : e && (this._a = 1, this._b = 0, this._c = 0, this._d = 1, this._tx = 0, this._ty = 0), t.cx || e) {
      const i = t.cx;
      i && (i[0] !== 1 || i[1] !== 1 || i[2] !== 1 || i[4] || i[5] || i[6] || i[7]) ? this._cx = i : this._cx = null, this._alpha = i ? i[3] : 1;
    }
    t.n !== void 0 && (this._name = t.n), (t.f !== void 0 || e) && (this._filters = t.f && t.f.length ? t.f : null), (t.bm !== void 0 || e) && (this._blend = t.bm || "normal"), t.vis !== void 0 && (this._visible = !!t.vis), this.invalidate();
  }
  invalidate() {
    let t = this;
    for (; t && !t._dirty; )
      t._dirty = !0, t = t._parent;
    for (t = this._parent; t; )
      t._dirty = !0, t = t._parent;
  }
  // ---------------------------------------------------------------- properties
  get parent() {
    return this._parent;
  }
  get name() {
    return this._name;
  }
  set name(t) {
    this._name = t;
  }
  get stage() {
    let t = this;
    for (; t; ) {
      if (t instanceof W) return t;
      t = t._parent;
    }
    return null;
  }
  get root() {
    let t = this;
    for (; t._parent && !(t._parent instanceof W); ) t = t._parent;
    return t._parent ? t : this instanceof W ? this : t;
  }
  get loaderInfo() {
    return { bytesLoaded: 1, bytesTotal: 1, url: location.href, parameters: {} };
  }
  get x() {
    return this._tx;
  }
  set x(t) {
    t = +t, !isNaN(t) && (this._tx = Math.round(t * 20) / 20, this._scriptMoved = !0, this.invalidate());
  }
  get y() {
    return this._ty;
  }
  set y(t) {
    t = +t, !isNaN(t) && (this._ty = Math.round(t * 20) / 20, this._scriptMoved = !0, this.invalidate());
  }
  get scaleX() {
    return this._b === 0 ? this._a : Math.sign(this._a || 1) * Math.hypot(this._a, this._b);
  }
  set scaleX(t) {
    if (t = +t, isNaN(t)) return;
    const e = this.scaleX;
    this._b === 0 || e === 0 ? (this._a = t, this._b = 0) : (this._a *= t / e, this._b *= t / e), this._scriptMoved = !0, this.invalidate();
  }
  get scaleY() {
    return this._c === 0 ? this._d : Math.sign(this._d || 1) * Math.hypot(this._c, this._d);
  }
  set scaleY(t) {
    if (t = +t, isNaN(t)) return;
    const e = this.scaleY;
    this._c === 0 || e === 0 ? (this._d = t, this._c = 0) : (this._c *= t / e, this._d *= t / e), this._scriptMoved = !0, this.invalidate();
  }
  get rotation() {
    return Math.atan2(this._b, this._a) * 180 / Math.PI;
  }
  set rotation(t) {
    if (t = +t, isNaN(t)) return;
    const e = Math.hypot(this._a, this._b), i = Math.hypot(this._c, this._d), n = t % 360 * Math.PI / 180;
    this._a = Math.cos(n) * e, this._b = Math.sin(n) * e, this._c = -Math.sin(n) * i, this._d = Math.cos(n) * i, this._scriptMoved = !0, this.invalidate();
  }
  get alpha() {
    return this._alpha;
  }
  set alpha(t) {
    t = +t, !isNaN(t) && (this._alpha = t, this._scriptMoved = !0, this.invalidate());
  }
  get visible() {
    return this._visible;
  }
  set visible(t) {
    this._visible = !!t, this.invalidate();
  }
  get blendMode() {
    return this._blend;
  }
  set blendMode(t) {
    this._blend = t, this.invalidate();
  }
  get filters() {
    return this._filters ? this._filters.slice() : [];
  }
  set filters(t) {
    this._filters = t && t.length ? t.slice() : null, this.invalidate();
  }
  get width() {
    const t = N(this._matrix(), this._localBounds());
    return t ? t[2] - t[0] : 0;
  }
  set width(t) {
    const e = this._localBounds(), i = e ? e[2] - e[0] : 0;
    i > 0 && (this.scaleX = t / i);
  }
  get height() {
    const t = N(this._matrix(), this._localBounds());
    return t ? t[3] - t[1] : 0;
  }
  set height(t) {
    const e = this._localBounds(), i = e ? e[3] - e[1] : 0;
    i > 0 && (this.scaleY = t / i);
  }
  get mouseX() {
    return this.globalToLocal(new P(y.x, y.y)).x;
  }
  get mouseY() {
    return this.globalToLocal(new P(y.x, y.y)).y;
  }
  _matrix() {
    return [this._a, this._b, this._c, this._d, this._tx, this._ty];
  }
  _world() {
    let t = this._matrix(), e = this._parent;
    for (; e; )
      t = yt(e._matrix(), t), e = e._parent;
    return t;
  }
  localToGlobal(t) {
    const [e, i] = k(this._world(), t.x, t.y);
    return new P(e, i);
  }
  globalToLocal(t) {
    const [e, i] = k(U(this._world()), t.x, t.y);
    return new P(e, i);
  }
  getBounds(t) {
    const e = N(t ? yt(U(t._world()), this._world()) : this._matrix(), this._localBounds());
    return e ? new Z(e[0], e[1], e[2] - e[0], e[3] - e[1]) : new Z();
  }
  getRect(t) {
    return this.getBounds(t);
  }
  hitTestPoint(t, e, i = !1) {
    if (!i) {
      const r = N(this._world(), this._localBounds());
      return !!r && t >= r[0] && t <= r[2] && e >= r[1] && e <= r[3];
    }
    const [n, a] = k(U(this._world()), t, e);
    return this._hitLocal(n, a);
  }
  hitTestObject(t) {
    const e = N(this._world(), this._localBounds()), i = N(t._world(), t._localBounds());
    return !!e && !!i && e[0] < i[2] && e[2] > i[0] && e[1] < i[3] && e[3] > i[1];
  }
  // ---------------------------------------------------------------- dispatch
  dispatchEvent(t) {
    if (t.target || (t.target = this), t.eventPhase = 2, this._fire(t), t.bubbles && !t._stop) {
      let e = this._parent;
      for (t.eventPhase = 3; e && !t._stop; )
        e._fire(t), e = e._parent;
    }
    return !0;
  }
  // ---------------------------------------------------------------- render
  _localBounds() {
    return null;
  }
  _hitLocal(t, e) {
    return !1;
  }
  _draw(t) {
  }
  _render(t) {
    if (!this._visible) return;
    const e = this._alpha;
    if (!(e <= 0 && !this._filters)) {
      if (t.save(), t.transform(this._a, this._b, this._c, this._d, this._tx, this._ty), this._filters || this._cx)
        this._renderLayer(t, e);
      else {
        e < 1 && (t.globalAlpha *= e);
        const i = At[this._blend];
        i && (t.globalCompositeOperation = i), this._draw(t);
      }
      t.restore(), this._dirty = !1;
    }
  }
  /** Render into an offscreen bitmap so filters and colour transforms apply to
   * the composite, like Flash does. Re-used while the subtree is unchanged. */
  _renderLayer(t, e) {
    const i = this._localBounds();
    if (!i) return;
    const n = t.getTransform(), a = [n.a, n.b, n.c, n.d, n.e, n.f], r = Math.sqrt(Math.abs(n.a * n.d - n.b * n.c)) || 1;
    let o = 0;
    for (const m of this._filters || [])
      (m.t === "glow" || m.t === "drop" || m.t === "blur") && (o = Math.max(o, (Math.max(m.bx, m.by) * 1.5 + (m.dist || 0)) * r));
    const h = N(a, i), l = Math.floor(h[0] - o - 1), c = Math.floor(h[1] - o - 1), u = Math.min(4096, Math.ceil(h[2] + o + 1) - l), d = Math.min(4096, Math.ceil(h[3] + o + 1) - c);
    if (u <= 0 || d <= 0) return;
    const g = [n.a, n.b, n.c, n.d, (n.e - l).toFixed(2), (n.f - c).toFixed(2), u, d].join(",");
    let b = this._layer;
    if (!b || b.key !== g || this._dirty) {
      const m = this._layerPool || (this._layerPool = []);
      let j = 0;
      const x = () => {
        let f = m[j];
        if (!f) {
          const w = document.createElement("canvas");
          f = m[j] = w.getContext("2d", { willReadFrequently: !0 });
        }
        return j++, f.canvas.width !== u || f.canvas.height !== d ? (f.canvas.width = u, f.canvas.height = d) : (f.setTransform(1, 0, 0, 1, 0, 0), f.clearRect(0, 0, u, d)), f;
      };
      let v = x();
      v.setTransform(n.a, n.b, n.c, n.d, n.e - l, n.f - c), this._draw(v), v.setTransform(1, 0, 0, 1, 0, 0);
      const T = 1e5;
      for (const f of this._filters || []) {
        if (f.t === "cm") {
          de(v, u, d, f.m);
          continue;
        }
        if (f.t !== "glow" && f.t !== "drop" || f.inner) continue;
        const w = x(), I = f.c, q = Math.max(0, f.s ?? 1), Y = Math.max(1, Math.ceil(q));
        w.shadowColor = `rgba(${I[0]},${I[1]},${I[2]},${Math.min(1, I[3] * q / Y)})`, w.shadowBlur = Math.max(f.bx, f.by) * Math.sqrt(f.q || 1) * 0.6 * r, w.shadowOffsetX = (f.t === "drop" ? Math.cos(f.angle) * f.dist * r : 0) + T, w.shadowOffsetY = f.t === "drop" ? Math.sin(f.angle) * f.dist * r : 0;
        for (let V = 0; V < Y; V++) w.drawImage(v.canvas, -T, 0);
        w.shadowColor = "transparent", w.shadowBlur = 0, w.shadowOffsetX = 0, w.shadowOffsetY = 0, f.knockout || w.drawImage(v.canvas, 0, 0), v = w;
      }
      this._cx && fe(v, u, d, this._cx);
      const R = m.indexOf(v);
      R > 0 && ([m[0], m[R]] = [m[R], m[0]]), b = this._layer = { canvas: v.canvas, key: g, x: l, y: c };
    }
    t.save(), t.setTransform(1, 0, 0, 1, 0, 0), e < 1 && (t.globalAlpha *= e);
    const D = At[this._blend];
    D && (t.globalCompositeOperation = D), t.drawImage(b.canvas, b.x, b.y), t.restore();
  }
}
function fe(s, t, e, i) {
  const n = s.getImageData(0, 0, t, e), a = n.data;
  for (let r = 0; r < a.length; r += 4)
    a[r + 3] && (a[r] = a[r] * i[0] + i[4], a[r + 1] = a[r + 1] * i[1] + i[5], a[r + 2] = a[r + 2] * i[2] + i[6], a[r + 3] = a[r + 3] + i[7]);
  s.putImageData(n, 0, 0);
}
function de(s, t, e, i) {
  const n = s.getImageData(0, 0, t, e), a = n.data;
  for (let r = 0; r < a.length; r += 4) {
    const o = a[r], h = a[r + 1], l = a[r + 2], c = a[r + 3];
    c && (a[r] = i[0] * o + i[1] * h + i[2] * l + i[3] * c + i[4], a[r + 1] = i[5] * o + i[6] * h + i[7] * l + i[8] * c + i[9], a[r + 2] = i[10] * o + i[11] * h + i[12] * l + i[13] * c + i[14], a[r + 3] = i[15] * o + i[16] * h + i[17] * l + i[18] * c + i[19]);
  }
  s.putImageData(n, 0, 0);
}
class et extends $ {
  _setup() {
    super._setup(), this.mouseEnabled = !0, this.doubleClickEnabled = !1, this.tabEnabled = !1, this.focusRect = null;
  }
}
class it extends et {
  _setup() {
    super._setup(), this._children = [], this.mouseChildren = !0, this.tabChildren = !0;
  }
  get numChildren() {
    return this._children.length;
  }
  _insertTimelineChild(t) {
    const e = t._depth;
    let i = this._children.findIndex((n) => n._depth !== void 0 && n._depth > e);
    i < 0 && (i = this._children.length), this._children.splice(i, 0, t), t._parent = this, this.invalidate();
  }
  _detach(t) {
    t._parent && t._parent.removeChild(t);
  }
  addChild(t) {
    return this.addChildAt(t, this._children.length - (t._parent === this ? 1 : 0));
  }
  addChildAt(t, e) {
    if (!t) throw new TypeError("Parameter child must be non-null.");
    if (t === this) throw new Error("An object cannot be added as a child of itself.");
    if (t._parent) {
      const i = t._parent, n = i._children.indexOf(t);
      n >= 0 && i._children.splice(n, 1), i.invalidate();
    }
    return e = Math.max(0, Math.min(e, this._children.length)), this._children.splice(e, 0, t), t._parent = this, t._depth = void 0, t._placeOp = null, this.invalidate(), t;
  }
  removeChild(t) {
    const e = this._children.indexOf(t);
    if (e < 0) throw new Error("ArgumentError: Error #2025: The supplied DisplayObject must be a child of the caller.");
    return this.removeChildAt(e);
  }
  removeChildAt(t) {
    const e = this._children[t];
    if (!e) throw new RangeError("RangeError: Error #2006: The supplied index is out of bounds.");
    return this._children.splice(t, 1), e._parent = null, this.invalidate(), e;
  }
  removeChildren(t = 0, e = 2147483647) {
    const i = this._children.slice(t, e + 1);
    for (const n of i) this.removeChild(n);
  }
  getChildAt(t) {
    const e = this._children[t];
    if (!e) throw new RangeError("RangeError: Error #2006: The supplied index is out of bounds.");
    return e;
  }
  getChildIndex(t) {
    const e = this._children.indexOf(t);
    if (e < 0) throw new Error("ArgumentError: Error #2025: The supplied DisplayObject must be a child of the caller.");
    return e;
  }
  setChildIndex(t, e) {
    const i = this.getChildIndex(t);
    this._children.splice(i, 1), this._children.splice(Math.max(0, Math.min(e, this._children.length)), 0, t), this.invalidate();
  }
  swapChildrenAt(t, e) {
    const i = this.getChildAt(t), n = this.getChildAt(e);
    this._children[t] = n, this._children[e] = i, this.invalidate();
  }
  swapChildren(t, e) {
    this.swapChildrenAt(this.getChildIndex(t), this.getChildIndex(e));
  }
  contains(t) {
    let e = t;
    for (; e; ) {
      if (e === this) return !0;
      e = e._parent;
    }
    return !1;
  }
  getChildByName(t) {
    return this._children.find((e) => e._name === t) || null;
  }
  _localBounds() {
    let t = null;
    for (const e of this._children)
      e._visible && (t = pt(t, N(e._matrix(), e._localBounds())));
    return t;
  }
  _hitLocal(t, e) {
    for (const i of this._children) {
      if (!i._visible) continue;
      const [n, a] = k(U(i._matrix()), t, e);
      if (i._hitLocal(n, a)) return !0;
    }
    return !1;
  }
  _draw(t) {
    for (const e of this._children) e._render(t);
  }
}
class qt {
  constructor(t) {
    this._owner = t, this._cmds = [], this._bounds = null, this.lw = 0;
  }
  push(t, e = [], i = 0) {
    this._cmds.push(t);
    for (let n = 0; n < e.length; n += 2)
      this._bounds = pt(this._bounds, [e[n] - i, e[n + 1] - i, e[n] + i, e[n + 1] + i]);
    this._owner.invalidate();
  }
  clear() {
    this._cmds = [], this._bounds = null, this._owner.invalidate();
  }
  lineStyle(t = NaN, e = 0, i = 1, ...n) {
    this.lw = isNaN(t) ? 0 : Math.max(t, 0.5), this.push({ k: "ls", w: t, c: e >>> 0, a: i });
  }
  beginFill(t = 0, e = 1) {
    this.push({ k: "bf", c: t >>> 0, a: e });
  }
  endFill() {
    this.push({ k: "ef" });
  }
  moveTo(t, e) {
    this.push({ k: "mt", x: t, y: e }, [t, e], this.lw);
  }
  lineTo(t, e) {
    this.push({ k: "lt", x: t, y: e }, [t, e], this.lw);
  }
  curveTo(t, e, i, n) {
    this.push({ k: "ct", cx: t, cy: e, x: i, y: n }, [t, e, i, n], this.lw);
  }
  drawRect(t, e, i, n) {
    this.push({ k: "rect", x: t, y: e, w: i, h: n }, [t, e, t + i, e + n], this.lw);
  }
  drawCircle(t, e, i) {
    this.push({ k: "circ", x: t, y: e, r: i }, [t - i, e - i, t + i, e + i], this.lw);
  }
  _draw(t) {
    if (!this._cmds.length) return;
    let e = null, i = null, n = new Path2D(), a = null, r = 0, o = 0;
    const h = () => {
      e && !isNaN(e.w) && (t.lineWidth = e.w <= 0 ? 1 : e.w, t.strokeStyle = G(e.c, e.a), t.lineCap = "round", t.lineJoin = "round", t.stroke(n)), n = new Path2D(), n.moveTo(r, o);
    };
    for (const l of this._cmds)
      switch (l.k) {
        case "ls":
          h(), e = l;
          break;
        case "bf":
          i = l, a = new Path2D(), a.moveTo(r, o);
          break;
        case "ef":
          i && a && (t.fillStyle = G(i.c, i.a), t.fill(a)), i = null, a = null;
          break;
        case "mt":
          r = l.x, o = l.y, n.moveTo(r, o), a?.moveTo(r, o);
          break;
        case "lt":
          r = l.x, o = l.y, n.lineTo(r, o), a?.lineTo(r, o);
          break;
        case "ct":
          r = l.x, o = l.y, n.quadraticCurveTo(l.cx, l.cy, r, o), a?.quadraticCurveTo(l.cx, l.cy, r, o);
          break;
        case "rect":
          n.rect(l.x, l.y, l.w, l.h), a?.rect(l.x, l.y, l.w, l.h);
          break;
        case "circ":
          n.moveTo(l.x + l.r, l.y), n.arc(l.x, l.y, l.r, 0, Math.PI * 2), a?.moveTo(l.x + l.r, l.y), a?.arc(l.x, l.y, l.r, 0, Math.PI * 2);
          break;
      }
    i && a && (t.fillStyle = G(i.c, i.a), t.fill(a)), h();
  }
  _hit(t, e) {
    const i = this._bounds;
    return !!i && t >= i[0] && t <= i[2] && e >= i[1] && e <= i[3] && this._cmds.some((n) => n.k === "bf");
  }
}
const Dt = /* @__PURE__ */ new WeakMap();
function ot(s, t) {
  let e = Dt.get(s);
  e || Dt.set(s, e = /* @__PURE__ */ new Map());
  const i = e.get(t);
  if (i) return i;
  const n = t > 1 ? ot(s, t - 1) : /* @__PURE__ */ new Map(), a = /* @__PURE__ */ new Map();
  n.forEach((r, o) => a.set(o, r));
  for (const r of s.frames[t - 1] || []) {
    if (r.o === "r") {
      a.delete(r.d);
      continue;
    }
    const o = a.get(r.d);
    if (r.id !== void 0 && (!r.mv || !o))
      a.set(r.d, { create: r, id: r.id, m: r.m, cx: r.cx, n: r.n, f: r.f, bm: r.bm, r: r.r, vis: r.vis });
    else if (o) {
      const h = { ...o };
      r.id !== void 0 && (h.create = r, h.id = r.id), r.m && (h.m = r.m), r.cx && (h.cx = r.cx), r.f && (h.f = r.f), r.bm && (h.bm = r.bm), r.r !== void 0 && (h.r = r.r), r.n && (h.n = r.n), r.vis !== void 0 && (h.vis = r.vis), a.set(r.d, h);
    }
  }
  return e.set(t, a), a;
}
const lt = [];
class Ot extends it {
  _setup() {
    super._setup(), this._graphics = null, this.buttonMode = !1, this.useHandCursor = !0, this.hitArea = null, this._def = null, this._frame = 0, this._playing = !1;
  }
  get graphics() {
    return this._graphics || (this._graphics = new qt(this)), this._graphics;
  }
  _init(t) {
    const e = t === 0 ? E.root : t !== void 0 ? L(t) : void 0;
    e && e.k === "sprite" && (this._def = e, this._playing = e.n > 1, this._enterFrame(1));
  }
  /** Bring the timeline children in line with frame n. */
  _enterFrame(t) {
    const e = this._def, i = this._frame;
    if (this._frame = t, i && t === i + 1)
      for (const n of e.frames[t - 1] || []) {
        const a = this._timelineChild(n.d);
        if (n.o === "r") {
          a && this._removeTimelineChild(a);
          continue;
        }
        if (n.id !== void 0 && (!n.mv || !a))
          a && this._removeTimelineChild(a), this._createTimelineChild(n.d, ot(e, t).get(n.d));
        else if (a) {
          const r = ot(e, t).get(n.d);
          n.id !== void 0 && r.create !== a._placeOp ? (this._removeTimelineChild(a), this._createTimelineChild(n.d, r)) : a._scriptMoved || (a._applyPlace(n, !1), n.r !== void 0 && a instanceof K && a._def);
        }
      }
    else {
      const n = ot(e, t);
      for (const r of this._children.slice()) {
        if (r._depth === void 0 || !r._placeOp) continue;
        const o = n.get(r._depth);
        o && o.create === r._placeOp ? r._scriptMoved || r._applyPlace(o, !0) : this._removeTimelineChild(r);
      }
      const a = [...n.keys()].sort((r, o) => r - o);
      for (const r of a)
        this._timelineChild(r) || this._createTimelineChild(r, n.get(r));
    }
    this.invalidate(), this instanceof K && lt.push({ clip: this, frame: t });
  }
  _timelineChild(t) {
    return this._children.find((e) => e._depth === t && e._placeOp);
  }
  _removeTimelineChild(t) {
    const e = this._children.indexOf(t);
    e >= 0 && this._children.splice(e, 1), t._parent = null, t._name && this[t._name] === t && (this[t._name] = null);
  }
  _createTimelineChild(t, e) {
    const i = L(e.id);
    if (!i) return null;
    const n = zt.get(e.id);
    B = { parent: this, depth: t, st: e };
    let a = null;
    try {
      if (n) a = new n();
      else if (i.k === "sprite") a = new K();
      else if (i.k === "shape") a = new Yt();
      else if (i.k === "text") a = new Xt();
      else if (i.k === "edit") a = new Gt();
      else
        return B = null, null;
    } catch (r) {
      xt(`construct ${n ? n.name : i.k} #${e.id}`, r), a = this._timelineChild(t) || null;
    }
    return B = null, a && e.n && (this[e.n] = a), a;
  }
  _localBounds() {
    let t = super._localBounds();
    return this._graphics && (t = pt(t, this._graphics._bounds)), t;
  }
  _hitLocal(t, e) {
    return super._hitLocal(t, e) ? !0 : !!this._graphics && this._graphics._hit(t, e);
  }
  _draw(t) {
    this._graphics && this._graphics._draw(t), super._draw(t);
  }
}
class K extends Ot {
  _setup() {
    super._setup(), this._scripts = null, this.enabled = !0, this.trackAsMenu = !1;
  }
  get currentFrame() {
    return this._frame || 1;
  }
  get totalFrames() {
    return this._def ? this._def.n : 1;
  }
  get framesLoaded() {
    return this.totalFrames;
  }
  get isPlaying() {
    return this._playing;
  }
  get currentLabel() {
    if (!this._def) return null;
    let t = null, e = 0;
    for (const i in this._def.labels) {
      const n = this._def.labels[i];
      n <= this._frame && n > e && (t = i, e = n);
    }
    return t;
  }
  get currentFrameLabel() {
    if (!this._def) return null;
    for (const t in this._def.labels) if (this._def.labels[t] === this._frame) return t;
    return null;
  }
  addFrameScript(...t) {
    this._scripts || (this._scripts = /* @__PURE__ */ new Map());
    for (let e = 0; e + 1 < t.length; e += 2)
      t[e + 1] ? this._scripts.set(t[e], t[e + 1]) : this._scripts.delete(t[e]);
  }
  play() {
    this._playing = this.totalFrames > 1;
  }
  stop() {
    this._playing = !1;
  }
  nextFrame() {
    this._goto(this.currentFrame + 1, !1);
  }
  prevFrame() {
    this._goto(this.currentFrame - 1, !1);
  }
  gotoAndStop(t, e = null) {
    this._goto(t, !1);
  }
  gotoAndPlay(t, e = null) {
    this._goto(t, !0);
  }
  _resolve(t) {
    if (typeof t == "string") {
      const e = this._def?.labels[t];
      if (e !== void 0) return e;
      const i = parseInt(t, 10);
      if (!isNaN(i)) return i;
      throw new Error(`ArgumentError: Error #2109: Frame label ${t} not found in scene.`);
    }
    return Math.floor(+t);
  }
  _goto(t, e) {
    if (this._playing = e && this.totalFrames > 1, !this._def) return;
    const i = Math.max(1, Math.min(this._resolve(t), this._def.n));
    i !== this._frame && (this._enterFrame(i), vt(this));
  }
  _advance() {
    if (!this._def || !this._playing || this._born === Tt || this._def.n <= 1) return;
    const t = this._frame >= this._def.n ? 1 : this._frame + 1;
    this._enterFrame(t);
  }
}
function vt(s) {
  for (let t = 0; t < lt.length; t++) {
    const e = lt[t];
    if (s && e.clip !== s || (lt.splice(t--, 1), e.clip._frame !== e.frame)) continue;
    const i = e.clip._scripts?.get(e.frame - 1);
    if (i)
      try {
        i.call(e.clip);
      } catch (n) {
        xt(`${e.clip.constructor.name} frame ${e.frame}`, n);
      }
  }
}
class Yt extends $ {
  _setup() {
    super._setup(), this._def = null, this._graphics = null;
  }
  _init(t) {
    const e = t !== void 0 ? L(t) : void 0;
    e && e.k === "shape" && (this._def = e);
  }
  get graphics() {
    return this._graphics || (this._graphics = new qt(this)), this._graphics;
  }
  _localBounds() {
    let t = this._def ? [...this._def.b] : null;
    return this._graphics && (t = pt(t, this._graphics._bounds)), t;
  }
  _hitLocal(t, e) {
    return !!this._def && se(this._def, t, e) || !!this._graphics && this._graphics._hit(t, e);
  }
  _draw(t) {
    this._def && ee(t, this._def), this._graphics && this._graphics._draw(t);
  }
}
class Xt extends $ {
  _setup() {
    super._setup(), this._def = null;
  }
  _init(t) {
    const e = t !== void 0 ? L(t) : void 0;
    e && e.k === "text" && (this._def = e);
  }
  get text() {
    return "";
  }
  // DefineText bounds are already in the character's space (after the text matrix)
  _localBounds() {
    return this._def ? [...this._def.b] : null;
  }
  _hitLocal(t, e) {
    const i = this._localBounds();
    return !!i && t >= i[0] && t <= i[2] && e >= i[1] && e <= i[3];
  }
  _draw(t) {
    this._def && ne(t, this._def);
  }
}
const It = {
  k: "edit",
  b: [0, 0, 100, 100],
  text: "",
  multiline: !1,
  wordWrap: !1,
  html: !1,
  readOnly: !1,
  noSelect: !1,
  border: !1,
  outlines: !1,
  password: !1,
  autoSize: !1,
  var: "",
  h: 12,
  c: [0, 0, 0, 1]
};
class Gt extends et {
  _setup() {
    super._setup(), this._def = It, this._text = "", this._style = this._makeStyle(It), this.selectable = !0, this.embedFonts = !1, this.antiAliasType = "normal", this.type = "dynamic";
  }
  _makeStyle(t) {
    const e = t.font !== void 0 ? L(t.font) : void 0;
    return {
      font: t.font,
      size: t.h ?? 12,
      color: t.c ? [...t.c] : [0, 0, 0, 1],
      align: t.align ?? 0,
      leading: t.leading ?? 0,
      leftMargin: t.leftMargin ?? 0,
      rightMargin: t.rightMargin ?? 0,
      indent: t.indent ?? 0,
      bold: !!(e && e.k === "font" && e.bold),
      face: e && e.k === "font" ? e.name : "Arial"
    };
  }
  _init(t) {
    const e = t !== void 0 ? L(t) : void 0;
    e && e.k === "edit" && (this._def = e, this._style = this._makeStyle(e), this.selectable = !e.noSelect, this.embedFonts = e.outlines, this.type = e.readOnly ? "dynamic" : "input", this._text = e.html ? Lt(e.text, this._style) : e.text);
  }
  get text() {
    return this._text;
  }
  set text(t) {
    t = t == null ? "" : String(t), t !== this._text && (this._text = t, this.invalidate());
  }
  get htmlText() {
    return this._text;
  }
  set htmlText(t) {
    this._text = Lt(String(t), this._style), this.invalidate();
  }
  appendText(t) {
    this.text = this._text + t;
  }
  get length() {
    return this._text.length;
  }
  get textColor() {
    const t = this._style.color;
    return t[0] << 16 | t[1] << 8 | t[2];
  }
  set textColor(t) {
    this._style.color = [t >> 16 & 255, t >> 8 & 255, t & 255, 1], this.invalidate();
  }
  get textWidth() {
    return this._text.length * this._style.size * 0.5;
  }
  get textHeight() {
    return this._style.size * 1.2;
  }
  _localBounds() {
    return [...this._def.b];
  }
  _hitLocal(t, e) {
    const i = this._def.b;
    return t >= i[0] && t <= i[2] && e >= i[1] && e <= i[3];
  }
  _draw(t) {
    this._text && he(t, this._def, this._text, this._style);
  }
}
class _e extends et {
}
class Mt {
  constructor(t = 0, e = 0, i = !0, n = 4294967295) {
    this._views = /* @__PURE__ */ new Set(), this.transparent = i, this._canvas = null, this._ctx = null;
    const a = Ct(new.target), r = a !== void 0 ? _t.get(a) : void 0;
    if (r)
      this._src = r, this._w = r.width, this._h = r.height;
    else {
      this._w = Math.max(0, Math.floor(t)), this._h = Math.max(0, Math.floor(e));
      const o = this._writable();
      this._src = o.canvas;
      const h = i ? n >>> 0 : (n | 4278190080) >>> 0;
      h >>> 24 > 0 && (o.fillStyle = G(h, (h >>> 24) / 255), o.fillRect(0, 0, this._w, this._h));
    }
  }
  _writable() {
    if (!this._ctx) {
      const t = document.createElement("canvas");
      t.width = this._w, t.height = this._h;
      const e = t.getContext("2d", { willReadFrequently: !1 });
      this._src && e.drawImage(this._src, 0, 0), this._canvas = t, this._ctx = e, this._src = t;
    }
    return this._ctx;
  }
  _changed() {
    for (const t of this._views) t.invalidate();
  }
  get width() {
    return this._w;
  }
  get height() {
    return this._h;
  }
  get rect() {
    return new Z(0, 0, this._w, this._h);
  }
  copyPixels(t, e, i, n = null, a = null, r = !1) {
    const o = this._writable();
    let h = e.x, l = e.y, c = e.width, u = e.height, d = i.x, g = i.y;
    h < 0 && (d -= h, c += h, h = 0), l < 0 && (g -= l, u += l, l = 0), c = Math.min(c, t._w - h), u = Math.min(u, t._h - l), !(c <= 0 || u <= 0) && (d = Math.round(d), g = Math.round(g), (!r || !this.transparent) && o.clearRect(d, g, c, u), o.drawImage(t._src, h, l, c, u, d, g, c, u), this._changed());
  }
  fillRect(t, e) {
    const i = this._writable();
    e >>>= 0, i.clearRect(t.x, t.y, t.width, t.height);
    const n = this.transparent ? (e >>> 24) / 255 : 1;
    n > 0 && (i.fillStyle = G(e, n), i.fillRect(t.x, t.y, t.width, t.height)), this._changed();
  }
  draw(t, e = null, ...i) {
    const n = this._writable();
    n.save(), e && n.setTransform(e.a, e.b, e.c, e.d, e.tx, e.ty), t instanceof Mt ? n.drawImage(t._src, 0, 0) : t instanceof $ && t._draw(n), n.restore(), this._changed();
  }
  getPixel32(t, e) {
    const i = this._writable().getImageData(t, e, 1, 1).data;
    return (i[3] << 24 | i[0] << 16 | i[1] << 8 | i[2]) >>> 0;
  }
  getPixel(t, e) {
    return this.getPixel32(t, e) & 16777215;
  }
  setPixel32(t, e, i) {
    const n = this._writable();
    n.clearRect(t, e, 1, 1), n.fillStyle = G(i >>> 0, (i >>> 24) / 255), n.fillRect(t, e, 1, 1), this._changed();
  }
  setPixel(t, e, i) {
    this.setPixel32(t, e, (i | 4278190080) >>> 0);
  }
  clone() {
    const t = new Mt(this._w, this._h, this.transparent, 0);
    return t._writable().drawImage(this._src, 0, 0), t;
  }
  lock() {
  }
  unlock() {
  }
  dispose() {
    this._w = this._h = 0;
  }
}
class pe extends $ {
  constructor(t = null, e = "auto", i = !1) {
    super(), this.bitmapData = t, this.pixelSnapping = e, this.smoothing = i;
  }
  _setup() {
    super._setup(), this._bmp = null, this.smoothing = !1, this.pixelSnapping = "auto";
  }
  get bitmapData() {
    return this._bmp;
  }
  set bitmapData(t) {
    this._bmp && this._bmp._views.delete(this), this._bmp = t, t && t._views.add(this), this.invalidate();
  }
  _localBounds() {
    return this._bmp ? [0, 0, this._bmp._w, this._bmp._h] : null;
  }
  _hitLocal(t, e) {
    return !!this._bmp && t >= 0 && e >= 0 && t <= this._bmp._w && e <= this._bmp._h;
  }
  _draw(t) {
    this._bmp && this._bmp._w && t.drawImage(this._bmp._src, 0, 0);
  }
}
class W extends it {
  _setup() {
    super._setup(), this.stageWidth = 800, this.stageHeight = 480, this.frameRate = 30, this.quality = "high", this.scaleMode = "showAll", this.align = "", this.focus = null, this.showDefaultContextMenu = !0;
  }
  get mouseX() {
    return y.x;
  }
  get mouseY() {
    return y.y;
  }
}
let nt = null;
function Kt() {
  if (!nt) {
    const s = tt();
    nt = s.createGain(), nt.connect(s.destination);
  }
  return nt;
}
class z {
  constructor(t = 1, e = 0) {
    this.volume = t, this.pan = e, this.leftToLeft = 1, this.leftToRight = 0, this.rightToLeft = 0, this.rightToRight = 1;
  }
}
const ut = /* @__PURE__ */ new Set();
class Ht extends H {
  constructor() {
    super(...arguments), this._src = null, this._gain = null, this._st = new z(), this._start = 0;
  }
  get soundTransform() {
    return new z(this._st.volume, this._st.pan);
  }
  set soundTransform(t) {
    this._st = new z(t.volume, t.pan), this._gain && (this._gain.gain.value = t.volume);
  }
  get position() {
    return this._src ? (tt().currentTime - this._start) * 1e3 : 0;
  }
  get leftPeak() {
    return 0;
  }
  get rightPeak() {
    return 0;
  }
  stop() {
    if (this._src) {
      this._src.onended = null;
      try {
        this._src.stop();
      } catch {
      }
      this._src = null;
    }
    ut.delete(this);
  }
}
class ge extends H {
  constructor(...t) {
    super();
    const e = Ct(new.target);
    this._buf = e !== void 0 && Bt.get(e) || null, Wt(this);
  }
  get length() {
    return this._buf ? this._buf.duration * 1e3 : 0;
  }
  get bytesLoaded() {
    return 1;
  }
  get bytesTotal() {
    return 1;
  }
  play(t = 0, e = 0, i = null) {
    const n = new Ht();
    if (i && (n._st = new z(i.volume, i.pan)), !this._buf) return n;
    const a = tt(), r = a.createBufferSource();
    r.buffer = this._buf;
    const o = a.createGain();
    o.gain.value = n._st.volume, r.connect(o), o.connect(Kt());
    const h = Math.max(0, t / 1e3), l = Math.max(1, Math.floor(e));
    return l > 1 ? (r.loop = !0, r.start(0, h), r.stop(a.currentTime + this._buf.duration * l - h)) : r.start(0, h), r.onended = () => {
      ut.delete(n), n._src = null;
      const c = new C(C.SOUND_COMPLETE);
      n.dispatchEvent(c);
    }, n._src = r, n._gain = o, n._start = a.currentTime - h, ut.add(n), n;
  }
  close() {
  }
}
class me {
  static {
    this._st = new z();
  }
  static get soundTransform() {
    return new z(this._st.volume, this._st.pan);
  }
  static set soundTransform(t) {
    this._st = new z(t.volume, t.pan), Kt().gain.value = t.volume;
  }
  static stopAll() {
    for (const t of [...ut]) t.stop();
  }
  static {
    this.bufferTime = 0;
  }
}
const we = performance.now();
function Oe() {
  return Math.floor(performance.now() - we);
}
const ft = /* @__PURE__ */ new Set();
class ye extends H {
  constructor(t, e = 0) {
    super(), this.delay = t, this.repeatCount = e, this._count = 0, this._next = 0, this.running = !1;
  }
  get currentCount() {
    return this._count;
  }
  start() {
    this.running || (this.running = !0, this._next = performance.now() + Math.max(1, this.delay), ft.add(this));
  }
  stop() {
    this.running = !1, ft.delete(this);
  }
  reset() {
    this.stop(), this._count = 0;
  }
}
function be(s) {
  for (const t of [...ft]) {
    let e = 0;
    for (; t.running && s >= t._next && e < 4; )
      t._count++, e++, t._next += Math.max(1, t.delay), t.dispatchEvent(new st(st.TIMER)), t.repeatCount > 0 && t._count >= t.repeatCount && (t.stop(), t.dispatchEvent(new st(st.TIMER_COMPLETE)));
    t.running && s >= t._next && (t._next = s + Math.max(1, t.delay));
  }
}
const dt = /* @__PURE__ */ new Map();
let wt = !1;
function Ft() {
  wt || (wt = !0, setTimeout(() => {
    wt = !1;
    for (const s of dt.values()) s._flushIfChanged();
  }, 250));
}
typeof window < "u" && window.addEventListener("pagehide", () => {
  for (const s of dt.values()) s._flushIfChanged();
});
class kt extends H {
  constructor(t) {
    super(), this._key = t, this._data = {}, this._changed = !1;
    let e = {};
    try {
      const i = localStorage.getItem(t);
      i && (e = JSON.parse(i));
    } catch {
    }
    this.data = e;
  }
  get data() {
    return this._proxy;
  }
  set data(t) {
    this._data = t && typeof t == "object" ? t : {}, this._proxy = new Proxy(this._data, {
      set: (e, i, n) => (e[i] !== n && (e[i] = n, this._changed = !0, Ft()), !0),
      deleteProperty: (e, i) => (delete e[i], this._changed = !0, Ft(), !0)
    });
  }
  _flushIfChanged() {
    this._changed && this.flush();
  }
  static getLocal(t, e = null, i = !1) {
    const n = "fd2:so:" + t;
    let a = dt.get(n);
    return a || dt.set(n, a = new kt(n)), a;
  }
  flush(t = 0) {
    this._changed = !1;
    try {
      localStorage.setItem(this._key, JSON.stringify(this._data));
    } catch {
    }
    return "flushed";
  }
  clear() {
    this.data = {};
    try {
      localStorage.removeItem(this._key);
    } catch {
    }
  }
  get size() {
    return JSON.stringify(this.data).length;
  }
}
const y = {
  x: 0,
  y: 0,
  down: !1
};
function Vt(s, t, e, i) {
  const n = s._children;
  for (let a = n.length - 1; a >= 0; a--) {
    const r = n[a];
    if (!r._visible) continue;
    const o = yt(t, r._matrix());
    if (r instanceof it) {
      if (r.mouseChildren) {
        const c = Vt(r, o, e, i);
        if (c) return c;
      } else if (r.mouseEnabled) {
        const [c, u] = k(U(o), e, i);
        if (r._hitLocal(c, u)) return r;
      }
      continue;
    }
    const [h, l] = k(U(o), e, i);
    if (r._hitLocal(h, l)) {
      if (r instanceof et) {
        if (r.mouseEnabled) return r;
        continue;
      }
      if (s.mouseEnabled && !(s instanceof W)) return s;
    }
  }
  if (s instanceof Ot && s._graphics && s.mouseEnabled) {
    const [a, r] = k(U(t), e, i);
    if (s._graphics._hit(a, r)) return s;
  }
  return null;
}
function Nt(s) {
  const t = [];
  for (; s && !(s instanceof W); )
    t.push(s), s = s._parent;
  return t;
}
let A = null, rt = null;
function S(s, t, e, i = null) {
  const n = new O(s, e);
  n.target = t, n.stageX = y.x, n.stageY = y.y;
  const a = t.globalToLocal(new P(y.x, y.y));
  return n.localX = a.x, n.localY = a.y, n.buttonDown = y.down, n.relatedObject = i, n;
}
function at() {
  if (!bt) return;
  const s = Vt(bt, [1, 0, 0, 1, 0, 0], y.x, y.y);
  if (s === A) return;
  const t = A;
  A = s;
  const e = Nt(t), i = Nt(s);
  t && t.stage && t.dispatchEvent(S(O.MOUSE_OUT, t, !0, s));
  for (const n of e) i.includes(n) || n.dispatchEvent(S(O.ROLL_OUT, n, !1, s));
  for (const n of i.slice().reverse()) e.includes(n) || n.dispatchEvent(S(O.ROLL_OVER, n, !1, t));
  s && s.dispatchEvent(S(O.MOUSE_OVER, s, !0, t));
}
function ve(s) {
  const { canvas: t } = s, e = t.getContext("2d"), i = E.w, n = E.h, a = new AbortController(), r = { signal: a.signal };
  B = null;
  const o = new W();
  o.stageWidth = i, o.stageHeight = n, o.frameRate = E.fps, bt = o, B = { parent: o, depth: 0, st: { create: { o: "p", d: 0, id: 0 }, id: 0 } };
  const h = s.root || K;
  Et.set(h, 0), new h(), B = null;
  let l = 1, c = 0, u = 0;
  const d = () => {
    const p = window.devicePixelRatio || 1, _ = s.viewport?.getBoundingClientRect(), M = _?.width || window.innerWidth, F = _?.height || window.innerHeight, X = Math.min(M / i, F / n);
    t.style.width = `${Math.floor(i * X)}px`, t.style.height = `${Math.floor(n * X)}px`, t.width = Math.floor(i * X * p), t.height = Math.floor(n * X * p), l = t.width / i, c = 0, u = 0;
  };
  d(), window.addEventListener("resize", d, r);
  const g = (p) => {
    const _ = t.getBoundingClientRect();
    y.x = (p.clientX - _.left) / _.width * i, y.y = (p.clientY - _.top) / _.height * n;
  }, b = (p) => {
    g(p), at();
    const _ = A;
    _ && _.dispatchEvent(S(O.MOUSE_MOVE, _, !0));
  }, D = (p) => {
    tt().resume(), g(p), y.down = !0, at(), rt = A, A ? A.dispatchEvent(S(O.MOUSE_DOWN, A, !0)) : o.dispatchEvent(S(O.MOUSE_DOWN, o, !0));
  }, m = (p) => {
    if (!y.down) return;
    g(p), y.down = !1, at();
    const _ = A;
    _ ? _.dispatchEvent(S(O.MOUSE_UP, _, !0)) : o.dispatchEvent(S(O.MOUSE_UP, o, !0)), _ && _ === rt && _.dispatchEvent(S(O.CLICK, _, !0)), rt = null;
  }, j = (p) => p.preventDefault();
  t.addEventListener("pointermove", b, r), t.addEventListener("pointerdown", D, r), window.addEventListener("pointerup", m, r), t.addEventListener("contextmenu", j, r);
  const x = 1e3 / E.fps;
  let v = performance.now(), T = 0;
  const R = () => {
    Tt++;
    const p = (M) => {
      M instanceof K && M._advance();
      for (const F of M._children.slice()) F instanceof it && p(F);
    };
    p(o);
    const _ = new C(C.ENTER_FRAME);
    for (const M of [...ct]) {
      const F = M._listeners.get(C.ENTER_FRAME);
      if (!F || !F.length) {
        ct.delete(M);
        continue;
      }
      !M.stage && F.every((X) => X.weak) || (_.target = M, _._stop = _._stopNow = !1, M._fire(_));
    }
    vt(), at();
  }, f = () => {
    e.setTransform(1, 0, 0, 1, 0, 0), e.globalAlpha = 1, e.globalCompositeOperation = "source-over";
    const p = E.bg;
    e.fillStyle = `rgb(${p[0]},${p[1]},${p[2]})`, e.fillRect(0, 0, t.width, t.height), e.setTransform(l, 0, 0, l, c, u), e.imageSmoothingEnabled = !0, o._render(e);
  }, w = { timers: 0, tick: 0, render: 0, frames: 0 };
  window.__perf = w;
  const I = (p, _) => {
    w[p] = w[p] * 0.95 + _ * 0.05;
  };
  let q = 0, Y = !1;
  const V = (p) => {
    if (Y) return;
    let _ = performance.now();
    be(p), I("timers", performance.now() - _), T += Math.min(250, p - v), v = p;
    let M = 0;
    for (; T >= x && M < 4; )
      _ = performance.now(), R(), I("tick", performance.now() - _), T -= x, M++;
    M >= 4 && (T = 0), M && (_ = performance.now(), f(), I("render", performance.now() - _), w.frames++), q = requestAnimationFrame(V);
  };
  return vt(), f(), q = requestAnimationFrame(V), {
    stage: o,
    dispose() {
      if (!Y) {
        Y = !0, cancelAnimationFrame(q), a.abort(), me.stopAll();
        for (const p of [...ft]) p.stop();
        y.down = !1, rt = null;
      }
    }
  };
}
const Jt = /* @__PURE__ */ new Set([
  H.prototype,
  $.prototype,
  et.prototype,
  it.prototype,
  Ot.prototype,
  K.prototype,
  Yt.prototype,
  Xt.prototype,
  Gt.prototype,
  _e.prototype,
  pe.prototype,
  W.prototype,
  ge.prototype,
  Ht.prototype,
  ye.prototype,
  kt.prototype
]);
async function ke(s) {
  try {
    await Zt(s.assetBase, (i, n) => {
      const a = s.status.dataset.loadingLabel ?? "";
      s.status.textContent = `${a} ${Math.floor(i / n * 100)}%`.trim();
    });
    const t = await import("./frontline-tower-defense-game.js");
    return t.registerClasses(), s.status.hidden = !0, ve({
      canvas: s.canvas,
      viewport: s.viewport,
      root: t.DocumentClass
    });
  } catch (t) {
    throw s.status.dataset.failed = "true", console.error(t), t;
  }
}
export {
  Mt as B,
  C as E,
  K as M,
  P,
  Z as R,
  Ot as S,
  ye as T,
  Ee as _,
  O as a,
  ce as b,
  Wt as c,
  Me as d,
  pe as e,
  st as f,
  Oe as g,
  xe as h,
  ge as i,
  kt as j,
  Ce as k,
  z as l,
  me as m,
  ke as s
};
