using Microsoft.VisualStudio.Text.Adornments;

namespace Xakpc.VisualStudio.Extensions.HtmxPal.Services
{
    /// <summary>
    /// Helper class for creating ContainerElement instances.
    /// </summary>
    internal static class ContainerElementHelper
    {
        /// <summary>
        /// Creates a rich content container element with a description and a hyperlink.
        /// </summary>
        /// <param name="description">The description of the rich content.</param>
        /// <param name="link">The URL of the hyperlink.</param>
        /// <returns>The created ContainerElement.</returns>
        public static ContainerElement CreateRichContent(string description, string link)
        {
            var converter = new MarkdownConverter();

            var elements = converter.Convert(description);
            elements.Add(new ContainerElement(ContainerElementStyle.Wrapped,
                CreateHyperlink(link)));
            return new ContainerElement(ContainerElementStyle.Stacked | ContainerElementStyle.VerticalPadding,
                elements.ToArray());
        }

        /// <summary>
        /// Creates a rich content container element with a description.
        /// </summary>
        /// <param name="description">The description of the rich content.</param>
        /// <returns>The created ContainerElement.</returns>
        public static ContainerElement CreateRichContent(string description)
        {
            var converter = new MarkdownConverter();

            var elements = converter.Convert(description);
            return new ContainerElement(ContainerElementStyle.Stacked | ContainerElementStyle.VerticalPadding,
                elements.ToArray());
        }

        private static ClassifiedTextElement CreateHyperlink(string url)
        {
            return ClassifiedTextElement.CreateHyperlink("HTMX Reference", "Click here to see more details in official documentation", () =>
            {
                System.Diagnostics.Process.Start(url);
            });
        }
    }
}
