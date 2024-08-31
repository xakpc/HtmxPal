The `hx-swap` attribute allows you to specify how the response will be swapped in relative to the target of an AJAX request. If you do not specify the option, the default is `htmx.config.defaultSwapStyle` (innerHtml).

The possible values of this attribute are:

- `innerHTML` - Replace the inner html of the target element
- `outerHTML` - Replace the entire target element with the response
- `textContent` - Replace the text content of the target element, without parsing the response as HTML
- `beforebegin` - Insert the response before the target element
- `afterbegin` - Insert the response before the first child of the target element
- `beforeend` - Insert the response after the last child of the target element
- `afterend` - Insert the response after the target element
- `delete` - Deletes the target element regardless of the response
- `none` - Does not append content from response (out of band items will still be processed).

These options are based on standard DOM naming and the `Element.insertAdjacentHTML` specification.

So in this code:

```html
<div hx-get="/example" hx-swap="afterend">Get Some HTML & Append It</div>;
```

The `div` will issue a request to `/example` and append the returned content after the `div`