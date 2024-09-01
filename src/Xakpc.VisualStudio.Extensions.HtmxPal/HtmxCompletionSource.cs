using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xakpc.VisualStudio.Extensions.HtmxPal.Services;

namespace Xakpc.VisualStudio.Extensions.HtmxPal
{
    /// <summary>
    /// Provides completion items for htmx attributes.
    /// </summary>
    [Export(typeof(IAsyncCompletionSourceProvider))]
    [Name("htmx completion source")]
    [ContentType("html")]
    [ContentType("htmlx")]
    [ContentType("html-delegation")] // VS 2022
    [ContentType("razor")]
    [ContentType("LegacyRazorCSharp")] // VS 2022
    internal class HtmxCompletionSourceProvider : IAsyncCompletionSourceProvider
    {
        private readonly Lazy<HtmxCompletionSource> Source = new Lazy<HtmxCompletionSource>(() => new HtmxCompletionSource());

        /// <summary>
        /// Gets or creates an instance of <see cref="HtmxCompletionSource"/>.
        /// </summary>
        /// <param name="textView">The text view for which the completion source is requested.</param>
        /// <returns>An instance of <see cref="HtmxCompletionSource"/>.</returns>
        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            Output.WriteInfo("HtmxCompletionSourceProvider:GetOrCreate: got a request for a source.");
            return Source.Value;
        }
    }

    /// <summary>
    /// Provides completion items and descriptions for htmx attributes.
    /// </summary>
    internal class HtmxCompletionSource : IAsyncCompletionSource
    {
        // Icon
        private static ImageElement CompletionItemIcon = new ImageElement(KnownMonikers.HTMLEndTag.ToImageId(), "htmx");

        /// <summary>
        /// Asynchronously gets the completion context.
        /// </summary>
        /// <param name="session">The completion session.</param>
        /// <param name="trigger">The completion trigger.</param>
        /// <param name="triggerLocation">The location where the trigger occurred.</param>
        /// <param name="applicableToSpan">The span to which the completion is applicable.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the completion context.</returns>
        public Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken cancellationToken)
        {
            var arr = ToolTipsProvider.Instance.Keywords.Select(ConvertToItem).ToImmutableArray();

            Output.WriteInfo($"HtmxCompletionSource:GetCompletionContextAsync: got a request for a context. Returning {arr.Length} items.");

            return Task.FromResult(new CompletionContext(arr));
        }

        /// <summary>
        /// Asynchronously gets the description for a completion item.
        /// </summary>
        /// <param name="session">The completion session.</param>
        /// <param name="item">The completion item.</param>
        /// <param name="token">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the description of the completion item.</returns>
        public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
        {
            if (ToolTipsProvider.Instance.TryGetValue(item.DisplayText, out var element))
            {
                // add some spacing between text paragraphs (should really cache that as well)
                var elements = element.Elements.ToList();
                var newElements = new object[elements.Count * 2 - 1];
                for (int i = 0; i < elements.Count; i++)
                {
                    newElements[i * 2] = elements[i];
                    if (i < elements.Count - 1)
                    {
                        newElements[i * 2 + 1] = new ClassifiedTextElement(new ClassifiedTextRun(PredefinedClassificationTypeNames.WhiteSpace, " "));
                    }
                }
                element = new ContainerElement(element.Style | ContainerElementStyle.Wrapped, newElements);
                return Task.FromResult((object)element);
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Initializes the completion process.
        /// </summary>
        /// <param name="trigger">The completion trigger.</param>
        /// <param name="triggerLocation">The location where the trigger occurred.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The data that indicates whether the completion should participate.</returns>
        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken cancellationToken)
        {
            Output.WriteInfo($"HtmxCompletionSource:InitializeCompletion: triggered by {trigger.Reason}:{trigger.Character}.");
            if (!triggerLocation.IsInsideHtmlTag())
            {
                return CompletionStartData.DoesNotParticipateInCompletion;
            }

            var tokenSpan = triggerLocation.GetContainingToken();
            var currentToken = tokenSpan.GetText();
            Output.WriteInfo($"HtmxCompletionSource:InitializeCompletion: {currentToken}");

            if (currentToken.StartsWith("hx", StringComparison.OrdinalIgnoreCase) ||
                currentToken.StartsWith("hx-", StringComparison.OrdinalIgnoreCase) ||
                (trigger.Character == '-' && tokenSpan.GetPreviousToken().GetText().Equals("hx", StringComparison.OrdinalIgnoreCase)))
            {
                return new CompletionStartData(CompletionParticipation.ProvidesItems, tokenSpan);
            }

            return CompletionStartData.DoesNotParticipateInCompletion;
        }

        /// <summary>
        /// Converts a keyword to a completion item.
        /// </summary>
        /// <param name="text">The keyword text.</param>
        /// <returns>A completion item for the keyword.</returns>
        private CompletionItem ConvertToItem(string text)
        {
            string insertText = text == "hx-on" ? $"hx-on:" : $"{text}=\"\"";
            return new CompletionItem(
                            text,
                            insertText: insertText,
                            sortText: text,
                            filterText: text,
                            automationText: text,
                            source: this,
                            filters: ImmutableArray<CompletionFilter>.Empty,
                            icon: CompletionItemIcon,
                            suffix: default,
                            attributeIcons: ImmutableArray<ImageElement>.Empty);
        }
    }
}
