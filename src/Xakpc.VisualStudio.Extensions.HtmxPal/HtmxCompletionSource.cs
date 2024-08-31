using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Configuration;
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
    [Name("Htmx completion item source")]
    [ContentType("html")]
    [ContentType("htmlx")] // .html
    [ContentType("Razor")] // .cshtml
    internal class HtmxCompletionSourceProvider : IAsyncCompletionSourceProvider
    {
        private readonly Lazy<HtmxCompletionSource> Source;

        [Import]
        IAsyncCompletionBroker AsyncCompletionBroker { get;set;}

        public HtmxCompletionSourceProvider()
        {
            Source = new Lazy<HtmxCompletionSource>(() => new HtmxCompletionSource(AsyncCompletionBroker));
        }

        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            Output.WriteInfo("HtmxCompletionSourceProvider:GetOrCreate: got a request for a source.");
            return Source.Value;
        }
    }

    /// <summary>
    ///  
    /// </summary>
    internal class HtmxCompletionSource : IAsyncCompletionSource
    {
        // CompletionItem takes array of CompletionFilters.
        static CompletionFilter Filter = new CompletionFilter("htmx", "hx", CompletionItemIcon);
        static ImmutableArray<CompletionFilter> Filters = ImmutableArray.Create(Filter);

        // Icon
        private static ImageElement CompletionItemIcon = new ImageElement(KnownMonikers.HTMLEndTag.ToImageId(), "htmx");
        private IAsyncCompletionBroker _asyncCompletionBroker;

        public HtmxCompletionSource(IAsyncCompletionBroker asyncCompletionBroker)
        {
            _asyncCompletionBroker = asyncCompletionBroker;
            _asyncCompletionBroker.CompletionTriggered += OnAsyncCompletionSessionStarted;
        }

        public void Dispose()
        {
            _asyncCompletionBroker.CompletionTriggered -= OnAsyncCompletionSessionStarted;
        }

        private void OnAsyncCompletionSessionStarted(object sender, CompletionTriggeredEventArgs e)
        {
            var sessions = _asyncCompletionBroker.GetSession(e.TextView);
            
            Output.WriteInfo($"HtmxCompletionSource:OnAsyncCompletionSessionStarted: got a request for a session. Returning {sessions} sessions.");
        }

        public Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken cancellationToken)
        {
            var arr = ToolTipsProvider.Instance.Keywords.Select(ConvertToItem).ToImmutableArray();

            Output.WriteInfo($"HtmxCompletionSource:GetCompletionContextAsync: got a request for a context. Returning {arr.Length} items.");

            return Task.FromResult(new CompletionContext(arr));
        }

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
                            filters: Filters,
                            icon: CompletionItemIcon,
                            suffix: default,
                            attributeIcons: ImmutableArray<ImageElement>.Empty);
        }

        public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
        {
            if (ToolTipsProvider.Instance.TryGetValue(item.DisplayText, out var element, false))
            {
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
    }   
}
