using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
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
    [Name("htmx attribute completion source")]
    [ContentType("html")]
    [ContentType("htmlx")]
    [ContentType("html-delegation")] // VS 2022
    [ContentType("razor")]
    [ContentType("LegacyRazorCSharp")] // VS 2022
    [Order(After = "htmx completion source")]
    internal class HtmxAttributeCompletionSourceProvider : IAsyncCompletionSourceProvider
    {
        private readonly Lazy<HtmxAttributesCompletionSource> Source = new Lazy<HtmxAttributesCompletionSource>(() => new HtmxAttributesCompletionSource());

        /// <summary>  
        /// Gets or creates an instance of <see cref="HtmxAttributesCompletionSource"/>.  
        /// </summary>  
        /// <param name="textView">The text view for which the source is requested.</param>  
        /// <returns>An instance of <see cref="IAsyncCompletionSource"/>.</returns>  
        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            Output.WriteInfo("HtmxAttributeCompletionSourceProvider:GetOrCreate: got a request for a source.");
            return Source.Value;
        }
    }

    /// <summary>  
    /// Provides completion items for htmx attributes.  
    /// </summary>  
    internal class HtmxAttributesCompletionSource : IAsyncCompletionSource
    {
        private static ImageElement CompletionItemIcon = new ImageElement(KnownMonikers.Enumeration.ToImageId(), "htmx-attribute");

        /// <summary>  
        /// Gets the completion context asynchronously.  
        /// </summary>  
        /// <param name="session">The current completion session.</param>  
        /// <param name="trigger">The completion trigger.</param>  
        /// <param name="triggerLocation">The location in the buffer where the trigger occurred.</param>  
        /// <param name="applicableToSpan">The span of text to which the completion is applicable.</param>  
        /// <param name="token">A cancellation token.</param>  
        /// <returns>A task that represents the asynchronous operation. The task result contains the completion context.</returns>  
        public Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            Console.WriteLine("HtmxAttributesCompletionSource:GetCompletionContextAsync");

            if (!triggerLocation.IsInsideHxAttribute(out var attribute))
            {
                return Task.FromResult<CompletionContext>(null);
            }

            var arr = AttributeToolTipsProvider.Instance.GetKeywords(attribute)?.Select(ConvertToItem).ToImmutableArray();
            if (arr == null)
            {
                return Task.FromResult<CompletionContext>(null);
            }

            session.Properties.AddProperty("HxAttribute", attribute);

            return Task.FromResult(new CompletionContext(arr.Value));
        }

        /// <summary>  
        /// Gets the description of a completion item asynchronously.  
        /// </summary>  
        /// <param name="session">The current completion session.</param>  
        /// <param name="item">The completion item.</param>  
        /// <param name="token">A cancellation token.</param>  
        /// <returns>A task that represents the asynchronous operation. The task result contains the description of the completion item.</returns>  
        public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
        {
            var attribute = session.Properties.GetProperty<string>("HxAttribute");
            if (AttributeToolTipsProvider.Instance.TryGetValue(attribute, item.DisplayText, out var element))
            {
                return Task.FromResult<object>(element);
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>  
        /// Initializes the completion process.  
        /// </summary>  
        /// <param name="trigger">The completion trigger.</param>  
        /// <param name="triggerLocation">The location in the buffer where the trigger occurred.</param>  
        /// <param name="token">A cancellation token.</param>  
        /// <returns>A <see cref="CompletionStartData"/> indicating whether the source participates in completion.</returns>  
        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            if (!triggerLocation.IsInsideHtmlTag())
            {
                return CompletionStartData.DoesNotParticipateInCompletion;
            }

            if (triggerLocation.IsInsideHxAttribute(out var attribute))
            {
                Output.WriteInfo($"HtmxAttributesCompletionSource:InitializeCompletion: {attribute} attribute completion triggered.");

                return new CompletionStartData(CompletionParticipation.ProvidesItems,
                    new SnapshotSpan(triggerLocation.Snapshot, triggerLocation.Position, 0));
            }

            return CompletionStartData.DoesNotParticipateInCompletion;
        }

        /// <summary>  
        /// Converts a keyword to a completion item.  
        /// </summary>  
        /// <param name="text">The keyword text.</param>  
        /// <returns>A <see cref="CompletionItem"/> representing the keyword.</returns>  
        private CompletionItem ConvertToItem(string text)
        {
            return new CompletionItem(
                            text,
                            insertText: text,
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
