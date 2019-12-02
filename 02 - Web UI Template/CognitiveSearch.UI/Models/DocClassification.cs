﻿// Copyright(c) Microsoft Corporation, Sara Hudson, Makayla Dorroh, Caitlin Jones, Gabby Allen, Anna Tuggle, Shaun Duarte.
// All rights reserved. Licensed under the MIT License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace CognitiveSearch.UI.Models
{
    public class DocClassifications
    {
        public class DocClassification : TableEntity
        {
            public DocClassification(string PKey, string RKey)
            {
                this.PartitionKey = PKey;
                this.RowKey = "DC" + RKey;
            }

            public DocClassification() { }

            public string DocClassID { get; set; }
            public string Classification { get; set; }
        }
    }
}
