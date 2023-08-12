using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

#if GAME_SN && BRANCH_STABLE
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;
using Oculus.Newtonsoft.Json.Serialization;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
#endif

namespace Common.Configuration
{
	using Reflection;

	partial class Config
	{
		public partial class Field
		{
			// field with this attribute will not be saved (will be removed during save from json file)
			[AttributeUsage(AttributeTargets.Field)]
			public class LoadOnlyAttribute: Attribute {}
		}

		public class SerializerSettingsAttribute: Attribute
		{
			public bool VerboseErrors { get; init; }
			public bool IgnoreNullValues { get; init; }
			public bool IgnoreDefaultValues { get; init; }

			public Type[] Converters { get; init; }
		}

		class ConfigContractResolver: DefaultContractResolver
		{
			// serialize only fields (including private and readonly, except static and with NonSerialized attribute)
			// don't serialize properties
			protected override List<MemberInfo> GetSerializableMembers(Type objectType) =>
				objectType.fields().Where(field => !field.IsStatic && !field.CheckAttr<NonSerializedAttribute>()).Cast<MemberInfo>().ToList();

			// we can deserialize all members and serialize members without LoadOnly attribute
			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var property = base.CreateProperty(member, memberSerialization);

				property.Writable = true;
				property.Readable = !member.CheckAttr<Field.LoadOnlyAttribute>();

				return property;
			}
		}


		static JsonSerializerSettings DinitSerializer(Type configType)
		{
			JsonSerializerSettings settings = new()
			{
				Formatting = Formatting.Indented,
				ContractResolver = new ConfigContractResolver(),
				ObjectCreationHandling = ObjectCreationHandling.Replace,
				Converters = { new StringEnumConverter() }
			};

			if (configType.getAttr<SerializerSettingsAttribute>() is SerializerSettingsAttribute settingsAttr)
			{
				if (settingsAttr.IgnoreNullValues)	  settings.NullValueHandling = NullValueHandling.Ignore;
				if (settingsAttr.IgnoreDefaultValues) settings.DefaultValueHandling = DefaultValueHandling.Ignore;

				if (settingsAttr.VerboseErrors)
					settings.Error = (_, args) => $"<color=red>{args.ErrorContext.Error.Message}</color>".OnScreen(); // TODO make more general

				if (settingsAttr.Converters != null)
				{
					foreach (var type in settingsAttr.Converters)
					{
						Debug.assert(typeof(JsonConverter).IsAssignableFrom(type));
						settings.Converters.Add(Activator.CreateInstance(type) as JsonConverter);
					}
				}
			}

			return settings;
		}

		JsonSerializerSettings srzSettings;

		string Serialize() => JsonConvert.SerializeObject(this, srzSettings ??= DinitSerializer(GetType()));

		static Config Deserialize(string text, Type configType) => JsonConvert.DeserializeObject(text, configType, DinitSerializer(configType)) as Config;
	}
}