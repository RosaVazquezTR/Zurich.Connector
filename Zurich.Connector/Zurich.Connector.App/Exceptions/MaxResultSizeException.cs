﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.App.Exceptions
{
    /// <summary>
    /// Custom maximum result size exception
    /// </summary>
    public class MaxResultSizeException : Exception
    {
        public MaxResultSizeException(string message) : base(message)
        {

        }
    }
}
