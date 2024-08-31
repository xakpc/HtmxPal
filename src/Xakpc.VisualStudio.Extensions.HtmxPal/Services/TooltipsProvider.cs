using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Xakpc.VisualStudio.Extensions.HtmxPal.Services
{
    /// <summary>
    /// Provides tooltips for HTMX attributes.
    /// </summary>
    internal class ToolTipsProvider
    {
        private static readonly Lazy<ToolTipsProvider> lazy = new Lazy<ToolTipsProvider>(() => new ToolTipsProvider());
        private readonly ConcurrentDictionary<string, ContainerElement> _cachedDictionary = new ConcurrentDictionary<string, ContainerElement>();

        /// <summary>
        /// Gets the singleton instance of the <see cref="ToolTipsProvider"/> class.
        /// </summary>
        public static ToolTipsProvider Instance { get { return lazy.Value; } }

        /// <summary>
        /// Gets the list of HTMX attribute keywords.
        /// </summary>
        public IReadOnlyList<string> Keywords { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolTipsProvider"/> class.
        /// </summary>
        public ToolTipsProvider()
        {
            var directory = GetAttributesDirectoryPath();
            var strings = Directory.GetFiles(directory).Select(path => Path.GetFileNameWithoutExtension(path));
            Keywords = strings.ToImmutableList();
        }

        /// <summary>
        /// Tries to get the tooltip content for the specified key.
        /// </summary>
        /// <param name="key">The key of the attribute.</param>
        /// <param name="element">When this method returns, contains the tooltip content if the key was found; otherwise, null.</param>
        /// <param name="useLink">Specifies whether to include a link to the HTMX documentation.</param>
        /// <returns><c>true</c> if the key was found; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string key, out ContainerElement element, bool useLink = true)
        {
            if (_cachedDictionary.TryGetValue(key, out element))
            {
                return true;
            }
            else
            {
                var path = GetAttributePath(key);

                if (Keywords.Contains(key))
                {
                    var markdown = File.ReadAllText(path);
                    element = useLink ?
                        ContainerElementHelper.CreateRichContent(markdown, $"https://htmx.org/attributes/{key}/") :
                        ContainerElementHelper.CreateRichContent(markdown);
                    _cachedDictionary.TryAdd(key, element);
                    return true;
                }

                Output.WriteWarining($"ToolTipsProvider:TryGetValue: The key '{key}' was not found.");

                element = null;
                return false;
            }
        }

        /// <summary>
        /// Gets the path to the HTMX attributes directory.
        /// </summary>
        /// <returns>The path to the HTMX attributes directory.</returns>
        private string GetAttributesDirectoryPath()
        {
            return Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "htmx\\attributes\\"
                 );
        }

        /// <summary>
        /// Gets the full path to the specified attribute file.
        /// </summary>
        /// <param name="filename">The name of the attribute file.</param>
        /// <returns>The full path to the specified attribute file.</returns>
        private string GetAttributePath(string filename)
        {
            return Path.Combine(GetAttributesDirectoryPath(), filename) + ".md";
        }
    }
}
