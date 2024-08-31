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
    /// <summary>
    /// Provides a source for QuickInfo tooltips for HTML, HTMLX, and Razor content types.
    /// </summary>
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name("htmx tooltip quickInfo source")]
    [ContentType("html")]
    [ContentType("htmlx")]
    [ContentType("razor")]    
    internal class HtmxQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        /// <summary>
        /// Tries to create a QuickInfo source for the given text buffer.
        /// </summary>
        /// <param name="textBuffer">The text buffer for which to create the QuickInfo source.</param>
        /// <returns>An instance of <see cref="IAsyncQuickInfoSource"/>.</returns>
        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {            
            Output.WriteInfo("HtmxQuickInfoSourceProvider:TryCreateQuickInfoSource: got a request for a source.");
            return new HtmxQuickInfoSource(NavigatorService, textBuffer);
        }
    }

    /// <summary>
    /// Provides QuickInfo tooltips for HTML, HTMLX, and Razor content types.
    /// </summary>
    internal class HtmxQuickInfoSource : IAsyncQuickInfoSource
    {
        private ITextStructureNavigatorSelectorService _navigator;
        private ITextBuffer _subjectBuffer;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmxQuickInfoSource"/> class.
        /// </summary>
        /// <param name="navigator">The text structure navigator selector service.</param>
        /// <param name="subjectBuffer">The text buffer for which to provide QuickInfo.</param>
        public HtmxQuickInfoSource(ITextStructureNavigatorSelectorService navigator, ITextBuffer subjectBuffer)
        {
            _navigator = navigator;
            _subjectBuffer = subjectBuffer;
        }

        /// <summary>
        /// Asynchronously gets the QuickInfo item for the specified session.
        /// </summary>
        /// <param name="session">The QuickInfo session.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the QuickInfo item.</returns>
        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            var (containerElement, applicableToSpan) = await BuildQuickInfoElementsAsync(session, cancellationToken);

            if (containerElement != null && applicableToSpan != null)
            {
                return new QuickInfoItem(applicableToSpan, containerElement);
            }

            return null;
        }

        /// <summary>
        /// Disposes the QuickInfo source.
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Asynchronously builds the QuickInfo elements for the specified session.
        /// </summary>
        /// <param name="session">The QuickInfo session.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the container element and the applicable span.</returns>
        private async ValueTask<(ContainerElement, ITrackingSpan applicableToSpan)> BuildQuickInfoElementsAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            // Map the trigger point down to our buffer.
            SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(_subjectBuffer.CurrentSnapshot);
            if (!subjectTriggerPoint.HasValue)
            {
                return (null, null);
            }

            var currentSnapshot = subjectTriggerPoint.Value.Snapshot;
            var navigator = _navigator.GetTextStructureNavigator(_subjectBuffer);
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

        /// <summary>
        /// Gets the attribute text at the specified trigger point.
        /// </summary>
        /// <param name="subjectTriggerPoint">The trigger point.</param>
        /// <returns>The attribute text.</returns>
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

        /// <summary>
        /// Determines whether the character at the specified point is a valid attribute character.
        /// </summary>
        /// <param name="point">The snapshot point.</param>
        /// <param name="c">The character at the point.</param>
        /// <returns><c>true</c> if the character is a valid attribute character; otherwise, <c>false</c>.</returns>
        private bool IsValidAttributeChar(SnapshotPoint point, out char c)
        {
            c = point.GetChar();
            return char.IsLetterOrDigit(c) || c == '-';
        }
    }
}
