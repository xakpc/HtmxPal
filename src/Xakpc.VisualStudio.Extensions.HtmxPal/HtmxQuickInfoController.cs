using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Xakpc.VisualStudio.Extensions.HtmxPal
{
    [Export(typeof(IIntellisenseControllerProvider))]
    [Name("Htmx ToolTip QuickInfo Controller")]
    [ContentType("html")]
    [ContentType("htmlx")] // .html
    [ContentType("razor")] // .cshtml
    internal class HtmxQuickInfoControllerProvider : IIntellisenseControllerProvider
    {
        [Import]
        internal IAsyncQuickInfoBroker QuickInfoBroker { get; set; }

        public IIntellisenseController TryCreateIntellisenseController(ITextView textView, IList<ITextBuffer> subjectBuffers)
        {
            Output.WriteInfo("HtmxQuickInfoControllerProvider:TryCreateIntellisenseController: got a request for a controller.");

            return new HtmxQuickInfoController(textView, subjectBuffers, this);
        }
    }

    internal class HtmxQuickInfoController : IIntellisenseController
    {
        private ITextView _textView;
        private IList<ITextBuffer> _subjectBuffers;
        private HtmxQuickInfoControllerProvider _provider;
        private IAsyncQuickInfoSession _session;

        public HtmxQuickInfoController(ITextView textView, IList<ITextBuffer> subjectBuffers, HtmxQuickInfoControllerProvider provider)
        {
            _textView = textView;
            _subjectBuffers = subjectBuffers;
            _provider = provider;

            _textView.MouseHover += this.OnTextViewMouseHover;
        }

        private DateTime _lastCheckTime = DateTime.MinValue;
        private const int DebounceMs = 300;

#pragma warning disable VSTHRD100 // Avoid async void methods
        private async void OnTextViewMouseHover(object sender, MouseHoverEventArgs e)
#pragma warning restore VSTHRD100 // Avoid async void methods
        {
            Output.WriteInfo("HtmxQuickInfoController:OnTextViewMouseHover: got a mouse hover event.");

            // Debounce (not sure if needed)
            if ((DateTime.Now - _lastCheckTime).TotalMilliseconds < DebounceMs)
            {
                return;
            }

            _lastCheckTime = DateTime.Now;

            //find the mouse position by mapping down to the subject buffer
            SnapshotPoint? point = _textView.BufferGraph.MapDownToFirstMatch
                 (new SnapshotPoint(_textView.TextSnapshot, e.Position),
                PointTrackingMode.Positive,
                snapshot => _subjectBuffers.Contains(snapshot.TextBuffer),
                PositionAffinity.Predecessor);

            if (point != null)
            {
                ITrackingPoint triggerPoint = point.Value.Snapshot.CreateTrackingPoint(point.Value.Position,
                PointTrackingMode.Positive);

                if (!_provider.QuickInfoBroker.IsQuickInfoActive(_textView))
                {
                    _session = await _provider.QuickInfoBroker.
                        TriggerQuickInfoAsync(_textView, triggerPoint, QuickInfoSessionOptions.TrackMouse);
                }
            }
        }

        public void Detach(ITextView textView)
        {
            if (_textView == textView)
            {
                _textView.MouseHover -= this.OnTextViewMouseHover;
                _textView = null;
            }
        }

        public void ConnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }

        public void DisconnectSubjectBuffer(ITextBuffer subjectBuffer)
        {
        }
    }
}
