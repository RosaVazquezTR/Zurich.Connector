﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Web.Models
{
    /// <summary>
    /// Store CDM Mapping of properties
    /// </summary>
    public class CDMMappingViewModel
    {
        /// <summary>
        /// CMD mapping of properties.
        /// </summary>
        public List<CDMElementViewModel> Structured { get; set; }
        /// <summary>
        /// Place holder to store properties which is not part of the structured definition.
        /// </summary>
        public List<CDMElementViewModel> Unstructured { get; set; }
    }

    /// <summary>
    /// Store the CDM mapping of individual elements
    /// </summary>
    public class CDMElementViewModel
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Type of the property
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Name of the response element
        /// </summary>
        public string ResponseElement { get; set; }
        /// <summary>
        /// Property to validate if is an inner property
        /// </summary>
        public bool IsInner { get; set; } = true;
    }
}
