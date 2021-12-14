using System;
using System.Runtime.Serialization;

namespace FubarDev.WebDavServer.Locking.InMemory
{
    /// <summary>
    /// Base exception for all exceptions thrown by the <see cref="ILockStore"/>.
    /// </summary>
    [Serializable]
    public class LockException : ApplicationException
    {
        /// <summary>
        ///
        /// </summary>
        public LockException()
        {
        }

        public LockException(string message) : base(message)
        {
        }

        public LockException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LockException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
