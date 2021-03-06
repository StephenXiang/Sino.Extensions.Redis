﻿using System;

namespace Sino.Extensions.Redis
{
    /// <summary>
    /// Provides data for the event that is raised when a Redis MONITOR message is received
    /// </summary>
    public class RedisMonitorEventArgs : EventArgs
    {
        /// <summary>
        /// Monitor output
        /// </summary>
        public object Message { get; private set; }

        internal RedisMonitorEventArgs(object message)
        {
            Message = message;
        }
    }
}
