using System.Security.Cryptography.X509Certificates;

namespace CodeTitans.Signature
{
    /// <summary>
    /// Helper class to store info about available stores.
    /// </summary>
    sealed class NamedStore
    {
        /// <summary>
        /// Init constructor.
        /// </summary>
        public NamedStore(StoreName name, StoreLocation location)
        {
            Name = name;
            Location = location;
        }

        /// <summary>
        /// Init constructor.
        /// </summary>
        public NamedStore(StoreName name)
        {
            Name = name;
            Location = StoreLocation.CurrentUser;
        }

        #region Properties

        public StoreName Name
        {
            get;
            private set;
        }

        public StoreLocation Location
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
            if (Location == StoreLocation.CurrentUser)
                return Name.ToString();

            return Name + " (machine)";
        }
    }
}
