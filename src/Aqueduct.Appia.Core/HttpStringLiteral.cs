using System;

namespace Aqueduct.Appia.Core
{
    public sealed class HttpStringLiteral
    {
        private readonly string _inner;
        /// <summary>
        /// Initializes a new instance of the HttpStringLiteral class.
        /// </summary>
        public HttpStringLiteral(string inner)
        {
            _inner = inner;
        }

        public override bool Equals(object obj)
        {
            if (obj is String)
                return _inner.Equals(obj);
            if (obj is HttpStringLiteral)
                return _inner.Equals(((HttpStringLiteral)obj)._inner);

            return _inner.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _inner.GetHashCode();
        }

        public override string ToString()
        {
            return _inner.ToString();
        }
    }
}
