using System.Collections.Specialized;

namespace ExitGames.Configuration
{
    /// <summary>
    /// Reads values from a <see cref="T:System.Collections.Specialized.NameValueCollection"/>.
    /// </summary>
    public class NameValueCollectionReader
    {
        /// <summary>
        ///  The name value collection.
        /// </summary>
        private readonly NameValueCollection collection;

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:ExitGames.Configuration.NameValueCollectionReader"/> class.
        /// </summary>
        /// <param name="collection"> The collection.</param>
        public NameValueCollectionReader(NameValueCollection collection)
        {
            this.collection = collection;
        }

        /// <summary>
        /// Gets an optional string value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns> If found the value; otherwise the <paramref name = "defaultValue" />.</returns>
        public string GetOptionalValue(string key, string defaultValue)
        {
            string str;
            if (this.TryGetValue(key, out str))
            {
                return str;
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets an optional boolean value.
        /// </summary>
        /// <param name="key"> The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>If found the value; otherwise the <paramref name = "defaultValue" />.</returns>
        public bool GetOptionalValueBoolean(string key, bool defaultValue)
        {
            bool flag;
            if (!this.TryGetValueBoolean(key, out flag))
            {
                return defaultValue;
            }
            return flag;
        }

        /// <summary>
        ///   Gets an optional byte value.
        /// </summary>
        /// <param name="key"> The key.</param>
        /// <param name="defaultValue"> The default value.</param>
        /// <returns>If found the value; otherwise the <paramref name = "defaultValue" />.</returns>
        public byte GetOptionalValueByte(string key, byte defaultValue)
        {
            byte num;
            if (!this.TryGetValueByte(key, out num))
            {
                return defaultValue;
            }
            return num;
        }

        /// <summary>
        /// Gets an optional integer value.
        /// </summary>
        /// <param name="key"> The key.</param>
        /// <param name="defaultValue"> The default value.</param>
        /// <returns>If found the value; otherwise the <paramref name = "defaultValue" />.</returns>
        public int GetOptionalValueInteger(string key, int defaultValue)
        {
            int num;
            if (!this.TryGetValueInteger(key, out num))
            {
                return defaultValue;
            }
            return num;
        }

        /// <summary>
        ///  Gets a mandatory string value.
        /// </summary>
        /// <param name="key"> The key.</param>
        /// <returns> The value.</returns>
        /// <exception cref="T:ExitGames.Configuration.ConfigurationException">
        ///    Key missing in collection.
        /// </exception>
        public string GetValue(string key)
        {
            string str;
            if (!this.TryGetValue(key, out str))
            {
                throw new ConfigurationException(string.Format("Key '{0}' missing", key));
            }
            return str;
        }

        /// <summary>
        /// Gets a mandatory boolean value.
        /// </summary>
        /// <param name="key">  The key.</param>
        /// <returns>   The value.</returns>
        /// <exception cref="T:ExitGames.Configuration.ConfigurationException">
        ///   Key missing in collection.
        /// </exception>
        public bool GetValueBoolean(string key)
        {
            bool flag;
            if (!this.TryGetValueBoolean(key, out flag))
            {
                throw new ConfigurationException(string.Format("Parse Error: Value of '{0}' has wrong format. Boolean expected.", key));
            }
            return flag;
        }

        /// <summary>
        /// Gets a mandatory byte value.
        /// </summary>
        /// <param name="key"> The key.</param>
        /// <returns> The value.</returns>
        /// <exception cref="T:ExitGames.Configuration.ConfigurationException">
        ///     Key missing in collection.
        /// </exception>
        public byte GetValueByte(string key)
        {
            byte num;
            if (!this.TryGetValueByte(key, out num))
            {
                throw new ConfigurationException(string.Format("Parse Error: Value of '{0}' has wrong format. Byte expected.", key));
            }
            return num;
        }

        /// <summary>
        ///   Gets a mandatory integer value.
        /// </summary>
        /// <param name="key">  The key.</param>
        /// <returns> The value.</returns>
        /// <exception cref="T:ExitGames.Configuration.ConfigurationException">
        ///     Key missing in collection.
        /// </exception>
        public int GetValueInteger(string key)
        {
            int num;
            if (!this.TryGetValueInteger(key, out num))
            {
                throw new ConfigurationException(string.Format("Parse Error: Value of '{0}' has wrong format. Integer expected.", key));
            }
            return num;
        }

        /// <summary>
        ///  Tries to find a string value.
        /// </summary>
        /// <param name="key">   The key.</param>
        /// <param name="value">    The result value.</param>
        /// <returns> True on succes, otherwise false.</returns>
        public bool TryGetValue(string key, out string value)
        {
            value = this.collection.Get(key);
            return (value != null);
        }

        /// <summary>
        ///  Tries to find a boolean value.
        /// </summary>
        /// <param name="key"> The key.</param>
        /// <param name="value"> The result value.</param>
        /// <returns> True on succes, otherwise false.</returns>
        /// <exception cref="T:ExitGames.Configuration.ConfigurationException">
        ///       Value parse error.
        /// </exception>
        public bool TryGetValueBoolean(string key, out bool value)
        {
            string str;
            if (this.TryGetValue(key, out str))
            {
                if (!bool.TryParse(str, out value))
                {
                    throw new ConfigurationException(string.Format("Parse Error: Value of '{0}' has wrong format. Boolean expected.", key));
                }
                return true;
            }
            value = false;
            return false;
        }

        /// <summary>
        /// Tries to find a byte value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value"> The result value.</param>
        /// <returns>True on succes, otherwise false.</returns>
        ///  <exception cref="T:ExitGames.Configuration.ConfigurationException">
        ///    Value parse error.
        ///  </exception>
        public bool TryGetValueByte(string key, out byte value)
        {
            string str;
            if (this.TryGetValue(key, out str))
            {
                if (!byte.TryParse(str, out value))
                {
                    throw new ConfigurationException(string.Format("Parse Error: Value of '{0}' has wrong format. Byte expected.", key));
                }
                return true;
            }
            value = 0;
            return false;
        }

        /// <summary>
        ///  Tries to find an integer value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value"> The result value.</param>
        /// <returns>True on succes, otherwise false.</returns>
        ///  <exception cref="T:ExitGames.Configuration.ConfigurationException">
        ///     Value parse error.
        ///  </exception>
        public bool TryGetValueInteger(string key, out int value)
        {
            string str;
            if (this.TryGetValue(key, out str))
            {
                if (!int.TryParse(str, out value))
                {
                    throw new ConfigurationException(string.Format("Parse Error: Value of '{0}' has wrong format. Integer expected.", key));
                }
                return true;
            }
            value = 0;
            return false;
        }

        /// <summary>
        /// Gets the collection.
        /// </summary>
        public NameValueCollection Collection
        {
            get
            {
                return this.collection;
            }
        }
    }
}
