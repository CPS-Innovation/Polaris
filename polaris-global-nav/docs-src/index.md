---
layout: page.11ty.cjs
title: <global-nav> ⌲ Home
---

# &lt;global-nav>

`<global-nav>` is an awesome element. It's a great introduction to building web components with LitElement, with nice documentation site as well.

## As easy as HTML

<section class="columns">
  <div>

`<global-nav>` is just an HTML element. You can it anywhere you can use HTML!

```html
<global-nav></global-nav>
```

  </div>
  <div>

<global-nav></global-nav>

  </div>
</section>

## Configure with attributes

<section class="columns">
  <div>

`<global-nav>` can be configured with attributed in plain HTML.

```html
<global-nav name="HTML"></global-nav>
```

  </div>
  <div>

<global-nav name="HTML"></global-nav>

  </div>
</section>

## Declarative rendering

<section class="columns">
  <div>

`<global-nav>` can be used with declarative rendering libraries like Angular, React, Vue, and lit-html

```js
import {html, render} from 'lit-html';

const name = 'lit-html';

render(
  html`
    <h2>This is a &lt;global-nav&gt;</h2>
    <global-nav .name=${name}></global-nav>
  `,
  document.body
);
```

  </div>
  <div>

<h2>This is a &lt;global-nav&gt;</h2>
<global-nav name="lit-html"></global-nav>

  </div>
</section>
