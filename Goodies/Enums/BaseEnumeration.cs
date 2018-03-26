using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace MedZiane.Goodies.Enums
{
    /// <summary>
    /// An abstract base enumeration that implements comparison and equality.
    /// Source: https://github.com/henrikfroehling/TraktApiSharp/blob/next-version/Source/Lib/TraktApiSharp/Enums/TraktEnumeration.cs
    /// </summary>
    public abstract class BaseEnumeration : IComparable<BaseEnumeration>, IEquatable<BaseEnumeration>
    {
        /// <summary>
        /// The display name assigned to the unknown value.
        /// </summary>
        protected const string DisplayNameUnknown = "Unknown";

        /// <summary>
        /// Creates a BaseEnumeration
        /// </summary>
        protected BaseEnumeration()
            : this(0, string.Empty, string.Empty, DisplayNameUnknown)
        {
        }

        /// <summary>
        /// Creates a BaseEnumeration object.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="objectName"></param>
        /// <param name="uriName"></param>
        /// <param name="displayName"></param>
        protected BaseEnumeration(int value, string objectName, string uriName, string displayName)
        {
            Value = value;
            ObjectName = objectName;
            UriName = uriName;
            DisplayName = displayName;
        }

        /// <summary>
        /// Gets the numeric value of the enumeration.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Gets the enumeration name for object properties.
        /// </summary>
        public string ObjectName { get; }

        /// <summary>
        /// Gets the enumeration name for URI path parameters.
        /// </summary>
        public string UriName { get; }

        /// <summary>
        /// Gets the human readable name of the enumeration.
        /// See also <seealso cref="ToString()" />.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Returns the human readable name of the enumeration.
        /// See also <seealso cref="DisplayName" />.
        /// </summary>
        public override string ToString() 
            => DisplayName;

        /// <summary>
        /// Compares the <see cref="Value" /> of this enumeration with the value of another enumeration.
        /// </summary>
        /// <param name="other">The other enumeration to compare with.</param>
        /// <returns>An indication of their relative values.</returns>
        public int CompareTo(BaseEnumeration other) 
            => Value.CompareTo(other.Value);

        /// <summary>Returns, whether this enumeration instance is equal to another enumeration instance.</summary>
        /// <param name="other">The other enumeration instance to compare with.</param>
        /// <returns>True if both enumeration instances are equal, otherwise false.</returns>
        public bool Equals(BaseEnumeration other)
        {
            if (other == null)
                return false;

            var typeMatches = GetType().Equals(other.GetType());
            var valueMatches = Value.Equals(other.Value);

            return typeMatches && valueMatches;
        }

        /// <summary>Returns the hash code of this enumeration.</summary>
        /// <returns>An hash code of this enumeration.</returns>
        public override int GetHashCode() => Value.GetHashCode();

        /// <summary>Returns a list of all enumerations of an enumeration of type T.</summary>
        /// <typeparam name="T">The enumeration, of which a list of all enumerations should be returned.</typeparam>
        /// <returns>A list of all enumerations of an enumeration of type T.</returns>
        public static IEnumerable<T> GetAll<T>() where T : BaseEnumeration, new()
        {
            var derivedEnumType = typeof(T);
            var derivedEnum = Activator.CreateInstance(derivedEnumType) as T;
            var fieldInfos = derivedEnumType.GetRuntimeFields().Where(f => f.FieldType == derivedEnumType && f.IsStatic && f.IsInitOnly);

            foreach (var field in fieldInfos)
            {
                if (field.GetValue(derivedEnum) is T value)
                    yield return value;
            }
        }

        /// <summary>Calculates the absolute difference of the <see cref="Value" /> for two enumerations.</summary>
        /// <param name="first">The first enumeration.</param>
        /// <param name="second">The second enumeration.</param>
        /// <returns>The absolute difference of the <see cref="Value" /> for two enumerations.</returns>
        public static int AbsoluteDifference(BaseEnumeration first, BaseEnumeration second) 
            => Math.Abs(first.Value - second.Value);

        /// <summary>Creates an enumeration of type T from the given value.</summary>
        /// <typeparam name="T">The type of the enumeration, which should be created.</typeparam>
        /// <param name="value">The value from which the enumeration should be created.</param>
        /// <returns>
        /// An enumeration of type T or null, if the value could not be found
        /// in the available values for the enumeration.
        /// </returns>
        public static T FromValue<T>(int value) where T : BaseEnumeration, new() 
            => Search<T>(e => e.Value == value);

        /// <summary>Creates an enumeration of type T from the given object name.</summary>
        /// <typeparam name="T">The type of the enumeration, which should be created.</typeparam>
        /// <param name="objectName">The object name from which the enumeration should be created.</param>
        /// <returns>
        /// An enumeration of type T or null, if the value could not be found
        /// in the available values for the enumeration.
        /// </returns>
        public static T FromObjectName<T>(string objectName) where T : BaseEnumeration, new() 
            => Search<T>(e => e.ObjectName == objectName);

        /// <summary>Creates an enumeration of type T from the given URI name.</summary>
        /// <typeparam name="T">The type of the enumeration, which should be created.</typeparam>
        /// <param name="uriName">The URI name from which the enumeration should be created.</param>
        /// <returns>
        /// An enumeration of type T or null, if the value could not be found
        /// in the available values for the enumeration.
        /// </returns>
        public static T FromUriName<T>(string uriName) where T : BaseEnumeration, new() 
            => Search<T>(e => e.UriName == uriName);

        /// <summary>Creates an enumeration of type T from the given display name.</summary>
        /// <typeparam name="T">The type of the enumeration, which should be created.</typeparam>
        /// <param name="displayName">The display name from which the enumeration should be created.</param>
        /// <returns>
        /// An enumeration of type T, or null if the value could not be found in the available values for the enumeration.
        /// </returns>
        public static T FromDisplayName<T>(string displayName) where T : BaseEnumeration, new() 
            => Search<T>(e => e.DisplayName == displayName);

        private static T Search<T>(Func<BaseEnumeration, bool> predicate) where T : BaseEnumeration, new()
        {
            var matchingItem = GetAll<T>().FirstOrDefault(predicate);
            return matchingItem as T;
        }
    }

    /// <summary>
    /// Converts an enumeration of type T to and from JSON.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EnumerationConverter<T> : JsonConverter where T : BaseEnumeration, new()
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType) 
            => objectType == typeof(string);

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;

            var enumString = reader.Value as string;

            return string.IsNullOrEmpty(enumString) 
                ? Activator.CreateInstance(typeof(T)) 
                : BaseEnumeration.FromObjectName<T>(enumString);
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var enumeration = (T) value;
            writer.WriteValue(enumeration.ObjectName);
        }
    }
}