using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Adornments;
using System.Collections.Generic;
using System.Text;

namespace Xakpc.VisualStudio.Extensions.HtmxPal
{
    /// <summary>
    /// Converts Markdown text into a list of ContainerElement objects for display in Visual Studio.
    /// </summary>
    internal class MarkdownConverter
    {
        /// <summary>
        /// Converts the given Markdown string into a list of ContainerElement objects.
        /// </summary>
        /// <param name="markdown">The Markdown string to convert.</param>
        /// <returns>A list of ContainerElement objects representing the converted Markdown.</returns>
        public List<ContainerElement> Convert(string markdown)
        {
            var containers = new List<ContainerElement>();
            var lines = markdown.Split('\n');

            ContainerElement currentContainer = new ContainerElement(ContainerElementStyle.Stacked);
            List<ClassifiedTextElement> currentElements = new List<ClassifiedTextElement>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (string.IsNullOrEmpty(line))
                {
                    // Empty line, create a new paragraph
                    if (currentElements.Count > 0)
                    {
                        currentContainer = new ContainerElement(ContainerElementStyle.Stacked, currentElements.ToArray());
                        containers.Add(currentContainer);
                        currentElements.Clear();
                    }
                }
                else if (line.StartsWith("```"))
                {
                    // Code block
                    var codeBlock = new StringBuilder();
                    i++;
                    while (i < lines.Length && !lines[i].StartsWith("```"))
                    {
                        codeBlock.Append(lines[i]);
                        i++;
                    }

                    currentElements.Add(new ClassifiedTextElement(
                        new ClassifiedTextRun(PredefinedClassificationTypeNames.MarkupNode, codeBlock.ToString().TrimEnd(),
                            ClassifiedTextRunStyle.UseClassificationFont)
                    ));
                }
                else
                {
                    // Process inline elements
                    currentElements.Add(ProcessInlineElements(line));
                }
            }

            // Add any remaining elements to the last container
            if (currentElements.Count > 0)
            {
                currentContainer = new ContainerElement(ContainerElementStyle.Stacked, currentElements.ToArray());
                containers.Add(currentContainer);
            }

            return containers;
        }

        /// <summary>
        /// Processes a line of Markdown text and converts it into a ClassifiedTextElement with appropriate styling.
        /// </summary>
        /// <param name="line">The line of Markdown text to process.</param>
        /// <returns>A ClassifiedTextElement representing the processed line.</returns>
        private ClassifiedTextElement ProcessInlineElements(string line)
        {
            var runs = new List<ClassifiedTextRun>();
            int index = 0;

            if (line.StartsWith("-"))
            {
                // replace first dash with a bullet in line string
                line = "•" + line.Substring(1);
            }

            while (index < line.Length)
            {
                if (line.Substring(index).StartsWith("**"))
                {
                    int endBold = line.IndexOf("**", index + 2);
                    if (endBold != -1)
                    {
                        runs.Add(new ClassifiedTextRun(PredefinedClassificationTypeNames.Text, line.Substring(index + 2, endBold - index - 2),
                            ClassifiedTextRunStyle.Bold));
                        index = endBold + 2;
                    }
                    else
                    {
                        runs.Add(new ClassifiedTextRun(PredefinedClassificationTypeNames.Text, line.Substring(index),
                            ClassifiedTextRunStyle.Plain));
                        break;
                    }
                }
                else if (line[index] == '`')
                {
                    int endCode = line.IndexOf('`', index + 1);
                    if (endCode != -1)
                    {
                        runs.Add(new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, line.Substring(index + 1, endCode - index - 1),
                            ClassifiedTextRunStyle.UseClassificationFont));
                        index = endCode + 1;
                    }
                    else
                    {
                        runs.Add(new ClassifiedTextRun(PredefinedClassificationTypeNames.Text, line.Substring(index),
                            ClassifiedTextRunStyle.Plain));
                        break;
                    }
                }
                else
                {
                    int nextSpecial = line.IndexOfAny(new char[] { '*', '`' }, index);
                    if (nextSpecial == -1)
                    {
                        runs.Add(new ClassifiedTextRun(PredefinedClassificationTypeNames.Text, line.Substring(index),
                            ClassifiedTextRunStyle.Plain));
                        break;
                    }
                    else
                    {
                        runs.Add(new ClassifiedTextRun(PredefinedClassificationTypeNames.Text, line.Substring(index, nextSpecial - index),
                            ClassifiedTextRunStyle.Plain));
                        index = nextSpecial;
                    }
                }
            }

            return new ClassifiedTextElement(runs.ToArray());
        }
    }
}
