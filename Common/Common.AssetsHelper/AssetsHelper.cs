using System;
using System.IO;
using System.Linq;

using UnityEngine;

namespace Common
{
	using Reflection;

	static class AssetsHelper
	{
		const string assetsExt = ".assets";

		public static Sprite LoadSprite(string textureName) => TextureToSprite(LoadTexture(textureName));
		public static Sprite LoadSprite(string textureName, float pixelsPerUnit, float border) => TextureToSprite(LoadTexture(textureName), pixelsPerUnit, border);

		public static Texture2D LoadTexture(string textureName)
		{
			return LoadAsset<Texture2D>(textureName) ??
				   LoadTextureFromFile(Paths.assetsPath + textureName) ??
				   LoadTextureFromFile(Paths.modRootPath + textureName) ??
				   LoadTextureFromFile(textureName);
		}

		public static GameObject LoadPrefab(string prefabName) => LoadAsset<GameObject>(prefabName);


		// bundle should be placed and named like this - '{ModRoot}\assets\{ModID}.assets'
		static readonly object assetBundle;

		static AssetsHelper()
		{
			string bundlePath = Paths.assetsPath + Mod.id + assetsExt;

			if (File.Exists(bundlePath))
			{
				MethodWrapper loadBundle = Type.GetType("UnityEngine.AssetBundle, UnityEngine.AssetBundleModule").method("LoadFromFile", typeof(string)).wrap();
				assetBundle = loadBundle.invoke(bundlePath);
			}
		}

		static MethodWrapper _loadAsset;

		static T LoadAsset<T>(string name) where T: UnityEngine.Object
		{																								$"AssetHelper: trying to load asset '{name}' ({typeof(T)}) from bundle".logDbg();
			if (assetBundle == null)
				return null;

			_loadAsset ??= Type.GetType("UnityEngine.AssetBundle, UnityEngine.AssetBundleModule").method("LoadAsset", typeof(string), typeof(Type)).wrap();
			return _loadAsset.invoke(assetBundle, name, typeof(T)) as T;
		}

		static Sprite TextureToSprite(Texture2D tex) =>
			tex == null? null: Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));

		static Sprite TextureToSprite(Texture2D tex, float pixelsPerUnit, float border) =>
			tex == null? null: Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.Tight, new Vector4(border, border, border, border));

		static Texture2D LoadTextureFromFile(string textureFilePath)
		{																								$"AssetHelper: trying to load texture from file '{textureFilePath}'".logDbg();
			if (!Path.HasExtension(textureFilePath))
			{
				var dir = Path.GetDirectoryName(textureFilePath);

				if (!Directory.Exists(dir))
					return null;

				textureFilePath = Directory.GetFiles(dir, Path.GetFileName(textureFilePath) + ".*").FirstOrDefault(path => Path.GetExtension(path) != assetsExt);
			}

			if (!File.Exists(textureFilePath))
				return null;

			Texture2D tex = new (2, 2);
			return ImageConversion.LoadImage(tex, File.ReadAllBytes(textureFilePath))? tex: null;
		}
	}
}