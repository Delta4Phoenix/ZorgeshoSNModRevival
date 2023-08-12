using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

#if GAME_SN && BRANCH_STABLE
using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Serialization;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
#endif

namespace Common.GameSerialization
{
	using Reflection;

	static class SaveLoad
	{
		class SaveContractResolver: DefaultContractResolver
		{
			// serialize only fields (including private and readonly, except static and with NonSerialized attribute)
			// don't serialize properties
			protected override List<MemberInfo> GetSerializableMembers(Type objectType) =>
				objectType.fields().Where(field => !field.IsStatic && !field.CheckAttr<NonSerializedAttribute>()).ToList<MemberInfo>();

			// we can deserialize/serialize all members
			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				var property = base.CreateProperty(member, memberSerialization);
				property.Writable = property.Readable = true;

				return property;
			}
		}

		static readonly JsonSerializerSettings srzSettings = new()
		{
			Formatting = Mod.Consts.isDevBuild? Formatting.Indented: Formatting.None,
			ContractResolver = new SaveContractResolver()
		};

		public static void Save<T>(string id, T saveData)
		{
			using var _ = Debug.DProfiler("SaveLoad.save");

			File.WriteAllText(GetPath(id), JsonConvert.SerializeObject(saveData, srzSettings));
		}

		public static T Load<T>(string id)
		{
			using var _ = Debug.DProfiler("SaveLoad.load");

			string filePath = GetPath(id);
			return File.Exists(filePath)? JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), srzSettings): default;
		}

		public static bool Load<T>(string id, out T saveData)
		{
			saveData = Load<T>(id);
			return !saveData.Equals(default);
		}

		static string GetPath(string id) => Path.Combine(Paths.savesPath, id + ".json");
	}
}