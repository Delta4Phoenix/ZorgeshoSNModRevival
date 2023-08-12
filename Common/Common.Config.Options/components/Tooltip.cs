using System;
using UnityEngine;

#if GAME_SN
using System.Collections.Generic;
#endif

namespace Common.Configuration
{
	partial class Options
	{
		partial class Factory
		{
			class TooltipModifier: IModifier
			{
				public void process(ModOption option)
				{
					if (option.cfgField.getAttr<FieldAttribute>() is not FieldAttribute fieldAttr)
						return;

					if (fieldAttr.tooltipType != null || fieldAttr.tooltip != null)
						option.AddHandler(new Components.XTooltip.Add(fieldAttr.tooltipType, fieldAttr.tooltip));
				}
			}

			class HeadingTooltipModifier: IModifier
			{
				static bool processed = false;

				public void process(ModOption option)
				{
					if (processed || !(processed = true)) // process only the first added option
						return;

					Debug.assert(instance == null); // if this the first option, ModOptions shouldn't be created yet

					if (option.cfgField.getAttr<NameAttribute>(true) is not NameAttribute nameAttr)
						return;

					if (nameAttr.tooltipType != null || nameAttr.tooltip != null)
						option.AddHandler(new Components.XTooltip.AddToHeading(nameAttr.tooltipType, nameAttr.tooltip));
				}
			}
		}


		public static partial class Components
		{
			#region base tooltip
			public class XTooltip: MonoBehaviour, ITooltip
			{
				public class Add: ModOption.IOnGameObjectChangeHandler
				{
					string tooltip;
					readonly Type tooltipCmpType;
					readonly bool localizeAllow; // is it needed to add tooltip string to LanguageHandler

					static readonly UniqueIDs uniqueIDs = new();

					ModOption parentOption;

					public Add(string tooltip, bool localizeAllow = true)
					{
						this.tooltip = tooltip;
						this.localizeAllow = localizeAllow;
					}
					public Add(Type tooltipCmpType, string tooltip, bool localizeAllow = true): this(tooltip, localizeAllow)
					{
						this.tooltipCmpType = tooltipCmpType;

						Debug.assert(tooltipCmpType == null || typeof(XTooltip).IsAssignableFrom(tooltipCmpType),
							$"Tooltip type {tooltipCmpType} is not derived from Options.Components.Tooltip");
					}

					public void Init(ModOption option)
					{
						parentOption = option;

						if (tooltip == null)
							return;
#if GAME_SN
						// adjust text size for default tooltip (before we registering string with LanguageHelper)
						if (tooltipCmpType == null)
							tooltip = $"<size=14>" + tooltip + "</size>";
#endif
						if (localizeAllow)
						{
							string stringID = option.id + ".tooltip";
							uniqueIDs.EnsureUniqueID(ref stringID); // in case we add more than one tooltip to the option (e.g. for heading)

							registerLabel(stringID, ref tooltip, false);
						}
					}

					protected virtual GameObject GetTargetGameObject(GameObject optionGameObject) => optionGameObject;

					public void Handle(GameObject gameObject)
					{
						GameObject targetGameObject = GetTargetGameObject(gameObject);

						// using TranslationLiveUpdate component instead of Text (same result in this case and we don't need to add reference to Unity UI)
						GameObject caption = targetGameObject.GetComponentInChildren<TranslationLiveUpdate>().gameObject;

						Type cmpType = tooltipCmpType ?? typeof(XTooltip);
						(caption.AddComponent(cmpType) as XTooltip).Init(tooltip, parentOption);
					}
				}

				// for addind tooltip to the options heading
				// warning: supposed to be used on the first added option only
				public class AddToHeading: Add
				{
					public AddToHeading(Type tooltipCmpType, string tooltip): base(tooltipCmpType, tooltip) {}

					protected override GameObject GetTargetGameObject(GameObject optionGameObject)
					{
						int index = optionGameObject.transform.GetSiblingIndex();
						Debug.assert(index > 0);

						return optionGameObject.transform.parent.GetChild(index - 1).gameObject;
					}
				}

				void Init(string tooltip, ModOption parentOption)
				{
					this.VTooltip = tooltip;
					this.parentOption = parentOption;
				}

				protected ModOption parentOption;

				public virtual string VTooltip
				{
					get => _tooltip;
					set => _tooltip = value;
				}
				protected string _tooltip;

				protected virtual string GetTooltip() => VTooltip;

				public void GetTooltip(TooltipData tooltip) { tooltip.prefix.Append(GetTooltip()); }
				public bool showTooltipOnDrag => false;


				static readonly Type layoutElementType = Type.GetType("UnityEngine.UI.LayoutElement, UnityEngine.UI");
				void Start()
				{
					Destroy(gameObject.GetComponent(layoutElementType)); // for removing empty space after label

					VTooltip = LanguageHelper.Str(_tooltip); // using field, not property
				}
			}
			#endregion

			#region tooltip with cache
			public abstract class TooltipCached: XTooltip // to avoid creating strings on each frame
			{
				protected abstract bool NeedUpdate { get; }

				string tooltipCached;
				protected sealed override string GetTooltip() => NeedUpdate? (tooltipCached = VTooltip): tooltipCached;
			}

			public abstract class TooltipCached<T1>: TooltipCached where T1: struct
			{
				T1? param1 = null;

				protected bool IsParamsChanged(T1 param1)
				{
					if (Equals(this.param1, param1))
						return false;

					this.param1 = param1;
					return true;
				}
			}

			public abstract class TooltipCached<T1, T2>: TooltipCached where T1: struct where T2: struct
			{
				T1? param1 = null;
				T2? param2 = null;

				protected bool IsParamsChanged(T1 param1, T2 param2)
				{
					if (Equals(this.param1, param1) && Equals(this.param2, param2))
						return false;

					this.param1 = param1;
					this.param2 = param2;
					return true;
				}
			}
			#endregion
		}
	}
}