The `hx-sync` attribute allows you to synchronize AJAX requests between multiple elements.

The `hx-sync` attribute consists of a CSS selector to indicate the element to synchronize on, followed optionally by a colon and then by an optional syncing strategy. The available strategies are:

- `drop` - drop (ignore) this request if an existing request is in flight (the default)
- `abort` - drop (ignore) this request if an existing request is in flight, and, if that is not the case, abort this request if another request occurs while it is still in flight
- `replace` - abort the current request, if any, and replace it with this request
- `queue` - place this request in the request queue associated with the given element

The queue modifier can take an additional argument indicating exactly how to queue:

- `queue first` - queue the first request to show up while a request is in flight
- `queue last` - queue the last request to show up while a request is in flight
- `queue all` - queue all requests that show up while a request is in flight

**Notes**

- `hx-sync` is inherited and can be placed on a parent element

This example resolves a race condition between a form's submit request and an individual input's validation request:

```html
<form hx-post="/store">
    <input id="title" name="title" type="text"
        hx-post="/validate"
        hx-trigger="change"
        hx-sync="closest form:abort">
    <button type="submit">Submit</button>
</form>
```

If you'd rather prioritize the validation request over the submit request, you can use the drop strategy:

```html
<form hx-post="/store">
    <input id="title" name="title" type="text"
        hx-post="/validate"
        hx-trigger="change"
        hx-sync="closest form:drop"
    >
    <button type="submit">Submit</button>
</form>
```

When dealing with forms that contain many inputs, you can prioritize the submit request over all input validation requests:

```html
<form hx-post="/store" hx-sync="this:replace">
    <input id="title" name="title" type="text" hx-post="/validate" hx-trigger="change" />
    <button type="submit">Submit</button>
</form>
```

When implementing active search functionality:

```html
<input type="search"
    hx-get="/search"
    hx-trigger="keyup changed delay:500ms, search"
    hx-target="#search-results"
    hx-sync="this:replace">
```
