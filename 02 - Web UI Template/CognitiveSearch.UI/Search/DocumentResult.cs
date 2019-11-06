// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CognitiveSearch.UI.Models.Annotations;
using static CognitiveSearch.UI.Models.EntityClassifications;
using static CognitiveSearch.UI.Models.TextClassifications;

namespace CognitiveSearch.UI
{
    public class DocumentResult
    {
        public List<object> Facets { get; set; }
        public Document Result { get; set; }
        public IList<SearchResult> Results { get; set; }
        public int? Count { get; set; }
        public string Token { get; set; }
        public List<object> Tags { get; set; }
        public string SearchId { get; set; }
        public string[] textStartChars { get; set; }
        public string[] textEndChars { get; set; }
        public string[] entityStartChars { get; set; }
        public string[] entityEndChars { get; set; }
        public Annotation[] entityAnnotations { get; set; }
        public Annotation[] textAnnotations { get; set; }
        public Annotation annotation { get; set; }
        public List<string> comments { get; set; }
        public string classification { get; set; }
        public string docClassification { get; set; }
        public string classID { get; set; }
    }
}
