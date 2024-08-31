The `hx-trigger` attribute allows you to specify what triggers an AJAX request. A trigger value can be one of the following:

- An event name (e.g. "click" or "my-custom-event") followed by an event filter and a set of event modifiers
- A polling definition of the form `every <timing declaration>`
- A comma-separated list of such events

**Standard Events**

A standard event, such as click can be specified as the trigger like so:

```html
<div hx-get="/clicked" hx-trigger="click">Click Me</div>
```

**Standard Event Filters**

Events can be filtered by enclosing a boolean javascript expression in square brackets after the event name.

```html
<div hx-get="/clicked" hx-trigger="click[ctrlKey]">Control Click Me</div>
```

**Standard Event Modifiers**

Standard events can also have modifiers that change how they behave. Some modifiers include:

- `once` - the event will only trigger once
- `changed` - the event will only change if the value of the element has changed
- `delay:<timing declaration>` - a delay will occur before an event triggers a request
- `throttle:<timing declaration>` - a throttle will occur after an event triggers a request

**Non-standard Events**

There are some additional non-standard events that htmx supports:

- `load` - triggered on load
- `revealed` - triggered when an element is scrolled into the viewport
- `intersect` - fires once when an element first intersects the viewport

**Polling**

By using the syntax `every <timing declaration>` you can have an element poll periodically:

```html
<div hx-get="/latest_updates" hx-trigger="every 1s">
  Nothing Yet!
</div>
```

**Multiple Triggers**

Multiple triggers can be provided, separated by commas:

```html
<div hx-get="/news" hx-trigger="load, click delay:1s"></div>
```

**Notes**

- `hx-trigger` is not inherited
- `hx-trigger` can be used without an AJAX request, in which case it will only fire the `htmx:trigger` event
