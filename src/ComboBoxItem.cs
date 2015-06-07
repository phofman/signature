using System;

namespace CodeTitans.Signature
{
    /// <summary>
    /// Helper class to display items in combo-box.
    /// </summary>
    sealed class ComboBoxItem
    {
        private readonly string _text;

        /// <summary>
        /// Init constructor.
        /// </summary>
        public ComboBoxItem(object data, string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            Data = data;
            _text = text;
        }

        #region Properties

        public object Data
        {
            get;
            private set;
        }

        #endregion

        /// <summary>
        /// Casts the data part as specified type.
        /// </summary>
        public T DataAs<T>()
        {
            return (T) Data;
        }

        public override string ToString()
        {
            return _text;
        }
    }
}
