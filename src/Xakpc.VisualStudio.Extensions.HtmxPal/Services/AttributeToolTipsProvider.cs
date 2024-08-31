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
    /// Provides tooltips for attributes by reading markdown files from a specified directory.
    /// </summary>
    internal class AttributeToolTipsProvider
    {
        private static readonly Lazy<AttributeToolTipsProvider> lazy = new Lazy<AttributeToolTipsProvider>(() => new AttributeToolTipsProvider());
        private readonly ConcurrentDictionary<string, List<(string Key, Lazy<ContainerElement> KeyDescription)>> _cachedDictionary
           = new ConcurrentDictionary<string, List<(string, Lazy<ContainerElement>)>>();

        /// <summary>
        /// Gets the singleton instance of the <see cref="AttributeToolTipsProvider"/> class.
        /// </summary>
        public static AttributeToolTipsProvider Instance { get { return lazy.Value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeToolTipsProvider"/> class.
        /// </summary>
        public AttributeToolTipsProvider()
        {            
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "htmx");

            foreach (var directory in Directory.GetDirectories(path))
            {
                if (directory.EndsWith("attributes", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var directoryName = Path.GetFileNameWithoutExtension(directory);

                var items = new List<(string, Lazy<ContainerElement>)>();
                foreach (var file in Directory.GetFiles(directory, "*.md"))
                {
                    var key = Path.GetFileNameWithoutExtension(file);
                    var lazyElement = new Lazy<ContainerElement>(() =>
                    {
                        var markdown = File.ReadAllText(file);

                        return ContainerElementHelper.CreateRichContent(markdown);
                    });

                    items.Add((key, lazyElement));
                }

                _cachedDictionary.TryAdd(directoryName, items);
            }
        }

        /// <summary>
        /// Gets the list of keywords for a specified key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <returns>A read-only list of keywords, or null if the key is not found.</returns>
        public IReadOnlyList<string> GetKeywords(string key)
        {
            if (_cachedDictionary.TryGetValue(key, out var list))
            {
                return list.Select(x => x.Key).ToImmutableList();
            }

            Output.WriteWarining($"AttributeToolTipsProvider.GetKeywords: key '{key}' not found.");
            return null;
        }

        /// <summary>
        /// Tries to get the value of a specified attribute and key.
        /// </summary>
        /// <param name="attribute">The attribute to look up.</param>
        /// <param name="key">The key to look up.</param>
        /// <param name="element">When this method returns, contains the value associated with the specified attribute and key, if the key is found; otherwise, null.</param>
        /// <returns><c>true</c> if the attribute and key are found; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string attribute, string key, out ContainerElement element)
        {
            element = null;

            if (_cachedDictionary.TryGetValue(attribute, out var list))
            {
                element = list.FirstOrDefault(x => x.Key == key).KeyDescription.Value;
                return element != null;
            }

            Output.WriteWarining($"AttributeToolTipsProvider.TryGetValue: attribute '{attribute}' and key '{key}' not found.");
            return false;
        }
    }
}
