using Microsoft.VisualStudio.Text;

namespace Xakpc.VisualStudio.Extensions.HtmxPal
{
    internal static class SnapshotPointExtensions
    {
        public static bool IsInsideHxAttribute(this SnapshotPoint point, out string attribute)
        {
            attribute = null;
            var line = point.GetContainingLine();
            var linePosition = point.Position;
            var snapshot = line.Snapshot;

            if (linePosition == 0 || snapshot[linePosition - 2] != '=' || snapshot[linePosition - 1] != '"')
            {
                return false;
            }

            int tokenStart = linePosition - 3;
            while (tokenStart > line.Start.Position)
            {
                char currentChar = snapshot[tokenStart];
                if (char.IsWhiteSpace(currentChar) || currentChar == '>')
                {
                    tokenStart++;
                    break;
                }
                tokenStart--;
            }

            var value = snapshot.GetText(tokenStart, linePosition - 3 - tokenStart + 1);

            if (value.StartsWith("hx-"))
            {
                attribute = value;
                return true;
            }

            return false;
        }

        public static bool IsInsideHtmlTag(this SnapshotPoint point)
        {
            var snapshot = point.Snapshot;
            var position = point.Position;

            // Check if we're already at the start of the snapshot
            if (position == 0)
                return false;

            int openBracketPos = -1;

            // Look backwards for opening bracket
            for (int i = position - 1; i >= 0; i--)
            {
                if (snapshot[i] == '>')
                    return false; // Found closing bracket first, so we're not inside a tag
                if (snapshot[i] == '<')
                {
                    openBracketPos = i;
                    break;
                }
            }

            return openBracketPos != -1;
        }

        public static SnapshotSpan GetContainingToken(this SnapshotPoint point)
        {
            var line = point.GetContainingLine();

            var linePosition = point.Position;
            var snapshot = line.Snapshot;

            int tokenStart = linePosition;
            while (tokenStart > line.Start.Position && IsValidTokenChar(snapshot[tokenStart - 1]))
            {
                tokenStart--;
            }

            int tokenEnd = linePosition;
            while (tokenEnd < line.End.Position && IsValidTokenChar(snapshot[tokenEnd]))
            {
                tokenEnd++;
            }

            return new SnapshotSpan(line.Snapshot, tokenStart, tokenEnd - tokenStart);
        }

        public static SnapshotSpan GetPreviousToken(this SnapshotSpan tokenSpan)
        {
            var line = tokenSpan.Start.GetContainingLine();
            var lineText = line.GetText();
            var tokenStartPosition = tokenSpan.Start.Position - line.Start.Position;

            int previousTokenEnd = tokenStartPosition - 1;
            while (previousTokenEnd > 0 && char.IsWhiteSpace(lineText[previousTokenEnd - 1]))
            {
                previousTokenEnd--;
            }

            int previousTokenStart = previousTokenEnd;
            while (previousTokenStart > 0 && IsValidTokenChar(lineText[previousTokenStart - 1]))
            {
                previousTokenStart--;
            }

            return new SnapshotSpan(line.Snapshot, line.Start + previousTokenStart, previousTokenEnd - previousTokenStart);
        }

        private static bool IsValidTokenChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '-';
        }
    }
}
