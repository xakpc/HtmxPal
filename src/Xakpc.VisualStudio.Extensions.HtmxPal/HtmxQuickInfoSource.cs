using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xakpc.VisualStudio.Extensions.HtmxPal.Services;

namespace Xakpc.VisualStudio.Extensions.HtmxPal
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("Htmx ToolTip QuickInfo Source")]
    [Order(Before = "Default Quick Info Presenter")]
    [ContentType("html")]
    [ContentType("htmlx")] // .html
    [ContentType("razor")] // .cshtml
    internal class HtmxQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        [Import]
        internal ITextBufferFactoryService TextBufferFactoryService { get; set; }

        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            Output.WriteInfo("HtmxQuickInfoSourceProvider:TryCreateQuickInfoSource: got a request for a source.");

            return new HtmxQuickInfoSource(this, textBuffer);
        }
    }

    internal class HtmxQuickInfoSource : IAsyncQuickInfoSource
    {
        private HtmxQuickInfoSourceProvider _provider;
        private ITextBuffer _subjectBuffer;
        
        private bool _isDisposed;

        public HtmxQuickInfoSource(HtmxQuickInfoSourceProvider provider, ITextBuffer subjectBuffer)
        {
            _provider = provider;
            _subjectBuffer = subjectBuffer;
        }

        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            var (containerElement, applicableToSpan) = await AugmentQuickInfoSessionAsync(session, cancellationToken);

            if (containerElement != null && applicableToSpan != null)
            {
                return new QuickInfoItem(applicableToSpan, containerElement);
            }

            return null;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        private async ValueTask<(ContainerElement, ITrackingSpan applicableToSpan)> AugmentQuickInfoSessionAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            // Map the trigger point down to our buffer.
            SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(_subjectBuffer.CurrentSnapshot);
            if (!subjectTriggerPoint.HasValue)
            {
                return (null, null);
            }

            var currentSnapshot = subjectTriggerPoint.Value.Snapshot;
            var navigator = _provider.NavigatorService.GetTextStructureNavigator(_subjectBuffer);
            var extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);

            // extract full hx- attribute text
            var searchText = GetAttributeText(subjectTriggerPoint.Value);

            if (string.IsNullOrEmpty(searchText))
            {
                return (null, null);
            }

            Output.WriteInfo("HtmxQuickInfoSource:AugmentQuickInfoSessionAsync: searchText=" + searchText);

            if (ToolTipsProvider.Instance.TryGetValue(searchText, out var value))
            {
                var applicableToSpan = currentSnapshot.CreateTrackingSpan(
                    extent.Span.Start,
                    extent.Span.Length,
                    SpanTrackingMode.EdgeInclusive
                );
                return (value, applicableToSpan);
            }

            return (null, null);
        }

        private string GetAttributeText(SnapshotPoint subjectTriggerPoint)
        {
            SnapshotPoint start = subjectTriggerPoint;
            var currentSnapshot = subjectTriggerPoint.Snapshot;
            var sb = new StringBuilder();

            // Search backwards for the start of the attribute
            while (start > 0 && IsValidAttributeChar(start - 1, out var c))
            {
                sb.Insert(0, c);
                start -= 1;
            }

            // Search forwards for the end of the attribute
            SnapshotPoint end = subjectTriggerPoint;
            while (end < currentSnapshot.Length && IsValidAttributeChar(end, out var c))
            {
                if (sb.Length >= 2)
                {
                    if (sb[0] != 'h' || sb[1] != 'x')
                    {
                        return default; // not a valid attribute
                    }
                }

                sb.Append(c);
                end += 1;
            }

            if (sb.Length < 3) // "hx-" is the minimum length
            {
                return default;
            }

            return sb.ToString();
        }


        private bool IsValidAttributeChar(SnapshotPoint point, out char c)
        {
            c = point.GetChar();
            return char.IsLetterOrDigit(c) || c == '-';
        }
    }
}
