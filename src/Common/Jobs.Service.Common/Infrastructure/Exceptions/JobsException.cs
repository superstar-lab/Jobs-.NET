﻿using System;

namespace Jobs.Service.Common.Infrastructure.Exceptions
{
    public class JobsException : Exception
    {
        public JobsException()
        {
        }

        public JobsException(string message) : base(message)
        {
        }

        public JobsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
