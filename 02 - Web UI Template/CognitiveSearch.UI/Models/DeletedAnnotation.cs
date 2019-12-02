﻿// Copyright(c) Microsoft Corporation, Sara Hudson, Makayla Dorroh, Caitlin Jones, Gabby Allen, Anna Tuggle, Shaun Duarte.
// All rights reserved. Licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace CognitiveSearch.UI.Models
{
    public class DeletedAnnotations
    {
        public class DeletedAnnotation : TableEntity
        {
            public DeletedAnnotation(string PKey, string RKey)
            {
                this.PartitionKey = PKey;
                this.RowKey = "A" + RKey;
            }

            public DeletedAnnotation() { }

            public string AnnotationID { get; set; }
            public string ClassificationID { get; set; }
            public string DocumentID { get; set; }
            public string StartCharLocation { get; set; }
            public string EndCharLocation { get; set; }
            public string HighlightedText { get; set; }
            public int Accept { get; set; }
            public int Deny { get; set; }
        }
    }
}
