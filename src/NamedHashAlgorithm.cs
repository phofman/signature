using System;

namespace CodeTitans.Signature
{
    /// <summary>
    /// Helper class to describe HASH algorithm used during signing.
    /// </summary>
    sealed class NamedHashAlgorithm
    {
        /// <summary>
        /// Init constructor.
        /// </summary>
        public NamedHashAlgorithm(string name, string uri)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException("uri");

            Name = name;
            Uri = uri;
        }

        #region Properties

        /// <summary>
        /// Gets the public name of the algorithm.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets unique identification of the hashing algorithm (http://www.w3.org/TR/2001/WD-xmlenc-core-20010626/).
        /// </summary>
        public string Uri
        {
            get;
            private set;
        }

        #endregion

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
