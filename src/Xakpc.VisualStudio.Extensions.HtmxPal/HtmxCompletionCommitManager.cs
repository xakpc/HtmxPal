using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;

namespace Xakpc.VisualStudio.Extensions.HtmxPal
{
    /// <summary>
    /// Provides an instance of <see cref="HtmxCompletionCommitManager"/> for handling completion commits.
    /// </summary>
    [Export(typeof(IAsyncCompletionCommitManagerProvider))]
    [Name("htmx completion commit manager")]
    [ContentType("html")]
    [ContentType("htmlx")]
    [ContentType("html-delegation")] // VS 2022
    [ContentType("razor")]
    [ContentType("LegacyRazorCSharp")] // VS 2022
    internal class HtmxCompletionCommitManagerProvider : IAsyncCompletionCommitManagerProvider
    {
        private readonly Lazy<HtmxCompletionCommitManager> Manager = new Lazy<HtmxCompletionCommitManager>(() => new HtmxCompletionCommitManager());

        /// <summary>
        /// Gets or creates an instance of <see cref="HtmxCompletionCommitManager"/>.
        /// </summary>
        /// <param name="textView">The text view for which the manager is requested.</param>
        /// <returns>An instance of <see cref="IAsyncCompletionCommitManager"/>.</returns>
        public IAsyncCompletionCommitManager GetOrCreate(ITextView textView)
        {
            Output.WriteInfo("HtmxCompletionCommitManagerProvider:GetOrCreate: got a request for a manager.");
            return Manager.Value;
        }
    }

    /// <summary>
    /// Manages the commit of completion items for HTMX.
    /// </summary>
    internal class HtmxCompletionCommitManager : IAsyncCompletionCommitManager
    {
        /// <summary>
        /// Gets the characters that can potentially commit a completion.
        /// </summary>
        public IEnumerable<char> PotentialCommitCharacters => new List<char> { ' ', '>', '=' };

        /// <summary>
        /// Determines whether a completion should be committed based on the typed character.
        /// </summary>
        /// <param name="session">The current completion session.</param>
        /// <param name="location">The location in the buffer where the character was typed.</param>
        /// <param name="typedChar">The character that was typed.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns><c>true</c> if the completion should be committed; otherwise, <c>false</c>.</returns>
        public bool ShouldCommitCompletion(IAsyncCompletionSession session, SnapshotPoint location, char typedChar, CancellationToken token)
        {
            // Check if the typed character is in the list of potential commit characters
            return PotentialCommitCharacters.Contains(typedChar);
        }

        /// <summary>
        /// Attempts to commit the specified completion item.
        /// </summary>
        /// <param name="session">The current completion session.</param>
        /// <param name="buffer">The text buffer.</param>
        /// <param name="item">The completion item to commit.</param>
        /// <param name="typedChar">The character that was typed.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A <see cref="CommitResult"/> indicating whether the commit was handled.</returns>
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
