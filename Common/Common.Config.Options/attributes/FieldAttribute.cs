using System;
using System.Reflection;

namespace Common.Configuration
{
	using Reflection;

	partial class Options
	{
		// Attribute for creating options UI elements
		// AttributeTargets.Class is just for convenience during development (try to create options UI elements for all inner fields)
		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
		public class FieldAttribute: Attribute, Config.IConfigAttribute, Config.IFieldAttribute, Config.IRootConfigInfo
		{
			Config rootConfig;
			public void SetRootConfig(Config config) => rootConfig = config;

			public readonly string label;
			public readonly string tooltip;
			public readonly Type tooltipType; // component derived from Options.Components.Tooltip

			public FieldAttribute(string label = null, string tooltip = null, Type tooltipType = null)
			{
				this.label = label;
				this.tooltip = tooltip;
				this.tooltipType = tooltipType;
			}

			public void Process(object config)
			{
				foreach (var field in config.GetType().fields())
				{
					Process(config, field);

					if (Config.UisInnerFieldsProcessable(field))
						Process(field.GetValue(config));
				}
			}

			public void Process(object config, FieldInfo field)
			{																			$"Options.FieldAttribute.process fieldName:'{field.Name}' fieldType:{field.FieldType} label: '{label}'".logDbg();
				Config.Field cfgField = new (config, field, rootConfig);

				if (Factory.create(cfgField) is ModOption option)
					add(option);
				else
					$"FieldAttribute.process: error while creating option for field {field.Name}".logError();
			}
		}
	}
}