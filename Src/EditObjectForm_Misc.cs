using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BtmI2p.MiscUtils;
using Xunit;

namespace BtmI2p.MiscClientForms
{
    public enum EEditObjectFormMode
    {
        View,
        Add,
        Edit
    }
    
    public class EditObjectFormStringEnum
    {
        public string SelectedValue = "";
        public List<string> ValuesRange = new List<string>{ "" };
    }
    public class PropertyControlRelation
    {
        public static List<PropertyControlRelation>
            GetSaveControls<TValue>(
                TValue data,
                EEditObjectFormMode mode,
                List<string> propNames = null,
                Dictionary<string,string> overridenNames = null 
            )
        {
            Assert.NotNull(data);
            if(overridenNames == null)
                overridenNames = new Dictionary<string, string>();
            var tValueType = typeof(TValue);
            var fieldsAndProperties = tValueType.GetFields()
                .Concat<MemberInfo>(
                    tValueType.GetProperties()
                ).Where(_ => propNames == null || propNames.Contains(_.Name));
            return fieldsAndProperties
                .Select(fieldCopy => Create(fieldCopy, data, mode, overridenNames))
                .ToList();
        }
        public string Name;
        public Func<Control> CreateControl;
        public Action<Control> SaveValueFromControl;

        public static PropertyControlRelation Create(
            MemberInfo mInfo,
            object data,
            EEditObjectFormMode mode,
            Dictionary<string,string> overridenNames = null
        )
        {
            Assert.NotNull(mInfo);
            Assert.Contains(
                mInfo.MemberType,
                new []
                {
                    MemberTypes.Field, 
                    MemberTypes.Property
                }
            );
            if(overridenNames == null)
                overridenNames = new Dictionary<string, string>();
            bool isField = mInfo.MemberType == MemberTypes.Field;
            var valueType = isField
                ? ((FieldInfo)mInfo).FieldType
                : ((PropertyInfo)mInfo).PropertyType;
            Func<object> getValueFunc = () => isField
                ? ((FieldInfo)mInfo).GetValue(data)
                : ((PropertyInfo)mInfo).GetValue(data);
            Action<object> setValueFunc = newValue =>
            {
                if (isField)
                    ((FieldInfo)mInfo).SetValue(data, newValue);
                else
                    ((PropertyInfo)mInfo).SetValue(data, newValue);
            };
            var everyControlAction = (Action<Control>)(c =>
            {
                c.Dock = DockStyle.Top;
            });
            var result = new PropertyControlRelation()
            {
                Name = overridenNames.ContainsKey(mInfo.Name)
                    ? overridenNames[mInfo.Name]
                    : mInfo.Name
            };
            if (mode == EEditObjectFormMode.View)
            {
                result.CreateControl = () =>
                {
                    var ctrl = new TextBox();
                    ctrl.Text = getValueFunc().WriteObjectToJson(valueType);
                    everyControlAction(ctrl);
                    return ctrl;
                };
                result.SaveValueFromControl = (ctrl) =>
                {

                };
            }
            else
            {
                if (valueType.IsEnum)
                {
                    result.CreateControl = () =>
                    {
                        var ctrl = new ComboBox()
                        {
                            DropDownStyle = ComboBoxStyle.DropDownList
                        };
                        ctrl.Items.AddRange(
                            Enum.GetNames(valueType).Cast<object>().ToArray());
                        ctrl.SelectedItem = Enum.GetName(valueType, getValueFunc());
                        everyControlAction(ctrl);
                        return ctrl;
                    };
                    result.SaveValueFromControl = ctrl =>
                    {
                        var cBox = (ComboBox) ctrl;
                        setValueFunc(Enum.Parse(valueType, (string) cBox.SelectedItem));
                    };
                }
                else if (valueType == typeof (string))
                {
                    result.CreateControl = () =>
                    {
                        var ctrl = new TextBox() {Text = (string) getValueFunc()};
                        everyControlAction(ctrl);
                        return ctrl;
                    };
                    result.SaveValueFromControl = ctrl =>
                    {
                        var tBox = (TextBox) ctrl;
                        setValueFunc(tBox.Text);
                    };
                }
                else if (valueType == typeof(EditObjectFormStringEnum))
                {
                    var oldValue = getValueFunc() as EditObjectFormStringEnum;
                    Assert.NotNull(oldValue);
                    Assert.Contains(oldValue.SelectedValue, oldValue.ValuesRange);
                    result.CreateControl = () =>
                    {
                        var ctrl = new ComboBox()
                        {
                            DropDownStyle = ComboBoxStyle.DropDownList
                        };
                        ctrl.Items.AddRange(
                            oldValue.ValuesRange.Cast<object>().ToArray());
                        ctrl.SelectedItem = oldValue.SelectedValue;
                        everyControlAction(ctrl);
                        return ctrl;
                    };
                    result.SaveValueFromControl = ctrl =>
                    {
                        var cBox = (ComboBox)ctrl;
                        oldValue.SelectedValue = (string) cBox.SelectedItem;
                        Assert.Contains(oldValue.SelectedValue, oldValue.ValuesRange);
                        setValueFunc(oldValue);
                    };
                }
                else if (
                    valueType == typeof (sbyte)
                    || valueType == typeof (decimal)
                    )
                {
                    result.CreateControl = () =>
                    {
                        var ctrl = new TextBox()
                        {
                            Text = Convert.ToString(getValueFunc())
                        };
                        everyControlAction(ctrl);
                        return ctrl;
                    };
                    result.SaveValueFromControl = ctrl =>
                    {
                        var tBox = (TextBox) ctrl;
                        var text = tBox.Text;
                        setValueFunc(Convert.ChangeType(text, valueType));
                    };
                }
                else if (valueType == typeof (DateTime))
                {
                    result.CreateControl = () =>
                    {
                        var ctrl = new DateTimePicker()
                        {
                            MinDate = DateTime.MinValue,
                            MaxDate = DateTime.MaxValue,
                            Format = DateTimePickerFormat.Custom,
                            CustomFormat = "dd-MM-yyyy hh:mm:ss",
                            Value = (DateTime) (getValueFunc())
                        };
                        everyControlAction(ctrl);
                        return ctrl;
                    };
                    result.SaveValueFromControl = ctrl =>
                    {
                        var dtPicker = (DateTimePicker) ctrl;
                        setValueFunc(dtPicker.Value);
                    };
                }

                else
                {
                    throw new NotSupportedException("Type not supported");
                }
            }
            return result;
        }
    }
}
