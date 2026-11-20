import { M as d, S as b, _ as g, a as A, b as m, c, d as h, B as _, R as B, e as V, P as R, E as w, T as se, f as ie, g as te, h as $, i as C, j as Bt, k as i, l as P, m as O } from "./frontline-tower-defense-main.js";
class Rt extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class zt extends b {
  constructor() {
    var e = null, t = void 0, s = void 0;
    super(), e = g(p, this.parent), t = Math.floor(this.x / e.gridSize), s = Math.floor(this.y / e.gridSize), e.setGrid(t, s, 2), this.parent.removeChild(this);
  }
}
class It extends b {
  constructor() {
    super(), this.addEventListener(A.CLICK, this.clicked, !1, 0, !0);
  }
  clicked(e) {
    g(M, this.parent).buy();
  }
}
class f {
  __fi() {
    this.poison = 0, this.rad = 0, this.burn = 0, this.freeze = 0;
  }
  constructor(e, t, s, l) {
    m(this), c(this), this.burn = e, this.rad = t, this.poison = s, this.freeze = l;
  }
}
class Gt extends b {
  __fi() {
    this.blood = null, this.gs = 0, this.hs = 0, this.clearRec = null, this.crater = null, this.icet = null, this.canvas = null, this.sandt = null, this.rockt = null, this.DisplayScreen = null, this.grass = null, this.items = new Array(), this.patht = null, this.game = null, this.scorch = null;
  }
  constructor() {
    super(), this.game = g(p, this.parent), this.game.decalFX = this, this.gs = h(this.game.gridSize), this.hs = h(this.gs * 0.5), this.scorch = new ht(128, 32), this.crater = new be(128, 32), this.blood = new xe(128, 32), this.patht = new Qe(768, 48), this.rockt = new it(768, 48), this.icet = new Ge(768, 48), this.sandt = new at(768, 48), this.mouseEnabled = !1, this.canvas = new _(800, 600, !0, 0), this.clearRec = new B(0, 0, 800, 600), this.DisplayScreen = new V(this.canvas), this.addChild(this.DisplayScreen);
  }
  Splat(e, t) {
    var s = void 0, l = void 0;
    e += -4 + Math.random() * 8, t += -4 + Math.random() * 8, s = Math.random() * 3.9, l = new B(Math.floor(s) * 16, 0, 16, 16), this.canvas.copyPixels(this.blood, l, new R(e - 8, t - 8), null, null, !0);
  }
  Crater(e, t) {
    var s = void 0, l = void 0;
    e += -4 + Math.random() * 8, t += -4 + Math.random() * 8, s = Math.random() * 3.9, l = new B(Math.floor(s) * 32, 0, 32, 32), this.canvas.copyPixels(this.crater, l, new R(e - 16, t - 16), null, null, !0);
  }
  checkGrid(e, t) {
    return e < 0 || e > 29 || t < 0 || t > 25 ? h(0) : h(h(this.game.levelArray[e][t]));
  }
  drawPath(e) {
    var t = null, s = void 0, l = void 0, o = void 0, v = void 0, u = h(0), n = void 0, W = void 0, y = void 0, S = void 0;
    for (e == "forest" ? t = this.patht : e == "ice" ? t = this.icet : e == "sand" ? t = this.sandt : e == "rocky" && (t = this.rockt), s = this.game.levelArray.length, l = 0; l < s; ) {
      for (o = this.game.levelArray[l].length, v = 0; v < o; )
        this.game.levelArray[l][v] == 1 && (u = h(0), this.checkGrid(l, v - 1) != 1 && (u = h(u + 1)), this.checkGrid(l, v + 1) != 1 && (u = h(u + 4)), this.checkGrid(l - 1, v) != 1 && (u = h(u + 8)), this.checkGrid(l + 1, v) != 1 && (u = h(u + 2)), n = l * this.gs + this.hs, W = v * this.gs + this.hs, y = u, S = new B(1 + Math.floor(y) * 48, 1, 48, 48), this.canvas.copyPixels(t, S, new R(n - 24, W - 24), null, null, !0)), v++;
      l++;
    }
  }
  Scorch(e, t) {
    var s = void 0, l = void 0;
    e += -4 + Math.random() * 8, t += -4 + Math.random() * 8, s = Math.random() * 3.9, l = new B(Math.floor(s) * 32, 0, 32, 32), this.canvas.copyPixels(this.scorch, l, new R(e - 16, t - 16), null, null, !0);
  }
}
class Y extends b {
  __fi() {
    this.gy = 0, this.vx = NaN, this.vy = NaN, this.gx = 0, this.game = null;
  }
  constructor() {
    super(), this.game = g(p, this.parent.parent);
  }
  SetGrid() {
    this.gx = h(Math.floor(this.x / this.game.gridSize)), this.gy = h(Math.floor(this.y / this.game.gridSize));
  }
  Tick(e) {
  }
}
class Xt extends Y {
  constructor() {
    super(), this.SetGrid(), this.game.endPoints.push(this), this.visible = !1;
  }
}
class k extends b {
  __fi() {
    this.gy = 0, this.upperTemp = 200, this.hv = null, this.coreTemp = 35, this.rad = 0, this.poison = 0, this.oldAng = 1e3, this.pth = null, this.baseTemp = 35, this.traveled = 0, this.freezeAnim = null, this.speed = 1, this.health = 50, this.cold_resistance = 1, this.endPoint = null, this.bEnemy = !0, this.fxTick = h(0), this.freeze = 0, this.radius = h(8), this.ty = 0, this.tx = 0, this.bFrozen = !1, this.radiation_resistance = 1, this.vx = NaN, this.vy = NaN, this.sValue = h(100), this.lowerTemp = -200, this.cValue = h(10), this.burn = 0, this.bMetal = !1, this.poison_resistance = 1, this.ang = NaN, this.bDead = !1, this.heat_resistance = 1, this.gx = 0, this.game = null, this.baseSpeed = NaN;
  }
  constructor() {
    super(), this.mouseEnabled = !1, this.hv = new Ie(), this.addChild(this.hv), this.hv.y = -20, this.hv.update(Math.floor(this.health));
  }
  Move(e) {
    var t = void 0, s = void 0;
    this.x += this.vx * this.speed * e, this.y += this.vy * this.speed * e, this.gx == Math.floor(this.tx / 32) && this.gy == Math.floor(this.ty / 32) && this.x > this.gx * this.game.gridSize + 4 && this.x < this.gx * this.game.gridSize + 28 && this.y > this.gy * this.game.gridSize + 4 && this.y < this.gy * this.game.gridSize + 28 && (this.ty = h(Number(this.pth.pop()) * this.game.gridSize + 16), this.tx = h(Number(this.pth.pop()) * this.game.gridSize + 16), t = this.tx - this.x, s = this.ty - this.y, this.ang = Math.atan2(s, t), this.vx = Math.cos(this.ang), this.vy = Math.sin(this.ang), this.setAngle());
  }
  makeFX() {
    ++this.fxTick, this.fxTick > 4 && (this.fxTick = h(0), this.burn > 0 && new r(this.game, this.game.SpritePool.flame, 16, this.x, this.y - 8, -0.2 + Math.random() * 0.4, -Math.random() * 0.2, 8, 0.1, 8, 0, null, null, 1, !0), this.poison > 0 && new r(this.game, this.game.SpritePool.poison, 16, this.x, this.y - 8, -0.2 + Math.random() * 0.4, -Math.random() * 0.5, 8, 0.2, 8), this.rad > 0 && new r(this.game, this.game.SpritePool.rad, 16, this.x, this.y - 8, -0.2 + Math.random() * 0.4, -Math.random() * 0.5, 8, 0.2, 8, 0, null, null, 1, !0), this.freeze > 0 && new r(this.game, this.game.SpritePool.freeze, 16, this.x, this.y - 8, -0.2 + Math.random() * 0.4, -Math.random() * 0.5, 8, 0.1, 8, 0, null, null, 1, !0));
  }
  getDir() {
    var e = void 0, t = void 0;
    this.pth == null ? (this.pth = this.game.path.findPath(this.gx, this.gy, this.endPoint.gx, this.endPoint.gy), this.ty = h(Number(this.pth.pop()) * this.game.gridSize + 16), this.tx = h(Number(this.pth.pop()) * this.game.gridSize + 16), e = this.tx - this.x, t = this.ty - this.y, this.ang = Math.atan2(t, e), this.vx = Math.cos(this.ang), this.vy = Math.sin(this.ang), this.setAngle(), this.bMetal && (this.baseTemp = 200, this.coreTemp = 200, this.upperTemp = 500, this.lowerTemp = 0, this.cValue = h(20))) : this.pth.length <= 0 && !this.bDead && (this.bDead = !0, this.game.levelHealth -= this.health, this.game.killEnemy(this));
  }
  drawPath() {
    var e = null, t = void 0, s = void 0;
    for (e = g(d, this.parent), t = this.pth.length, e.graphics.lineStyle(2, 0), e.graphics.moveTo(this.x, this.y), s = t - 1; s >= 0; )
      e.graphics.lineTo(this.pth[s - 1] * 24 + 12, this.pth[s] * 24 + 12), s--, s--;
  }
  deathFX() {
  }
  SetGrid() {
    this.gx = h(Math.floor(this.x / this.game.gridSize)), this.gy = h(Math.floor(this.y / this.game.gridSize));
  }
  setAngle() {
  }
  damageFX() {
    this.coreTemp > this.baseTemp && (this.coreTemp -= 0.02), this.coreTemp < this.baseTemp && (this.coreTemp += 0.02), this.burn > 0 && (--this.burn, this.freeze = 0, this.health -= 0.5), this.freeze > 0 && --this.freeze, this.rad > 0 && (--this.rad, this.health -= 0.3), this.poison > 0 && (this.bMetal ? this.poison = 0 : (--this.poison, this.health -= 0.5)), this.bFrozen && this.freeze <= 0 && this.unFreeze(), this.freeze > 0 ? this.speed = 0 : this.poison > 0 ? this.speed = this.baseSpeed * 0.6 : this.rad > 0 ? this.bMetal ? this.speed = this.baseSpeed * 0.4 : this.speed = this.baseSpeed * 0.6 : this.burn > 0 ? this.bMetal || (this.speed = this.baseSpeed * 1.2) : this.speed = this.baseSpeed, this.health <= 0 && (this.bFrozen && this.unFreeze(), this.deathFX(), this.game.killEnemy(this), this.game.player.funds += this.cValue), this.makeFX();
  }
  Tick(e) {
    this.SetGrid(), this.getDir(), this.Move(e), this.damageFX(), this.hv.update(Math.floor(this.health));
  }
  findPath() {
    this.pth = this.game.path.findPath(this.gx, this.gy, this.endPoint.gx, this.endPoint.gy);
  }
  unFreeze() {
    this.visible = !0, this.parent.removeChild(this.freezeAnim), this.bFrozen = !1, this.freezeAnim = null;
  }
  takeDamage(e, t, s = null) {
    this.health -= e, this.rad += t.rad * this.radiation_resistance, this.coreTemp += t.burn * this.heat_resistance, this.poison += t.poison * this.poison_resistance, this.coreTemp -= t.freeze * this.cold_resistance, s != null && (s.exp += Math.floor(e)), this.coreTemp > this.upperTemp && (this.burn = this.coreTemp), this.coreTemp < this.lowerTemp && (this.freeze = Math.abs(this.coreTemp * 0.2), this.bFrozen || (this.bFrozen = !0, this.dofreeze())), this.health <= 0 && (this.bFrozen && this.unFreeze(), this.deathFX(), this.game.killEnemy(this), this.game.player.funds += this.cValue, this.game.levelScore += this.cValue * 2);
  }
  snapToGrid() {
    this.SetGrid(), this.x = this.gx * this.game.gridSize + this.game.gridSize * 0.5, this.y = this.gy * this.game.gridSize + this.game.gridSize * 0.5;
  }
  dofreeze() {
    this.bFrozen = !0, this.bMetal ? this.freezeAnim = new he() : (this.visible = !1, this.freezeAnim = new ae()), this.freezeAnim.x = this.x, this.freezeAnim.y = this.y + 3, this.freezeAnim.gotoAndStop(Math.floor(Math.random() * 5)), this.parent.addChild(this.freezeAnim);
  }
}
class Ht extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Ut extends b {
  __fi() {
    this.targetFrame = 0, this.owner = null, this.bRunning = !1;
  }
  constructor() {
    super(), this.owner = g(p, this.parent), this.owner.frameController = this, this.addEventListener(w.ENTER_FRAME, this.tick), this.alpha = 0, this.visible = !1, this.mouseEnabled = !1;
  }
  tick(e) {
    this.bRunning && (this.owner.currentFrame != this.targetFrame ? this.alpha < 1 ? this.alpha += 0.02 : this.owner.gotoAndStop(this.targetFrame) : this.alpha > 0 ? this.alpha -= 0.02 : (this.visible = !1, this.bRunning = !1, this.alpha = 0, this.owner.arrived(this.targetFrame)));
  }
  getFrame(e) {
    e = h(e), this.targetFrame = h(e), this.bRunning = !0, this.visible = !0;
  }
}
class ae extends d {
  constructor() {
    super();
  }
}
class he extends d {
  constructor() {
    super();
  }
}
class jt extends b {
  __fi() {
    this.clearRec = null, this.sparks = new Array(), this.bcanvas = null, this.Canvas = null, this.tracers = new Array(), this.tbcanvas = null, this.beams = new Array(), this.Canvas2 = null, this.game = null;
  }
  constructor() {
    super(), this.mouseEnabled = !1, this.game = g(p, this.parent), this.clearRec = new B(0, 0, 640, 480), this.addEventListener(w.ENTER_FRAME, this.Tick, !1, 0, !0);
  }
  renderTracer(e) {
    var t = void 0, s = void 0, l = void 0, o = void 0, v = void 0, u = void 0, n = void 0;
    t = e.wgt, s = (e.x + e.ex) * 0.5, l = (e.y + e.ey) * 0.5, o = (e.x + s) * 0.5, v = (e.y + l) * 0.5, u = (e.ex + s) * 0.5, n = (e.ey + l) * 0.5, this.Canvas.graphics.lineStyle(t * Math.random(), 16764023, e.life * 0.04), this.Canvas.graphics.moveTo(e.x, e.y), this.Canvas.graphics.lineTo(o, v), this.Canvas.graphics.lineStyle(t * Math.random(), 16764023, e.life * 0.01), this.Canvas.graphics.lineTo(s, l), this.Canvas.graphics.lineStyle(t * Math.random(), 16777079, e.life * 0.03), this.Canvas.graphics.lineTo(u, n), this.Canvas.graphics.lineStyle(t * Math.random(), 16764023, e.life * 0.05), this.Canvas.graphics.lineTo(e.ex, e.ey);
  }
  removeBeam(e) {
    var t = void 0, s = void 0, l = void 0;
    for (t = this.beams.length, s = 0; s < t; )
      l = this.beams[s], l == e && this.beams.splice(s, 1), s++;
  }
  removeTracer(e) {
    var t = void 0, s = void 0, l = void 0;
    for (t = this.tracers.length, s = 0; s < t; )
      l = this.tracers[s], l == e && this.tracers.splice(s, 1), s++;
  }
  renderBeam(e) {
    var t = void 0, s = void 0, l = void 0, o = void 0, v = void 0, u = NaN, n = NaN, W = void 0;
    for (t = e.life / 100, s = e.sx, l = e.sy, o = e.vx, v = e.vy, this.Canvas2.graphics.lineStyle(1 + t, 16777215, t + 0.1), this.Canvas2.graphics.moveTo(s, l), W = 0; W < 9; )
      t -= 0.05, this.Canvas.graphics.lineStyle(3 + t * 2, 16777215, t + 0.8), u = -6 + Math.random() * 12, n = -6 + Math.random() * 12, n = n - Math.sin(W * 0.5) * 5, s += o, l += v, s += u, l += n, this.Canvas2.graphics.lineTo(s, l), W++;
    this.Canvas2.graphics.lineTo(e.ex, e.ey);
  }
  Tick(e) {
    var t = void 0, s = void 0, l = void 0, o = void 0, v = void 0, u = void 0, n = void 0, W = void 0;
    for (this.Canvas.graphics.clear(), this.Canvas2.graphics.clear(), t = this.sparks.length, s = 0; s < t; )
      n = this.sparks[s], n != null && (this.renderSpark(n), n.Tick()), s++;
    for (l = this.tracers.length, o = 0; o < l; )
      n = this.tracers[o], n != null && (this.renderTracer(n), n.Tick()), o++;
    for (v = this.beams.length, u = 0; u < v; )
      W = this.beams[u], W != null && (this.renderBeam(W), W.Tick()), u++;
  }
  removeSpark(e) {
    var t = void 0, s = void 0, l = void 0;
    for (t = this.sparks.length, s = 0; s < t; )
      l = this.sparks[s], l == e && this.sparks.splice(s, 1), s++;
  }
  renderSpark(e) {
    this.Canvas.graphics.lineStyle(1, 16777164, e.life / 10), this.Canvas.graphics.moveTo(e.x, e.y), this.Canvas.graphics.lineTo(e.x - e.vx, e.y - e.vy);
  }
}
class p extends d {
  __fi() {
    this.Enemies = new Array(), this.spawners = new Array(), this.WaveControl = null, this.hud1 = null, this.path = null, this.gridSize = h(32), this.Soundbox = null, this.instruction = null, this.Options = null, this.SpritePool = null, this.renderElements = new Array(), this.Towers = new Array(), this.Selected = null, this.bLoggedIn = !1, this.fx = null, this.levelType = "forest", this.player = null, this.frameController = null, this.mainTick = null, this.gameWidth = h(25), this.levelHealth = 1e3, this.playerName = null, this.firstLevelFrame = h(10), this.levelArray = null, this.decalFX = null, this.lastTick = NaN, this._removed_ad_id = "", this.levelScore = 0, this.gameSpeed = NaN, this.lastLevelFrame = h(40), this.level = null, this.Projectiles = new Array(), this.currentLevel = 0, this.endPoints = new Array(), this.pausePage = null, this.gameHeight = h(25), this.bFirstPlay = !1, this.startPoints = new Array(), this.game = null, this.baseSpeed = 1;
  }
  constructor() {
    super(), this.addFrameScript(9, this.frame10, 10, this.frame11, 11, this.frame12, 12, this.frame13, 13, this.frame14, 14, this.frame15, 15, this.frame16, 16, this.frame17, 17, this.frame18, 18, this.frame19, 19, this.frame20, 20, this.frame21, 21, this.frame22, 22, this.frame23, 23, this.frame24, 24, this.frame25, 26, this.frame27, 28, this.frame29, 30, this.frame31), this.mouseEnabled = !1, this.gotoAndStop(1), this.game = this, this.SpritePool = new ut(), this.player = new We(!1, this), this.Soundbox = new mt(this), this.mainTick = new se(24, 0), this.mainTick.addEventListener(ie.TIMER, this.tick, !1, 0, !0), this.path = new ue(this), this.WaveControl = new Ft(this), this.Soundbox.playbgm();
  }
  createEnemy(e, t) {
    t = h(t);
    var s = void 0, l = null, o = null;
    switch (l = this.getRandomStart(), o = this.getRandomEnd(), e) {
      case "enemy1":
        s = new ke(this, o);
        break;
      case "enemy2":
        s = new Fe(this, o);
        break;
      case "enemy3":
        s = new Ne(this, o);
        break;
      case "enemy4":
        s = new Ae(this, o);
        break;
      case "enemy5":
        s = new Le(this, o);
        break;
      case "bot_1":
        s = new Te(this, o);
        break;
      case "Jeep1":
        s = new oe(this, o);
        break;
      case "Jeep2":
        s = new le(this, o);
        break;
      case "Jeep3":
        s = new ne(this, o);
        break;
      case "Jeep4":
        s = new re(this, o);
        break;
      case "Tank1":
        s = new ge(this, o);
        break;
      case "Tank2":
        s = new pe(this, o);
        break;
      case "Tank3":
        s = new ye(this, o);
    }
    s.x = l.x, s.y = l.y, this.level.addChild(s), this.Enemies.push(s), s.health *= 1 + t * 0.1;
  }
  createNewWave() {
  }
  getRandomStart() {
    var e = NaN, t = h(0);
    return e = this.startPoints.length - 0.01, t = h(Math.floor(Math.random() * e)), this.startPoints[t];
  }
  killSpawner(e) {
    var t = void 0, s = void 0;
    for (t = this.spawners.length, s = 0; s < t; ) {
      if (this.spawners[s] == e) {
        this.spawners.splice(s, 1);
        break;
      }
      s++;
    }
  }
  startLevel() {
    this.hidePaused(), this.lastTick = te(), this.mainTick.start();
  }
  frame10() {
    this.WaveControl.addWave("enemy3", 4, 15, 300), this.WaveControl.addWave("enemy3", 4, 15, 300), this.WaveControl.addWave("enemy3", 5, 15, 300), this.WaveControl.addWave("enemy4", 15, 15, 800), this.WaveControl.addWave("enemy4", 8, 15, 500), this.WaveControl.addWave("enemy4", 8, 15, 500), this.WaveControl.addWave("enemy2", 7, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 1e3), this.WaveControl.addWave("enemy1", 8, 15, 1e3), this.WaveControl.addWave("enemy1", 8, 15, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("Jeep4", 3, 45, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 500), this.WaveControl.addWave("enemy5", 12, 15, 500), this.WaveControl.addWave("Jeep1", 5, 45, 50), this.decalFX.drawPath("forest");
  }
  frame14() {
    this.WaveControl.addWave("enemy4", 4, 15, 500), this.WaveControl.addWave("enemy2", 4, 15, 500), this.WaveControl.addWave("enemy3", 4, 15, 800), this.WaveControl.addWave("enemy4", 4, 20, 1e3), this.WaveControl.addWave("enemy5", 4, 20, 1e3), this.WaveControl.addWave("enemy1", 6, 15, 1e3), this.WaveControl.addWave("enemy4", 6, 10, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy3", 8, 20, 1e3), this.WaveControl.addWave("enemy2", 8, 15, 1e3), this.WaveControl.addWave("enemy1", 7, 10, 1e3), this.WaveControl.addWave("enemy5", 8, 15, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 3, 55, 2e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy5", 12, 20, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 5, 25, 15), this.WaveControl.addWave("enemy2", 7, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 2e3), this.WaveControl.addWave("enemy3", 12, 15, 1e3), this.WaveControl.addWave("enemy4", 12, 20, 1e3), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 1e3), this.WaveControl.addWave("Jeep1", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 14, 15, 1e3), this.WaveControl.addWave("enemy5", 13, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 200), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("enemy1", 13, 15, 1e3), this.WaveControl.addWave("enemy4", 14, 15, 1e3), this.WaveControl.addWave("enemy2", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 15, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 30, 15), this.WaveControl.addWave("enemy1", 7, 30, 1e3), this.WaveControl.addWave("Jeep3", 3, 95, 50), this.WaveControl.addWave("Jeep4", 3, 95, 1e3), this.WaveControl.addWave("enemy1", 16, 15, 1e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy3", 20, 30, 1e3), this.WaveControl.addWave("enemy4", 17, 20, 1e3), this.WaveControl.addWave("Jeep1", 7, 45, 100), this.decalFX.drawPath("ice");
  }
  killTower(e) {
    var t = void 0, s = void 0;
    for (t = this.Towers.length, s = 0; s < t; ) {
      if (this.Towers[s] == e) {
        this.Towers.splice(s, 1), this.level.removeChild(e);
        break;
      }
      s++;
    }
  }
  frame15() {
    this.WaveControl.addWave("enemy4", 2, 15, 1e3), this.WaveControl.addWave("enemy2", 2, 15, 1e3), this.WaveControl.addWave("enemy3", 4, 15, 1e3), this.WaveControl.addWave("enemy4", 4, 20, 1e3), this.WaveControl.addWave("enemy5", 4, 20, 1e3), this.WaveControl.addWave("enemy1", 6, 15, 1e3), this.WaveControl.addWave("enemy4", 6, 10, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy3", 8, 20, 1e3), this.WaveControl.addWave("enemy2", 8, 15, 1e3), this.WaveControl.addWave("enemy1", 7, 10, 1e3), this.WaveControl.addWave("enemy5", 8, 15, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 3, 55, 2e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy5", 12, 20, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 5, 25, 15), this.WaveControl.addWave("enemy2", 7, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 2e3), this.WaveControl.addWave("enemy3", 12, 15, 1e3), this.WaveControl.addWave("enemy4", 12, 20, 1e3), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 1e3), this.WaveControl.addWave("Jeep1", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 14, 15, 1e3), this.WaveControl.addWave("enemy5", 13, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 200), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("enemy1", 13, 15, 1e3), this.WaveControl.addWave("enemy4", 14, 15, 1e3), this.WaveControl.addWave("enemy2", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 15, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 30, 15), this.WaveControl.addWave("enemy1", 7, 30, 1e3), this.WaveControl.addWave("Jeep3", 3, 95, 50), this.WaveControl.addWave("Jeep4", 3, 95, 1e3), this.WaveControl.addWave("enemy1", 16, 15, 1e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy3", 20, 30, 1e3), this.WaveControl.addWave("enemy4", 17, 20, 1e3), this.WaveControl.addWave("Jeep1", 7, 45, 2e3), this.WaveControl.addWave("enemy4", 10, 40, 25), this.WaveControl.addWave("enemy5", 10, 40, 1e3), this.WaveControl.addWave("Jeep3", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 10, 20, 100), this.WaveControl.addWave("Jeep2", 2, 45, 100), this.decalFX.drawPath("sand");
  }
  frame16() {
    this.WaveControl.addWave("enemy4", 2, 15, 1e3), this.WaveControl.addWave("enemy2", 2, 15, 1e3), this.WaveControl.addWave("enemy3", 4, 15, 1e3), this.WaveControl.addWave("enemy4", 4, 20, 1e3), this.WaveControl.addWave("enemy5", 4, 20, 1e3), this.WaveControl.addWave("enemy1", 6, 15, 1e3), this.WaveControl.addWave("enemy4", 6, 10, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy3", 8, 20, 1e3), this.WaveControl.addWave("enemy2", 8, 15, 1e3), this.WaveControl.addWave("enemy1", 7, 10, 1e3), this.WaveControl.addWave("enemy5", 8, 15, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 3, 55, 2e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy5", 12, 20, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 5, 25, 15), this.WaveControl.addWave("enemy2", 7, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 2e3), this.WaveControl.addWave("enemy3", 12, 15, 1e3), this.WaveControl.addWave("enemy4", 12, 20, 1e3), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 1e3), this.WaveControl.addWave("Jeep1", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 14, 15, 1e3), this.WaveControl.addWave("enemy5", 13, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 200), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("enemy1", 13, 15, 1e3), this.WaveControl.addWave("enemy4", 14, 15, 1e3), this.WaveControl.addWave("enemy2", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 15, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 30, 15), this.WaveControl.addWave("enemy1", 7, 30, 1e3), this.WaveControl.addWave("Jeep3", 3, 95, 50), this.WaveControl.addWave("Jeep4", 3, 95, 1e3), this.WaveControl.addWave("enemy1", 16, 15, 1e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy3", 20, 30, 1e3), this.WaveControl.addWave("enemy4", 17, 20, 1e3), this.WaveControl.addWave("Jeep1", 7, 45, 2e3), this.WaveControl.addWave("enemy4", 10, 40, 25), this.WaveControl.addWave("enemy5", 10, 40, 1e3), this.WaveControl.addWave("Jeep3", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 10, 20, 100), this.WaveControl.addWave("Jeep2", 2, 45, 1e3), this.WaveControl.addWave("enemy1", 18, 15, 1e3), this.WaveControl.addWave("enemy2", 17, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy5", 15, 15, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 30), this.decalFX.drawPath("forest");
  }
  frame18() {
    this.WaveControl.addWave("enemy4", 2, 15, 1e3), this.WaveControl.addWave("enemy2", 2, 15, 1e3), this.WaveControl.addWave("enemy3", 4, 15, 1e3), this.WaveControl.addWave("enemy4", 4, 20, 1e3), this.WaveControl.addWave("enemy5", 4, 20, 1e3), this.WaveControl.addWave("enemy1", 6, 15, 1e3), this.WaveControl.addWave("enemy4", 6, 10, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy3", 8, 20, 1e3), this.WaveControl.addWave("enemy2", 8, 15, 1e3), this.WaveControl.addWave("enemy1", 7, 10, 1e3), this.WaveControl.addWave("enemy5", 8, 15, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 3, 55, 2e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy5", 12, 20, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 5, 25, 15), this.WaveControl.addWave("enemy2", 7, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 2e3), this.WaveControl.addWave("enemy3", 12, 15, 1e3), this.WaveControl.addWave("enemy4", 12, 20, 1e3), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 1e3), this.WaveControl.addWave("Jeep1", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 14, 15, 1e3), this.WaveControl.addWave("enemy5", 13, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 200), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("enemy1", 13, 15, 1e3), this.WaveControl.addWave("enemy4", 14, 15, 1e3), this.WaveControl.addWave("enemy2", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 15, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 30, 15), this.WaveControl.addWave("enemy1", 7, 30, 1e3), this.WaveControl.addWave("Jeep3", 3, 95, 50), this.WaveControl.addWave("Jeep4", 3, 95, 1e3), this.WaveControl.addWave("enemy1", 16, 15, 1e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy3", 20, 30, 1e3), this.WaveControl.addWave("enemy4", 17, 20, 1e3), this.WaveControl.addWave("Jeep1", 7, 45, 2e3), this.WaveControl.addWave("enemy4", 10, 40, 25), this.WaveControl.addWave("enemy5", 10, 40, 1e3), this.WaveControl.addWave("Jeep3", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 10, 20, 100), this.WaveControl.addWave("Jeep2", 2, 45, 1e3), this.WaveControl.addWave("enemy1", 18, 15, 1e3), this.WaveControl.addWave("enemy2", 17, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy5", 15, 15, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy4", 15, 15, 1e3), this.WaveControl.addWave("enemy5", 16, 15, 1e3), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("Tank2", 2, 65, 3e3), this.WaveControl.addWave("enemy4", 10, 35, 20), this.WaveControl.addWave("enemy5", 10, 35, 1e3), this.WaveControl.addWave("Jeep3", 7, 45, 2e3), this.WaveControl.addWave("Jeep2", 7, 45, 2500), this.WaveControl.addWave("Tank1", 2, 65, 30), this.decalFX.drawPath("sand");
  }
  frame12() {
    this.WaveControl.addWave("enemy3", 2, 15, 1e3), this.WaveControl.addWave("enemy1", 2, 15, 1e3), this.WaveControl.addWave("enemy4", 4, 15, 1e3), this.WaveControl.addWave("enemy2", 4, 20, 1e3), this.WaveControl.addWave("enemy5", 4, 20, 1e3), this.WaveControl.addWave("enemy3", 6, 15, 1e3), this.WaveControl.addWave("enemy2", 6, 10, 1e3), this.WaveControl.addWave("enemy4", 6, 15, 1e3), this.WaveControl.addWave("enemy5", 8, 20, 1e3), this.WaveControl.addWave("enemy1", 8, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 10, 1e3), this.WaveControl.addWave("enemy5", 8, 15, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 3, 55, 2e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy5", 12, 20, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 5, 25, 15), this.WaveControl.addWave("enemy2", 7, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 2e3), this.WaveControl.addWave("enemy3", 12, 15, 1e3), this.WaveControl.addWave("enemy4", 12, 20, 1e3), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 1e3), this.WaveControl.addWave("Jeep1", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 14, 15, 1e3), this.WaveControl.addWave("enemy5", 13, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 200), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("enemy1", 13, 15, 1e3), this.WaveControl.addWave("enemy4", 14, 15, 1e3), this.WaveControl.addWave("enemy2", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 15, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 50), this.decalFX.drawPath("sand");
  }
  getRandomEnd() {
    var e = NaN, t = h(0);
    return e = this.endPoints.length - 0.01, t = h(Math.floor(Math.random() * e)), this.endPoints[t];
  }
  sortActors() {
    var e = $(0), t = void 0, s = void 0;
    for (e = $($(this.level.numChildren)), t = 0; t < e - 1; ) {
      for (s = t + 1; s < e; )
        this.level.getChildAt(t).y > this.level.getChildAt(s).y && this.level.swapChildrenAt(t, s), s++;
      t++;
    }
  }
  frame11() {
    this.WaveControl.addWave("enemy3", 2, 15, 1e3), this.WaveControl.addWave("enemy1", 2, 15, 1e3), this.WaveControl.addWave("enemy4", 4, 15, 1e3), this.WaveControl.addWave("enemy2", 4, 20, 1e3), this.WaveControl.addWave("enemy5", 4, 20, 1e3), this.WaveControl.addWave("enemy3", 6, 15, 1e3), this.WaveControl.addWave("enemy2", 6, 10, 1e3), this.WaveControl.addWave("enemy4", 6, 15, 1e3), this.WaveControl.addWave("enemy5", 8, 20, 1e3), this.WaveControl.addWave("enemy1", 8, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 10, 1e3), this.WaveControl.addWave("enemy5", 8, 15, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 3, 55, 2e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy5", 12, 20, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 5, 25, 15), this.WaveControl.addWave("enemy2", 7, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 2e3), this.WaveControl.addWave("enemy3", 12, 15, 1e3), this.WaveControl.addWave("enemy4", 12, 20, 1e3), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 1e3), this.WaveControl.addWave("Jeep1", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 14, 15, 1e3), this.WaveControl.addWave("enemy5", 13, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 200), this.WaveControl.addWave("Jeep2", 4, 45, 50), this.decalFX.drawPath("sand");
  }
  frame17() {
    this.WaveControl.addWave("enemy4", 2, 15, 1e3), this.WaveControl.addWave("enemy2", 2, 15, 1e3), this.WaveControl.addWave("enemy3", 4, 15, 1e3), this.WaveControl.addWave("enemy4", 4, 20, 1e3), this.WaveControl.addWave("enemy5", 4, 20, 1e3), this.WaveControl.addWave("enemy1", 6, 15, 1e3), this.WaveControl.addWave("enemy4", 6, 10, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy3", 8, 20, 1e3), this.WaveControl.addWave("enemy2", 8, 15, 1e3), this.WaveControl.addWave("enemy1", 7, 10, 1e3), this.WaveControl.addWave("enemy5", 8, 15, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 3, 55, 2e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy5", 12, 20, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 5, 25, 15), this.WaveControl.addWave("enemy2", 7, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 2e3), this.WaveControl.addWave("enemy3", 12, 15, 1e3), this.WaveControl.addWave("enemy4", 12, 20, 1e3), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 1e3), this.WaveControl.addWave("Jeep1", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 14, 15, 1e3), this.WaveControl.addWave("enemy5", 13, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 200), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("enemy1", 13, 15, 1e3), this.WaveControl.addWave("enemy4", 14, 15, 1e3), this.WaveControl.addWave("enemy2", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 15, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 30, 15), this.WaveControl.addWave("enemy1", 7, 30, 1e3), this.WaveControl.addWave("Jeep3", 3, 95, 50), this.WaveControl.addWave("Jeep4", 3, 95, 1e3), this.WaveControl.addWave("enemy1", 16, 15, 1e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy3", 20, 30, 1e3), this.WaveControl.addWave("enemy4", 17, 20, 1e3), this.WaveControl.addWave("Jeep1", 7, 45, 2e3), this.WaveControl.addWave("enemy4", 10, 40, 25), this.WaveControl.addWave("enemy5", 10, 40, 1e3), this.WaveControl.addWave("Jeep3", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 10, 20, 100), this.WaveControl.addWave("Jeep2", 2, 45, 1e3), this.WaveControl.addWave("enemy1", 18, 15, 1e3), this.WaveControl.addWave("enemy2", 17, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy5", 15, 15, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy4", 15, 15, 1e3), this.WaveControl.addWave("enemy5", 16, 15, 1e3), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("Tank2", 2, 65, 30), this.decalFX.drawPath("forest");
  }
  arrived(e) {
    e >= this.firstLevelFrame && e <= this.lastLevelFrame ? (this.startLevel(), this.Soundbox.playNoise(), this.Soundbox.stopbgm()) : (this.Soundbox.playbgm(), this.Soundbox.stopNoise());
  }
  frame23() {
    this.WaveControl.addWave("enemy4", 2, 15, 1e3), this.WaveControl.addWave("enemy2", 2, 15, 1e3), this.WaveControl.addWave("enemy3", 4, 15, 1e3), this.WaveControl.addWave("enemy4", 4, 20, 1e3), this.WaveControl.addWave("enemy5", 4, 20, 1e3), this.WaveControl.addWave("enemy1", 6, 15, 1e3), this.WaveControl.addWave("enemy4", 6, 10, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy3", 8, 20, 1e3), this.WaveControl.addWave("enemy2", 8, 15, 1e3), this.WaveControl.addWave("enemy1", 7, 10, 1e3), this.WaveControl.addWave("enemy5", 8, 15, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 3, 55, 2e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy5", 12, 20, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 5, 25, 15), this.WaveControl.addWave("enemy2", 7, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 2e3), this.WaveControl.addWave("enemy3", 12, 15, 1e3), this.WaveControl.addWave("enemy4", 12, 20, 1e3), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 1e3), this.WaveControl.addWave("Jeep1", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 14, 15, 1e3), this.WaveControl.addWave("enemy5", 13, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 200), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("enemy1", 13, 15, 1e3), this.WaveControl.addWave("enemy4", 14, 15, 1e3), this.WaveControl.addWave("enemy2", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 15, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 30, 15), this.WaveControl.addWave("enemy1", 7, 30, 1e3), this.WaveControl.addWave("Jeep3", 3, 95, 50), this.WaveControl.addWave("Jeep4", 3, 95, 1e3), this.WaveControl.addWave("enemy1", 16, 15, 1e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy3", 20, 30, 1e3), this.WaveControl.addWave("enemy4", 17, 20, 1e3), this.WaveControl.addWave("Jeep1", 7, 45, 2e3), this.WaveControl.addWave("enemy4", 10, 40, 25), this.WaveControl.addWave("enemy5", 10, 40, 1e3), this.WaveControl.addWave("Jeep3", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 10, 20, 100), this.WaveControl.addWave("Jeep2", 2, 45, 1e3), this.WaveControl.addWave("enemy1", 18, 15, 1e3), this.WaveControl.addWave("enemy2", 17, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy5", 15, 15, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy4", 15, 15, 1e3), this.WaveControl.addWave("enemy5", 16, 15, 1e3), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("Tank2", 2, 65, 3e3), this.WaveControl.addWave("enemy4", 10, 35, 20), this.WaveControl.addWave("enemy5", 10, 35, 1e3), this.WaveControl.addWave("Jeep3", 7, 45, 2e3), this.WaveControl.addWave("Jeep2", 7, 45, 2500), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("enemy3", 20, 15, 1e3), this.WaveControl.addWave("enemy2", 18, 15, 700), this.WaveControl.addWave("enemy1", 19, 15, 1e3), this.WaveControl.addWave("Jeep3", 8, 45, 2e3), this.WaveControl.addWave("Tank3", 2, 65, 3e3), this.WaveControl.addWave("Jeep3", 7, 45, 2e3), this.WaveControl.addWave("Jeep4", 7, 45, 1e3), this.WaveControl.addWave("Jeep2", 7, 45, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("bot_1", 3, 45, 4e3), this.WaveControl.addWave("Jeep2", 7, 45, 1e3), this.WaveControl.addWave("Jeep3", 7, 45, 2e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("Tank2", 2, 65, 3e3), this.WaveControl.addWave("bot_1", 3, 45, 4e3), this.WaveControl.addWave("Jeep4", 7, 45, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("Jeep1", 7, 45, 2e3), this.WaveControl.addWave("bot_1", 3, 45, 4e3), this.WaveControl.addWave("Tank3", 2, 65, 3e3), this.WaveControl.addWave("Tank1", 4, 65, 3e3), this.WaveControl.addWave("Tank2", 4, 65, 3e3), this.WaveControl.addWave("Tank3", 4, 65, 3e3), this.WaveControl.addWave("bot_1", 3, 45, 4e3), this.WaveControl.addWave("enemy3", 45, 15, 10), this.decalFX.drawPath("sand");
  }
  frame24() {
    this.decalFX.drawPath("sand");
  }
  frame25() {
    this.WaveControl.addWave("enemy4", 2, 15, 300), this.WaveControl.addWave("enemy2", 2, 15, 300), this.WaveControl.addWave("enemy3", 4, 15, 400), this.WaveControl.addWave("enemy4", 4, 20, 400), this.WaveControl.addWave("enemy5", 4, 20, 500), this.WaveControl.addWave("enemy1", 6, 15, 500), this.WaveControl.addWave("enemy4", 6, 10, 500), this.WaveControl.addWave("enemy5", 6, 15, 500), this.WaveControl.addWave("enemy3", 8, 20, 600), this.WaveControl.addWave("enemy2", 8, 15, 600), this.WaveControl.addWave("enemy1", 7, 10, 600), this.WaveControl.addWave("enemy5", 8, 15, 600), this.WaveControl.addWave("enemy4", 10, 15, 800), this.WaveControl.addWave("Jeep3", 3, 55, 800), this.WaveControl.addWave("enemy1", 10, 15, 800), this.WaveControl.addWave("enemy5", 10, 20, 800), this.WaveControl.addWave("enemy4", 10, 15, 800), this.WaveControl.addWave("enemy3", 8, 25, 15), this.WaveControl.addWave("enemy2", 8, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 800), this.WaveControl.addWave("Jeep3", 5, 45, 800), this.WaveControl.addWave("enemy2", 8, 25, 25), this.WaveControl.addWave("enemy1", 8, 20, 1e3), this.WaveControl.addWave("Jeep2", 4, 45, 800), this.WaveControl.addWave("enemy3", 12, 15, 800), this.WaveControl.addWave("enemy4", 12, 20, 800), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 800), this.WaveControl.addWave("Jeep1", 5, 45, 2e3), this.WaveControl.addWave("Tank1", 2, 45, 25);
  }
  frame13() {
    this.WaveControl.addWave("Jeep1", 2, 15, 1e3), this.WaveControl.addWave("enemy1", 2, 15, 1e3), this.WaveControl.addWave("enemy4", 4, 15, 1e3), this.WaveControl.addWave("enemy2", 4, 20, 1e3), this.WaveControl.addWave("enemy5", 4, 20, 1e3), this.WaveControl.addWave("enemy3", 6, 15, 1e3), this.WaveControl.addWave("enemy2", 6, 10, 1e3), this.WaveControl.addWave("enemy4", 6, 15, 1e3), this.WaveControl.addWave("enemy5", 8, 20, 1e3), this.WaveControl.addWave("enemy1", 8, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 10, 1e3), this.WaveControl.addWave("enemy5", 8, 15, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 3, 55, 2e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy5", 12, 20, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 5, 25, 15), this.WaveControl.addWave("enemy2", 7, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 2e3), this.WaveControl.addWave("enemy3", 12, 15, 1e3), this.WaveControl.addWave("enemy4", 12, 20, 1e3), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 1e3), this.WaveControl.addWave("Jeep1", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 14, 15, 1e3), this.WaveControl.addWave("enemy5", 13, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 200), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("enemy1", 13, 15, 1e3), this.WaveControl.addWave("enemy4", 14, 15, 1e3), this.WaveControl.addWave("enemy2", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 15, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 30, 15), this.WaveControl.addWave("enemy1", 7, 30, 1e3), this.WaveControl.addWave("Jeep3", 3, 95, 50), this.WaveControl.addWave("Jeep4", 3, 95, 50), this.decalFX.drawPath("sand");
  }
  frame21() {
    this.WaveControl.addWave("enemy4", 2, 15, 1e3), this.WaveControl.addWave("enemy2", 2, 15, 1e3), this.WaveControl.addWave("enemy3", 4, 15, 1e3), this.WaveControl.addWave("enemy4", 4, 20, 1e3), this.WaveControl.addWave("enemy5", 4, 20, 1e3), this.WaveControl.addWave("enemy1", 6, 15, 1e3), this.WaveControl.addWave("enemy4", 6, 10, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy3", 8, 20, 1e3), this.WaveControl.addWave("enemy2", 8, 15, 1e3), this.WaveControl.addWave("enemy1", 7, 10, 1e3), this.WaveControl.addWave("enemy5", 8, 15, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 3, 55, 2e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy5", 12, 20, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 5, 25, 15), this.WaveControl.addWave("enemy2", 7, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 2e3), this.WaveControl.addWave("enemy3", 12, 15, 1e3), this.WaveControl.addWave("enemy4", 12, 20, 1e3), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 1e3), this.WaveControl.addWave("Jeep1", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 14, 15, 1e3), this.WaveControl.addWave("enemy5", 13, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 200), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("enemy1", 13, 15, 1e3), this.WaveControl.addWave("enemy4", 14, 15, 1e3), this.WaveControl.addWave("enemy2", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 15, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 30, 15), this.WaveControl.addWave("enemy1", 7, 30, 1e3), this.WaveControl.addWave("Jeep3", 3, 95, 50), this.WaveControl.addWave("Jeep4", 3, 95, 1e3), this.WaveControl.addWave("enemy1", 16, 15, 1e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy3", 20, 30, 1e3), this.WaveControl.addWave("enemy4", 17, 20, 1e3), this.WaveControl.addWave("Jeep1", 7, 45, 2e3), this.WaveControl.addWave("enemy4", 10, 40, 25), this.WaveControl.addWave("enemy5", 10, 40, 1e3), this.WaveControl.addWave("Jeep3", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 10, 20, 100), this.WaveControl.addWave("Jeep2", 2, 45, 1e3), this.WaveControl.addWave("enemy1", 18, 15, 1e3), this.WaveControl.addWave("enemy2", 17, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy5", 15, 15, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy4", 15, 15, 1e3), this.WaveControl.addWave("enemy5", 16, 15, 1e3), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("Tank2", 2, 65, 3e3), this.WaveControl.addWave("enemy4", 10, 35, 20), this.WaveControl.addWave("enemy5", 10, 35, 1e3), this.WaveControl.addWave("Jeep3", 7, 45, 2e3), this.WaveControl.addWave("Jeep2", 7, 45, 2500), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("enemy3", 20, 15, 1e3), this.WaveControl.addWave("enemy2", 18, 15, 700), this.WaveControl.addWave("enemy1", 19, 15, 1e3), this.WaveControl.addWave("Jeep3", 8, 45, 2e3), this.WaveControl.addWave("Tank3", 2, 65, 3e3), this.WaveControl.addWave("Jeep3", 7, 45, 2e3), this.WaveControl.addWave("Jeep4", 7, 45, 1e3), this.WaveControl.addWave("Jeep2", 7, 45, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("bot_1", 3, 45, 4e3), this.WaveControl.addWave("Jeep2", 7, 45, 1e3), this.WaveControl.addWave("Jeep3", 7, 45, 2e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("Tank2", 2, 65, 3e3), this.WaveControl.addWave("bot_1", 3, 45, 40), this.decalFX.drawPath("rocky");
  }
  frame22() {
    this.WaveControl.addWave("enemy4", 2, 15, 1e3), this.WaveControl.addWave("enemy2", 2, 15, 1e3), this.WaveControl.addWave("enemy3", 4, 15, 1e3), this.WaveControl.addWave("enemy4", 4, 20, 1e3), this.WaveControl.addWave("enemy5", 4, 20, 1e3), this.WaveControl.addWave("enemy1", 6, 15, 1e3), this.WaveControl.addWave("enemy4", 6, 10, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy3", 8, 20, 1e3), this.WaveControl.addWave("enemy2", 8, 15, 1e3), this.WaveControl.addWave("enemy1", 7, 10, 1e3), this.WaveControl.addWave("enemy5", 8, 15, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 3, 55, 2e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy5", 12, 20, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 5, 25, 15), this.WaveControl.addWave("enemy2", 7, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 2e3), this.WaveControl.addWave("enemy3", 12, 15, 1e3), this.WaveControl.addWave("enemy4", 12, 20, 1e3), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 1e3), this.WaveControl.addWave("Jeep1", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 14, 15, 1e3), this.WaveControl.addWave("enemy5", 13, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 200), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("enemy1", 13, 15, 1e3), this.WaveControl.addWave("enemy4", 14, 15, 1e3), this.WaveControl.addWave("enemy2", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 15, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 30, 15), this.WaveControl.addWave("enemy1", 7, 30, 1e3), this.WaveControl.addWave("Jeep3", 3, 95, 50), this.WaveControl.addWave("Jeep4", 3, 95, 1e3), this.WaveControl.addWave("enemy1", 16, 15, 1e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy3", 20, 30, 1e3), this.WaveControl.addWave("enemy4", 17, 20, 1e3), this.WaveControl.addWave("Jeep1", 7, 45, 2e3), this.WaveControl.addWave("enemy4", 10, 40, 25), this.WaveControl.addWave("enemy5", 10, 40, 1e3), this.WaveControl.addWave("Jeep3", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 10, 20, 100), this.WaveControl.addWave("Jeep2", 2, 45, 1e3), this.WaveControl.addWave("enemy1", 18, 15, 1e3), this.WaveControl.addWave("enemy2", 17, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy5", 15, 15, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy4", 15, 15, 1e3), this.WaveControl.addWave("enemy5", 16, 15, 1e3), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("Tank2", 2, 65, 3e3), this.WaveControl.addWave("enemy4", 10, 35, 20), this.WaveControl.addWave("enemy5", 10, 35, 1e3), this.WaveControl.addWave("Jeep3", 7, 45, 2e3), this.WaveControl.addWave("Jeep2", 7, 45, 2500), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("enemy3", 20, 15, 1e3), this.WaveControl.addWave("enemy2", 18, 15, 700), this.WaveControl.addWave("enemy1", 19, 15, 1e3), this.WaveControl.addWave("Jeep3", 8, 45, 2e3), this.WaveControl.addWave("Tank3", 2, 65, 3e3), this.WaveControl.addWave("Jeep3", 7, 45, 2e3), this.WaveControl.addWave("Jeep4", 7, 45, 1e3), this.WaveControl.addWave("Jeep2", 7, 45, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("bot_1", 3, 45, 4e3), this.WaveControl.addWave("Jeep2", 7, 45, 1e3), this.WaveControl.addWave("Jeep3", 7, 45, 2e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("Tank2", 2, 65, 3e3), this.WaveControl.addWave("bot_1", 3, 45, 4e3), this.WaveControl.addWave("Jeep4", 7, 45, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("Jeep1", 7, 45, 2e3), this.WaveControl.addWave("bot_1", 3, 45, 4e3), this.WaveControl.addWave("Tank3", 2, 65, 30), this.decalFX.drawPath("sand");
  }
  frame19() {
    this.WaveControl.addWave("enemy4", 2, 15, 1e3), this.WaveControl.addWave("enemy2", 2, 15, 1e3), this.WaveControl.addWave("enemy3", 4, 15, 1e3), this.WaveControl.addWave("enemy4", 4, 20, 1e3), this.WaveControl.addWave("enemy5", 4, 20, 1e3), this.WaveControl.addWave("enemy1", 6, 15, 1e3), this.WaveControl.addWave("enemy4", 6, 10, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy3", 8, 20, 1e3), this.WaveControl.addWave("enemy2", 8, 15, 1e3), this.WaveControl.addWave("enemy1", 7, 10, 1e3), this.WaveControl.addWave("enemy5", 8, 15, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 3, 55, 2e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy5", 12, 20, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 5, 25, 15), this.WaveControl.addWave("enemy2", 7, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 2e3), this.WaveControl.addWave("enemy3", 12, 15, 1e3), this.WaveControl.addWave("enemy4", 12, 20, 1e3), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 1e3), this.WaveControl.addWave("Jeep1", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 14, 15, 1e3), this.WaveControl.addWave("enemy5", 13, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 200), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("enemy1", 13, 15, 1e3), this.WaveControl.addWave("enemy4", 14, 15, 1e3), this.WaveControl.addWave("enemy2", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 15, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 30, 15), this.WaveControl.addWave("enemy1", 7, 30, 1e3), this.WaveControl.addWave("Jeep3", 3, 95, 50), this.WaveControl.addWave("Jeep4", 3, 95, 1e3), this.WaveControl.addWave("enemy1", 16, 15, 1e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy3", 20, 30, 1e3), this.WaveControl.addWave("enemy4", 17, 20, 1e3), this.WaveControl.addWave("Jeep1", 7, 45, 2e3), this.WaveControl.addWave("enemy4", 10, 40, 25), this.WaveControl.addWave("enemy5", 10, 40, 1e3), this.WaveControl.addWave("Jeep3", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 10, 20, 100), this.WaveControl.addWave("Jeep2", 2, 45, 1e3), this.WaveControl.addWave("enemy1", 18, 15, 1e3), this.WaveControl.addWave("enemy2", 17, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy5", 15, 15, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy4", 15, 15, 1e3), this.WaveControl.addWave("enemy5", 16, 15, 1e3), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("Tank2", 2, 65, 3e3), this.WaveControl.addWave("enemy4", 10, 35, 20), this.WaveControl.addWave("enemy5", 10, 35, 1e3), this.WaveControl.addWave("Jeep3", 7, 45, 2e3), this.WaveControl.addWave("Jeep2", 7, 45, 2500), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("enemy3", 20, 15, 1e3), this.WaveControl.addWave("enemy2", 18, 15, 700), this.WaveControl.addWave("enemy1", 19, 15, 1e3), this.WaveControl.addWave("Jeep3", 8, 45, 2e3), this.WaveControl.addWave("Tank3", 2, 65, 30), this.decalFX.drawPath("forest");
  }
  frame29() {
    this.WaveControl.addWave("enemy4", 2, 15, 300), this.WaveControl.addWave("enemy2", 2, 15, 300), this.WaveControl.addWave("enemy3", 4, 15, 400), this.WaveControl.addWave("enemy4", 4, 20, 400), this.WaveControl.addWave("enemy5", 4, 20, 500), this.WaveControl.addWave("enemy1", 6, 15, 500), this.WaveControl.addWave("enemy4", 6, 10, 500), this.WaveControl.addWave("enemy5", 6, 15, 500), this.WaveControl.addWave("enemy3", 8, 20, 600), this.WaveControl.addWave("enemy2", 8, 15, 600), this.WaveControl.addWave("enemy1", 7, 10, 600), this.WaveControl.addWave("enemy5", 8, 15, 600), this.WaveControl.addWave("enemy4", 10, 15, 800), this.WaveControl.addWave("Jeep3", 3, 55, 800), this.WaveControl.addWave("enemy1", 10, 15, 800), this.WaveControl.addWave("enemy5", 10, 20, 800), this.WaveControl.addWave("enemy4", 10, 15, 800), this.WaveControl.addWave("enemy3", 8, 25, 15), this.WaveControl.addWave("enemy2", 8, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 800), this.WaveControl.addWave("Jeep3", 5, 45, 800), this.WaveControl.addWave("enemy4", 6, 10, 500), this.WaveControl.addWave("enemy5", 6, 15, 500), this.WaveControl.addWave("enemy3", 8, 20, 600), this.WaveControl.addWave("enemy2", 8, 15, 600), this.WaveControl.addWave("enemy1", 7, 10, 600), this.WaveControl.addWave("enemy5", 8, 15, 600), this.WaveControl.addWave("enemy4", 10, 15, 800), this.WaveControl.addWave("Jeep3", 3, 55, 800), this.WaveControl.addWave("enemy1", 10, 15, 800), this.WaveControl.addWave("enemy5", 10, 20, 800), this.WaveControl.addWave("enemy4", 10, 15, 800), this.WaveControl.addWave("enemy3", 8, 25, 15), this.WaveControl.addWave("enemy2", 8, 25, 25), this.WaveControl.addWave("enemy1", 8, 20, 1e3), this.WaveControl.addWave("Jeep2", 4, 45, 800), this.WaveControl.addWave("enemy3", 12, 15, 800), this.WaveControl.addWave("enemy4", 12, 20, 800), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 800), this.WaveControl.addWave("Jeep1", 5, 45, 1e3), this.WaveControl.addWave("Tank2", 2, 45, 800), this.WaveControl.addWave("Jeep2", 5, 45, 1e3), this.WaveControl.addWave("Tank1", 2, 45, 800), this.WaveControl.addWave("Jeep2", 5, 45, 800), this.WaveControl.addWave("Tank3", 2, 45, 25);
  }
  frame20() {
    this.WaveControl.addWave("enemy4", 2, 15, 1e3), this.WaveControl.addWave("enemy2", 2, 15, 1e3), this.WaveControl.addWave("enemy3", 4, 15, 1e3), this.WaveControl.addWave("enemy4", 4, 20, 1e3), this.WaveControl.addWave("enemy5", 4, 20, 1e3), this.WaveControl.addWave("enemy1", 6, 15, 1e3), this.WaveControl.addWave("enemy4", 6, 10, 1e3), this.WaveControl.addWave("enemy5", 6, 15, 1e3), this.WaveControl.addWave("enemy3", 8, 20, 1e3), this.WaveControl.addWave("enemy2", 8, 15, 1e3), this.WaveControl.addWave("enemy1", 7, 10, 1e3), this.WaveControl.addWave("enemy5", 8, 15, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("Jeep3", 3, 55, 2e3), this.WaveControl.addWave("enemy1", 10, 15, 1e3), this.WaveControl.addWave("enemy5", 12, 20, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 5, 25, 15), this.WaveControl.addWave("enemy2", 7, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 1e3), this.WaveControl.addWave("Jeep3", 5, 45, 2e3), this.WaveControl.addWave("enemy3", 12, 15, 1e3), this.WaveControl.addWave("enemy4", 12, 20, 1e3), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 1e3), this.WaveControl.addWave("Jeep1", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 14, 15, 1e3), this.WaveControl.addWave("enemy5", 13, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 15, 200), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("enemy1", 13, 15, 1e3), this.WaveControl.addWave("enemy4", 14, 15, 1e3), this.WaveControl.addWave("enemy2", 10, 15, 1e3), this.WaveControl.addWave("enemy3", 15, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy4", 10, 15, 1e3), this.WaveControl.addWave("enemy2", 7, 30, 15), this.WaveControl.addWave("enemy1", 7, 30, 1e3), this.WaveControl.addWave("Jeep3", 3, 95, 50), this.WaveControl.addWave("Jeep4", 3, 95, 1e3), this.WaveControl.addWave("enemy1", 16, 15, 1e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy3", 20, 30, 1e3), this.WaveControl.addWave("enemy4", 17, 20, 1e3), this.WaveControl.addWave("Jeep1", 7, 45, 2e3), this.WaveControl.addWave("enemy4", 10, 40, 25), this.WaveControl.addWave("enemy5", 10, 40, 1e3), this.WaveControl.addWave("Jeep3", 3, 45, 2e3), this.WaveControl.addWave("enemy3", 10, 20, 100), this.WaveControl.addWave("Jeep2", 2, 45, 1e3), this.WaveControl.addWave("enemy1", 18, 15, 1e3), this.WaveControl.addWave("enemy2", 17, 15, 1e3), this.WaveControl.addWave("Jeep4", 5, 45, 1e3), this.WaveControl.addWave("enemy5", 15, 15, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("enemy2", 14, 15, 1e3), this.WaveControl.addWave("enemy4", 15, 15, 1e3), this.WaveControl.addWave("enemy5", 16, 15, 1e3), this.WaveControl.addWave("Jeep2", 4, 45, 1e3), this.WaveControl.addWave("Tank2", 2, 65, 3e3), this.WaveControl.addWave("enemy4", 10, 35, 20), this.WaveControl.addWave("enemy5", 10, 35, 1e3), this.WaveControl.addWave("Jeep3", 7, 45, 2e3), this.WaveControl.addWave("Jeep2", 7, 45, 2500), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("enemy3", 20, 15, 1e3), this.WaveControl.addWave("enemy2", 18, 15, 700), this.WaveControl.addWave("enemy1", 19, 15, 1e3), this.WaveControl.addWave("Jeep3", 8, 45, 2e3), this.WaveControl.addWave("Tank3", 2, 65, 3e3), this.WaveControl.addWave("Jeep3", 7, 45, 2e3), this.WaveControl.addWave("Jeep4", 7, 45, 1e3), this.WaveControl.addWave("Jeep2", 7, 45, 1e3), this.WaveControl.addWave("Tank1", 2, 65, 3e3), this.WaveControl.addWave("bot_1", 3, 45, 40), this.decalFX.drawPath("sand");
  }
  frame27() {
    this.WaveControl.addWave("enemy4", 2, 15, 300), this.WaveControl.addWave("enemy2", 2, 15, 300), this.WaveControl.addWave("enemy3", 4, 15, 400), this.WaveControl.addWave("enemy4", 4, 20, 400), this.WaveControl.addWave("enemy5", 4, 20, 500), this.WaveControl.addWave("enemy1", 6, 15, 500), this.WaveControl.addWave("enemy4", 6, 10, 500), this.WaveControl.addWave("enemy5", 6, 15, 500), this.WaveControl.addWave("enemy3", 8, 20, 600), this.WaveControl.addWave("enemy2", 8, 15, 600), this.WaveControl.addWave("enemy1", 7, 10, 600), this.WaveControl.addWave("enemy5", 8, 15, 600), this.WaveControl.addWave("enemy4", 10, 15, 800), this.WaveControl.addWave("Jeep3", 3, 55, 800), this.WaveControl.addWave("enemy1", 10, 15, 800), this.WaveControl.addWave("enemy5", 10, 20, 800), this.WaveControl.addWave("enemy4", 10, 15, 800), this.WaveControl.addWave("enemy3", 8, 25, 15), this.WaveControl.addWave("Jeep3", 3, 55, 800), this.WaveControl.addWave("enemy1", 10, 15, 800), this.WaveControl.addWave("enemy5", 10, 20, 800), this.WaveControl.addWave("enemy4", 10, 15, 800), this.WaveControl.addWave("enemy3", 8, 25, 15), this.WaveControl.addWave("enemy2", 8, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 800), this.WaveControl.addWave("Jeep3", 5, 45, 800), this.WaveControl.addWave("enemy2", 8, 25, 25), this.WaveControl.addWave("enemy1", 8, 20, 1e3), this.WaveControl.addWave("Jeep2", 4, 45, 800), this.WaveControl.addWave("enemy3", 12, 15, 800), this.WaveControl.addWave("enemy4", 12, 20, 800), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 800), this.WaveControl.addWave("Jeep1", 5, 45, 1e3), this.WaveControl.addWave("Tank3", 2, 45, 800), this.WaveControl.addWave("Tank2", 2, 45, 800), this.WaveControl.addWave("Tank1", 2, 45, 25);
  }
  showPaused() {
    this.pausePage.visible = !0;
  }
  frame31() {
    this.WaveControl.addWave("enemy4", 2, 15, 300), this.WaveControl.addWave("enemy2", 2, 15, 300), this.WaveControl.addWave("enemy3", 4, 15, 400), this.WaveControl.addWave("enemy4", 4, 20, 400), this.WaveControl.addWave("enemy5", 4, 20, 500), this.WaveControl.addWave("enemy1", 6, 15, 500), this.WaveControl.addWave("enemy4", 6, 10, 500), this.WaveControl.addWave("enemy5", 6, 15, 500), this.WaveControl.addWave("enemy3", 8, 20, 600), this.WaveControl.addWave("enemy2", 8, 15, 600), this.WaveControl.addWave("enemy1", 7, 10, 600), this.WaveControl.addWave("enemy5", 8, 15, 600), this.WaveControl.addWave("enemy4", 10, 15, 800), this.WaveControl.addWave("Jeep3", 3, 55, 800), this.WaveControl.addWave("enemy1", 10, 15, 800), this.WaveControl.addWave("enemy5", 10, 20, 800), this.WaveControl.addWave("enemy4", 10, 15, 800), this.WaveControl.addWave("enemy3", 8, 25, 15), this.WaveControl.addWave("enemy2", 8, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 800), this.WaveControl.addWave("Jeep3", 5, 45, 800), this.WaveControl.addWave("enemy2", 8, 15, 600), this.WaveControl.addWave("enemy1", 7, 10, 600), this.WaveControl.addWave("enemy5", 8, 15, 600), this.WaveControl.addWave("enemy4", 10, 15, 800), this.WaveControl.addWave("Jeep3", 3, 55, 800), this.WaveControl.addWave("enemy1", 10, 15, 800), this.WaveControl.addWave("enemy5", 10, 20, 800), this.WaveControl.addWave("enemy4", 10, 15, 800), this.WaveControl.addWave("enemy3", 8, 25, 15), this.WaveControl.addWave("enemy2", 8, 25, 1e3), this.WaveControl.addWave("enemy1", 12, 20, 800), this.WaveControl.addWave("Jeep3", 5, 45, 800), this.WaveControl.addWave("enemy2", 8, 25, 25), this.WaveControl.addWave("enemy1", 8, 20, 1e3), this.WaveControl.addWave("Jeep2", 4, 45, 800), this.WaveControl.addWave("enemy3", 12, 15, 800), this.WaveControl.addWave("enemy4", 12, 20, 800), this.WaveControl.addWave("enemy5", 10, 15, 1e3), this.WaveControl.addWave("enemy1", 12, 15, 800), this.WaveControl.addWave("Jeep1", 5, 45, 1e3), this.WaveControl.addWave("Tank2", 2, 45, 800), this.WaveControl.addWave("Jeep2", 5, 45, 1e3), this.WaveControl.addWave("Tank1", 2, 45, 800), this.WaveControl.addWave("Jeep2", 5, 45, 800), this.WaveControl.addWave("Tank3", 2, 45, 25);
  }
  stopLevel() {
    this.mainTick.stop();
  }
  killEnemy(e) {
    var t = void 0, s = void 0;
    for (t = this.Enemies.length, e.x = -50, e.y = -50, s = t - 1; s > -1; ) {
      if (this.Enemies[s] == e) {
        this.Enemies.splice(s, 1), this.level.removeChild(e);
        break;
      }
      s--;
    }
  }
  setGrid(e, t, s) {
    this.levelArray[e][t] = s;
  }
  Update(e) {
    var t = void 0, s = void 0, l = void 0, o = void 0, v = void 0, u = void 0, n = void 0, W = void 0, y = void 0, S = void 0, D = void 0;
    for (s = this.Enemies.length, l = 0; l < s; )
      t = this.Enemies[l], t?.Tick(e), l++;
    for (o = this.Projectiles.length, v = 0; v < o; )
      t = this.Projectiles[v], t?.Tick(e), v++;
    for (u = this.Towers.length, n = 0; n < u; )
      t = this.Towers[n], t?.Tick(e), n++;
    for (W = this.spawners.length, y = 0; y < W; )
      t = this.spawners[y], t?.Tick(e), y++;
    for (this.WaveControl.Tick(e), S = this.renderElements.length, D = 0; D < S; )
      t = this.renderElements[D], t?.Tick(e), D++;
  }
  hidePaused() {
    this.pausePage.visible = !1, this.pausePage.alpha = 1;
  }
  inRange(e, t) {
    var s = void 0, l = void 0, o = void 0, v = void 0, u = void 0;
    return s = e.x - t.x, l = e.y - t.y, o = s * s + l * l, v = e.Range + e.Range * e.RLevel, u = v * v, o <= u;
  }
  buildTower(e, t) {
    var s = null;
    if (this.Selected == null || this.Selected.bHeld == !1) {
      switch (this.player.funds -= t, e) {
        case "tower1":
          s = new yt(this), this.level.addChild(s), this.Towers.push(s);
          break;
        case "tower2":
          s = new Ct(this), this.level.addChild(s), this.Towers.push(s);
          break;
        case "tower3":
          s = new xt(this), this.level.addChild(s), this.Towers.push(s);
          break;
        case "tower4":
          s = new St(this), this.level.addChild(s), this.Towers.push(s);
          break;
        case "tower5":
          s = new wt(this), this.level.addChild(s), this.Towers.push(s);
          break;
        case "tower6":
          s = new Tt(this), this.level.addChild(s), this.Towers.push(s);
          break;
        case "tower7":
          s = new bt(this), this.level.addChild(s), this.Towers.push(s);
          break;
        case "tower8":
          s = new Mt(this), this.level.addChild(s), this.Towers.push(s);
          break;
        case "tower9":
          s = new kt(this), this.level.addChild(s), this.Towers.push(s);
          break;
        case "tower10":
          s = new _t(this), this.level.addChild(s), this.Towers.push(s);
          break;
        case "tower11":
          s = new ft(this), this.level.addChild(s), this.Towers.push(s);
          break;
        case "suptower1":
          s = new Wt(this), this.level.addChild(s), this.Towers.push(s);
          break;
        case "suptower2":
          s = new gt(this), this.level.addChild(s), this.Towers.push(s);
          break;
        case "suptower3":
          s = new pt(this), this.level.addChild(s), this.Towers.push(s);
      }
      s.select(), s.saleValue = t * 0.5;
    }
  }
  tick(e) {
    var t = void 0;
    t = te(), this.gameSpeed = (t - this.lastTick) * 0.08, this.lastTick = t, this.gameSpeed *= this.baseSpeed, this.gameSpeed > 8 && (this.gameSpeed = 8), this.Update(this.gameSpeed), this.sortActors();
  }
  getFrame(e) {
    this.frameController.getFrame(e);
  }
  killSprite(e) {
    var t = void 0, s = void 0;
    for (t = this.renderElements.length, s = 0; s < t; ) {
      if (this.renderElements[s] == e) {
        this.renderElements.splice(s, 1);
        break;
      }
      s++;
    }
  }
  getLevel(e) {
    this.newLevel(this.gameWidth, this.gameHeight), this.WaveControl.resetWC(), this.frameController.getFrame(e + this.firstLevelFrame - 1), this.currentLevel = h(e), this.levelScore = 0, this.levelHealth = 1e3, this.baseSpeed = 1, this.player.funds = this.player.startFunds;
  }
  newLevel(e, t) {
    var s = void 0, l = void 0;
    for (this.levelArray = new Array(), s = 0; s < e; ) {
      for (this.levelArray[s] = new Array(), l = 0; l < t; )
        this.levelArray[s].push(0), l++;
      s++;
    }
    this.Selected = null, this.startPoints = new Array(), this.levelHealth = 1e3, this.endPoints = new Array(), this.Towers = new Array(), this.Enemies = new Array(), this.Projectiles = new Array(), this.renderElements = new Array(), this.spawners = new Array();
  }
  getGrid(e, t) {
    return h(h(this.levelArray[e][t]));
  }
  getEnemy(e, t) {
    var s = void 0, l = null, o = null, v = void 0;
    for (s = this.Enemies.length, l = null, o = null, v = 0; v < s; )
      o = this.Enemies[v], this.inRange(e, o) && (l == null || t == "near" && o.traveled > l.traveled || t == "far" && o.traveled < l.traveled || t == "strong" && o.health > l.health || t == "weak" && o.health < l.health ? l = o : t == "heat" && o.coreTemp > l.coreTemp ? o.coreTemp > 120 && (l = o) : t == "cold" && o.coreTemp < l.coreTemp && (o.bMetal || (l = o))), v++;
    return l;
  }
  killProjectile(e, t = !1) {
    var s = void 0, l = void 0;
    for (s = this.Projectiles.length, l = 0; l < s; ) {
      if (this.Projectiles[l] == e) {
        t || this.decalFX.Crater(e.x, e.y), this.Projectiles.splice(l, 1);
        break;
      }
      l++;
    }
  }
}
class $t {
  constructor(e) {
    m(this), c(this);
  }
}
class Ot extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Vt extends b {
  __fi() {
    this.rf = null, this.rg = null, this.dg = null, this.fundText = null, this.hbar = null, this.ucost = null, this.livesText = null, this.scoreText = null, this.countText = null, this.wavesText = null, this.game = null;
  }
  constructor() {
    super(), this.mouseEnabled = !1, this.game = g(p, this.parent), this.addEventListener(w.ENTER_FRAME, this.Tick, !1, 0, !0);
  }
  Tick(e) {
    this.fundText.text = String(this.game.player.funds), this.livesText.text = String(Math.floor(this.game.levelHealth)), this.hbar.scaleX = this.game.levelHealth * 1e-3, this.scoreText.text = String(this.game.levelScore), this.countText.text = String(Math.floor(this.game.WaveControl.nextSpawn / 100)), this.wavesText.text = "Wave: " + this.game.WaveControl.currentWave + "/" + this.game.WaveControl.numWaves, this.game.Selected != null && (this.rg.text = String(this.game.Selected.Range + this.game.Selected.Range * this.game.Selected.RLevel), this.dg.text = String(this.game.Selected.Damage + this.game.Selected.Damage * this.game.Selected.DLevel), this.rf.text = String(this.game.Selected.RoF + this.game.Selected.RoF * this.game.Selected.RoFLevel), this.ucost.text = String(Math.floor(this.game.Selected.saleValue * 0.5)));
  }
}
class oe extends k {
  __fi() {
    this.desiredAngle = 0, this.hull = null;
  }
  constructor(e, t) {
    super(), this.endPoint = t, this.game = e, this.speed = 0.9, this.baseSpeed = this.speed, this.bMetal = !0, this.health = 150, this.radiation_resistance = 0.8, this.heat_resistance = 0.7;
  }
  closestAngle(e, t) {
    var s = NaN;
    return s = t - e, s %= 360, s += 540, s %= 360, s - 180;
  }
  deathFX() {
    new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 2, 10, 0.1, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1.2, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10), this.game.decalFX.Crater(this.x, this.y), this.game.Soundbox.playHit("explode");
  }
  setAngle() {
    var e = NaN, t = void 0;
    e = this.ang * 180 / Math.PI, e != this.desiredAngle && (t = this.closestAngle(this.desiredAngle, e) * 0.3, Math.abs(t) < 0.1 ? this.desiredAngle = e : this.desiredAngle += t), this.desiredAngle >= 180 ? this.desiredAngle -= 360 : this.desiredAngle <= -180 && (this.desiredAngle += 360), this.hull.gotoAndStop(Math.floor((this.desiredAngle + 180) * 0.2));
  }
  Move(e) {
    var t = void 0, s = void 0;
    this.x += this.vx * this.speed * e, this.y += this.vy * this.speed * e, this.gx == Math.floor(this.tx / 32) && this.gy == Math.floor(this.ty / 32) && this.x > this.gx * this.game.gridSize + 4 && this.x < this.gx * this.game.gridSize + 28 && this.y > this.gy * this.game.gridSize + 4 && this.y < this.gy * this.game.gridSize + 28 && (this.ty = h(Number(this.pth.pop()) * this.game.gridSize + 16), this.tx = h(Number(this.pth.pop()) * this.game.gridSize + 16), t = this.tx - this.x, s = this.ty - this.y, this.ang = Math.atan2(s, t), this.vx = Math.cos(this.ang), this.vy = Math.sin(this.ang)), this.setAngle();
  }
}
class le extends k {
  __fi() {
    this.desiredAngle = 0, this.hull = null;
  }
  constructor(e, t) {
    super(), this.endPoint = t, this.game = e, this.speed = 1, this.baseSpeed = this.speed, this.bMetal = !0, this.health = 200, this.heat_resistance = 0.8;
  }
  closestAngle(e, t) {
    var s = NaN;
    return s = t - e, s %= 360, s += 540, s %= 360, s - 180;
  }
  deathFX() {
    new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 2, 10, 0.1, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1.2, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10), this.game.decalFX.Crater(this.x, this.y), this.game.Soundbox.playHit("explode");
  }
  setAngle() {
    var e = NaN, t = void 0;
    e = this.ang * 180 / Math.PI, e != this.desiredAngle && (t = this.closestAngle(this.desiredAngle, e) * 0.3, Math.abs(t) < 0.1 ? this.desiredAngle = e : this.desiredAngle += t), this.desiredAngle > 180 ? this.desiredAngle -= 360 : this.desiredAngle < -180 && (this.desiredAngle += 360), this.hull.gotoAndStop(Math.round((this.desiredAngle + 180) / 5));
  }
  Move(e) {
    var t = void 0, s = void 0;
    this.x += this.vx * this.speed * e, this.y += this.vy * this.speed * e, this.gx == Math.floor(this.tx / 32) && this.gy == Math.floor(this.ty / 32) && this.x > this.gx * this.game.gridSize + 4 && this.x < this.gx * this.game.gridSize + 28 && this.y > this.gy * this.game.gridSize + 4 && this.y < this.gy * this.game.gridSize + 28 && (this.ty = h(Number(this.pth.pop()) * this.game.gridSize + 16), this.tx = h(Number(this.pth.pop()) * this.game.gridSize + 16), t = this.tx - this.x, s = this.ty - this.y, this.ang = Math.atan2(s, t), this.vx = Math.cos(this.ang), this.vy = Math.sin(this.ang)), this.setAngle();
  }
}
class ne extends k {
  __fi() {
    this.desiredAngle = 0, this.hull = null;
  }
  constructor(e, t) {
    super(), this.endPoint = t, this.game = e, this.speed = 1.2, this.baseSpeed = this.speed, this.bMetal = !0, this.health = 150, this.heat_resistance = 0.7;
  }
  closestAngle(e, t) {
    var s = NaN;
    return s = t - e, s %= 360, s += 540, s %= 360, s - 180;
  }
  deathFX() {
    new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 2, 10, 0.1, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1.2, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10), this.game.decalFX.Crater(this.x, this.y), this.game.Soundbox.playHit("explode");
  }
  setAngle() {
    var e = NaN, t = void 0;
    e = this.ang * 180 / Math.PI, e != this.desiredAngle && (t = this.closestAngle(this.desiredAngle, e) * 0.3, Math.abs(t) < 0.1 ? this.desiredAngle = e : this.desiredAngle += t), this.desiredAngle > 180 ? this.desiredAngle -= 360 : this.desiredAngle < -180 && (this.desiredAngle += 360), this.hull.gotoAndStop(Math.round((this.desiredAngle + 180) / 5));
  }
  Move(e) {
    var t = void 0, s = void 0;
    this.x += this.vx * this.speed * e, this.y += this.vy * this.speed * e, this.gx == Math.floor(this.tx / 32) && this.gy == Math.floor(this.ty / 32) && this.x > this.gx * this.game.gridSize + 4 && this.x < this.gx * this.game.gridSize + 28 && this.y > this.gy * this.game.gridSize + 4 && this.y < this.gy * this.game.gridSize + 28 && (this.ty = h(Number(this.pth.pop()) * this.game.gridSize + 16), this.tx = h(Number(this.pth.pop()) * this.game.gridSize + 16), t = this.tx - this.x, s = this.ty - this.y, this.ang = Math.atan2(s, t), this.vx = Math.cos(this.ang), this.vy = Math.sin(this.ang)), this.setAngle();
  }
}
class re extends k {
  __fi() {
    this.desiredAngle = 0, this.hull = null;
  }
  constructor(e, t) {
    super(), this.endPoint = t, this.game = e, this.speed = 1.3, this.baseSpeed = this.speed, this.bMetal = !0, this.health = 120, this.heat_resistance = 0.9;
  }
  closestAngle(e, t) {
    var s = NaN;
    return s = t - e, s %= 360, s += 540, s %= 360, s - 180;
  }
  deathFX() {
    new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 2, 10, 0.1, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1.2, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10), this.game.decalFX.Crater(this.x, this.y), this.game.Soundbox.playHit("explode");
  }
  setAngle() {
    var e = NaN, t = void 0;
    e = this.ang * 180 / Math.PI, e != this.desiredAngle && (t = this.closestAngle(this.desiredAngle, e) * 0.3, Math.abs(t) < 0.1 ? this.desiredAngle = e : this.desiredAngle += t), this.desiredAngle > 180 ? this.desiredAngle -= 360 : this.desiredAngle < -180 && (this.desiredAngle += 360), this.hull.gotoAndStop(Math.round((this.desiredAngle + 180) / 5));
  }
  Move(e) {
    var t = void 0, s = void 0;
    this.x += this.vx * this.speed * e, this.y += this.vy * this.speed * e, this.gx == Math.floor(this.tx / 32) && this.gy == Math.floor(this.ty / 32) && this.x > this.gx * this.game.gridSize + 4 && this.x < this.gx * this.game.gridSize + 28 && this.y > this.gy * this.game.gridSize + 4 && this.y < this.gy * this.game.gridSize + 28 && (this.ty = h(Number(this.pth.pop()) * this.game.gridSize + 16), this.tx = h(Number(this.pth.pop()) * this.game.gridSize + 16), t = this.tx - this.x, s = this.ty - this.y, this.ang = Math.atan2(s, t), this.vx = Math.cos(this.ang), this.vy = Math.sin(this.ang)), this.setAngle();
  }
}
class x extends d {
  __fi() {
    this.bScale = !1, this.game = null;
  }
  constructor() {
    super();
    try {
      this.game = g(p, this.parent);
    } catch {
      this.game = g(p, this.parent.parent);
    }
    this.gotoAndStop(1), this.addEventListener(A.CLICK, this.clicked, !1, 0, !0), this.addEventListener(A.ROLL_OVER, this.over, !1, 0, !0), this.addEventListener(A.ROLL_OUT, this.out, !1, 0, !0);
  }
  clicked(e) {
  }
  out(e) {
    this.bScale ? (this.scaleX = 1, this.scaleY = 1) : this.gotoAndStop(1);
  }
  over(e) {
    this.bScale ? (this.scaleX = 1.1, this.scaleY = 1.1) : this.gotoAndStop(2);
  }
}
class T extends x {
  __fi() {
    this.unlocked = !1, this.levelNumber = 0;
  }
  constructor() {
    super();
  }
  init() {
    this.game.player.Levels[this.levelNumber - 1] > 0 ? this.unlocked = !0 : this.gotoAndStop(3);
  }
  clicked(e) {
    this.unlocked && this.game.getLevel(this.levelNumber);
  }
  out(e) {
    this.unlocked && this.gotoAndStop(1);
  }
  over(e) {
    this.unlocked && this.gotoAndStop(2);
  }
}
class qt extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(1), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class Kt extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(10), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class Yt extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(11), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class Qt extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(12), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class Zt extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(13), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class es extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(14), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class ts extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(16), this.lvlText.text = "Sniper Elite: Mission 1", this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class ss extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(18), this.lvlText.text = "Sniper Elite: Mission 2", this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class is extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(2), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class as extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(20), this.lvlText.text = "Sniper Elite: Mission 3", this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class hs extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(22), this.lvlText.text = "Sniper Elite: Mission 4", this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class os extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(3), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class ls extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(4), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class ns extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(5), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class rs extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(6), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class ds extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(7), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class vs extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(8), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class ms extends T {
  __fi() {
    this.lvlText = null, this.scoreText = null;
  }
  constructor() {
    super(), this.levelNumber = h(9), this.lvlText.text = String(this.levelNumber), this.scoreText.text = String(this.game.player.Levels[this.levelNumber]), this.init();
  }
}
class cs extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class us extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class Ws extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class gs extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class ps extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class ys extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class _s extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class fs extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class Cs extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class xs extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class Ss extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class ws extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class Ts extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class bs extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class Ms extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class ks extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class Fs extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class Ns extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    g(p, this.parent).level = this;
  }
}
class As extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class de extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.stop();
  }
}
class F extends d {
  __fi() {
    this.gy = 0, this.ugCredit = h(0), this.Range = h(100), this.bBlocked = !1, this.exp = 0, this.loading = h(0), this.RLevel = 0, this.Level = h(1), this.saleValue = h(20), this.RoFLevel = 0, this.Type = "rifle", this.eTarget = null, this.lvlInd = null, this.RoF = h(1), this.Damage = h(0), this.DLevel = 0, this.targeting = "near", this.bHeld = !0, this.angle = NaN, this.expLimit = 1e3, this.gx = 0, this.game = null;
  }
  constructor() {
    super(), this.mouseEnabled = !1;
  }
  upGrade(e) {
    switch (this.game.player.funds -= Math.floor(this.saleValue * 0.5), e) {
      case "damage":
        this.DLevel += 0.2;
        break;
      case "rof":
        this.RoFLevel += 0.2;
        break;
      case "range":
        this.RLevel += 0.2;
    }
  }
  select() {
    this.game.Selected = this;
  }
  clicked() {
    this.bHeld ? this.dropTower() : this.select();
  }
  levelUp() {
  }
  relocate() {
    this.game.setGrid(this.gx, this.gy, 0), this.game.player.funds -= 10, this.bHeld = !0;
  }
  aim() {
  }
  dropTower() {
    this.SetGrid(), this.game.getGrid(this.gx, this.gy) == 0 && (this.snapToGrid(), this.game.setGrid(this.gx, this.gy, 2), this.bHeld = !1, this.bBlocked = !1);
  }
  sell() {
    this.game.setGrid(this.gx, this.gy, 0), this.game.player.funds += this.saleValue, this.bHeld = !0, this.game.killTower(this);
  }
  SetGrid() {
    this.gx = h(Math.floor(this.x / this.game.gridSize)), this.gy = h(Math.floor(this.y / this.game.gridSize));
  }
  Fire() {
  }
  Tick(e) {
    this.bHeld ? (this.x = this.game.mouseX, this.y = this.game.mouseY, this.x > 640 && (this.x = 640), this.snapToGrid(), this.game.getGrid(this.gx, this.gy) == 0 ? this.bBlocked = !1 : this.bBlocked = !0) : (this.loading = h(this.loading + (this.RoF + this.RoF * this.RoFLevel) * e), this.eTarget == null || this.eTarget.health <= 0 ? this.eTarget = this.game.getEnemy(this, this.targeting) : (this.aim(), this.loading >= 100 && (this.checkTarget() ? (this.loading = h(0), this.Fire()) : (this.eTarget = this.game.getEnemy(this, this.targeting), this.loading = h(90)))), this.exp >= this.expLimit && (this.exp -= this.expLimit, this.expLimit *= 1.2, this.levelUp()));
  }
  checkTarget() {
    var e = void 0;
    return e = !1, this.eTarget != null && this.eTarget.bEnemy && this.game.inRange(this, this.eTarget) && (e = !0), e || (this.eTarget = null), e;
  }
  snapToGrid() {
    this.SetGrid(), this.x = this.gx * this.game.gridSize + this.game.gridSize * 0.5, this.y = this.gy * this.game.gridSize + this.game.gridSize * 0.5;
  }
}
class ve extends F {
  __fi() {
    this.bCanFire = !1;
  }
  constructor() {
    super(), this.mouseEnabled = !1;
  }
  clicked() {
  }
  testRadial(e, t, s) {
    var l = void 0, o = void 0, v = !1, u = void 0, n = void 0;
    return l = e - s.x, o = t - s.y, v = !1, u = s.radius * s.radius + s.radius * s.radius, n = l * l + o * o, n <= u && (v = !0), v;
  }
  Tick(e) {
    this.loading = h(this.loading + this.RoF * e), this.aim(), this.loading >= 100 && this.game.player.funds >= 0 && (this.bCanFire = !0);
  }
}
class Ls extends ve {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(300), this.RoF = h(8), this.Range = h(120), this.game = g(p, this.parent.parent), this.game.Towers.push(this), this.bHeld = !1, this.game.Selected = this, this.turret.gotoAndStop(1);
  }
  aim() {
    var e = void 0, t = void 0, s = void 0;
    e = this.stage.mouseX - this.x, t = this.stage.mouseY - this.y, s = Math.atan2(t, e) * 180 / 3.16, this.turret.gotoAndStop(Math.round((s + 180) / 5));
  }
  Fire() {
    var e = void 0, t = void 0, s = void 0, l = void 0, o = void 0, v = void 0, u = void 0, n = void 0, W = void 0, y = null;
    for (this.bCanFire = !1, --this.game.player.funds, e = this.stage.mouseX - this.x, t = this.stage.mouseY - this.y, s = Math.sqrt(e * e + t * t), l = e / s * 20, o = t / s * 20, this.eTarget = null, v = !1, u = this.stage.mouseX, n = this.stage.mouseY, W = 10; v == !1; )
      y = this.checkHit(u, n), y != null && (v = !0, this.eTarget = y), u -= l * 0.25, n -= o * 0.25, --W <= 0 && (u = this.stage.mouseX, n = this.stage.mouseY, v = !0);
    new r(this.game, this.game.SpritePool.mFlash, 16, this.x + l - o * 0.1, this.y + o - 25 + l * 0.1, 0, 0, 8, 1, 8, 0, null, null, 1, !0), new E(this.x + l - o * 0.1, this.y + o - 25 + l * 0.1, u, n, 6, this.game, 2), this.eTarget != null ? this.eTarget.bMetal ? (new N(this.eTarget.x, this.eTarget.y - 6, 20, this.game), this.eTarget.takeDamage((this.Damage + this.Damage * this.DLevel) * 0.5, new f(5, 0, 0, 0), this)) : (new r(this.game, this.game.SpritePool.bloodSplat, 16, this.eTarget.x, this.eTarget.y, l * 0.05, o * 0.05, 8, 0.5, 8), this.eTarget.takeDamage(this.Damage + this.Damage * this.DLevel, new f(2, 0, 0, 0), this)) : (new N(u, n, 20, this.game), new N(u, n, 20, this.game), new N(u, n, 20, this.game)), this.game.Soundbox.playShot("sniper");
  }
  checkHit(e, t) {
    var s = void 0, l = null, o = void 0;
    for (s = this.game.Enemies.length, l = null, o = 0; o < s; ) {
      if (this.testRadial(e, t, this.game.Enemies[o])) {
        l = this.game.Enemies[o];
        break;
      }
      o++;
    }
    return l;
  }
}
class z extends d {
  __fi() {
    this.basketItem = null, this.DTool = null, this.CRED = null, this.selectedItem = null, this.confScreen = null, this.game = null;
  }
  constructor() {
    super(), this.gotoAndStop(1), this.game = g(p, this.parent), this.CRED.text = String(this.game.player.credits), this.addEventListener(w.ENTER_FRAME, this.tick, !1, 0, !0), this.hideConfirm();
  }
  showConfirm() {
    this.confScreen.visible = !0;
  }
  hideConfirm() {
    this.confScreen.visible = !1;
  }
  selectItem(e) {
    this.selectedItem = e;
  }
  tick(e) {
    this.selectedItem != null && (this.DTool.desc.text = this.selectedItem.itemDesc), this.CRED.text = String(this.game.player.credits);
  }
  buyItem(e) {
    this.showConfirm(), this.basketItem = e;
  }
  purchaseBasketItem() {
    this.hideConfirm(), this.game.player.addCredit(-this.basketItem.itemCost), this.game.player.buyItem(this.basketItem.itemID, this.basketItem.type), this.basketItem.checkItem();
  }
}
class me extends d {
  __fi() {
    this.PI = NaN, this.explosion = NaN, this.blastSize = h(40), this.game = null, this.damageType = null, this.fxCount = h(0), this.speed = NaN, this.tgt = null, this.damage = 0, this.grid = 0, this.msl = null, this.radius = 6, this.dType = null, this.vx = NaN, this.vy = NaN, this.who = null, this.alive = !0, this.gx = 0, this.gy = 0;
  }
  constructor(e, t, s, l = 0, o = 0, v = 0, u = null, n = 1, W = null, y = 30, S = null) {
    v = h(v), super(), this.PI = Math.PI, this.game = e, this.x = t, this.y = s, this.vx = l, this.vy = o, this.damage = h(v), this.dType = W, this.tgt = u, this.speed = n, this.grid = h(this.game.gridSize), this.game.Projectiles.push(this), this.explosion = y, this.who = S;
  }
  collide() {
    this.alive && (this.alive = !1, this.parent.removeChild(this), new r(this.game, this.game.SpritePool.fireX, 64, this.x, this.y, 0, 0, 16, 0.5, 16, 0, null, null, 1, !0, null), new r(this.game, this.game.SpritePool.explosion, 64, this.x, this.y, 0, -1, 16, 0.5, 16, 0, null, null, 1, !0, null), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10), new Q(this.game, this.x, this.y, 55, this.damage, this.dType, this.who), this.game.killProjectile(this));
  }
  setVelocity() {
    var e = void 0, t = void 0, s = void 0, l = void 0;
    this.tgt.health > 0 && this.tgt.x > 0 ? (e = this.tgt.x - this.x, t = this.tgt.y - this.y, s = Math.sqrt(e * e + t * t), this.vx = e / s * this.speed, this.vy = t / s * this.speed, l = Math.atan2(this.vy, this.vx) * 180 / this.PI, this.msl.gotoAndStop(Math.round((l + 180) / 5))) : (new r(this.game, this.game.SpritePool.fireX, 64, this.x, this.y, 0, 0, 16, 0.5, 16, 0, null, null, 1, !0, null), new r(this.game, this.game.SpritePool.explosion, 64, this.x, this.y, 0, -1, 16, 0.5, 16, 0, null, null, 1, !0, null), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10), this.game.killProjectile(this), this.parent.removeChild(this));
  }
  testRadial() {
    var e = void 0, t = void 0, s = void 0, l = void 0;
    e = this.x - this.tgt.x, t = this.y - this.tgt.y, s = this.radius * this.radius, l = e * e + t * t, l <= s && this.collide();
  }
  Tick(e) {
    this.alive && (this.x += this.vx * e, this.y += this.vy * e, this.setGrid(), this.setVelocity(), this.testCollision(), ++this.fxCount, this.fxCount >= 4 && (this.fxCount = h(0), new r(this.game, this.game.SpritePool.trail, 16, this.x - this.vx * 3, this.y - this.vy * 3 - 6, 0, 0, 10, 0.5, 10)));
  }
  testCollision() {
    this.tgt.gx >= this.gx - 1 && this.tgt.gx <= this.gx + 1 && this.tgt.gy >= this.gy - 1 && this.tgt.gy <= this.gy + 1 && this.testRadial();
  }
  setGrid() {
    this.gx = h(Math.floor(this.x / this.grid)), this.gy = h(Math.floor(this.y / this.grid));
  }
}
class Ds extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class ce extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class ue {
  __fi() {
    this.game = null;
  }
  constructor(e) {
    m(this), c(this), this.game = e;
  }
  getArray(e, t) {
    return e < 0 || e > 29 || t < 0 || t > 25 ? h(0) : h(h(this.game.levelArray[e][t]));
  }
  findPath(e, t, s, l) {
    var o = void 0, v = void 0, u = void 0, n = void 0, W = NaN, y = void 0;
    for (o = {}, v = [], o.unchecked = [], o.done = !1, u = Math.abs(e - s) + Math.abs(t - l), o.name = "cell_" + t + "_" + e, o[o.name] = {
      x: e,
      y: t,
      visited: !0,
      px: null,
      py: null,
      cost: u
    }, o.unchecked[o.unchecked.length] = o[o.name]; o.unchecked.length > 0; ) {
      if (n = o.unchecked.shift(), W = 0, n.x == s && n.y == l) {
        for (; n.px != null; )
          v[v.length] = n.x, v[v.length] = n.y, n = o["cell_" + n.py + "_" + n.px];
        o.done = !0;
        break;
      }
      n.visited = !0, y = !1, this.getArray(n.x + 1, n.y) == 1 && (W = Math.abs(n.x + 1 - s) + Math.abs(n.y - l), o.name = "cell_" + n.y + "_" + (n.x + 1), o[o.name] == null && (o[o.name] = {
        x: n.x + 1,
        y: n.y,
        visited: !1,
        px: n.x,
        py: n.y,
        cost: W
      }, y == !1 && (o.unchecked[o.unchecked.length] = o[o.name]))), y = !1, this.getArray(n.x - 1, n.y) == 1 && (W = Math.abs(n.x - 1 - s) + Math.abs(n.y - l), o.name = "cell_" + n.y + "_" + (n.x - 1), o[o.name] == null && (o[o.name] = {
        x: n.x - 1,
        y: n.y,
        visited: !1,
        px: n.x,
        py: n.y,
        cost: W
      }, y == !1 && (o.unchecked[o.unchecked.length] = o[o.name]))), y = !1, this.getArray(n.x, n.y + 1) == 1 && (W = Math.abs(n.x - s) + Math.abs(n.y + 1 - l), o.name = "cell_" + (n.y + 1) + "_" + n.x, o[o.name] == null && (o[o.name] = {
        x: n.x,
        y: n.y + 1,
        visited: !1,
        px: n.x,
        py: n.y,
        cost: W
      }, y == !1 && (o.unchecked[o.unchecked.length] = o[o.name]))), y = !1, this.getArray(n.x, n.y - 1) == 1 && (W = Math.abs(n.x - s) + Math.abs(n.y - 1 - l), o.name = "cell_" + (n.y - 1) + "_" + n.x, o[o.name] == null && (o[o.name] = {
        x: n.x,
        y: n.y - 1,
        visited: !1,
        px: n.x,
        py: n.y,
        cost: W
      }, y == !1 && (o.unchecked[o.unchecked.length] = o[o.name]))), y = !1, this.getArray(n.x + 1, n.y + 1) == 1 && this.getArray(n.x, n.y + 1) == 1 && this.getArray(n.x + 1, n.y) == 1 && (W = Math.abs(n.x + 1 - s) + Math.abs(n.y + 1 - l), o.name = "cell_" + (n.y + 1) + "_" + (n.x + 1), o[o.name] == null && (o[o.name] = {
        x: n.x + 1,
        y: n.y + 1,
        visited: !1,
        px: n.x,
        py: n.y,
        cost: W
      }, y == !1 && (o.unchecked[o.unchecked.length] = o[o.name]))), y = !1, this.getArray(n.x - 1, n.y - 1) == 1 && this.getArray(n.x, n.y - 1) == 1 && this.getArray(n.x - 1, n.y) == 1 && (W = Math.abs(n.x - 1 - s) + Math.abs(n.y - 1 - l), o.name = "cell_" + (n.y - 1) + "_" + (n.x - 1), o[o.name] == null && (o[o.name] = {
        x: n.x - 1,
        y: n.y - 1,
        visited: !1,
        px: n.x,
        py: n.y,
        cost: W
      }, y == !1 && (o.unchecked[o.unchecked.length] = o[o.name]))), y = !1, this.getArray(n.x - 1, n.y + 1) == 1 && this.getArray(n.x, n.y + 1) == 1 && this.getArray(n.x - 1, n.y) == 1 && (W = Math.abs(n.x - 1 - s) + Math.abs(n.y + 1 - l), o.name = "cell_" + (n.y + 1) + "_" + (n.x - 1), o[o.name] == null && (o[o.name] = {
        x: n.x - 1,
        y: n.y + 1,
        visited: !1,
        px: n.x,
        py: n.y,
        cost: W
      }, y == !1 && (o.unchecked[o.unchecked.length] = o[o.name]))), y = !1, this.getArray(n.x + 1, n.y - 1) == 1 && this.getArray(n.x, n.y - 1) == 1 && this.getArray(n.x + 1, n.y) == 1 && (W = Math.abs(n.x + 1 - s) + Math.abs(n.y - 1 - l), o.name = "cell_" + (n.y - 1) + "_" + (n.x + 1), o[o.name] == null && (o[o.name] = {
        x: n.x + 1,
        y: n.y - 1,
        visited: !1,
        px: n.x,
        py: n.y,
        cost: W
      }, y == !1 && (o.unchecked[o.unchecked.length] = o[o.name])));
    }
    if (o.done)
      return v;
  }
}
class Ps extends b {
  constructor() {
    var e = null, t = void 0, s = void 0;
    super(), e = g(p, this.parent), t = Math.floor(this.x / e.gridSize), s = Math.floor(this.y / e.gridSize), e.setGrid(t, s, 1), this.parent.removeChild(this);
  }
}
class Es extends x {
  constructor() {
    super(), this.addEventListener(w.ENTER_FRAME, this.tick, !1, 0, !0);
  }
  clicked(e) {
    this.game.baseSpeed = 0, this.game.showPaused();
  }
  tick(e) {
    this.game.baseSpeed > 0 ? this.gotoAndStop(2) : this.gotoAndStop(1);
  }
}
class Js extends d {
  constructor() {
    super();
  }
}
class Bs extends x {
  __fi() {
    this.unlocked = !1, this.levelNumber = 0;
  }
  constructor() {
    super();
  }
  clicked(e) {
    this.game.getFrame(3);
  }
}
class We {
  __fi() {
    this.startFunds = h(500), this.Levels = null, this.funds = h(500), this.credits = h(0), this.GameData = null, this.Towers = null, this.game = null;
  }
  constructor(e, t) {
    m(this), c(this), this.game = t, this.GameData = Bt.getLocal("FD-2-32-317", "/", !1), this.GameData.data.bPlayed == 1 ? this.LoadData() : this.createNewData();
  }
  saveLevel(e, t) {
    e = h(e), t = h(t), h(this.Levels[e]) < t && (this.Levels[e] = t, this.saveData());
  }
  addStartFunds(e) {
    e = h(e), this.startFunds = h(this.startFunds + e), this.saveData();
  }
  LoadData() {
    var e = void 0;
    for (this.GameData.data.bPlayed = 1, this.credits = h(h(this.GameData.data.credits)), this.startFunds = h(h(this.GameData.data.stFunds)), this.Towers = new Array(), this.Levels = new Array(), e = 0; e < 20; )
      this.Towers.push(h(this.GameData.data["Tower" + e])), e++;
    for (e = 0; e < 40; )
      this.Levels.push(h(this.GameData.data["Level" + e])), e++;
    this.funds = h(this.startFunds);
  }
  saveData() {
    var e = void 0;
    for (this.GameData.data.bPlayed = 1, this.GameData.data.credits = this.credits, this.GameData.data.stFunds = this.startFunds, e = 0; e < 20; )
      this.GameData.data["Tower" + e] = h(this.Towers[e]), e++;
    for (e = 0; e < 40; )
      this.GameData.data["Level" + e] = h(this.Levels[e]), e++;
  }
  createNewData() {
    var e = void 0;
    for (this.credits = h(1e3), this.Towers = new Array(), this.Levels = new Array(), e = 0; e < 20; )
      this.Towers.push(0), e++;
    for (this.Towers[2] = 1, this.Towers[3] = 1, this.Towers[4] = 1, this.Towers[13] = 1, e = 0; e < 40; )
      this.Levels.push(0), e++;
    this.Levels[0] = 1, this.saveData();
  }
  unlockTower(e) {
    e = h(e), this.Towers[e] = 1, this.saveData();
  }
  buyItem(e, t) {
    e = h(e), t == "tower" && this.unlockTower(e), t == "bonus" && this.addStartFunds(e), t == "level" && (this.Levels[e - 1] = 1, this.saveData());
  }
  addCredit(e) {
    this.credits = h(this.credits + e), this.GameData.data.credits = this.credits;
  }
}
class Q {
  __fi() {
    this.who = null, this.game = null, this.radius = NaN, this.damage = NaN, this.x = NaN, this.y = NaN, this.dType = null;
  }
  constructor(e, t, s, l, o, v, u = null) {
    m(this), c(this), this.game = e, this.x = t, this.y = s, this.radius = l * 2, this.dType = v, this.damage = o, this.who = u, this.blast();
  }
  inRange(e) {
    var t = void 0, s = void 0, l = void 0, o = void 0, v = void 0;
    return t = !1, s = this.x - e.x, l = this.y - e.y, o = s * s + l * l, v = this.radius * this.radius, o <= v && (t = !0), t;
  }
  blast() {
    var e = void 0, t = null, s = void 0;
    for (e = this.game.Enemies.length, s = 0; s < e; )
      t = g(k, this.game.Enemies[s]), t != null && this.inRange(t) && t.takeDamage(this.damage, this.dType, this.who), s++;
  }
}
class q {
  __fi() {
    this.Rate = 10, this.counter = 0, this.dType = null, this.y = NaN, this.radius = NaN, this.Type = null, this.Life = h(100), this.damage = NaN, this.game = null, this.who = null, this.x = NaN;
  }
  constructor(e, t, s, l, o, v, u = null, n = "", W = 100, y = 10) {
    W = h(W), m(this), c(this), this.game = e, this.x = t, this.y = s, this.radius = l * 2, this.dType = v, this.damage = o, this.who = u, this.Type = n, this.Life = h(W), this.Rate = y, this.game.Projectiles.push(this);
  }
  inRange(e) {
    var t = void 0, s = void 0, l = void 0, o = void 0, v = void 0;
    return t = !1, s = this.x - e.x, l = this.y - e.y, o = s * s + l * l, v = this.radius * this.radius, o <= v && (t = !0), t;
  }
  blast() {
    var e = void 0, t = null, s = void 0;
    for (e = this.game.Enemies.length, s = 0; s < e; )
      t = g(k, this.game.Enemies[s]), t != null && this.inRange(t) && t.takeDamage(0, this.dType, this.who), s++;
  }
  Tick(e) {
    this.Life = h(this.Life - e), this.counter += e, this.Life <= 0 && this.game.killProjectile(this, !0), this.counter >= this.Rate && (this.counter = 0, this.blast()), Math.random() > 0.7 && (this.Type == "fire" ? new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.2 + Math.random() * 0.4, -(Math.random() * 0.5), 10, 0.1, 10, 0, null, null, 1, !0) : new r(this.game, this.game.SpritePool.bioCloud, 32, this.x, this.y, -0.2 + Math.random() * 0.4, -(Math.random() * 0.3), 10, 0.1, 10, 0, null, null, 1, !1));
  }
}
class I extends x {
  __fi() {
    this.uType = null;
  }
  constructor() {
    super(), this.addEventListener(w.ENTER_FRAME, this.tick, !1, 0, !0);
  }
  clicked(e) {
    this.alpha >= 1 && this.game.Selected.upGrade(this.uType);
  }
  tick(e) {
    this.game.Selected != null ? this.uType == "damage" ? this.game.player.funds > this.game.Selected.saleValue * 0.5 && this.game.Selected.DLevel < 1 ? this.alpha = 1 : this.alpha = 0.2 : this.uType == "rof" ? this.game.player.funds > this.game.Selected.saleValue * 0.5 > 0 && this.game.Selected.RoFLevel < 1 ? this.alpha = 1 : this.alpha = 0.2 : this.uType == "range" && (this.game.player.funds > this.game.Selected.saleValue * 0.5 > 0 && this.game.Selected.RLevel < 1 ? this.alpha = 1 : this.alpha = 0.2) : this.alpha = 0.2;
  }
}
class Rs extends I {
  constructor() {
    super(), this.uType = "range";
  }
}
class zs extends d {
  __fi() {
    this.sRing = null, this.game = null;
  }
  constructor() {
    super(), this.mouseEnabled = !1, this.visible = !1, this.game = g(p, this.parent), this.addEventListener(w.ENTER_FRAME, this.Tick, !1, 0, !0);
  }
  Tick(e) {
    var t = void 0;
    this.game.Selected != null ? (this.x = this.game.Selected.x, this.y = this.game.Selected.y, t = this.game.Selected.Range + this.game.Selected.Range * this.game.Selected.RLevel, this.sRing.scaleX = t / 100, this.sRing.scaleY = this.sRing.scaleX, this.visible = !0, this.game.Selected.bHeld ? this.game.Selected.bBlocked ? this.sRing.gotoAndStop(2) : this.sRing.gotoAndStop(1) : this.sRing.gotoAndStop(1)) : this.visible = !1;
  }
}
class Is extends b {
  __fi() {
    this.clearRec = null, this.canvas = null, this.glowScreen = null, this.gcanvas = null, this.DisplayScreen = null, this.items = new Array(), this.game = null;
  }
  constructor() {
    super(), this.mouseEnabled = !1, this.game = g(p, this.parent), this.addEventListener(w.ENTER_FRAME, this.Tick, !1, 0, !0), this.mouseEnabled = !1, this.canvas = new _(800, 480, !0, 0), this.gcanvas = new _(800, 480, !0, 0), this.clearRec = new B(0, 0, 800, 480), this.DisplayScreen = new V(this.canvas), this.glowScreen = new V(this.gcanvas), this.glowScreen.blendMode = "add", this.addChild(this.DisplayScreen), this.addChild(this.glowScreen);
  }
  render(e, t, s, l, o, v, u, n) {
    l = h(l), o = h(o);
    var W = null, y = null, S = null;
    W = new B(o * l, 0, l, l), u ? (y = new R(t - l * 0.5, s - n - l * 0.5), S = new R(t - l * 0.5, s - l * 0.5), this.canvas.copyPixels(e, W, y, null, null, !0), this.canvas.copyPixels(this.game.SpritePool.shadow, W, S, null, null, !0)) : (y = new R(t - l * 0.5, s - l * 0.5), v ? this.gcanvas.copyPixels(e, W, y, null, null, !0) : this.canvas.copyPixels(e, W, y, null, null, !0));
  }
  Tick(e) {
    var t = void 0, s = void 0, l = void 0;
    for (this.canvas.fillRect(this.clearRec, 0), this.gcanvas.fillRect(this.clearRec, 0), t = this.game.renderElements.length, s = 0; s < t; )
      l = this.game.renderElements[s], this.render(l.bmp, l.x, l.y, l.bSize, l.frame, l.glow, l.shadow, l.z), s++;
  }
}
class Gs extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Xs extends I {
  constructor() {
    super(), this.uType = "rof";
  }
}
class Hs extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Us extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class M extends d {
  __fi() {
    this.itemName = null, this.itemDesc = null, this.ownedItem = !1, this.itemID = 0, this.mId = null, this.itemCost = 0, this.bPremium = !1, this.type = "tower", this.owner = null, this.game = null;
  }
  constructor() {
    super(), this.gotoAndStop(1), this.owner = g(z, this.parent), this.game = g(p, this.parent.parent), this.addEventListener(A.ROLL_OVER, this.over, !1, 0, !0), this.addEventListener(A.ROLL_OUT, this.out, !1, 0, !0);
  }
  checkItem() {
    var e = void 0;
    this.type == "tower" && this.game.player.Towers[this.itemID] == 1 && (this.bPremium = !0, e = new K(), this.addChild(e)), this.type == "level" && this.game.player.Levels[this.itemID - 1] > 0 && (this.bPremium = !0, e = new K(), this.addChild(e));
  }
  out(e) {
    this.gotoAndStop(1);
  }
  buy() {
    this.bPremium || this.game.player.credits >= this.itemCost && this.owner.buyItem(this);
  }
  over(e) {
    this.gotoAndStop(2), this.owner.selectItem(this);
  }
}
class js extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(5e3), this.itemName = "Flame Thrower", this.itemDesc = "Set your enemies alight with the War-Mart Flame Master Pro MKII. Can also be used to raise your enemies core temperature enabling them to be seen by heat seeking missiles.", this.itemID = h(9), this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class $s extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(6e3), this.itemName = "Napalm Support Strike", this.itemDesc = "Be the toast of your peers while you survey the toasted landscape you leave in your wake. Not only does this baby hit em hard it'll fry anything and anyone thats foolish enough to get too close", this.itemID = h(15), this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class Os extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(1e3), this.itemName = "Bonus Start Fund's", this.itemDesc = "Having trouble making end's meet. Purchase the War-Mongers investors package and find an extra $200 in your bank to help get your army up to speed.", this.itemID = h(200), this.type = "bonus", this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class Vs extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(5e3), this.itemName = "Sniper Mission 1", this.itemDesc = "Take Control in this sniper based bonus game. What a fun way to earn extra War-Mart Credits .", this.itemID = h(16), this.type = "level", this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class qs extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(6e3), this.itemName = "Sniper Mission 2", this.itemDesc = "Take Control in this sniper based bonus game. What a fun way to earn extra War-Mart Credits .", this.itemID = h(18), this.type = "level", this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class Ks extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(8e3), this.itemName = "Sniper Mission 3", this.itemDesc = "Take Control in this sniper based bonus game. What a fun way to earn extra War-Mart Credits .", this.itemID = h(20), this.type = "level", this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class Ys extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(7e3), this.itemName = "Sniper Mission 04", this.itemDesc = "Take Control in this sniper based bonus game. What a fun way to earn extra War-Mart Credits .", this.itemID = h(22), this.type = "level", this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class Qs extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(6e3), this.itemName = "Bio Cannon", this.itemDesc = "If putting holes in your enemy just isn't enough, why not given them a taste of our War-Mart Bio Cannon MK II, Designed to deliver a compact yet leathal dose of Bio-Chemical sludge thats guaranteed to have them praying for a quick end.. (small print: May be illegal under current international laws)", this.itemID = h(8), this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class Zs extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(1e4), this.itemName = "Bio Mortar", this.itemDesc = "If nothing pleases you more than watching your enemy staggering for safety while under the effects of our patented Bio-Chemical Sludge, why not share the fun with our new Bio Motar Cannon. Now you can watch as whole platoons are effected by just a single shot.", this.itemID = h(7), this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class ei extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(7e3), this.itemName = "Particle Beam Cannon", this.itemDesc = "Bringing warfare into the 21st centuary with style, the WarTech Particle Beam Cannon is the pinnicle in portable particle collision technology, tired of spending time clearing the battlefield of your enemies remains? with the Particle Beam Cannon there will be very little remains left to clear", this.itemID = h(6), this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class ti extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(4e3), this.itemName = "Mounted Machine Gun", this.itemDesc = "Speed things up a little with the 'Heckle and Klutz' Manual Mounted Machine Gun, Perfect for those 1 v 100 situations that just can't be avoided", this.itemID = h(10), this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class si extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(9e3), this.itemName = "Plasma Cannon", this.itemDesc = "Having trouble keeping up with enemy vehicles? Then you need the WarTech Plasma Motar Cannon. Not only does it deliver an impressive punch, it has the benificial side effect of putting a 'Chemical Spanner' in the works of enemy engines", this.itemID = h(11), this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class ii extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(8e3), this.itemName = "Missile Launcher", this.itemDesc = "Having trouble taking down those armoured vehicles? Then you need the WarTech SOB Heat Seeking Missile Launcher. Warning: Targets may need some pre-heating for successful lock-on", this.itemID = h(1), this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class ai extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(2e3), this.itemName = "Shotgun Tower", this.itemDesc = "Is your army complaining about lack of firepower? Then why not give treat them to the new and improved Heckle n Kluts G12 shotgun. In your face power without the price!", this.itemID = h(5), this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class hi extends M {
  __fi() {
    this.iName = null, this.iCost = null;
  }
  constructor() {
    super(), this.itemCost = h(12e3), this.itemName = "Bio-Chemical Support Strike", this.itemDesc = "Tired of inaccurate support engineers peppering your battlefield with stray motar strikes. Why try the new patented Bio-Chemical howitzer strike? all the benifits of the original howitzer with added contaminated real estate", this.itemID = h(14), this.setUp();
  }
  setUp() {
    this.iName.text = this.itemName, this.iCost.text = "$ " + this.itemCost, this.checkItem();
  }
}
class oi extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class li extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class K extends d {
  constructor() {
    super();
  }
}
class N {
  __fi() {
    this.vx = NaN, this.vy = NaN, this.game = null, this.x = NaN, this.y = NaN, this.life = NaN, this.bFade = !1;
  }
  constructor(e, t, s, l, o = 0, v = 0, u = !0) {
    m(this), c(this), this.game = l, this.x = e, this.y = t, this.life = s, this.bFade = u, o == 0 && v == 0 ? (this.vx = -4 + Math.random() * 8, this.vy = -4 + Math.random() * 4) : (this.vx = o, this.vy = v), this.game.fx.sparks.push(this);
  }
  Tick() {
    --this.life, this.x += this.vx, this.y += this.vy, this.bFade && (this.vy += 0.1, this.vx *= 0.99), this.life <= 0 && this.game.fx.removeSpark(this);
  }
}
class ni extends Y {
  constructor() {
    super(), this.SetGrid(), this.game.startPoints.push(this), this.visible = !1;
  }
}
class ge extends k {
  __fi() {
    this.desiredAngle = 0, this.hull = null;
  }
  constructor(e, t) {
    super(), this.endPoint = t, this.game = e, this.speed = 1, this.baseSpeed = this.speed, this.bMetal = !0, this.health = 500, this.heat_resistance = 0.7;
  }
  closestAngle(e, t) {
    var s = NaN;
    return s = t - e, s %= 360, s += 540, s %= 360, s - 180;
  }
  deathFX() {
    new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 2, 10, 0.1, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1.2, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10), this.game.decalFX.Crater(this.x, this.y), this.game.Soundbox.playHit("explode");
  }
  setAngle() {
    var e = NaN, t = void 0;
    e = this.ang * 180 / Math.PI, e != this.desiredAngle && (t = this.closestAngle(this.desiredAngle, e) * 0.3, Math.abs(t) < 0.1 ? this.desiredAngle = e : this.desiredAngle += t), this.desiredAngle > 180 ? this.desiredAngle -= 360 : this.desiredAngle < -180 && (this.desiredAngle += 360), this.hull.gotoAndStop(Math.round((this.desiredAngle + 180) / 5));
  }
  Move(e) {
    var t = void 0, s = void 0;
    this.x += this.vx * this.speed * e, this.y += this.vy * this.speed * e, this.gx == Math.floor(this.tx / 32) && this.gy == Math.floor(this.ty / 32) && this.x > this.gx * this.game.gridSize + 4 && this.x < this.gx * this.game.gridSize + 28 && this.y > this.gy * this.game.gridSize + 4 && this.y < this.gy * this.game.gridSize + 28 && (this.ty = h(Number(this.pth.pop()) * this.game.gridSize + 16), this.tx = h(Number(this.pth.pop()) * this.game.gridSize + 16), t = this.tx - this.x, s = this.ty - this.y, this.ang = Math.atan2(s, t), this.vx = Math.cos(this.ang), this.vy = Math.sin(this.ang)), this.setAngle();
  }
}
class pe extends k {
  __fi() {
    this.desiredAngle = 0, this.hull = null;
  }
  constructor(e, t) {
    super(), this.endPoint = t, this.game = e, this.speed = 0.7, this.baseSpeed = this.speed, this.bMetal = !0, this.health = 400, this.heat_resistance = 0.6;
  }
  closestAngle(e, t) {
    var s = NaN;
    return s = t - e, s %= 360, s += 540, s %= 360, s - 180;
  }
  deathFX() {
    new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 2, 10, 0.1, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1.2, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10), this.game.decalFX.Crater(this.x, this.y), this.game.Soundbox.playHit("explode");
  }
  setAngle() {
    var e = NaN, t = void 0;
    e = this.ang * 180 / Math.PI, e != this.desiredAngle && (t = this.closestAngle(this.desiredAngle, e) * 0.3, Math.abs(t) < 0.1 ? this.desiredAngle = e : this.desiredAngle += t), this.desiredAngle > 180 ? this.desiredAngle -= 360 : this.desiredAngle < -180 && (this.desiredAngle += 360), this.hull.gotoAndStop(Math.round((this.desiredAngle + 180) / 5));
  }
  Move(e) {
    var t = void 0, s = void 0;
    this.x += this.vx * this.speed * e, this.y += this.vy * this.speed * e, this.gx == Math.floor(this.tx / 32) && this.gy == Math.floor(this.ty / 32) && this.x > this.gx * this.game.gridSize + 4 && this.x < this.gx * this.game.gridSize + 28 && this.y > this.gy * this.game.gridSize + 4 && this.y < this.gy * this.game.gridSize + 28 && (this.ty = h(Number(this.pth.pop()) * this.game.gridSize + 16), this.tx = h(Number(this.pth.pop()) * this.game.gridSize + 16), t = this.tx - this.x, s = this.ty - this.y, this.ang = Math.atan2(s, t), this.vx = Math.cos(this.ang), this.vy = Math.sin(this.ang)), this.setAngle();
  }
}
class ye extends k {
  __fi() {
    this.desiredAngle = 0, this.hull = null;
  }
  constructor(e, t) {
    super(), this.endPoint = t, this.game = e, this.speed = 0.6, this.baseSpeed = this.speed, this.bMetal = !0, this.health = 550, this.heat_resistance = 0.6;
  }
  closestAngle(e, t) {
    var s = NaN;
    return s = t - e, s %= 360, s += 540, s %= 360, s - 180;
  }
  deathFX() {
    new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 2, 10, 0.1, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.flameShot, 32, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10, 0, null, null, 1, !0), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1.2, 10, 0.1, 10), new r(this.game, this.game.SpritePool.smoke, 64, this.x, this.y, -0.5 + Math.random() * 1, -Math.random() * 1, 10, 0.2, 10), this.game.decalFX.Crater(this.x, this.y), this.game.Soundbox.playHit("explode");
  }
  setAngle() {
    var e = NaN, t = void 0;
    e = this.ang * 180 / Math.PI, e != this.desiredAngle && (t = this.closestAngle(this.desiredAngle, e) * 0.3, Math.abs(t) < 0.1 ? this.desiredAngle = e : this.desiredAngle += t), this.desiredAngle > 180 ? this.desiredAngle -= 360 : this.desiredAngle < -180 && (this.desiredAngle += 360), this.hull.gotoAndStop(Math.round((this.desiredAngle + 180) / 5));
  }
  Move(e) {
    var t = void 0, s = void 0;
    this.x += this.vx * this.speed * e, this.y += this.vy * this.speed * e, this.gx == Math.floor(this.tx / 32) && this.gy == Math.floor(this.ty / 32) && this.x > this.gx * this.game.gridSize + 4 && this.x < this.gx * this.game.gridSize + 28 && this.y > this.gy * this.game.gridSize + 4 && this.y < this.gy * this.game.gridSize + 28 && (this.ty = h(Number(this.pth.pop()) * this.game.gridSize + 16), this.tx = h(Number(this.pth.pop()) * this.game.gridSize + 16), t = this.tx - this.x, s = this.ty - this.y, this.ang = Math.atan2(s, t), this.vx = Math.cos(this.ang), this.vy = Math.sin(this.ang)), this.setAngle();
  }
}
class E {
  __fi() {
    this.vx = NaN, this.vy = NaN, this.game = null, this.wgt = NaN, this.ex = NaN, this.ey = NaN, this.x = NaN, this.y = NaN, this.life = NaN, this.bFade = !1;
  }
  constructor(e, t, s, l, o, v, u = 1) {
    m(this), c(this), this.game = v, this.x = e, this.y = t, this.ex = s, this.ey = l, this.life = o, this.wgt = u, this.game.fx.tracers.push(this);
  }
  Tick() {
    --this.life, this.life <= 0 && this.game.fx.removeTracer(this);
  }
}
class ri extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class di extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class _e extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class fe extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Ce extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class xe extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Se extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class we extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Te extends k {
  __fi() {
    this.soldier = null;
  }
  constructor(e, t) {
    super(), this.endPoint = t, this.game = e, this.speed = 0.8, this.baseSpeed = this.speed, this.bMetal = !0, this.health = 1e3;
  }
  deathFX() {
    new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.5, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.5, 8), this.game.decalFX.Crater(this.x, this.y), this.game.Soundbox.playHit("explode");
  }
  setAngle() {
    this.soldier.gotoAndStop(Math.round((180 + this.ang * 180 / Math.PI) / 45) + 1);
  }
}
class vi extends b {
  __fi() {
    this.game = null;
  }
  constructor() {
    super(), this.addEventListener(A.CLICK, this.clicked, !1, 0, !0), this.game = g(p, this.parent);
  }
  clicked(e) {
    this.game.Selected = null;
  }
}
class mi extends b {
  __fi() {
    this.game = null;
  }
  constructor() {
    super(), this.addEventListener(A.CLICK, this.clicked, !1, 0, !0), this.game = g(p, this.parent);
  }
  clicked(e) {
    this.game.Selected.Fire();
  }
}
class ci extends b {
  constructor() {
    super(), this.addEventListener(A.MOUSE_UP, this.clicked, !1, 0, !0);
  }
  clicked(e) {
    g(F, this.parent).clicked();
  }
}
class ui extends d {
  constructor() {
    super();
  }
}
class Wi extends x {
  __fi() {
    this.unlocked = !1, this.levelNumber = 0;
  }
  constructor() {
    super();
  }
  clicked(e) {
    this.game.baseSpeed = 1, this.parent.visible = !1;
  }
}
class be extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class gi extends x {
  constructor() {
    super();
  }
  clicked(e) {
    this.game.getFrame(2);
  }
}
class pi extends d {
  __fi() {
    this.desc = null;
  }
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class yi extends I {
  constructor() {
    super(), this.uType = "damage";
  }
}
class _i extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Me extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class ke extends k {
  __fi() {
    this.soldier = null;
  }
  constructor(e, t) {
    super(), this.endPoint = t, this.game = e, this.speed = 1, this.baseSpeed = this.speed, this.heat_resistance = 0.8;
  }
  deathFX() {
    new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.5, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.4, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.2, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.3, 8), this.game.decalFX.Splat(this.x, this.y), this.game.Soundbox.playHit("splat");
  }
  setAngle() {
    this.soldier.gotoAndStop(Math.round((180 + this.ang * 180 / Math.PI) / 45) + 1);
  }
}
class Fe extends k {
  __fi() {
    this.soldier = null;
  }
  constructor(e, t) {
    super(), this.endPoint = t, this.game = e, this.speed = 0.8, this.baseSpeed = this.speed, this.radiation_resistance = 0.6;
  }
  deathFX() {
    new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.5, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.5, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.2, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.3, 8), this.game.decalFX.Splat(this.x, this.y), this.game.Soundbox.playHit("splat");
  }
  setAngle() {
    this.soldier.gotoAndStop(Math.round((180 + this.ang * 180 / Math.PI) / 45) + 1);
  }
}
class fi extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Ne extends k {
  __fi() {
    this.soldier = null;
  }
  constructor(e, t) {
    super(), this.endPoint = t, this.game = e, this.speed = 1, this.baseSpeed = this.speed;
  }
  deathFX() {
    new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.5, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.5, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.2, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.3, 8), this.game.decalFX.Splat(this.x, this.y), this.game.Soundbox.playHit("splat");
  }
  setAngle() {
    this.soldier.gotoAndStop(Math.round((180 + this.ang * 180 / Math.PI) / 45) + 1);
  }
}
class Ci extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Ae extends k {
  __fi() {
    this.soldier = null;
  }
  constructor(e, t) {
    super(), this.endPoint = t, this.game = e, this.speed = 1, this.baseSpeed = this.speed, this.poison_resistance = 0.3;
  }
  deathFX() {
    new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.5, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.5, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.2, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.3, 8), this.game.decalFX.Splat(this.x, this.y), this.game.Soundbox.playHit("splat");
  }
  setAngle() {
    this.soldier.gotoAndStop(Math.round((180 + this.ang * 180 / Math.PI) / 45) + 1);
  }
}
class xi extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Le extends k {
  __fi() {
    this.soldier = null;
  }
  constructor(e, t) {
    super(), this.endPoint = t, this.game = e, this.speed = 1, this.baseSpeed = this.speed, this.bMetal = !0;
  }
  deathFX() {
    new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.5, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.5, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.2, 8), new r(this.game, this.game.SpritePool.bloodSplat, 16, this.x, this.y, -1 + Math.random() * 2, -Math.random() * 2, 8, 0.3, 8), this.game.decalFX.Splat(this.x, this.y), this.game.Soundbox.playHit("splat");
  }
  setAngle() {
    this.soldier.gotoAndStop(Math.round((180 + this.ang * 180 / Math.PI) / 45) + 1);
  }
}
class Si extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class De extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Pe extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class wi extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class Ee extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Ti extends x {
  constructor() {
    super(), this.addEventListener(w.ENTER_FRAME, this.tick, !1, 0, !0);
  }
  clicked(e) {
    this.game.baseSpeed = 2;
  }
  tick(e) {
    this.game.baseSpeed > 1 ? this.gotoAndStop(1) : this.gotoAndStop(2);
  }
}
class bi extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Je extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Be extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class Re extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class ze extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Ie extends d {
  __fi() {
    this.healthText = null;
  }
  constructor() {
    super();
  }
  update(e) {
    this.healthText.text = String(e);
  }
}
class Ge extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Xe extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class J extends b {
  __fi() {
    this.ins2 = null, this.clButton = null, this.ins = null, this.game = null;
  }
  constructor(e, t, s, l = !0) {
    super(), this.game = e.game, this.mouseEnabled = !1, this.ins.text = t, this.ins2.text = s, this.game.baseSpeed = 0, l && (this.clButton.visible = !1);
  }
  close() {
    this.game.baseSpeed = 1, this.parent.removeChild(this);
  }
}
class Mi extends b {
  __fi() {
    this.inList = null, this.bblock = null, this.fg = null, this.game = null;
  }
  constructor() {
    var e = void 0;
    for (super(), this.inList = new Array(), e = 0; e < 10; )
      this.inList.push(0), e++;
    this.game = g(p, this.parent), this.addEventListener(w.ENTER_FRAME, this.tick, !1, 0, !0);
  }
  tick(e) {
    this.game.Towers.length <= 0 && this.inList[0] == 0 ? (this.inList[0] = 1, this.fg = new J(this, "Welcome back General. Before we move on I guess we should cover some of the basic's. Over to your right you'll find your command panel, this can be used to buy,sell , upgrade, and manage your defense towers", "To begin purchase one of the available towers from the command panel <note: A Large selection of towers are available from the war-mart>"), this.addChild(this.fg)) : this.game.Towers.length > 0 && this.inList[1] == 0 ? (this.inList[1] = 1, this.fg.close(), this.fg = new J(this, "Ok now you have purchased a tower you need to place it somewhere on the battle field. Notice the ring around your new tower? that indicates how far your tower can see.", "Place the tower where it will have a good view of the path that the enemy will follow. Enemies start in areas marked with a green flag and escape at exits marked with a red flag"), this.addChild(this.fg)) : this.game.Selected != null && this.game.Selected.bHeld == !1 && this.inList[2] == 0 ? (this.fg.close(), this.inList[2] = 1, this.fg = new J(this, "Awesome! If you have any funds left you can buy more towers or perhaps upgrade the one you have.", "To upgrade select a tower on the battle field and use the plus buttons next to the stat you wish to upgrade. When your ready hit the ok button below and try out your defenses!", !1), this.addChild(this.fg)) : this.game.Enemies.length <= 0 && this.game.levelScore > 0 && this.inList[3] == 0 ? (this.inList[3] = 1, this.fg = new J(this, "Well done General, that showed em! They were just basic soldiers, but some have resistance to certain damage types, for example black soldiers wear kevlar suits and only take half damage from bullets", "Be careful not to let to many enemy reach the exits though, any health they have left is deducted from your own. Reach zero and it's game over. Click ok to continue..", !1), this.addChild(this.fg)) : this.game.Enemies.length >= 10 && this.inList[4] == 0 ? (this.inList[4] = 1, this.fg = new J(this, "Sometimes you'll discover that your defenses are not upto the task and will need a helping hand. Thankfully you have the abilty to call in a support strike at any location on the battlefield.", "This abilty however needs to be charged and can only be used when the support strike button turns green. Click ok to continue and try using it now..", !1), this.addChild(this.fg)) : this.game.Enemies.length <= 3 && this.inList[4] == 1 && this.inList[5] == 0 ? (this.inList[5] = 1, this.fg = new J(this, "Well you seemed to survive that one so I'll guess you got the hang of it... From here on in I'll give you more control of the game.", "You can now use the buttons at the bottom of the command panel to send waves, fast forward, and pause the game... Well that covers the basic's, Good Luck General!", !1), this.addChild(this.fg)) : this.inList[5] == 1 && this.game.baseSpeed > 0 && (this.removeEventListener(w.ENTER_FRAME, this.tick), this.game.removeChild(this));
  }
  close() {
    this.parent.removeChild(this);
  }
}
class ki extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Fi extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class He extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Ni extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Ue extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class Ai extends x {
  __fi() {
    this.unlocked = !1, this.levelNumber = 0;
  }
  constructor() {
    super();
  }
  clicked(e) {
    this.game.getFrame(3);
  }
}
class je {
  __fi() {
    this.vx = NaN, this.vy = NaN, this.sx = NaN, this.sy = NaN, this.Tgt = null, this.game = null, this.ex = NaN, this.ey = NaN, this.life = NaN, this.vLen = NaN;
  }
  constructor(e, t, s, l, o, v, u = null) {
    m(this), c(this);
    var n = void 0, W = void 0;
    this.game = v, this.sx = e, this.sy = t, this.ex = s, this.ey = l, this.life = o, n = this.sx - this.ex, W = this.sy - this.ey, this.vx = -(n / 10), this.vy = -(W / 10), this.game.fx.beams.push(this);
  }
  Tick() {
    this.life -= 3, this.life <= 0 && this.game.fx.removeBeam(this);
  }
}
class Li extends d {
  constructor() {
    super();
  }
}
class Di extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Pi extends b {
  constructor() {
    super(), this.addEventListener(A.CLICK, this.clicked, !1, 0, !0);
  }
  clicked(e) {
    g(z, this.parent.parent).hideConfirm();
  }
}
class Ei extends b {
  constructor() {
    super(), this.addEventListener(A.CLICK, this.clicked, !1, 0, !0);
  }
  clicked(e) {
    g(z, this.parent.parent).purchaseBasketItem();
  }
}
class $e extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class Ji extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class Oe extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class Ve extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class qe extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class Bi extends x {
  __fi() {
    this.unlocked = !1, this.levelNumber = 0;
  }
  constructor() {
    super();
  }
  clicked(e) {
    this.game.getFrame(1);
  }
}
class Ri extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class zi extends x {
  constructor() {
    super(), this.addEventListener(w.ENTER_FRAME, this.tick, !1, 0, !0);
  }
  clicked(e) {
    this.alpha >= 1 && this.game.Selected.relocate();
  }
  tick(e) {
    this.game.Selected != null && this.game.player.funds >= 10 ? this.alpha = 1 : this.alpha = 0.5;
  }
}
class Ke extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Ii extends x {
  constructor() {
    super(), this.addEventListener(w.ENTER_FRAME, this.tick, !1, 0, !0);
  }
  clicked(e) {
    this.game.Soundbox.mute();
  }
  tick(e) {
    this.game.Soundbox.bMute ? this.gotoAndStop(2) : this.gotoAndStop(1);
  }
}
class Ye extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Gi extends d {
  __fi() {
    this.ins = null;
  }
  constructor() {
    super(), this.ins = g(J, this.parent), this.addEventListener(A.CLICK, this.clicked, !1, 0, !0);
  }
  clicked(e) {
    this.ins.close();
  }
}
class Xi extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Qe extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Hi extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Ui extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class ji extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Ze extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class $i extends x {
  __fi() {
    this.unlocked = !1, this.levelNumber = 0;
  }
  constructor() {
    super();
  }
  clicked(e) {
    this.game.levelHealth = 0;
  }
}
class et extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class tt extends d {
  constructor() {
    super();
  }
}
class r {
  __fi() {
    this.y = NaN, this.bSize = 0, this.bmp = null, this.bPoison = !1, this.radius = 6, this.glow = !1, this.bSupport = !1, this.game = null, this.shadow = !1, this.bFire = !1, this.vx = NaN, this.vy = NaN, this.vz = 0, this.dType = null, this.grid = 0, this.speed = NaN, this.bTrail = !1, this.numFrames = 0, this.bDie = !1, this.who = null, this.tFrame = 0, this.bProj = !1, this.tgt = null, this.fRate = NaN, this.frame = 0, this.damage = 0, this.gx = 0, this.gy = 0, this.lifeSpan = 0, this.x = NaN, this.z = 0, this.life = 0;
  }
  constructor(e, t, s, l, o, v = 0, u = 0, n = 0, W = 1, y = 100, S = 0, D = null, At = null, Lt = 1, Dt = !1, Pt = null, Z = !1, Et = !1, Jt = !1) {
    s = h(s), n = h(n), y = h(y), S = h(S), m(this), c(this);
    var U = void 0, j = void 0, ee = void 0;
    this.game = e, this.bmp = t, this.bSize = h(s), this.x = l, this.y = o, this.vx = v, this.vy = u, this.numFrames = h(n), this.fRate = W, this.lifeSpan = h(y), this.damage = h(S), this.dType = D, this.tgt = At, this.speed = Lt, this.glow = Dt, this.bSupport = Jt, this.damage >= 1 && (this.bProj = !0), this.grid = h(this.game.gridSize), this.game.renderElements.push(this), this.who = Pt, Z && (this.shadow = Z, this.bSupport ? (this.vz = -0.5, this.z = 450) : (U = this.tgt.x + this.tgt.vx * 40 - this.x, j = this.tgt.y + this.tgt.vy * 40 - this.y, ee = Math.sqrt(U * U + j * j), this.vz = ee * (this.speed * 0.018))), this.bTrail = Et;
  }
  destroyed() {
    this.game = null, this.bmp = null, this.dType = null, this.tgt = null;
  }
  Tick(e) {
    this.x += this.vx * e, this.y += this.vy * e, this.shadow ? (this.z += this.vz * e, this.vz -= 0.09 * e, this.bTrail && Math.random() > 0.5 && new r(this.game, this.game.SpritePool.trail, 16, this.x, this.y - this.z, 0, -0.1, 10, 0.2, 10), this.z < 0 && (new Q(this.game, this.x, this.y, 40, this.damage * 0.5, this.dType, this.who), this.bFire ? (new q(this.game, this.x, this.y, 20, 0, new f(100, 0, 0, 0), null, "fire", 500, 10), this.game.decalFX.Crater(this.x, this.y)) : this.bPoison && (new q(this.game, this.x, this.y, 20, 0, new f(0, 0, 20, 0), null, "poison", 1e3, 20), this.game.decalFX.Crater(this.x, this.y)), new r(this.game, this.game.SpritePool.explosion, 64, this.x, this.y, 0, -0.1, 16, 0.5, 16, 0, null, null, 1, !0, null), this.game.Soundbox.playHit("explode"), this.bDie = !0, this.game.killSprite(this), this.destroyed())) : this.bTrail && Math.random() > 0.5 && new r(this.game, this.game.SpritePool.trail, 16, this.x, this.y, 0, -0.1, 10, 0.5, 10), this.life += this.fRate * e, this.tFrame += this.fRate * e, this.frame = h(Math.floor(this.tFrame)), this.frame >= this.numFrames && (this.tFrame = 0), this.life > this.lifeSpan && (this.bDie = !0, this.game.killSprite(this)), this.bProj && (this.setGrid(), (!this.shadow || this.vz >= -0.05) && this.setVelocity(), this.shadow || this.testCollision());
  }
  setGrid() {
    this.gx = h(Math.floor(this.x / this.grid)), this.gy = h(Math.floor(this.y / this.grid));
  }
  testCollision() {
    this.bDie || this.tgt.gx >= this.gx - 1 && this.tgt.gx <= this.gx + 1 && this.tgt.gy >= this.gy - 1 && this.tgt.gy <= this.gy + 1 && this.testRadial();
  }
  testRadial() {
    var e = void 0, t = void 0, s = void 0, l = void 0;
    e = this.x - this.tgt.x, t = this.y - this.tgt.y, s = this.radius * this.radius + this.tgt.radius * this.tgt.radius, l = e * e + t * t, l <= s && this.collide();
  }
  collide() {
    this.tgt.takeDamage(this.damage, this.dType, this.who), this.bDie = !0, this.game.killSprite(this), this.destroyed();
  }
  setVelocity() {
    var e = void 0, t = void 0, s = void 0;
    !this.bDie && this.tgt.health > 0 && !this.bSupport && (e = this.tgt.x - this.x, t = this.tgt.y - this.y, this.shadow && (e = this.tgt.x + this.tgt.vx * 25 - this.x, t = this.tgt.y + this.tgt.vy * 25 - this.y), s = Math.sqrt(e * e + t * t), this.vx = e / s * this.speed, this.vy = t / s * this.speed);
  }
}
class Oi extends x {
  __fi() {
    this.unlocked = !1, this.levelNumber = 0;
  }
  constructor() {
    super();
  }
  clicked(e) {
    this.game.getLevel(this.game.currentLevel);
  }
}
class st extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class it extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class Vi extends d {
  __fi() {
    this.exp = NaN, this.totalText = null, this.playerCredits = 0, this.fundsText = null, this.totalScore = NaN, this.livesText = null, this.scoreText = null, this.creditsText = null, this.game = null;
  }
  constructor() {
    super(), this.game = g(p, this.parent), this.init();
  }
  init() {
    var e = void 0, t = void 0;
    e = Math.floor(this.game.levelHealth * 5), t = Math.floor(this.game.player.funds * 5), this.totalScore = Math.floor(this.game.levelScore) + e + t, this.playerCredits = h(Math.floor(this.totalScore * 0.25)), this.livesText.text = String(Math.floor(this.game.levelHealth)), this.scoreText.text = String(Math.floor(this.game.levelScore)), this.fundsText.text = String(Math.floor(this.game.player.funds)), this.totalText.text = String(this.totalScore), this.creditsText.text = String(this.playerCredits), this.saveScore();
  }
  saveScore() {
    this.game.player.saveLevel(this.game.currentLevel, this.totalScore), this.game.player.addCredit(this.playerCredits);
  }
}
class at extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class ht extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class qi extends x {
  __fi() {
    this.price = null;
  }
  constructor() {
    super(), this.addEventListener(w.ENTER_FRAME, this.tick, !1, 0, !0);
  }
  clicked(e) {
    this.alpha >= 1 && (this.game.Selected.sell(), this.game.Selected = null);
  }
  tick(e) {
    this.game.Selected != null ? (this.alpha = 1, this.price.text = String(this.game.Selected.saleValue)) : (this.alpha = 0.5, this.price.text = "000");
  }
}
class ot extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class lt extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class nt extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class Ki extends x {
  __fi() {
    this.unlocked = !1, this.levelNumber = 0;
  }
  constructor() {
    super();
  }
  clicked(e) {
    this.game.getFrame(5);
  }
}
class Yi extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class rt extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class Qi extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class dt extends _ {
  constructor(e, t) {
    super(e, t), m(this), c(this);
  }
}
class vt extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class Zi extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class ea extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class ta extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class sa extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class ia extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class aa extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class ha extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class oa extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class la extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class na extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class ra extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class da extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class va extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class ma extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class ca extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class ua extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Wa extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class ga extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class pa extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class ya extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class _a extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class fa extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Ca extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class xa extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Sa extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class wa extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class mt extends b {
  __fi() {
    this.game = null, this.snShot = null, this.bMus = !1, this.snd_explode = null, this.hs = null, this.electroShot = null, this.bioShot = null, this.snd_mtlHit = null, this.sgShot = null, this.ds = null, this.ms = null, this.bMute = !1, this.snd_bg2 = null, this.laserShot = null, this.launcherShot = null, this.snd_hit = null, this.snd_splat = null, this.flameShot = null, this.snd_bg1 = null, this.msShot = null, this.bgNoise = null, this.snd_bulletHit = null, this.bgMusic = null, this.rifleShot = null, this.fadeClock = null, this.machineGShot = null;
  }
  constructor(e) {
    super(), this.bgMusic = new $e(), this.bgNoise = new ce(), this.game = e, this.fadeClock = new se(20, 0), this.fadeClock.addEventListener(ie.TIMER, this.fadeout, !1, 0, !0), this.rifleShot = new st(), this.sgShot = new rt(), this.snShot = new vt(), this.msShot = new qe(), this.electroShot = new nt(), this.flameShot = new Be(), this.bioShot = new Ce(), this.launcherShot = new Ve(), this.laserShot = new Ue(), this.machineGShot = new Oe(), this.snd_explode = new Pe(), this.snd_splat = new ct(), O.soundTransform = new P(0.5);
  }
  mute() {
    var e = void 0;
    this.bMute ? (this.bMute = !1, O.soundTransform = new P(0.5), e = new P(0.6), this.ds != null && (this.ds.soundTransform = e), this.ms != null && (this.ms.soundTransform = e)) : (this.bMute = !0, O.soundTransform = new P(0), e = new P(0), this.ds != null && (this.ds.soundTransform = e), this.ms != null && (this.ms.soundTransform = e));
  }
  playbgm() {
    var e = void 0;
    this.bMute || this.bMus || (this.bMus = !0, e = new P(0.9), this.ms = this.bgMusic.play(0, 3, e));
  }
  stopNoise() {
    var e = void 0;
    this.ds != null && (e = new P(0), this.ds.soundTransform = e);
  }
  fadeout(e) {
    var t = void 0;
    this.ms.soundTransform.volume > 0.02 ? (t = new P(this.ms.soundTransform.volume - 0.01), this.ms.soundTransform = t) : (this.ms.stop(), this.bMus = !1, this.fadeClock.stop());
  }
  playShot(e) {
    if (!this.bMute)
      switch (e) {
        case "rifle":
          this.rifleShot.play();
          break;
        case "shotgun":
          this.sgShot.play();
          break;
        case "sniper":
          this.snShot.play();
          break;
        case "electro":
          this.electroShot.play();
          break;
        case "flame":
          this.flameShot.play();
          break;
        case "mg":
          this.machineGShot.play();
          break;
        case "bio":
          this.bioShot.play();
          break;
        case "missile":
          this.msShot.play();
          break;
        case "laser":
          this.laserShot.play();
          break;
        case "launcher":
          this.launcherShot.play();
      }
  }
  playNoise() {
    var e = void 0;
    this.bMute || (e = new P(0.6), this.ds = this.bgNoise.play(0, 100, e));
  }
  stopbgm() {
    this.fadeClock.start();
  }
  Win() {
    this.bMute;
  }
  playHit(e) {
    if (!this.bMute)
      switch (e) {
        case "explode":
          this.snd_explode.play();
          break;
        case "splat":
          this.snd_splat.play();
      }
  }
}
class ct extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class Ta extends C {
  constructor() {
    super(), m(this), c(this);
  }
}
class ut {
  __fi() {
    this.mFlash = null, this.bloodSplat = null, this.trail = null, this.poison = null, this.bioCloud = null, this.flameShot = null, this.explosion = null, this.empX = null, this.pshell = null, this.rad = null, this.freeze = null, this.fireX = null, this.shadow = null, this.bioShot = null, this.lBlast = null, this.bFlash = null, this.smoke = null, this.shell = null, this.flame = null;
  }
  constructor() {
    m(this), c(this), this.explosion = new De(1024, 64), this.bloodSplat = new Se(128, 16), this.mFlash = new Ye(128, 16), this.bFlash = new we(128, 16), this.empX = new Me(640, 64), this.fireX = new Ee(1024, 64), this.trail = new Ke(160, 16), this.flame = new Je(128, 16), this.rad = new ze(128, 16), this.poison = new et(128, 16), this.freeze = new Xe(128, 16), this.lBlast = new He(128, 16), this.flameShot = new Re(320, 32), this.bioShot = new fe(320, 32), this.smoke = new dt(640, 64), this.shadow = new ot(320, 32), this.shell = new lt(320, 32), this.pshell = new Ze(320, 32), this.bioCloud = new _e(320, 32);
  }
}
class G extends x {
  __fi() {
    this.reloadTime = 0.05, this.counter = 0, this.towerType = null, this.reloadBar = null, this.idx = 0, this.cost = 0;
  }
  constructor() {
    super(), this.addEventListener(w.ENTER_FRAME, this.tick, !1, 0, !0), this.reloadBar = new tt(), this.reloadBar.x = -14, this.reloadBar.y = 0, this.addChild(this.reloadBar), this.reloadBar.scaleX = 0, this.reloadBar.gotoAndStop(1);
  }
  clicked(e) {
    this.alpha >= 1 && this.counter >= 100 && (this.counter = 0, this.game.buildTower(this.towerType, this.cost));
  }
  tick(e) {
    this.counter += this.reloadTime * this.game.baseSpeed, this.counter > 100 ? (this.counter = 100, this.reloadBar.gotoAndStop(2)) : this.reloadBar.gotoAndStop(1), this.reloadBar.scaleX = this.counter * 0.01, this.game.player.Towers[this.idx] != 1 ? this.alpha = 0 : this.alpha = 1;
  }
}
class ba extends G {
  constructor() {
    super(), this.idx = h(15), this.cost = h(0), this.towerType = "suptower1";
  }
}
class Ma extends G {
  constructor() {
    super(), this.idx = h(14), this.cost = h(0), this.towerType = "suptower2";
  }
}
class ka extends G {
  constructor() {
    super(), this.idx = h(13), this.cost = h(0), this.towerType = "suptower3";
  }
}
class X extends F {
  __fi() {
    this.shots = h(0), this.shotLimit = h(1);
  }
  constructor() {
    super();
  }
  relocate() {
  }
  aim() {
  }
  dropTower() {
    this.SetGrid(), this.snapToGrid(), this.bHeld = !1, this.bBlocked = !1, this.game.Selected = null;
  }
  sell() {
  }
  Fire() {
  }
  Tick(e) {
    this.bHeld ? (this.x = this.game.mouseX, this.y = this.game.mouseY, this.x > 640 && (this.x = 640), this.snapToGrid(), this.bBlocked = !1) : (this.loading = h(this.loading + (this.RoF + this.RoF * this.RoFLevel) * e), this.loading >= 100 && (this.loading = h(0), this.Fire(), this.shots = h(this.shots + 1), this.shots >= this.shotLimit && (this.loading = h(-1e4), this.game.killTower(this))));
  }
}
class Wt extends X {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(100), this.RoF = h(3), this.Range = h(80), this.shotLimit = h(6), this.game = e, this.turret.gotoAndStop(1);
  }
  Fire() {
    var e = void 0, t = void 0, s = void 0;
    e = this.x - 60 + Math.random() * 120, t = this.y - 60 + Math.random() * 120, s = new r(this.game, this.game.SpritePool.shell, 32, e, t, 0, 0, 9, 0.2, 100, this.Damage, new f(100, 0, 0, 0), null, 1.6, !1, this, !0, !0, !0), s.bFire = !0, this.game.Soundbox.playShot("bio");
  }
}
class gt extends X {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(50), this.RoF = h(3), this.Range = h(60), this.shotLimit = h(3), this.game = e, this.turret.gotoAndStop(1);
  }
  Fire() {
    var e = void 0, t = void 0, s = void 0;
    e = this.x - 40 + Math.random() * 80, t = this.y - 40 + Math.random() * 80, s = new r(this.game, this.game.SpritePool.shell, 32, e, t, 0, 0, 9, 0.2, 100, this.Damage, new f(0, 0, 80, 0), null, 1.6, !1, this, !0, !0, !0), s.bPoison = !0, this.game.Soundbox.playShot("bio");
  }
}
class pt extends X {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(50), this.RoF = h(8), this.Range = h(60), this.shotLimit = h(8), this.game = e, this.turret.gotoAndStop(1);
  }
  Fire() {
    var e = void 0, t = void 0;
    e = this.x - 40 + Math.random() * 80, t = this.y - 40 + Math.random() * 80, new r(this.game, this.game.SpritePool.shell, 32, e, t, 0, 0, 9, 0.2, 100, this.Damage, new f(20, 0, 0, 0), null, 1.6, !1, this, !0, !0, !0), this.game.Soundbox.playShot("bio");
  }
}
class Fa extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Na extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class Aa extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class La extends x {
  __fi() {
    this.lastClick = h(200);
  }
  constructor() {
    super(), this.addEventListener(w.ENTER_FRAME, this.ticker, !1, 0, !0);
  }
  ticker(e) {
    ++this.lastClick, this.lastClick >= 200 ? this.alpha = 1 : this.alpha = 0.2;
  }
  clicked(e) {
    this.alpha == 1 && (this.game.WaveControl.newWave(), this.lastClick = h(0));
  }
}
class yt extends F {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(80), this.RoF = h(1), this.Range = h(150), this.expLimit = 2e3, this.targeting = "heat", this.Type = "mlauncher", e != null ? this.game = e : (this.game = g(p, this.parent.parent), this.game.Towers.push(this), this.bHeld = !1), this.turret.gotoAndStop(1);
  }
  aim() {
    var e = void 0, t = void 0, s = void 0;
    this.eTarget.health > 0 && (e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.atan2(t, e) * 180 / 3.16, this.turret.gotoAndStop(Math.round((s + 180) / 5)));
  }
  Fire() {
    var e = void 0, t = void 0, s = void 0, l = void 0, o = void 0, v = void 0;
    this.eTarget.coreTemp > 60 ? (e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.sqrt(e * e + t * t), l = e / s * 20, o = t / s * 20, v = new me(this.game, this.x + l, this.y + o, 0, 0, this.Damage + this.Damage * this.DLevel, this.eTarget, 4, new f(50, 0, 0, 0), (this.Damage + this.Damage * this.DLevel) * 0.2, this), this.game.level.addChild(v), this.game.Soundbox.playShot("launcher")) : this.eTarget = this.game.getEnemy(this, this.targeting);
  }
}
class _t extends F {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(25), this.RoF = h(7), this.Range = h(90), this.Type = "glauncher", this.expLimit = 1500, e != null ? this.game = e : (this.game = g(p, this.parent.parent), this.game.Towers.push(this), this.bHeld = !1), this.turret.gotoAndStop(1);
  }
  aim() {
    var e = void 0, t = void 0, s = void 0;
    this.eTarget.health > 0 && (e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.atan2(t, e) * 180 / 3.16, this.turret.gotoAndStop(Math.round((s + 180) / 5)));
  }
  Fire() {
    var e = void 0, t = void 0, s = void 0, l = void 0, o = void 0;
    e = this.eTarget.x + this.eTarget.vx * 4 - this.x, t = this.eTarget.y + this.eTarget.vy * 4 - this.y, s = Math.sqrt(e * e + t * t), l = e / s * 20, o = t / s * 20, new r(this.game, this.game.SpritePool.mFlash, 16, this.x + l, this.y + o - 8, 0, 0, 8, 1, 8, 0, null, null, 1, !0), new E(this.x + l, this.y + o - 8, this.eTarget.x, this.eTarget.y - 6, 6, this.game, 2), this.eTarget.bMetal ? (new N(this.eTarget.x, this.eTarget.y - 6, 20, this.game), this.eTarget.takeDamage((this.Damage + this.Damage * this.DLevel) * 0.5, new f(5, 0, 0, 0), this)) : (new r(this.game, this.game.SpritePool.bloodSplat, 16, this.eTarget.x, this.eTarget.y, l * 0.05, o * 0.05, 8, 0.5, 8), this.eTarget.takeDamage(this.Damage + this.Damage * this.DLevel, new f(2, 0, 0, 0), this)), this.game.Soundbox.playShot("mg");
  }
}
class ft extends F {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(40), this.RoF = h(1), this.Range = h(160), this.Type = "glauncher", this.targeting = "heat", this.expLimit = 2500, e != null ? this.game = e : (this.game = g(p, this.parent.parent), this.game.Towers.push(this), this.bHeld = !1), this.turret.gotoAndStop(1);
  }
  aim() {
    var e = void 0, t = void 0, s = void 0;
    this.eTarget.health > 0 && (e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.atan2(t, e) * 180 / 3.16, this.turret.gotoAndStop(Math.round((s + 180) / 5)));
  }
  Fire() {
    var e = void 0, t = void 0, s = void 0, l = void 0, o = void 0;
    e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.sqrt(e * e + t * t), l = e / s * 12, o = t / s * 12, new r(this.game, this.game.SpritePool.pshell, 32, this.x + l, this.y - 8 + o, 0, 0, 9, 0.2, 100, this.Damage, new f(0, 20, 0, 0), this.eTarget, 1.6, !0, this, !0), this.game.Soundbox.playShot("launcher");
  }
}
class Ct extends F {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(10), this.RoF = h(3), this.Range = h(60), this.targeting = "weak", this.expLimit = 500, e != null ? this.game = e : (this.game = g(p, this.parent.parent), this.game.Towers.push(this), this.bHeld = !1), this.turret.gotoAndStop(1);
  }
  aim() {
    var e = void 0, t = void 0, s = void 0;
    this.eTarget.health > 0 && (e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.atan2(t, e) * 180 / 3.16, this.turret.gotoAndStop(Math.round((s + 180) / 5)));
  }
  Fire() {
    var e = void 0, t = void 0, s = void 0, l = void 0, o = void 0;
    e = this.eTarget.x + this.eTarget.vx * 4 - this.x, t = this.eTarget.y + this.eTarget.vy * 4 - this.y, s = Math.sqrt(e * e + t * t), l = e / s * 20, o = t / s * 20, new r(this.game, this.game.SpritePool.mFlash, 16, this.x + l - o * 0.1, this.y + o - 10 + l * 0.1, 0, 0, 8, 1, 8, 0, null, null, 1, !0), new E(this.x + l - o * 0.1, this.y + o - 10 + l * 0.1, this.eTarget.x, this.eTarget.y - 6, 6, this.game, 2), this.eTarget.bMetal ? (new N(this.eTarget.x, this.eTarget.y - 6, 20, this.game), this.eTarget.takeDamage((this.Damage + this.Damage * this.DLevel) * 0.5, new f(5, 0, 0, 0), this)) : (new r(this.game, this.game.SpritePool.bloodSplat, 16, this.eTarget.x, this.eTarget.y, l * 0.05, o * 0.05, 8, 0.5, 8), this.eTarget.takeDamage(this.Damage + this.Damage * this.DLevel, new f(2, 0, 0, 0), this)), this.game.Soundbox.playShot("rifle");
  }
}
class xt extends F {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(80), this.RoF = h(2), this.Range = h(120), this.Type = "Sniper", this.targeting = "near", this.expLimit = 1e3, e != null ? this.game = e : (this.game = g(p, this.parent.parent), this.game.Towers.push(this), this.bHeld = !1), this.turret.gotoAndStop(1);
  }
  aim() {
    var e = void 0, t = void 0, s = void 0;
    this.eTarget.health > 0 && (e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.atan2(t, e) * 180 / 3.16, this.turret.gotoAndStop(Math.round((s + 180) / 5)));
  }
  Fire() {
    var e = void 0, t = void 0, s = void 0, l = void 0, o = void 0;
    e = this.eTarget.x + this.eTarget.vx * 4 - this.x, t = this.eTarget.y + this.eTarget.vy * 4 - this.y, s = Math.sqrt(e * e + t * t), l = e / s * 20, o = t / s * 20, new r(this.game, this.game.SpritePool.mFlash, 16, this.x + l - o * 0.1, this.y + o - 25 + l * 0.1, 0, 0, 8, 1, 8, 0, null, null, 1, !0), new E(this.x + l - o * 0.1, this.y + o - 25 + l * 0.1, this.eTarget.x, this.eTarget.y - 6, 6, this.game, 2), this.eTarget.bMetal ? (new N(this.eTarget.x, this.eTarget.y - 6, 20, this.game), this.eTarget.takeDamage((this.Damage + this.Damage * this.DLevel) * 0.5, new f(5, 0, 0, 0), this)) : (new r(this.game, this.game.SpritePool.bloodSplat, 16, this.eTarget.x, this.eTarget.y, l * 0.05, o * 0.05, 8, 0.5, 8), this.eTarget.takeDamage(this.Damage + this.Damage * this.DLevel, new f(2, 0, 0, 0), this)), this.game.Soundbox.playShot("sniper");
  }
}
class St extends F {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(50), this.RoF = h(3), this.Range = h(60), this.Type = "electro", this.targeting = "near", this.expLimit = 1200, e != null ? this.game = e : (this.game = g(p, this.parent.parent), this.game.Towers.push(this), this.game.Selected = this), this.turret.gotoAndStop(1);
  }
  aim() {
  }
  Fire() {
    this.eTarget.health > 0 && (new je(this.x, this.y - 24, this.eTarget.x, this.eTarget.y - 8, this.Damage + this.Damage * this.DLevel, this.game, this.eTarget), new r(this.game, this.game.SpritePool.bFlash, 16, this.eTarget.x, this.eTarget.y - 8, this.eTarget.vx, this.eTarget.vy, 8, 0.5, 8, 0, null, null, 0, !0), new r(this.game, this.game.SpritePool.bFlash, 16, this.x, this.y - 24, 0, 0, 8, 0.5, 8, 0, null, null, 0, !0), new N(this.eTarget.x, this.eTarget.y - 8, 20, this.game), this.eTarget.bMetal ? this.eTarget.takeDamage(this.Damage + this.Damage * this.DLevel, new f(0, 0, 0, 0), this) : this.eTarget.takeDamage(this.Damage + this.Damage * 0.6 * this.DLevel, new f(0, 0, 0, 0), this)), this.game.Soundbox.playShot("electro");
  }
}
class wt extends F {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(35), this.RoF = h(2), this.Range = h(70), this.Type = "shotgun", this.targeting = "strong", this.expLimit = 800, e != null ? this.game = e : (this.game = g(p, this.parent.parent), this.game.Towers.push(this), this.bHeld = !1), this.turret.gotoAndStop(1);
  }
  aim() {
    var e = void 0, t = void 0, s = void 0;
    this.eTarget.health > 0 && (e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.atan2(t, e) * 180 / 3.16, this.turret.gotoAndStop(Math.round((s + 180) / 5)));
  }
  Fire() {
    var e = void 0, t = void 0, s = void 0, l = void 0, o = void 0;
    e = this.eTarget.x + this.eTarget.vx * 4 - this.x, t = this.eTarget.y + this.eTarget.vy * 4 - this.y, s = Math.sqrt(e * e + t * t), l = e / s * 20, o = t / s * 20, new r(this.game, this.game.SpritePool.mFlash, 16, this.x + l - o * 0.1, this.y + o - 12 + l * 0.1, 0, 0, 8, 1, 8, 0, null, null, 1, !0), new E(this.x + l - o * 0.1, this.y + o - 12 + l * 0.1, this.eTarget.x + (-8 + Math.random() * 16), this.eTarget.y - 6 + (-8 + Math.random() * 16), 5, this.game, 3), new E(this.x + l - o * 0.1, this.y + o - 12 + l * 0.1, this.eTarget.x + (-8 + Math.random() * 16), this.eTarget.y - 6 + (-8 + Math.random() * 16), 5, this.game, 3), new E(this.x + l - o * 0.1, this.y + o - 12 + l * 0.1, this.eTarget.x + (-8 + Math.random() * 16), this.eTarget.y - 6 + (-8 + Math.random() * 16), 5, this.game, 3), this.eTarget.bMetal ? (new N(this.eTarget.x, this.eTarget.y - 6, 20, this.game), this.eTarget.takeDamage((this.Damage + this.Damage * this.DLevel) * 0.5, new f(20, 0, 0, 0))) : (new r(this.game, this.game.SpritePool.bloodSplat, 16, this.eTarget.x, this.eTarget.y, l * 0.05, o * 0.05, 8, 0.5, 8), this.eTarget.takeDamage(this.Damage + this.Damage * this.DLevel, new f(10, 0, 0, 0), this)), this.game.Soundbox.playShot("shotgun");
  }
}
class Tt extends F {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(150), this.RoF = h(1), this.Range = h(100), this.expLimit = 1500, this.targeting = "strong", e != null ? this.game = e : (this.game = g(p, this.parent.parent), this.game.Towers.push(this), this.bHeld = !1), this.turret.gotoAndStop(1);
  }
  aim() {
    var e = void 0, t = void 0, s = void 0;
    this.eTarget.health > 0 && (e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.atan2(t, e) * 180 / 3.16, this.turret.gotoAndStop(Math.round((s + 180) / 5)));
  }
  Fire() {
    var e = void 0, t = void 0, s = void 0, l = void 0, o = void 0, v = void 0, u = void 0, n = void 0, W = void 0, y = void 0, S = void 0, D = void 0;
    for (e = this.eTarget.x + this.eTarget.vx * 4 - this.x, t = this.eTarget.y + this.eTarget.vy * 4 - this.y, s = Math.sqrt(e * e + t * t), l = e / s * 20, o = t / s * 20, v = e / s * 5, u = t / s * 5, n = l + this.x, W = o + this.y - 10, y = Math.round(e / v) - 4, S = 2, D = 0; D < y; )
      S > 0.3 && (S -= 0.1), new r(this.game, this.game.SpritePool.lBlast, 16, n, W, 0, 0, 8, S, 8, 0, null, null, 1, !0), n += v, W += u, D++;
    new E(this.x + l - o * 0.1, this.y + o - 10 + l * 0.1, this.eTarget.x, this.eTarget.y - 6, 6, this.game, 2), new N(this.eTarget.x, this.eTarget.y - 6, 20, this.game), new N(this.eTarget.x, this.eTarget.y - 6, 20, this.game), new N(this.eTarget.x, this.eTarget.y - 6, 20, this.game), this.eTarget.takeDamage(this.Damage + this.Damage * this.DLevel, new f(40, 0, 0, 0), this), this.game.Soundbox.playShot("laser"), this.eTarget = null;
  }
}
class bt extends F {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(40), this.RoF = h(1), this.Range = h(170), this.expLimit = 2500, this.Type = "glauncher", this.targeting = "strong", e != null ? this.game = e : (this.game = g(p, this.parent.parent), this.game.Towers.push(this), this.bHeld = !1), this.turret.gotoAndStop(1);
  }
  aim() {
    var e = void 0, t = void 0, s = void 0;
    this.eTarget.health > 0 && (e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.atan2(t, e) * 180 / 3.16, this.turret.gotoAndStop(Math.round((s + 180) / 5)));
  }
  Fire() {
    var e = void 0, t = void 0, s = void 0, l = void 0, o = void 0;
    e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.sqrt(e * e + t * t), l = e / s * 12, o = t / s * 12, new r(this.game, this.game.SpritePool.shell, 32, this.x + l, this.y - 8 + o, 0, 0, 9, 0.2, 100, this.Damage, new f(0, 0, 30, 0), this.eTarget, 1.6, !1, this, !0, !0), this.game.Soundbox.playShot("launcher");
  }
}
class Mt extends F {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(12), this.RoF = h(3), this.Range = h(85), this.Type = "glauncher", this.targeting = "cold", this.expLimit = 800, e != null ? this.game = e : (this.game = g(p, this.parent.parent), this.game.Towers.push(this), this.bHeld = !1), this.turret.gotoAndStop(1);
  }
  aim() {
    var e = void 0, t = void 0, s = void 0;
    this.eTarget.health > 0 && (e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.atan2(t, e) * 180 / 3.16, this.turret.gotoAndStop(Math.round((s + 180) / 5)));
  }
  Fire() {
    var e = void 0, t = void 0, s = void 0, l = void 0, o = void 0;
    e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.sqrt(e * e + t * t), l = e / s * 20, o = t / s * 20, new r(this.game, this.game.SpritePool.bioShot, 32, this.x + l, this.y - 8 + o, -0.2 + Math.random() * 0.4, -0.5 - Math.random() * 0.5, 9, 0.2, 100, this.Damage, new f(0, 0, this.Damage * 5, 0), this.eTarget, 3, !1, this), this.game.Soundbox.playShot("bio");
  }
}
class kt extends F {
  __fi() {
    this.turret = null;
  }
  constructor(e = null) {
    super(), this.Damage = h(2), this.RoF = h(12), this.Range = h(55), this.Type = "glauncher", this.targeting = "near", this.expLimit = 500, e != null ? this.game = e : (this.game = g(p, this.parent.parent), this.game.Towers.push(this), this.bHeld = !1), this.turret.gotoAndStop(1);
  }
  aim() {
    var e = void 0, t = void 0, s = void 0;
    this.eTarget.health > 0 && (e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.atan2(t, e) * 180 / 3.16, this.turret.gotoAndStop(Math.round((s + 180) / 5)));
  }
  Fire() {
    var e = void 0, t = void 0, s = void 0, l = void 0, o = void 0;
    e = this.eTarget.x - this.x, t = this.eTarget.y - this.y, s = Math.sqrt(e * e + t * t), l = e / s * 20, o = t / s * 20, new r(this.game, this.game.SpritePool.flameShot, 32, this.x + l, this.y - 8 + o, -0.2 + Math.random() * 0.4, -0.5 - Math.random() * 0.5, 10, 0.2, 10, this.Damage, new f(this.Damage * 15, 0, 0, 0), this.eTarget, 1.5, !0, this), this.game.Soundbox.playShot("flame");
  }
}
class L extends x {
  __fi() {
    this.towerType = null, this.idx = 0, this.cost = 0;
  }
  constructor() {
    super(), this.addEventListener(w.ENTER_FRAME, this.tick, !1, 0, !0);
  }
  clicked(e) {
    this.alpha >= 1 && this.game.buildTower(this.towerType, this.cost);
  }
  tick(e) {
    this.game.player.Towers[this.idx] != 1 ? this.alpha = 0 : this.game.player.funds >= this.cost ? this.alpha = 1 : this.alpha = 0.2;
  }
}
class Da extends L {
  __fi() {
    this.costT = null;
  }
  constructor() {
    super(), this.idx = h(1), this.cost = h(800), this.towerType = "tower1", this.costT.text = "$ " + this.cost;
  }
}
class Pa extends L {
  __fi() {
    this.costT = null;
  }
  constructor() {
    super(), this.idx = h(10), this.cost = h(500), this.towerType = "tower10", this.costT.text = "$ " + this.cost;
  }
}
class Ea extends L {
  __fi() {
    this.costT = null;
  }
  constructor() {
    super(), this.idx = h(11), this.cost = h(1e3), this.towerType = "tower11", this.costT.text = "$ " + this.cost;
  }
}
class Ja extends L {
  __fi() {
    this.costT = null;
  }
  constructor() {
    super(), this.idx = h(2), this.cost = h(50), this.towerType = "tower2", this.costT.text = "$ " + this.cost;
  }
}
class Ba extends L {
  __fi() {
    this.costT = null;
  }
  constructor() {
    super(), this.idx = h(3), this.cost = h(400), this.towerType = "tower3", this.costT.text = "$ " + this.cost;
  }
}
class Ra extends L {
  __fi() {
    this.costT = null;
  }
  constructor() {
    super(), this.idx = h(4), this.cost = h(250), this.towerType = "tower4", this.costT.text = "$ " + this.cost;
  }
}
class za extends L {
  __fi() {
    this.costT = null;
  }
  constructor() {
    super(), this.idx = h(5), this.cost = h(150), this.towerType = "tower5", this.costT.text = "$ " + this.cost;
  }
}
class Ia extends L {
  __fi() {
    this.costT = null;
  }
  constructor() {
    super(), this.idx = h(6), this.cost = h(600), this.towerType = "tower6", this.costT.text = "$ " + this.cost;
  }
}
class Ga extends L {
  __fi() {
    this.costT = null;
  }
  constructor() {
    super(), this.idx = h(7), this.cost = h(950), this.towerType = "tower7", this.costT.text = "$ " + this.cost;
  }
}
class Xa extends L {
  __fi() {
    this.costT = null;
  }
  constructor() {
    super(), this.idx = h(8), this.cost = h(300), this.towerType = "tower8", this.costT.text = "$ " + this.cost;
  }
}
class Ha extends L {
  __fi() {
    this.costT = null;
  }
  constructor() {
    super(), this.idx = h(9), this.cost = h(250), this.towerType = "tower9", this.costT.text = "$ " + this.cost;
  }
}
class Ua extends d {
  constructor() {
    super(), this.addFrameScript(0, this.frame1);
  }
  frame1() {
    this.mouseEnabled = !1;
  }
}
class H extends d {
  __fi() {
    this.uType = null, this.game = null;
  }
  constructor() {
    super(), this.game = g(p, this.parent.parent), this.addEventListener(w.ENTER_FRAME, this.tick, !1, 0, !0), this.gotoAndStop(1);
  }
  tick(e) {
    this.game.Selected != null && (this.uType == "damage" ? this.gotoAndStop(this.game.Selected.DLevel * 5 + 1) : this.uType == "rof" ? this.gotoAndStop(this.game.Selected.RoFLevel * 5 + 1) : this.uType == "range" && this.gotoAndStop(this.game.Selected.RLevel * 5 + 1));
  }
}
class ja extends H {
  constructor() {
    super(), this.uType = "damage";
  }
}
class $a extends H {
  constructor() {
    super(), this.uType = "range";
  }
}
class Oa extends H {
  constructor() {
    super(), this.uType = "rof";
  }
}
class Va extends x {
  constructor() {
    super(), this.addEventListener(w.ENTER_FRAME, this.tick, !1, 0, !0);
  }
  clicked(e) {
    this.game.baseSpeed = 1;
  }
  tick(e) {
    this.game.baseSpeed != 1 ? this.gotoAndStop(2) : this.gotoAndStop(1);
  }
}
class Ft {
  __fi() {
    this.bAutoSpawn = !1, this.allOut = !1, this.nextSpawn = h(1e3), this.game = null, this.numWaves = h(0), this.currentWave = h(0), this.waves = new Array(), this.running = !0;
  }
  constructor(e) {
    m(this), c(this), this.game = e;
  }
  newWave() {
    var e = void 0, t = void 0, s = void 0;
    return e = !1, t = this.waves.shift(), t != null && (e = !0, this.currentWave = h(this.currentWave + 1), s = new Nt(this.game, t[0], t[1], t[2], this.currentWave), this.game.spawners.push(s), this.nextSpawn = h(Number(t[3]) * 2)), e;
  }
  resetWC() {
    this.waves = new Array(), this.numWaves = h(0), this.nextSpawn = h(500), this.allOut = !1, this.running = !0, this.currentWave = h(0);
  }
  addWave(e, t, s, l) {
    t = h(t), s = h(s), l = h(l), this.waves.push(new Array(e, t, s, l)), ++this.numWaves;
  }
  Tick(e) {
    this.running ? (this.nextSpawn = h(this.nextSpawn - e), this.nextSpawn <= 0 && (this.nextSpawn = h(1e3), this.newWave() || (this.running = !1, this.allOut = !0)), this.game.levelHealth <= 0 && (this.running = !1, this.game.getFrame(4), this.game.stopLevel(), this.game.baseSpeed = 0)) : this.allOut && (this.game.levelHealth <= 0 ? (this.allOut = !1, this.game.getFrame(4), this.game.stopLevel(), this.game.baseSpeed = 0) : this.game.Enemies.length <= 0 && (this.allOut = !1, this.game.stopLevel(), this.game.getFrame(6), this.game.baseSpeed = 0));
  }
}
class Nt {
  __fi() {
    this.space = 0, this.ticker = h(0), this.game = null, this.count = 0, this.wave = 0, this.eType = null;
  }
  constructor(e, t, s, l, o) {
    s = h(s), l = h(l), m(this), c(this), this.game = e, this.eType = t, this.count = h(s), this.space = h(l), this.wave = h(o);
  }
  destroy() {
    this.game.killSpawner(this), this.game = null;
  }
  Tick(e) {
    this.count > 0 ? (this.ticker = h(this.ticker + e), this.ticker >= this.space && (this.ticker = h(0), --this.count, this.game.createEnemy(this.eType, this.wave))) : this.destroy();
  }
}
function Ka() {
  i(Rt, "FD2__fla.Bio_T_102"), i(zt, "BlockSquare"), i(It, "BuyButton"), i(f, "DamageType"), i(Gt, "Decal"), i(Y, "GameActor"), i(Xt, "EndTile"), i(k, "Enemy"), i(Ht, "FD2__fla.Flame_T_104"), i(Ut, "FrameController"), i(ae, "FrozenSoldier"), i(he, "FrozenVehicle"), i(jt, "FxLayer"), i(p, "Game"), i($t, "GameOptions"), i(Ot, "FD2__fla.HellFire_T_100"), i(Vt, "Hud"), i(oe, "Jeep1"), i(le, "Jeep2"), i(ne, "Jeep3"), i(re, "Jeep4"), i(x, "gameButton"), i(T, "LevelButton"), i(qt, "LevelButton_1"), i(Kt, "LevelButton_10"), i(Yt, "LevelButton_11"), i(Qt, "LevelButton_12"), i(Zt, "LevelButton_13"), i(es, "LevelButton_14"), i(ts, "LevelButton_16"), i(ss, "LevelButton_18"), i(is, "LevelButton_2"), i(as, "LevelButton_20"), i(hs, "LevelButton_22"), i(os, "LevelButton_3"), i(ls, "LevelButton_4"), i(ns, "LevelButton_5"), i(rs, "LevelButton_6"), i(ds, "LevelButton_7"), i(vs, "LevelButton_8"), i(ms, "LevelButton_9"), i(cs, "Level_0001"), i(us, "FD2__fla.Level_0002_231"), i(Ws, "FD2__fla.Level_0003_232"), i(gs, "FD2__fla.Level_0004_234"), i(ps, "FD2__fla.Level_0005_235"), i(ys, "FD2__fla.Level_0006_236"), i(_s, "FD2__fla.Level_0007_237"), i(fs, "FD2__fla.Level_0008_238"), i(Cs, "FD2__fla.Level_0009_239"), i(xs, "FD2__fla.Level_0010_240"), i(Ss, "FD2__fla.Level_0011_241"), i(ws, "FD2__fla.Level_0012_242"), i(Ts, "FD2__fla.Level_0013_243"), i(bs, "FD2__fla.Level_0014_244"), i(Ms, "FD2__fla.Level_Bonus_01_245"), i(ks, "FD2__fla.Level_Bonus_02_248"), i(Fs, "FD2__fla.Level_Bonus_03_250"), i(Ns, "FD2__fla.Level_Bonus_04_251"), i(As, "FD2__fla.MachineG_T_106"), i(de, "FD2__fla.MainTimeline"), i(F, "Tower"), i(ve, "ManualTower"), i(Ls, "Manualtower1"), i(z, "Mart"), i(me, "Missile"), i(Ds, "FD2__fla.Missile_T_88"), i(ce, "NoiseBG"), i(ue, "PathFinder"), i(Ps, "PathTile"), i(Es, "PauseButton"), i(Js, "PausePage"), i(Bs, "PlayButton"), i(We, "Player"), i(Q, "RadialBlast"), i(q, "RadialCloud"), i(I, "ugButton"), i(Rs, "RangeButton"), i(zs, "RangeRing"), i(Is, "Renderer"), i(Gs, "FD2__fla.Rifle_T_90"), i(Xs, "RoFButton"), i(Hs, "FD2__fla.Rockn3_129"), i(Us, "FD2__fla.Rocky34_128"), i(M, "shopItem"), i(js, "ShopItem1"), i($s, "ShopItem10"), i(Os, "ShopItem11"), i(Vs, "ShopItem12"), i(qs, "ShopItem13"), i(Ks, "ShopItem14"), i(Ys, "ShopItem15"), i(Qs, "ShopItem2"), i(Zs, "ShopItem3"), i(ei, "ShopItem4"), i(ti, "ShopItem5"), i(si, "ShopItem6"), i(ii, "ShopItem7"), i(ai, "ShopItem8"), i(hi, "ShopItem9"), i(oi, "FD2__fla.Shotgun_T_96"), i(li, "FD2__fla.Sniper_T_92"), i(K, "SoldOut"), i(N, "Spark"), i(ni, "StartTile"), i(ge, "Tank1"), i(pe, "Tank2"), i(ye, "Tank3"), i(E, "Tracer"), i(ri, "FD2__fla.biker1Anim_61"), i(di, "FD2__fla.bikerAnim2_63"), i(_e, "bioCloud1"), i(fe, "bioFX"), i(Ce, "bio_1"), i(xe, "blood1"), i(Se, "bloodSplatter"), i(we, "blueFlash"), i(Te, "bot_1"), i(vi, "clickLayer"), i(mi, "clickLayerShoot"), i(ci, "clickObject"), i(ui, "confirmationPage"), i(Wi, "continueButton"), i(be, "crater1"), i(gi, "creditsButton"), i(pi, "dTool"), i(yi, "damageButton"), i(_i, "FD2__fla.e1Anims_20"), i(Me, "empEx"), i(ke, "enemy1"), i(Fe, "enemy2"), i(fi, "FD2__fla.enemy2Anims_27"), i(Ne, "enemy3"), i(Ci, "FD2__fla.enemy3Anims_34"), i(Ae, "enemy4"), i(xi, "FD2__fla.enemy4Anims_41"), i(Le, "enemy5"), i(Si, "FD2__fla.enemy5Anims_48"), i(De, "explosion1"), i(Pe, "explosion_1"), i(wi, "explosion_2"), i(Ee, "fExplode"), i(Ti, "ffButton"), i(bi, "FD2__fla.flag1_133"), i(Je, "flame1"), i(Be, "flame_1"), i(Re, "flamer"), i(ze, "freeze1"), i(Ie, "healthView"), i(Ge, "iceGrid"), i(Xe, "iced1"), i(J, "instruction"), i(Mi, "instructionManager"), i(ki, "FD2__fla.jeepanim1_57"), i(Fi, "FD2__fla.jeepanim2_59"), i(He, "laserB"), i(Ni, "FD2__fla.laserG_T_98"), i(Ue, "laser_1"), i(Ai, "levelMenuButton"), i(je, "lightning"), i(Li, "lvlUp"), i(Di, "FD2__fla.lvlUpanim_83"), i(Pi, "mCancelButton"), i(Ei, "mPurchaseButton"), i($e, "marchBGM"), i(Ji, "mech_1"), i(Oe, "mg_1"), i(Ve, "mg_2"), i(qe, "missile_1"), i(Bi, "mmButton"), i(Ri, "mono1"), i(zi, "moveButton"), i(Ke, "mtrail"), i(Ii, "muteButton"), i(Ye, "muzzleFlash"), i(Gi, "okButton"), i(Xi, "FD2__fla.paper1_254"), i(Qe, "pathGrids"), i(Hi, "FD2__fla.plasFire_T_108"), i(Ui, "poison1"), i(ji, "poison2"), i(Ze, "pshell1"), i($i, "quitButton"), i(et, "rad1"), i(tt, "reloader"), i(r, "renderSprite"), i(Oi, "retryButton"), i(st, "rifle_1"), i(it, "rockGrid"), i(Vi, "rundown"), i(at, "sandGrid"), i(ht, "scorch1"), i(qi, "sellButton"), i(ot, "shadow1"), i(lt, "shell1"), i(nt, "shock_1"), i(Ki, "shopButton"), i(Yi, "shot2"), i(rt, "shotgun_1"), i(Qi, "smallHit"), i(dt, "smoke1"), i(vt, "sniper_1"), i(Zi, "sniper_2"), i(ea, "FD2__fla.soldier1_b_51"), i(ta, "FD2__fla.soldier1_dd_52"), i(sa, "FD2__fla.soldier1_du_50"), i(ia, "FD2__fla.soldier1_f_53"), i(aa, "FD2__fla.soldier1_s_49"), i(ha, "FD2__fla.soldier2_S_28"), i(oa, "FD2__fla.soldier2_b_30"), i(la, "FD2__fla.soldier2_dd_31"), i(na, "FD2__fla.soldier2_f_32"), i(ra, "FD2__fla.soldier2_ud_29"), i(da, "FD2__fla.soldier3b_44"), i(va, "FD2__fla.soldier3dd_45"), i(ma, "FD2__fla.soldier3du_43"), i(ca, "FD2__fla.soldier3f_46"), i(ua, "FD2__fla.soldier3s_42"), i(Wa, "FD2__fla.soldier4b_23"), i(ga, "FD2__fla.soldier4dd_24"), i(pa, "FD2__fla.soldier4du_22"), i(ya, "FD2__fla.soldier4f_25"), i(_a, "FD2__fla.soldier4s_21"), i(fa, "FD2__fla.soldier5b_37"), i(Ca, "FD2__fla.soldier5dd_38"), i(xa, "FD2__fla.soldier5du_36"), i(Sa, "FD2__fla.soldier5f_39"), i(wa, "FD2__fla.soldier5s_35"), i(mt, "soundManager"), i(ct, "splat_1"), i(Ta, "splat_2"), i(ut, "spritePool"), i(G, "supportTowerButton"), i(ba, "supTowerButton_1"), i(Ma, "supTowerButton_2"), i(ka, "supTowerButton_3"), i(X, "supporttower"), i(Wt, "supportTower_1"), i(gt, "supportTower_2"), i(pt, "supportTower_3"), i(Fa, "FD2__fla.tank1Anim_65"), i(Na, "FD2__fla.tank2Anim_67"), i(Aa, "FD2__fla.tank3anim_69"), i(La, "testWaveButton"), i(yt, "tower1"), i(_t, "tower10"), i(ft, "tower11"), i(Ct, "tower2"), i(xt, "tower3"), i(St, "tower4"), i(wt, "tower5"), i(Tt, "tower6"), i(bt, "tower7"), i(Mt, "tower8"), i(kt, "tower9"), i(L, "towerButton"), i(Da, "towerButton1"), i(Pa, "towerButton10"), i(Ea, "towerButton11"), i(Ja, "towerButton2"), i(Ba, "towerButton3"), i(Ra, "towerButton4"), i(za, "towerButton5"), i(Ia, "towerButton6"), i(Ga, "towerButton7"), i(Xa, "towerButton8"), i(Ha, "towerButton9"), i(Ua, "FD2__fla.tree2_127"), i(H, "ugBar"), i(ja, "ugBarD"), i($a, "ugBarR"), i(Oa, "ugBarRof"), i(Va, "unPauseButton"), i(Ft, "waveController"), i(Nt, "waveSpawner");
}
const Ya = de;
export {
  Rt as Bio_T_102,
  zt as BlockSquare,
  It as BuyButton,
  f as DamageType,
  Gt as Decal,
  Ya as DocumentClass,
  Xt as EndTile,
  k as Enemy,
  Ht as Flame_T_104,
  Ut as FrameController,
  ae as FrozenSoldier,
  he as FrozenVehicle,
  jt as FxLayer,
  p as Game,
  Y as GameActor,
  $t as GameOptions,
  Ot as HellFire_T_100,
  Vt as Hud,
  oe as Jeep1,
  le as Jeep2,
  ne as Jeep3,
  re as Jeep4,
  T as LevelButton,
  qt as LevelButton_1,
  Kt as LevelButton_10,
  Yt as LevelButton_11,
  Qt as LevelButton_12,
  Zt as LevelButton_13,
  es as LevelButton_14,
  ts as LevelButton_16,
  ss as LevelButton_18,
  is as LevelButton_2,
  as as LevelButton_20,
  hs as LevelButton_22,
  os as LevelButton_3,
  ls as LevelButton_4,
  ns as LevelButton_5,
  rs as LevelButton_6,
  ds as LevelButton_7,
  vs as LevelButton_8,
  ms as LevelButton_9,
  cs as Level_0001,
  us as Level_0002_231,
  Ws as Level_0003_232,
  gs as Level_0004_234,
  ps as Level_0005_235,
  ys as Level_0006_236,
  _s as Level_0007_237,
  fs as Level_0008_238,
  Cs as Level_0009_239,
  xs as Level_0010_240,
  Ss as Level_0011_241,
  ws as Level_0012_242,
  Ts as Level_0013_243,
  bs as Level_0014_244,
  Ms as Level_Bonus_01_245,
  ks as Level_Bonus_02_248,
  Fs as Level_Bonus_03_250,
  Ns as Level_Bonus_04_251,
  As as MachineG_T_106,
  de as MainTimeline,
  ve as ManualTower,
  Ls as Manualtower1,
  z as Mart,
  me as Missile,
  Ds as Missile_T_88,
  ce as NoiseBG,
  ue as PathFinder,
  Ps as PathTile,
  Es as PauseButton,
  Js as PausePage,
  Bs as PlayButton,
  We as Player,
  Q as RadialBlast,
  q as RadialCloud,
  Rs as RangeButton,
  zs as RangeRing,
  Is as Renderer,
  Gs as Rifle_T_90,
  Xs as RoFButton,
  Hs as Rockn3_129,
  Us as Rocky34_128,
  js as ShopItem1,
  $s as ShopItem10,
  Os as ShopItem11,
  Vs as ShopItem12,
  qs as ShopItem13,
  Ks as ShopItem14,
  Ys as ShopItem15,
  Qs as ShopItem2,
  Zs as ShopItem3,
  ei as ShopItem4,
  ti as ShopItem5,
  si as ShopItem6,
  ii as ShopItem7,
  ai as ShopItem8,
  hi as ShopItem9,
  oi as Shotgun_T_96,
  li as Sniper_T_92,
  K as SoldOut,
  N as Spark,
  ni as StartTile,
  ge as Tank1,
  pe as Tank2,
  ye as Tank3,
  F as Tower,
  E as Tracer,
  ri as biker1Anim_61,
  di as bikerAnim2_63,
  _e as bioCloud1,
  fe as bioFX,
  Ce as bio_1,
  xe as blood1,
  Se as bloodSplatter,
  we as blueFlash,
  Te as bot_1,
  vi as clickLayer,
  mi as clickLayerShoot,
  ci as clickObject,
  ui as confirmationPage,
  Wi as continueButton,
  be as crater1,
  gi as creditsButton,
  pi as dTool,
  yi as damageButton,
  _i as e1Anims_20,
  Me as empEx,
  ke as enemy1,
  Fe as enemy2,
  fi as enemy2Anims_27,
  Ne as enemy3,
  Ci as enemy3Anims_34,
  Ae as enemy4,
  xi as enemy4Anims_41,
  Le as enemy5,
  Si as enemy5Anims_48,
  De as explosion1,
  Pe as explosion_1,
  wi as explosion_2,
  Ee as fExplode,
  Ti as ffButton,
  bi as flag1_133,
  Je as flame1,
  Be as flame_1,
  Re as flamer,
  ze as freeze1,
  x as gameButton,
  Ie as healthView,
  Ge as iceGrid,
  Xe as iced1,
  J as instruction,
  Mi as instructionManager,
  ki as jeepanim1_57,
  Fi as jeepanim2_59,
  He as laserB,
  Ni as laserG_T_98,
  Ue as laser_1,
  Ai as levelMenuButton,
  je as lightning,
  Li as lvlUp,
  Di as lvlUpanim_83,
  Pi as mCancelButton,
  Ei as mPurchaseButton,
  $e as marchBGM,
  Ji as mech_1,
  Oe as mg_1,
  Ve as mg_2,
  qe as missile_1,
  Bi as mmButton,
  Ri as mono1,
  zi as moveButton,
  Ke as mtrail,
  Ii as muteButton,
  Ye as muzzleFlash,
  Gi as okButton,
  Xi as paper1_254,
  Qe as pathGrids,
  Hi as plasFire_T_108,
  Ui as poison1,
  ji as poison2,
  Ze as pshell1,
  $i as quitButton,
  et as rad1,
  Ka as registerClasses,
  tt as reloader,
  r as renderSprite,
  Oi as retryButton,
  st as rifle_1,
  it as rockGrid,
  Vi as rundown,
  at as sandGrid,
  ht as scorch1,
  qi as sellButton,
  ot as shadow1,
  lt as shell1,
  nt as shock_1,
  Ki as shopButton,
  M as shopItem,
  Yi as shot2,
  rt as shotgun_1,
  Qi as smallHit,
  dt as smoke1,
  vt as sniper_1,
  Zi as sniper_2,
  ea as soldier1_b_51,
  ta as soldier1_dd_52,
  sa as soldier1_du_50,
  ia as soldier1_f_53,
  aa as soldier1_s_49,
  ha as soldier2_S_28,
  oa as soldier2_b_30,
  la as soldier2_dd_31,
  na as soldier2_f_32,
  ra as soldier2_ud_29,
  da as soldier3b_44,
  va as soldier3dd_45,
  ma as soldier3du_43,
  ca as soldier3f_46,
  ua as soldier3s_42,
  Wa as soldier4b_23,
  ga as soldier4dd_24,
  pa as soldier4du_22,
  ya as soldier4f_25,
  _a as soldier4s_21,
  fa as soldier5b_37,
  Ca as soldier5dd_38,
  xa as soldier5du_36,
  Sa as soldier5f_39,
  wa as soldier5s_35,
  mt as soundManager,
  ct as splat_1,
  Ta as splat_2,
  ut as spritePool,
  ba as supTowerButton_1,
  Ma as supTowerButton_2,
  ka as supTowerButton_3,
  G as supportTowerButton,
  Wt as supportTower_1,
  gt as supportTower_2,
  pt as supportTower_3,
  X as supporttower,
  Fa as tank1Anim_65,
  Na as tank2Anim_67,
  Aa as tank3anim_69,
  La as testWaveButton,
  yt as tower1,
  _t as tower10,
  ft as tower11,
  Ct as tower2,
  xt as tower3,
  St as tower4,
  wt as tower5,
  Tt as tower6,
  bt as tower7,
  Mt as tower8,
  kt as tower9,
  L as towerButton,
  Da as towerButton1,
  Pa as towerButton10,
  Ea as towerButton11,
  Ja as towerButton2,
  Ba as towerButton3,
  Ra as towerButton4,
  za as towerButton5,
  Ia as towerButton6,
  Ga as towerButton7,
  Xa as towerButton8,
  Ha as towerButton9,
  Ua as tree2_127,
  H as ugBar,
  ja as ugBarD,
  $a as ugBarR,
  Oa as ugBarRof,
  I as ugButton,
  Va as unPauseButton,
  Ft as waveController,
  Nt as waveSpawner
};
