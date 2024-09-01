using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Xakpc.VisualStudio.Extensions.HtmxPal
{
    /// <summary>
    /// Provides an IntelliSense controller for HTMX tooltips.
    /// </summary>
    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("htmx tooltip quickinfo controller")]
    [ContentType("html")]
    [ContentType("htmlx")]
    [ContentType("html-delegation")] // VS 2022
    [ContentType("razor")]
    [ContentType("LegacyRazorCSharp")] // VS 2022
    internal class HtmxQuickInfoControllerProvider : IIntellisenseControllerProvider
    {
        /// <summary>
        /// Gets or sets the QuickInfo broker.
        /// </summary>
        [Import]
        internal IAsyncQuickInfoBroker QuickInfoBroker { get; set; }

        /// <summary>
        /// Tries to create an IntelliSense controller for the specified text view and subject buffers.
        /// </summary>
        /// <param name="textView">The text view.</param>
        /// <param name="subjectBuffers">The subject buffers.</param>
        /// <returns>An IntelliSense controller, or null if none could be created.</returns>
        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            Output.WriteInfo("HtmxQuickInfoControllerProvider:TryCreateIntellisenseController: got a request for a controller.");

            return new HtmxQuickInfoController(textView, subjectBuffers, QuickInfoBroker);
        }
    }

    /// <summary>
    /// Controls the IntelliSense for HTMX tooltips.
    /// </summary>
    internal class HtmxQuickInfoController : IIntellisenseController
    {
        private ITextView _textView;
        private IList<ITextBuffer> _subjectBuffers;
        private IAsyncQuickInfoBroker _broker;

        private DateTime _lastCheckTime = DateTime.MinValue;
        private const int DebounceMs = 200;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmxQuickInfoController"/> class.
        /// </summary>
        /// <param name="textView">The text view.</param>
        /// <param name="subjectBuffers">The subject buffers.</param>
        /// <param name="broker">The QuickInfo broker.</param>
        public HtmxQuickInfoController(ITextView textView, IList<ITextBuffer> subjectBuffers, IAsyncQuickInfoBroker broker)
        {
            _textView = textView;
            _subjectBuffers = subjectBuffers;
            _broker = broker;

            _textView.MouseHover += this.OnTextViewMouseHover;
        }

        /// <summary>
        /// Handles the MouseHover event of the text view.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseHoverEventArgs"/> instance containing the event data.</param>
#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            // Debounce (not sure if needed)
            if ((DateTime.Now - _lastCheckTime).TotalMilliseconds < DebounceMs)
            {
                return;
            }

            _lastCheckTime = DateTime.Now;

            // find the mouse position by mapping down to the subject buffer
            var point = _textView.BufferGraph.MapDownToFirstMatch(new SnapshotPoint(_textView.TextSnapshot, e.Position),
                    PointTrackingMode.Positive,
                    snapshot => _subjectBuffers.Contains(snapshot.TextBuffer),
                    PositionAffinity.Predecessor);

            if (point != null)
            {
                ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position,
                PointTrackingMode.Positive);

                if (!_broker.IsQuickInfoActive(_textView))
                {
                    _ = await _broker.
                        TriggerQuickInfoAsync(_textView, triggerPoint, QuickInfoSessionOptions.TrackMouse);
                }
            }
        }

        /// <summary>
        /// Detaches the controller from the specified text view.
        /// </summary>
        /// <param name="textView">The text view.</param>
        public void Detach(ITextView textView)
        {
            if (_textView == textView)
            {
                _textView.MouseHover -= this.OnTextViewMouseHover;
                _textView = null;
            }
        }

        /// <summary>
        /// Connects the subject buffer to the controller.
        /// </summary>
        /// <param name="subjectBuffer">The subject buffer.</param>
        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        /// <summary>
        /// Disconnects the subject buffer from the controller.
        /// </summary>
        /// <param name="subjectBuffer">The subject buffer.</param>
        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }
    }
}
