using System;
using System.Collections.Generic;

using UnityEngine;

namespace Common.Configuration
{
	partial class Options
	{
		public static bool nonUniqueOptionsIDsWarning = true;

		public abstract class ModOption
		{
			static readonly UniqueIDs uniqueIDs = new();

			public interface IOnGameObjectChangeHandler
			{
				void Init(ModOption option);
				void Handle(GameObject gameObject);
			}

			readonly List<IOnGameObjectChangeHandler> handlers = new();

			public void AddHandler
				(IOnGameObjectChangeHandler handler)
			{
				handler.Init(this);
				handlers.Add(handler);
			}

			public readonly string id;
			protected readonly string label;

			public GameObject GameObject { get; protected set; }
			public readonly Config.Field cfgField;

			public ModOption(Config.Field cfgField, string label)
			{
				this.cfgField = cfgField;

				id = cfgField.id;
				uniqueIDs.EnsureUniqueID(ref id, nonUniqueOptionsIDsWarning);

				this.label = label ?? id.ClampLength(40);
				registerLabel(id, ref this.label);
			}

			public abstract void AddOption(Options options);

			public abstract void OnValueChange(EventArgs e);

			public virtual void OnGameObjectChange(GameObject go)
			{
				GameObject = go;
				handlers.ForEach(h => h.Handle(GameObject));
			}

			public void OnRemove() => uniqueIDs.FreeID(id);
		}
	}
}