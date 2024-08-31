using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xakpc.VisualStudio.Extensions.HtmxPal
{
    [Export(typeof(IAsyncCompletionCommitManagerProvider))]
    [Name("Htmx completion commit manager")]
    [ContentType("html")]
    [ContentType("htmlx")] // .html
    [ContentType("Razor")] // .cshtml
    internal class HtmxCompletionCommitManagerProvider : IAsyncCompletionCommitManagerProvider
    {
        private readonly Lazy<HtmxCompletionCommitManager> Manager;

        public HtmxCompletionCommitManagerProvider()
        {
            Manager = new Lazy<HtmxCompletionCommitManager>(() => new HtmxCompletionCommitManager());
        }

        public IAsyncCompletionCommitManager GetOrCreate(ITextView textView) => Manager.Value;
    }

    internal class HtmxCompletionCommitManager : IAsyncCompletionCommitManager
    {
        public IEnumerable<char> PotentialCommitCharacters => new List<char>();

        public bool ShouldCommitCompletion(IAsyncCompletionSession session, SnapshotPoint location, char typedChar, CancellationToken token)
        {
            return true;
        }

        public CommitResult TryCommit(IAsyncCompletionSession session, ITextBuffer buffer, CompletionItem item, char typedChar, CancellationToken token)
        {
            // Check if this is one of your custom completions
            if (item.InsertText.StartsWith("hx") && item.InsertText.EndsWith("=\"\""))
            {
                var span = session.ApplicableToSpan;

                // Commit the completion
                using (var edit = buffer.CreateEdit())
                {
                    edit.Replace(span.GetSpan(buffer.CurrentSnapshot), item.InsertText);
                    edit.Apply();
                }

                // Move the caret inside the quotes
                var newPosition = span.GetStartPoint(buffer.CurrentSnapshot).Position + item.InsertText.Length - 1;
                session.TextView.Caret.MoveTo(new SnapshotPoint(buffer.CurrentSnapshot, newPosition));

                Output.WriteInfo("HtmxCompletionCommitManager:TryCommit: committed completion.");
                return CommitResult.Handled;
            }

            return CommitResult.Unhandled;
        }
    }
}
