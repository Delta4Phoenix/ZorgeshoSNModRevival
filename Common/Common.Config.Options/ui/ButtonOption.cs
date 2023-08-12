﻿using System;

using UnityEngine;
using UnityEngine.Events;

namespace Common.Configuration
{
	using Reflection;

	partial class Options
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class ButtonAttribute: Config.Field.LoadOnlyAttribute {}

		partial class Factory
		{
			class ButtonOptionCreator: ICreator
			{
				public ModOption create(Config.Field cfgField)
				{
					if (cfgField.type != typeof(int) || !cfgField.checkAttr<ButtonAttribute>()) // it's good enough for now
						return null;

					return new ButtonOption(cfgField, cfgField.getAttr<FieldAttribute>()?.label);
				}
			}
		}


		public class ButtonOption: ModOption
		{
			public ButtonOption(Config.Field cfgField, string label): base(cfgField, label) {}

			public override void AddOption(Options options)
			{
				// HACK: SMLHelper don't have button options yet, so we add toggle and then change it to button in onGameObjectChange
				options.AddToggleOption(id, "", false);
			}

			public override void OnValueChange(EventArgs e) {}

			public override void OnGameObjectChange(GameObject go)
			{
				UnityEngine.Object.DestroyImmediate(go);
				optionsPanel.AddButton(modsTabIndex, label, new UnityAction(onClick));

				var transform = modOptionsTab.container.transform;
				var newGO = transform.GetChild(transform.childCount - 1).gameObject;
				base.OnGameObjectChange(newGO);
			}

			void onClick()
			{
				cfgField.value = cfgField.value.cast<int>() + 1; // cfgField will run attached actions when we change its value

				UnityHelper.ClearSelectedUIObject();
			}
		}
	}
}