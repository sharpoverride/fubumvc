﻿using System.Collections.Generic;
using HtmlTags;

namespace FubuMVC.Diagnostics.Models.Html
{
    public class HtmlConventionsPreviewViewModel
    {
        public HtmlConventionsPreviewViewModel()
        {
            Links = new List<PropertyLink>();
            Examples = new List<PropertyExample>();
        }

        public string Type { get; set; }
        public IEnumerable<PropertyLink> Links { get; set; }
        public IEnumerable<PropertyExample> Examples { get; set; }
    }

    public class PropertyLink
    {
        public string Source { get; set; }
        public string Path { get; set; }
    }

    public class PropertyExample
    {
        public PropertyExample()
        {
            Examples = new List<Example>();
        }

        public string Source { get; set; }
        public IEnumerable<Example> Examples { get; set; }
    }

    public class Example
    {
        public string Expression { get; set; }
        public string Source { get; set; }
        public HtmlTag Rendered { get; set; }
    }
}