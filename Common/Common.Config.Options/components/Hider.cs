﻿using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Common.Configuration
{
	partial class Options
	{
		partial class Factory
		{
			class HideableModifier: IModifier
			{
				public void process(ModOption option)
				{
					if (option.cfgField.getAttr<HideableAttribute>(true) is not HideableAttribute hideableAttr)
						return;

					string groupID = hideableAttr.groupID;

					if (groupID == null)
						option.cfgField.getAttrs<HideableAttribute>(true).ForEach(attr => groupID ??= attr.groupID);

					option.AddHandler(new Components.Hider.Add(hideableAttr.visChecker, groupID));
				}
			}
		}


		public static partial class Components
		{
			// component for hiding options elements
			// we need separate component for this to avoid conflicts with toggleable option's headings
			public class Hider: MonoBehaviour
			{
				public interface IVisibilityChecker { bool visible { get; } }

				// for use with class targeted Hideable attribute
				public class Ignore: IVisibilityChecker
				{
					public bool visible => true;
				}

				public class Simple: Config.Field.IAction, IVisibilityChecker
				{
					readonly string groupID;
					readonly Func<bool> visChecker;

					public Simple(string groupID, Func<bool> visChecker)
					{
						this.groupID = groupID;
						this.visChecker = visChecker;
					}

					public bool visible => visChecker();
					public void action() => setVisible(groupID, visible);
				}

				public class Add: ModOption.IOnGameObjectChangeHandler
				{
					string id;
					readonly string groupID;
					readonly IVisibilityChecker visChecker;

					public void Init(ModOption option) => id = option.id;

					public Add(IVisibilityChecker visChecker, string groupID = null)
					{
						this.visChecker = visChecker;
						this.groupID = groupID;
					}

					public void Handle(GameObject gameObject) =>
						gameObject.AddComponent<Hider>().init(id, groupID, visChecker);
				}

				string id, groupID;
				IVisibilityChecker visChecker;

				bool visible = true;

				void init(string id, string groupID, IVisibilityChecker visChecker)
				{
					this.id = id;
					this.groupID = groupID;
					this.visChecker = visChecker;
				}

				static readonly List<Hider> hiders = new();

				static IEnumerable<Hider> getHiders(string id)
				{
					hiders.RemoveAll(cmp => cmp == null); // cleaning up deleted objects
					return hiders.Where(cmp => cmp.id == id || cmp.groupID == id);
				}

				public static void refresh(string id) => getHiders(id).ForEach(cmp => cmp.refresh());
				public static void setVisible(string id, bool val) => getHiders(id).ForEach(cmp => cmp.setVisible(val));

				public void refresh() => setVisible(visChecker.visible);
				public void setVisible(bool val) => gameObject.SetActive(visible = val);

				Hider(): base() => hiders.Add(this); // we can't use Awake and OnDestroy here (hiders can be added to inactive objects)

				void OnEnable()
				{
					if (!visible || !visChecker.visible)
						setVisible(false);
				}
			}
		}
	}
}