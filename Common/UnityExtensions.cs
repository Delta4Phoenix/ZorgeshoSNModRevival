using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Common
{
	using Reflection;
	using Object = UnityEngine.Object;

	static class ObjectAndComponentExtensions
	{
		public static void CallAfterDelay(this GameObject go, float delay, UnityAction action) =>
			go.AddComponent<CallAfterDelay>().Init(delay, action);

		public static C EnsureComponent<C>(this GameObject go) where C: Component => go.EnsureComponent(typeof(C)) as C;
		public static Component EnsureComponent(this GameObject go, Type type) => go.GetComponent(type) ?? go.AddComponent(type);

		public static GameObject GetParent(this GameObject go) => go.transform.parent?.gameObject;
		public static GameObject GetChild(this GameObject go, string name) => go.transform.Find(name)?.gameObject;

		public static void SetTransform(this GameObject go, Vector3? pos = null, Vector3? localPos = null, Vector3? localAngles = null, Vector3? localScale = null)
		{
			var tr = go.transform;

			if (pos != null)			tr.position = (Vector3)pos;
			if (localPos != null)		tr.localPosition = (Vector3)localPos;
			if (localAngles != null)	tr.localEulerAngles = (Vector3)localAngles;
			if (localScale != null)		tr.localScale = (Vector3)localScale;
		}

		public static void SetParent(this GameObject go, GameObject parent, Vector3? localPos = null, Vector3? localAngles = null)
		{
			go.transform.SetParent(parent.transform, false);
			go.SetTransform(localPos: localPos, localAngles: localAngles);
		}

		public static GameObject CreateChild(this GameObject go, string name, Vector3? localPos = null)
		{
			GameObject child = new (name);
			child.SetParent(go);

			child.SetTransform(localPos: localPos);

			return child;
		}

		public static GameObject CreateChild(this GameObject go, GameObject prefab, string name = null,
											 Vector3? localPos = null, Vector3? localAngles = null, Vector3? localScale = null)
		{
			var child = Object.Instantiate(prefab, go.transform);

			if (name != null)
				child.name = name;

			child.SetTransform(localPos: localPos, localAngles: localAngles, localScale: localScale);

			return child;
		}

		// for use with inactive game objects
		public static C GetComponentInParent<C>(this GameObject go) where C: Component
		{
			return _get<C>(go);

			static _C _get<_C>(GameObject _go) where _C: Component => !_go? null: (_go.GetComponent<_C>() ?? _get<_C>(_go.GetParent()));
		}


		static void Udestroy(this Object obj, bool immediate)
		{
			if (immediate)
				Object.DestroyImmediate(obj);
			else
				Object.Destroy(obj);
		}

		public static void DestroyChild(this GameObject go, string name, bool immediate = true) =>
			go.GetChild(name)?.Udestroy(immediate);

		public static void DestroyChildren(this GameObject go, params string[] children) =>
			children.ForEach(name => go.DestroyChild(name, true));

		public static void DestroyComponent(this GameObject go, Type componentType, bool immediate = true) =>
			go.GetComponent(componentType)?.Udestroy(immediate);

		public static void DestroyComponent<C>(this GameObject go, bool immediate = true) where C: Component =>
			DestroyComponent(go, typeof(C), immediate);

		public static void DestroyComponentInChildren<C>(this GameObject go, bool immediate = true) where C: Component =>
			go.GetComponentInChildren<C>()?.Udestroy(immediate);


		// if fields is empty we try to copy all fields
		public static void CopyFieldsFrom<CT, CF>(this CT cmpTo, CF cmpFrom, params string[] fieldNames) where CT: Component where CF: Component
		{
			try
			{
				Type typeTo = cmpTo.GetType(), typeFrom = cmpFrom.GetType();

				foreach (var fieldTo in fieldNames.Length == 0? typeTo.fields(): fieldNames.Select(name => typeTo.field(name)))
				{
					if (typeFrom.field(fieldTo.Name) is FieldInfo fieldFrom)
					{																										$"copyFieldsFrom: copying field {fieldTo.Name} from {cmpFrom} to {cmpTo}".logDbg();
						fieldTo.SetValue(cmpTo, fieldFrom.GetValue(cmpFrom));
					}
				}
			}
			catch (Exception e) { Log.msg(e); }
		}
	}


	static class StructsExtension
	{
		public static Vector2 SetX(this Vector2 vec, float val) { vec.x = val; return vec; }
		public static Vector2 SetY(this Vector2 vec, float val) { vec.y = val; return vec; }

		public static Vector3 SetX(this Vector3 vec, float val) { vec.x = val; return vec; }
		public static Vector3 SetY(this Vector3 vec, float val) { vec.y = val; return vec; }
		public static Vector3 SetZ(this Vector3 vec, float val) { vec.z = val; return vec; }

		public static Color SetA(this Color color, float val) { color.a = val; return color; }
	}


	static class UnityHelper
	{
		public static GameObject CreatePersistentGameObject(string name)
		{																													$"UnityHelper.createPersistentGameObject: creating '{name}'".logDbg();
			GameObject obj = new (name, typeof(SceneCleanerPreserve));
			Object.DontDestroyOnLoad(obj);
			return obj;
		}

		public static GameObject CreatePersistentGameObject<C>(string name) where C: Component
		{
			var obj = CreatePersistentGameObject(name);
			obj.AddComponent<C>();
			return obj;
		}

		class CoroutineHost: MonoBehaviour { public static CoroutineHost main; }

		public static Coroutine StartCoroutine(IEnumerator coroutine)
		{
			CoroutineHost.main ??= CreatePersistentGameObject("CoroutineHost").AddComponent<CoroutineHost>();
			return CoroutineHost.main.StartCoroutine(coroutine);
		}

		// includes inactive objects
		// for use in non-performance critical code
		public static List<T> FindObjectsOfTypeAll<T>() where T: Behaviour
		{
			var list = Enumerable.Range(0, SceneManager.sceneCount).
				Select(i => SceneManager.GetSceneAt(i)).
				Where(s => s.isLoaded).
				SelectMany(s => s.GetRootGameObjects()).
				SelectMany(go => go.GetComponentsInChildren<T>(true)).
				ToList();

			$"FindObjectsOfTypeAll({typeof(T)}) result => all: {list.Count}, active: {list.Where(c => c.isActiveAndEnabled).Count()}".logDbg();
			return list;
		}

		// using reflection to avoid including UnityEngine.UI in all projects
		static readonly Type eventSystem = Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI");
		static readonly PropertyWrapper currentEventSystem = eventSystem.property("current").wrap();
		static readonly MethodWrapper setSelectedGameObject = eventSystem.method("SetSelectedGameObject", typeof(GameObject)).wrap();

		// unselects currently selected object (needed for buttons)
		public static void ClearSelectedUIObject() =>
			setSelectedGameObject.invoke(currentEventSystem.get(), null);

		// for use in non-performance critical code
		public static C FindNearest<C>(Vector3? pos, out float distance, Predicate<C> condition = null) where C: Component
		{
			using var _ = Debug.DProfiler($"UnityHelper.findNearest({typeof(C).Name})");

			distance = float.MaxValue;

			if (pos == null)
				return null;

			C result = null;
			Vector3 validPos = (Vector3)pos;

			foreach (var c in Object.FindObjectsOfType<C>())
			{
				if (condition != null && !condition(c))
					continue;

				float distSq = (c.transform.position - validPos).sqrMagnitude;

				if (distSq < distance)
				{
					distance = distSq;
					result = c;
				}
			}

			if (distance < float.MaxValue)
				distance = Mathf.Sqrt(distance);

			return result;
		}
	}


	static class InputHelper
	{
		public static int GetMouseWheelDir() => Math.Sign(GetMouseWheelValue());
		public static float GetMouseWheelValue() => getAxis? getAxis.invoke("Mouse ScrollWheel"): 0f;

		static readonly MethodWrapper<Func<string, float>> getAxis =
			Type.GetType("UnityEngine.Input, UnityEngine.InputLegacyModule")?.method("GetAxis")?.wrap<Func<string, float>>();
	}


	class CallAfterDelay: MonoBehaviour
	{
		float delay;
		UnityAction action;

		public void Init(float delay, UnityAction action)
		{
			this.delay = delay;
			this.action = action;
		}

		IEnumerator Start()
		{
			yield return new WaitForSeconds(delay);

			action();
			Destroy(this);
		}
	}
}