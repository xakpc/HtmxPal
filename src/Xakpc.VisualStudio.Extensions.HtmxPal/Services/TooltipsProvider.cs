using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Xakpc.VisualStudio.Extensions.HtmxPal.Services
{
    internal class ToolTipsProvider
    {
        private static readonly Lazy<ToolTipsProvider> lazy = new Lazy<ToolTipsProvider>(() => new ToolTipsProvider());
        public static ToolTipsProvider Instance { get { return lazy.Value; } }

        private readonly ConcurrentDictionary<string, ContainerElement> _cachedDictionary = new ConcurrentDictionary<string, ContainerElement>();

        public IReadOnlyList<string> Keywords { get; }

        public ToolTipsProvider()
        {
            var directory = GetAttibutesPath();
            var strings = Directory.GetFiles(directory).Select(path => Path.GetFileNameWithoutExtension(path));
            Keywords = strings.ToImmutableList();
        }

        public string GetAttibutesPath()
        {
            return Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "htmx\\attributes\\"
                 );
        }

        private string GetAttributePath(string filename)
        {
            return Path.Combine(GetAttibutesPath(), filename) + ".md";
        }

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

                element = null;
                return false;
            }
        }        


    }
}
