using System;
using System.Net;

namespace BungieNet.Api
{
    internal struct QueryStringItem
    {
        public QueryStringItem(string name, string value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }


        public string Name { get; }

        public string Value { get; }


        #region Object members

        public override int GetHashCode()
        {
            return ((Name?.GetHashCode() ?? 0) << 5) ^ (Value?.GetHashCode() ?? 0);
        }

        public override bool Equals(object obj)
        {
            return obj is QueryStringItem && Equals(obj);
        }

        public override string ToString()
        {
            return $"{WebUtility.UrlEncode(Name)}={WebUtility.UrlEncode(Value)}";
        }

        #endregion


        #region IEquatable<QueryStringItem> members

        public bool Equals(QueryStringItem other)
        {
            return Name == other.Name && Value == other.Value;
        }

        #endregion
    }
}