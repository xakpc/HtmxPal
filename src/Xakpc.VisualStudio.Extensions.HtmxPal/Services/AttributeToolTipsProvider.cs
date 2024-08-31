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
    internal class AttributeToolTipsProvider
    {
        private static readonly Lazy<AttributeToolTipsProvider> lazy = new Lazy<AttributeToolTipsProvider>(() => new AttributeToolTipsProvider());
        public static AttributeToolTipsProvider Instance { get { return lazy.Value; } }

        private readonly ConcurrentDictionary<string, List<(string Key, Lazy<ContainerElement> KeyDescription)>> _cachedDictionary 
            = new ConcurrentDictionary<string, List<(string, Lazy<ContainerElement>)>>();

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

        public IReadOnlyList<string> GetKeywords(string key)
        {
            if (_cachedDictionary.TryGetValue(key, out var list))
            {
                return list.Select(x => x.Key).ToImmutableList();
            }
            
            return null;
        }

        public bool TryGetValue(string attribute, string key, out ContainerElement element)
        {
            element = null;

            if (_cachedDictionary.TryGetValue(attribute, out var list))
            {
                element = list.FirstOrDefault(x => x.Key == key).KeyDescription.Value;                
                return element != null;
            }
                        
            return false;
        }
    }
}
