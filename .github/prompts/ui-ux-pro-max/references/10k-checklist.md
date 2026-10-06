# The $10K Checklist

**Metics Media · Field Guide No. 01 · Claude Code Era · MMXXVI**

Eight things that separate a $10K website from a $200 one. Use this as the quality bar for any frontend deliverable.

---

## 01 · Point of View, Not a Template

The site commits to a specific design direction — brutalist, editorial, dark-luxury, retro-modern, whatever — and executes it without flinching. A $200 site is generic. A $10K site has taste.

**Check:**
- [ ] Is there a clear, named aesthetic direction?
- [ ] Does every visual choice reinforce that direction?
- [ ] Would changing one element break the cohesion?

---

## 02 · Typography That Does Work

A paired display + body face, neither of them Inter or Roboto. Scale and weight carry hierarchy. The headlines feel chosen, not defaulted.

**Check:**
- [ ] Display font is distinctive and memorable
- [ ] Body font is readable and pairs well with display
- [ ] No Inter, Roboto, Arial, or system-font defaults
- [ ] Scale and weight alone communicate hierarchy

---

## 03 · A Restrained Color System

Three to five colors, used consistently. No rainbow palettes. Premium signals through restraint, not decoration.

**Check:**
- [ ] 3–5 colors total, defined as CSS variables
- [ ] One dominant color with sharp accents
- [ ] No rainbow/gradient-overuse
- [ ] Consistent application across all components

---

## 04 · Hierarchy That Breathes

Whitespace, scale, and contrast tell the viewer where to look without effort. The page has a clear primary, secondary, tertiary — no flat walls of content.

**Check:**
- [ ] Primary action/headline is immediately obvious
- [ ] Secondary and tertiary elements recede appropriately
- [ ] Generous whitespace, not cramped
- [ ] No wall of equal-weight content blocks

---

## 05 · Imagery With Intent

Not Unsplash defaults everyone's seen. Either custom photography, generated assets that match the art direction, or curation tight enough that the images feel commissioned.

**Check:**
- [ ] Images match the art direction
- [ ] No stock-photo cliches (handshake, generic office, etc.)
- [ ] Consistent aspect ratios and treatments
- [ ] Empty states and placeholders are designed, not default

---

## 06 · Motion That Whispers

Micro-interactions and scroll behavior feel hand-crafted, not AOS-fade-up slop. The bar: a designer would nod, not roll their eyes.

**Check:**
- [ ] Animations have purpose, not decoration
- [ ] Staggered reveals on load (animation-delay)
- [ ] Hover/active states are intentional
- [ ] No generic fade-up-everywhere

---

## 07 · Mobile That's Designed, Not Shrunk

Layout decisions for phone are different from desktop, not the desktop version compressed. This is where 90% of cheap sites collapse.

**Check:**
- [ ] Mobile layout is rethought, not just scaled
- [ ] Touch targets ≥ 44×44px
- [ ] No horizontal scroll at 375px
- [ ] Navigation adapts to thumb zone

---

## 08 · The Invisible Expensive Stuff

Sub-2s load, WCAG AA contrast, keyboard navigation, semantic HTML, real meta tags. Viewers don't see it directly but they feel "this site is fast and works" — the felt difference between expensive and cheap.

**Check:**
- [ ] Load time under 2 seconds
- [ ] WCAG AA contrast ratios (4.5:1 minimum)
- [ ] Full keyboard navigation
- [ ] Semantic HTML (`<nav>`, `<main>`, `<article>`, etc.)
- [ ] Real `<title>`, `<meta description>`, Open Graph tags
- [ ] No layout shift (CLS < 0.1)
