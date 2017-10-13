using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Reflection;
using Yc.Sql.Entity.Data.Core.Framework.Model.Attributes;
using Yc.Sql.Entity.Data.Core.Framework.Access;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Yc.Sql.Entity.Data.Core.Framework.Model.Context
{
    public abstract class BaseContext : IBaseContext
    {
        private Dictionary<string, PropertyInfo> currentProperties;

        private Dictionary<string, KeyValuePair<object, Type>> dbParameters = new Dictionary<string, KeyValuePair<object, Type>>();

        private int timeout = SqlDatabase.MAX_TIMEOUT;

        [JsonIgnore]
        [IgnoreDataMember]
        [XmlIgnore]
        public string Command { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        [XmlIgnore]
        public CommandType CommandType { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        [XmlIgnore]
        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                timeout = value;
            }
        }

        [JsonIgnore]
        [IgnoreDataMember]
        [XmlIgnore]
        public string ControllerFunction { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        [XmlIgnore]
        public string DependingDbTableNamesInCsv { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        [XmlIgnore]
        public bool IsLongRunning { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        [XmlIgnore]
        public bool MustCache { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        [XmlIgnore]
        public Dictionary<string, KeyValuePair<object, Type>> DbParameterContainer
        {
            get { return BuildParameterData(dbParameters); }
        }

        private Dictionary<string, KeyValuePair<object, Type>> BuildParameterData(Dictionary<string, KeyValuePair<object, Type>> dbParameters)
        {
            dbParameters = new Dictionary<string, KeyValuePair<object, Type>>();
            Dictionary<string, string> messages;
            if (VerifyMandatoryProperties(GetType(), out messages))
            {
                SetDataEntitiesFromDerivedMemberProperties(GetType(), typeof(SqlParameterAttribute), dbParameters);
            }
            else
            {
                throw new NoNullAllowedException(string.Join(";\n", messages.Values.ToArray()));
            }
            return dbParameters;
        }

        private void SetDataEntitiesFromDerivedMemberProperties(Type type, Type attributeType, Dictionary<string, KeyValuePair<object, Type>> dataCollection)
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var propertyInfo in properties)
            {
                var selectionAttribute = propertyInfo.GetCustomAttributes(false).FirstOrDefault(att => att.GetType() == attributeType);
                if (selectionAttribute == null && attributeType != null) continue;

                var name = selectionAttribute != null ? GetCustomAttributeData(selectionAttribute) : propertyInfo.Name;

                var functionAttribute = propertyInfo.GetCustomAttributes(false).FirstOrDefault(att => att.GetType() == typeof(FunctionsAttribute));
                if (functionAttribute == null) continue;

                if (!dataCollection.ContainsKey(name.ToString()) && ((string[])GetCustomAttributeData(functionAttribute)).Contains(ControllerFunction))
                {
                    dataCollection.Add(name.ToString(), new KeyValuePair<object, Type>(propertyInfo.GetValue(this, null), propertyInfo.PropertyType));
                }
            }
            if (type.BaseType != null && type.BaseType.Name == typeof(BaseContext).Name) return;

            SetDataEntitiesFromDerivedMemberProperties(type.BaseType, attributeType, dataCollection);
        }

        public void SetEntityValue(string entityName, object value)
        {
            try
            {
                if (currentProperties == null)
                {
                    currentProperties = new Dictionary<string, PropertyInfo>();
                    var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    foreach (var propertyInfo in properties)
                    {
                        var columnAttribute = propertyInfo.GetCustomAttributes(false).FirstOrDefault(att => att.GetType() == typeof(ColumnNamesAttribute));
                        if (columnAttribute == null) continue;

                        var columnNames = ((string[])GetCustomAttributeData(columnAttribute));
                        foreach (var columnName in columnNames)
                        {
                            currentProperties[columnName] = propertyInfo;
                        }
                    }
                }

                PropertyInfo property;
                if (currentProperties.TryGetValue(entityName, out property))
                {
                    property.SetValue(this, SafeCastToPropertyType(value, property), null);
                }
            }
            catch (Exception exception)
            {
                throw new Exception(string.Concat("SetEntityValue-failure processing property ", entityName, " and value ", value), exception);
            }
        }

        private object SafeCastToPropertyType(object value, PropertyInfo property)
        {
            if (property.PropertyType.IsEnum)
                return Enum.Parse(property.PropertyType, value.ToString());

            var t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (t.FullName == "System.Byte[]")
            {
                return (value == null) ? null : Convert.ChangeType(value, t);
            }

            return (value == null) ? null : Convert.ChangeType(value.ToString().Trim(), t);
        }

        private object GetCustomAttributeData(object customAttribute)
        {
            switch (customAttribute.GetType().Name)
            {
                case "SqlParameterAttribute":
                    return ((SqlParameterAttribute)customAttribute).ParameterName;
                case "FunctionsAttribute":
                    return ((FunctionsAttribute)customAttribute).Functions;
                case "ColumnNamesAttribute":
                    return ((ColumnNamesAttribute)customAttribute).ColumnNames;
                case "MandatoryAttribute":
                    return new Tuple<string, string[]>(((MandatoryAttribute)customAttribute).DescriptiveAttributeName, ((MandatoryAttribute)customAttribute).Functions);
                default:
                    throw new NotImplementedException(customAttribute.GetType().Name);
            }
        }

        protected string GetDescriptionFromEnumValue(Enum value)
        {
            var attribute = value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(EnumMemberAttribute), false).SingleOrDefault() as EnumMemberAttribute;
            return attribute == null ? value.ToString() : attribute.Value;
        }

        private bool VerifyMandatoryProperties(Type type, out Dictionary<string, string> messages)
        {
            messages = new Dictionary<string, string>();
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (var propertyInfo in properties)
            {
                var selectionAttribute = propertyInfo.GetCustomAttributes(false).FirstOrDefault(att => att.GetType() == typeof(MandatoryAttribute));
                if (selectionAttribute == null) continue;

                var mandatoryAttributeData = (Tuple<string, string[]>)GetCustomAttributeData(selectionAttribute);
                var name = mandatoryAttributeData.Item1 ?? propertyInfo.Name;

                var functionAttribute = propertyInfo.GetCustomAttributes(false).FirstOrDefault(att => att.GetType() == typeof(FunctionsAttribute));
                if (functionAttribute == null) continue;

                if (!messages.ContainsKey(name.ToString()) && (mandatoryAttributeData.Item2).Contains(ControllerFunction) && ((string[])GetCustomAttributeData(functionAttribute)).Contains(ControllerFunction) && !CheckIfPropertyIsSet(propertyInfo))
                {
                    messages.Add(name.ToString(), string.Format("{0} is not set", name));
                }
            }
            if (type.BaseType != null && type.BaseType.Name == typeof(BaseContext).Name) return messages.Count == 0;

            VerifyMandatoryProperties(type.BaseType, out messages);

            return messages.Count == 0;
        }

        private bool CheckIfPropertyIsSet(PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetValue(this, null) == null)
                return false;

            if (propertyInfo.PropertyType == typeof(string))
                return !String.IsNullOrEmpty(Convert.ToString(propertyInfo.GetValue(this, null)).Trim());

            if (propertyInfo.PropertyType == typeof(int))
                return Convert.ToInt64(propertyInfo.GetValue(this, null)) != default(int);

            if (propertyInfo.PropertyType == typeof(decimal))
                return Convert.ToDecimal(propertyInfo.GetValue(this, null)) != default(decimal);

            if (propertyInfo.PropertyType == typeof(double))
                return Convert.ToDouble(propertyInfo.GetValue(this, null)) != default(double);

            if (propertyInfo.PropertyType == typeof(DateTime))
            {
                return Convert.ToDateTime(propertyInfo.GetValue(this, null)) != DateTime.MinValue;
            }

            //TODO: add other types as well based on need
            return true;
        }
    }
}
