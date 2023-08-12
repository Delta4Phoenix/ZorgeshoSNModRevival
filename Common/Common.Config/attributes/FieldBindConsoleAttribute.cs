using System;
using System.Reflection;

namespace Common.Configuration
{
	using Utils;
	using Reflection;

	partial class Config
	{
		public partial class Field
		{
			[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
			public class BindConsole: Attribute, IConfigAttribute, IFieldAttribute, IRootConfigInfo
			{
				[AttributeUsage(AttributeTargets.Field)]
				public class SkipAttribute: Attribute {} // don't add field to console

				readonly string varNamespace; // optional namespace for use in console in case of duplicate names
				readonly bool addPrivateFields, ignoreSkipAttr;

				Config rootConfig;
				public void SetRootConfig(Config config) => rootConfig = config;

				public BindConsole(string varNamespace = null, bool addPrivateFields = false, bool ignoreSkipAttr = false)
				{
					this.varNamespace = varNamespace;
					this.addPrivateFields = addPrivateFields;
					this.ignoreSkipAttr = ignoreSkipAttr;
				}

				public void Process(object config)
				{
					config.GetType().fields().ForEach(field => Process(config, field));
				}

				public void Process(object config, FieldInfo field)
				{
					if ((field.FieldType.IsPrimitive || field.FieldType.IsEnum) &&
						(addPrivateFields || field.IsPublic) &&
						(ignoreSkipAttr || !field.CheckAttr<SkipAttribute>()))
					{
						CfgVarBinder.AddField(new FieldRanged(config, field, rootConfig), varNamespace);
					}

					if (UisInnerFieldsProcessable(field))
						Process(field.GetValue(config));
				}
			}
		}
	}
}