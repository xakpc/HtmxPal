using Microsoft.VisualStudio.Text.Adornments;
using System;

namespace Xakpc.VisualStudio.Extensions.HtmxPal.Tests
{
    [TestClass]
    public class MarkdownConverterTests
    {
        private MarkdownConverter _converter;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new MarkdownConverter();
        }

        [TestMethod]
        public void SimpleParagraph_Convert_ProperlyConverted()
        {
            var result = _converter.Convert("This is a simple paragraph.");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(ContainerElementStyle.Stacked | ContainerElementStyle.Wrapped, result[0].Style);
            Assert.AreEqual(1, result[0].Elements.Count());
            Assert.AreEqual(1, (result[0].Elements.First() as ClassifiedTextElement).Runs.Count());
            Assert.AreEqual("text", (result[0].Elements.First() as ClassifiedTextElement).Runs.ElementAt(0).ClassificationTypeName);
            Assert.AreEqual("This is a simple paragraph.", (result[0].Elements.First() as ClassifiedTextElement).Runs.ElementAt(0).Text);
        }

        [TestMethod]
        public void BoldText_Convert_ProperlyConverted()
        {
            List<ContainerElement> result = _converter.Convert("This has **bold** text.");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, (result[0].Elements.First() as ClassifiedTextElement).Runs.Count());
            var runs = (result[0].Elements.First() as ClassifiedTextElement).Runs.ToList();
            
            Assert.AreEqual("text", runs[0].ClassificationTypeName);

            Assert.AreEqual("This has ", runs[0].Text);
            Assert.AreEqual(ClassifiedTextRunStyle.Bold, runs[1].Style);
            Assert.AreEqual("bold", runs[1].Text);

            Assert.AreEqual("text", runs[2].ClassificationTypeName);
            Assert.AreEqual(" text.", runs[2].Text);
        }

        [TestMethod]
        public void InlineCode_Convert_ProperlyConverted()
        {
            List<ContainerElement> result = _converter.Convert("This has `inline code` in it.");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(3, (result[0].Elements.First() as ClassifiedTextElement).Runs.Count());
            var runs = (result[0].Elements.First() as ClassifiedTextElement).Runs.ToList();
            Assert.AreEqual("text", runs[0].ClassificationTypeName);
            Assert.AreEqual("This has ", runs[0].Text);
            Assert.AreEqual("keyword", runs[1].ClassificationTypeName);
            Assert.AreEqual("inline code", runs[1].Text);
            Assert.AreEqual("text", runs[2].ClassificationTypeName);
            Assert.AreEqual(" in it.", runs[2].Text);
        }

        [TestMethod]
        public void CodeBlock_Convert_ProperlyConverted()
        {
            List<ContainerElement> result = _converter.Convert("Here's a code block:\n```\nvar x = 5;\nconsole.log(x);\n```");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result[0].Elements.Count());
            var elements = result[0].Elements.ToList();
            Assert.AreEqual("text",(elements[0] as ClassifiedTextElement).Runs.First().ClassificationTypeName);
            Assert.AreEqual("Here's a code block:", (elements[0] as ClassifiedTextElement).Runs.First().Text);
            Assert.AreEqual("markup node", (elements[1] as ClassifiedTextElement).Runs.First().ClassificationTypeName);
            Assert.AreEqual("var x = 5;\r\nconsole.log(x);", (elements[1] as ClassifiedTextElement).Runs.First().Text);
        }

        [TestMethod]
        public void CodeBlockWithType_Convert_ProperlyConverted()
        {
            List<ContainerElement> result = _converter.Convert("""
                Here's a code block:
                ```html
                <button hx-post="/example" hx-ext="debug, json-enc">
                  This Button Uses Two Extensions
                </button>
                ```
                """);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result[0].Elements.Count());
            var elements = result[0].Elements.ToList();
            Assert.AreEqual("text", (elements[0] as ClassifiedTextElement).Runs.First().ClassificationTypeName);
            Assert.AreEqual("Here's a code block:", (elements[0] as ClassifiedTextElement).Runs.First().Text);
            Assert.AreEqual("markup node", (elements[1] as ClassifiedTextElement).Runs.First().ClassificationTypeName);
            Assert.AreEqual("<button hx-post=\"/example\" hx-ext=\"debug, json-enc\">\r\n  This Button Uses Two Extensions\r\n</button>", (elements[1] as ClassifiedTextElement).Runs.First().Text);
        }

        [TestMethod]
        public void MultipleParagraphs_Convert_ProperlyConverted()
        {
            List<ContainerElement> result = _converter.Convert("First paragraph.\n\nSecond paragraph.");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("First paragraph.", (result[0].Elements.First() as ClassifiedTextElement).Runs.First().Text);
            Assert.AreEqual("Second paragraph.", (result[1].Elements.First() as ClassifiedTextElement).Runs.First().Text);
        }

        [TestMethod]
        public void MixedFormatting_Convert_ProperlyConverted()
        {
            List<ContainerElement> result = _converter.Convert("This paragraph has **bold** and `code` in it.\n\nAnother paragraph with `more code`.");
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(5, (result[0].Elements.First() as ClassifiedTextElement).Runs.Count());
            Assert.AreEqual(3, (result[1].Elements.First() as ClassifiedTextElement).Runs.Count());
        }

        [TestMethod]
        public void EmptyInput_Convert_EmptyResult()
        {
            List<ContainerElement> result = _converter.Convert("");
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void OnlyWhitespace_Convert_EmptyResult()
        {
            List<ContainerElement> result = _converter.Convert("  \n  \n  ");
            Assert.AreEqual(0, result.Count);
        }


        [TestMethod]
        public void RealDescription_Convert_ProperResult()
        {
            const string text = """
            The `hx-target` attribute allows you to target a different element for swapping than the one issuing the AJAX request. The value of this attribute can be:

            • A CSS query selector of the element to target.
            • `this` which indicates that the element that the `hx-target` attribute is on is the target.
            • `closest <CSS selector>` which will find the closest ancestor element or itself, that matches the given CSS selector (e.g. `closest tr` will target the closest table row to the element).
            • `find <CSS selector>` which will find the first child descendant element that matches the given CSS selector.
            • `next` which resolves to `element.nextElementSibling`
            • `next <CSS selector>` which will scan the DOM forward for the first element that matches the given CSS selector. (e.g. `next .error` will target the closest following sibling element with `error` class)
            • `previous` which resolves to `element.previousElementSibling`
            • `previous <CSS selector>` which will scan the DOM backwards for the first element that matches the given CSS selector. (e.g `previous .error` will target the closest previous sibling with `error` class)

            Here is an example that targets a div:
            ```
            <div>
              <div id="response-div"></div>
              <button hx-post="/register" hx-target="#response-div" hx-swap="beforeend">
                Register!
              </button>
            </div>
            ```

            The response from the `/register` url will be appended to the `div` with the id `response-div`.

            This example uses `hx-target="this"` to make a link that updates itself when clicked:

            ```
            <a hx-post="/new-link" hx-target="this" hx-swap="outerHTML">New link</a>
            ```
            """;

            List<ContainerElement> result = _converter.Convert(text);
            Assert.AreEqual(6, result.Count);
        }
    }
}