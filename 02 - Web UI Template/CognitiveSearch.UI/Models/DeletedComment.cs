// Copyright(c) Microsoft Corporation, Sara Hudson, Makayla Dorroh, Caitlin Jones, Gabby Allen, Anna Tuggle, Shaun Duarte.
// All rights reserved. Licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace CognitiveSearch.UI.Models
{
    public class DeletedComments
    {
        public class DeletedComment : TableEntity
        {
            public DeletedComment(string PKey, string RKey)
            {
                this.PartitionKey = PKey;
                this.RowKey = "C" + RKey;
            }

            public DeletedComment() { }

            public string CommentID { get; set; }
            public string CommentText { get; set; }
            public DateTime Date { get; set; }
            public string AnnotationID { get; set; }
        }
    }
}
