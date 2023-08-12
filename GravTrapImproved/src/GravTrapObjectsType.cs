using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using Common;
using Common.GameSerialization;

namespace GravTrapImproved
{
	class GravTrapObjectsType: MonoBehaviour, IProtoEventListener
	{
		static class Types
		{
			public static int ListCount { get; private set; }
			static List<TypesConfig.TechTypeList> TypeLists;

			public static string GetListName(int index) => TypeLists[index].name;
			public static int GetListIndex(string name) => TypeLists.FindIndex(list => list.name == name);

			public static bool Contains(int index, TechType techType) => TypeLists[index].Contains(techType);
			public static void Init(TypesConfig typesConfig)
			{
				ListCount = typesConfig.techTypeLists.Count;

				TypeLists = typesConfig.techTypeLists.Select(list => new TypesConfig.TechTypeList(list)).ToList();
				TypeLists.Insert(0, new TypesConfig.TechTypeList("ids_All"));

				for (int i = 1; i <= ListCount; i++)
				{
					if (!typesConfig.noJoin.Contains(TypeLists[i].name))
						TypeLists[0].Add(TypeLists[i]);

					L10n.Add(TypeLists[i].name, TypeLists[i].name);
				}

				// can't use events for that
				UnityHelper.FindObjectsOfTypeAll<GravTrapObjectsType>().ForEach(cmp => cmp.RefreshIndex());
			}
		}

		public static void Init(TypesConfig typesConfig) => Types.Init(typesConfig);

		class SaveData
		{
			public int TrapObjType { get; init; }
			public string TrapObjTypeListName { get; init; }
		}
		string id;

		public int TechTypeListIndex
		{
			get => _techTypeListIndex;
			set => _techTypeListIndex = MathUtils.Mod(value, Types.ListCount + 1);
		}
		int _techTypeListIndex = 0;

		public string TechTypeListName // for GUI
		{
			get
			{
				if (_cachedIndex != TechTypeListIndex)
				{
					listName = Types.GetListName(TechTypeListIndex);
					_cachedGUIString = L10n.Str("ids_objectsType") + L10n.Str(listName);
					_cachedIndex = TechTypeListIndex;
				}

				return _cachedGUIString;
			}
		}
		int _cachedIndex = -1;
		string _cachedGUIString = null;

		string listName = null; // for restoring selected list in case of changes

		int RestoreIndex(string listName, int listIndex)
		{
			// trying name first
			int index = Types.GetListIndex(listName);

			// if list not found by name, try to use index
			return index != -1? index: Mathf.Min(Types.ListCount, listIndex);
		}

		void RefreshIndex()
		{
			TechTypeListIndex = RestoreIndex(listName, TechTypeListIndex);
			_cachedIndex = -1;
		}

		public void OnProtoDeserialize(ProtobufSerializer serializer)
		{
			if (SaveLoad.Load<SaveData>(id) is SaveData save)
				TechTypeListIndex = RestoreIndex(save.TrapObjTypeListName, save.TrapObjType);
			else
				TechTypeListIndex = 0;
		}

		public void OnProtoSerialize(ProtobufSerializer serializer)
		{
			SaveLoad.Save(id, new SaveData { TrapObjType = TechTypeListIndex, TrapObjTypeListName = Types.GetListName(TechTypeListIndex) });
		}

		// we may add this component while gameobject is inactive (while in inventory) and Awake for it is not called
		// so we need initialize it that way
		public static GravTrapObjectsType GetFrom(GameObject go) =>
			go.GetComponent<GravTrapObjectsType>() ?? go.AddComponent<GravTrapObjectsType>().Init();

		void Awake() => Init();

		bool inited = false;
		GravTrapObjectsType Init()
		{
			if (!inited && (inited = true))
			{
				id = GetComponent<PrefabIdentifier>().Id;
				OnProtoDeserialize(null);
			}

			return this;
		}

		public void HandleAttracted(GameObject obj, bool added)
		{
			if (added)
			{
				if (obj.TryGetComponent<Crash>(out var crash))
				{
					crash.AttackLastTarget(); // if target object is CrashFish we want to pull it out
				}
				else if (obj.TryGetComponent<SinkingGroundChunk>(out var chunk))
				{
					Destroy(chunk);

					var c = obj.AddComponent<BoxCollider>();
					c.size = new Vector3(0.736f, 0.51f, 0.564f);
					c.center = new Vector3(0.076f, 0.224f, 0.012f);

					obj.transform.Find("models").localPosition = Vector3.zero;
				}
			}
#if GAME_SN
			if (GetComponent<GravTrapMK2.Tag>() && obj.TryGetComponent<GasPod>(out var gasPod))
			{
				gasPod.grabbedByPropCannon = added;

				if (!added)
					gasPod.PrepareDetonationTime();
			}
#endif
		}

		TechType GetObjectTechType(GameObject obj)
		{
#if GAME_SN
			if (obj.GetComponentInParent<SinkingGroundChunk>() || obj.name.Contains("TreaderShale"))
				return TechType.ShaleChunk;

			if (obj.TryGetComponent<GasPod>(out var gasPod))
				return gasPod.detonated? TechType.None: TechType.GasPod;
#endif
			if (obj.TryGetComponent<Pickupable>(out var p))
				return p.GetTechType();

			return CraftData.GetTechType(obj);
		}

		public bool IsValidTarget(GameObject obj) // ! called on each frame for each attracted object
		{
			if (obj.GetComponent<Pickupable>()?.attached == true)
				return false;

			return Types.Contains(TechTypeListIndex, GetObjectTechType(obj));
		}
	}
}