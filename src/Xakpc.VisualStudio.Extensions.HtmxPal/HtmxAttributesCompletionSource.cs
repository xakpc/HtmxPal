using Microsoft.VisualStudio.Experimentation;
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
    [Name("Htmx attibute completion item source")]
    [ContentType("html")]
    [ContentType("htmlx")] // .html
    [ContentType("Razor")] // .cshtml
    [Order(After = "Htmx completion item source")]
    internal class HtmxAttributeCompletionSourceProvider : IAsyncCompletionSourceProvider
    {
        private readonly Lazy<HtmxAttributesCompletionSource> Source;

        [Import]
        IAsyncCompletionBroker AsyncCompletionBroker { get; set; }

        public HtmxAttributeCompletionSourceProvider()
        {
            Source = new Lazy<HtmxAttributesCompletionSource>(() => new HtmxAttributesCompletionSource(AsyncCompletionBroker));
        }

        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            Output.WriteInfo("HtmxAttributeCompletionSourceProvider:GetOrCreate: got a request for a source.");
            return Source.Value;
        }
    }

    internal class HtmxAttributesCompletionSource : IAsyncCompletionSource
    {
        private IAsyncCompletionBroker _asyncCompletionBroker;

        public HtmxAttributesCompletionSource(IAsyncCompletionBroker asyncCompletionBroker)
        {
            _asyncCompletionBroker = asyncCompletionBroker;
        }

        public async Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            Console.WriteLine("HtmxAttributesCompletionSource:GetCompletionContextAsync");

            if (!triggerLocation.IsInsideHxAttribute(out var attribute))
            {
                return null;
            }
            
            var arr = AttributeToolTipsProvider.Instance.GetKeywords(attribute)?.Select(ConvertToItem).ToImmutableArray();
            if (arr == null)
            {
                return null;
            }            

            session.Properties.AddProperty("HxAttribute", attribute);

            return new CompletionContext(arr.Value);
        }

        public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
        {
            var attribute = session.Properties.GetProperty<string>("HxAttribute");
            if (AttributeToolTipsProvider.Instance.TryGetValue(attribute, item.DisplayText, out var element))
            {
                return Task.FromResult<object>(element);
            }

            return Task.FromResult<object>(null);
        }

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
                            icon: default,
                            suffix: default,
                            attributeIcons: ImmutableArray<ImageElement>.Empty);
        }
    }
}
