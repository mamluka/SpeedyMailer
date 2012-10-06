using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.IO;
using Nancy.ModelBinding;
using Newtonsoft.Json;

namespace SpeedyMailer.Core.Container
{
	public class NancyJsonNetSerializer : ISerializer
	{
		private readonly JsonSerializer _serializer = new JsonSerializer();

		/// <summary>
		/// Initializes a new instance of the <see cref="NancyJsonNetSerializer"/> class.
		/// </summary>
		public NancyJsonNetSerializer()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NancyJsonNetSerializer"/> class,
		/// with the provided <paramref name="converters"/>.
		/// </summary>
		/// <param name="converters">Json converters used when serializing.</param>
		public NancyJsonNetSerializer(IEnumerable<JsonConverter> converters)
		{
			foreach (var converter in converters)
			{
				_serializer.Converters.Add(converter);
			}
		}

		/// <summary>
		/// Whether the serializer can serialize the content type
		/// </summary>
		/// <param name="contentType">Content type to serialise</param>
		/// <returns>True if supported, false otherwise</returns>
		public bool CanSerialize(string contentType)
		{
			return Helpers.IsJsonType(contentType);
		}

		/// <summary>
		/// Gets the list of extensions that the serializer can handle.
		/// </summary>
		/// <value>An <see cref="IEnumerable{T}"/> of extensions if any are available, otherwise an empty enumerable.</value>
		public IEnumerable<string> Extensions
		{
			get { yield return "json"; }
		}

		/// <summary>
		/// Serialize the given model with the given contentType
		/// </summary>
		/// <param name="contentType">Content type to serialize into</param>
		/// <param name="model">Model to serialize</param>
		/// <param name="outputStream">Output stream to serialize to</param>
		/// <returns>Serialised object</returns>
		public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
		{
			using (var writer = new JsonTextWriter(new StreamWriter(new UnclosableStreamWrapper(outputStream))))
			{
				_serializer.Serialize(writer, model);
			}
		}
	}

	public class JsonNetBodyDeserializer : IBodyDeserializer
	{
		private readonly JsonSerializer serializer = new JsonSerializer();

		/// <summary>
		/// Empty constructor if no converters are needed
		/// </summary>
		public JsonNetBodyDeserializer()
		{
		}

		/// <summary>
		/// Constructor to use when json converters are needed.
		/// </summary>
		/// <param name="converters">Json converters used when deserializing.</param>
		public JsonNetBodyDeserializer(IEnumerable<JsonConverter> converters)
		{
			foreach (var converter in converters)
			{
				this.serializer.Converters.Add(converter);
			}
		}

		/// <summary>
		/// Whether the deserializer can deserialize the content type
		/// </summary>
		/// <param name="contentType">Content type to deserialize</param>
		/// <returns>True if supported, false otherwise</returns>
		public bool CanDeserialize(string contentType)
		{
			return Helpers.IsJsonType(contentType);
		}

		/// <summary>
		/// Deserialize the request body to a model
		/// </summary>
		/// <param name="contentType">Content type to deserialize</param>
		/// <param name="bodyStream">Request body stream</param>
		/// <param name="context">Current context</param>
		/// <returns>Model instance</returns>
		public object Deserialize(string contentType, Stream bodyStream, BindingContext context)
		{
			var deserializedObject =
				this.serializer.Deserialize(new StreamReader(bodyStream), context.DestinationType);

			if (context.DestinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Except(context.ValidModelProperties).Any())
			{
				return CreateObjectWithBlacklistExcluded(context, deserializedObject);
			}

			return deserializedObject;
		}

		private static object CreateObjectWithBlacklistExcluded(BindingContext context, object deserializedObject)
		{
			var returnObject = Activator.CreateInstance(context.DestinationType);

			foreach (var property in context.ValidModelProperties)
			{
				CopyPropertyValue(property, deserializedObject, returnObject);
			}

			return returnObject;
		}

		private static void CopyPropertyValue(PropertyInfo property, object sourceObject, object destinationObject)
		{
			property.SetValue(destinationObject, property.GetValue(sourceObject, null), null);
		}
	}

	internal static class Helpers
	{
		/// <summary>
		/// Attempts to detect if the content type is JSON.
		/// Supports:
		///   application/json
		///   text/json
		///   application/vnd[something]+json
		/// Matches are case insentitive to try and be as "accepting" as possible.
		/// </summary>
		/// <param name="contentType">Request content type</param>
		/// <returns>True if content type is JSON, false otherwise</returns>
		public static bool IsJsonType(string contentType)
		{
			if (string.IsNullOrEmpty(contentType))
			{
				return false;
			}

			var contentMimeType = contentType.Split(';')[0];

			return contentMimeType.Equals("application/json", StringComparison.InvariantCultureIgnoreCase) ||
				   contentMimeType.Equals("text/json", StringComparison.InvariantCultureIgnoreCase) ||
				  (contentMimeType.StartsWith("application/vnd", StringComparison.InvariantCultureIgnoreCase) &&
				   contentMimeType.EndsWith("+json", StringComparison.InvariantCultureIgnoreCase));
		}
	}

	public class DemoBootstrapper : DefaultNancyBootstrapper
	{
		protected override NancyInternalConfiguration InternalConfiguration
		{
			get
			{
				// Insert at position 0 so it takes precedence over the built in one.
				return NancyInternalConfiguration.WithOverrides(
						c => c.Serializers.Insert(0, typeof(NancyJsonNetSerializer)));
			}
		}
	}
}
