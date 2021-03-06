﻿using ICanPay.Core.Utils;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace ICanPay.Core
{
    /// <summary>
    /// 网关数据
    /// </summary>
    public class GatewayData
    {
        #region 私有字段

        private readonly SortedDictionary<string, object> _values;
        private readonly string _defaultResult = "defaultResult";

        #endregion

        #region 属性

        public object this[string key]
        {
            get => _values[key];
            set => _values[key] = value;
        }

        public int Count => _values.Count;

        #endregion

        #region 构造函数

        public GatewayData()
        {
            _values = new SortedDictionary<string, object>();
        }

        #endregion

        #region 方法

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="key">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns></returns>
        public bool Add(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key", "参数名不能为空");
            }

            if (value is null || string.IsNullOrEmpty(value.ToString()))
            {
                throw new ArgumentNullException("value", "参数值不能为空");
            }

            if (Exists(key))
            {
                _values[key] = value;
            }
            else
            {
                _values.Add(key, value);
            }

            return true;
        }

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <returns></returns>
        public bool Add(object obj)
        {
            var type = obj.GetType();
            var properties = type.GetProperties();
            var fields = type.GetFields();

            Add(properties);
            Add(fields);

            return true;

            void Add(MemberInfo[] info)
            {
                foreach (var item in info)
                {
                    var notAddattributes = item.GetCustomAttributes(typeof(NotAddAttribute), true);
                    if (notAddattributes.Length > 0)
                    {
                        continue;
                    }

                    var renameAttribute = item.GetCustomAttributes(typeof(ReNameAttribute), true);
                    var key = renameAttribute.Length > 0 ? ((ReNameAttribute)renameAttribute[0]).Name : item.Name.ToSnakeCase();
                    object value;
                    switch (item.MemberType)
                    {
                        case MemberTypes.Field:
                            value = ((FieldInfo)item).GetValue(obj);
                            break;
                        case MemberTypes.Property:
                            value = ((PropertyInfo)item).GetValue(obj);
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    if (value is null || string.IsNullOrEmpty(value.ToString()))
                    {
                        continue;
                    }

                    if (Exists(key))
                    {
                        _values[key] = value;
                    }
                    else
                    {
                        _values.Add(key, value);
                    }
                }
            }
        }

        /// <summary>
        /// 根据参数名获取参数值
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns>参数值</returns>
        public object GetValue(string key)
        {
            _values.TryGetValue(key, out object value);
            return value;
        }

        /// <summary>
        /// 根据参数名获取参数值
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns>参数值</returns>
        public string GetStringValue(string key)
        {
            return GetValue(key)?.ToString();
        }

        /// <summary>
        /// 根据参数名获取参数值
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns>参数值</returns>
        public double GetDoubleValue(string key)
        {
            double.TryParse(GetStringValue(key), out double value);
            return value;
        }

        /// <summary>
        /// 根据参数名获取参数值
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns>参数值</returns>
        public int GetIntValue(string key)
        {
            int.TryParse(GetStringValue(key), out int value);
            return value;
        }

        /// <summary>
        /// 根据参数名获取参数值
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns>参数值</returns>
        public DateTime GetDateTimeValue(string key)
        {
            DateTime.TryParse(GetStringValue(key), out DateTime value);
            return value;
        }

        /// <summary>
        /// 根据参数名获取参数值
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns>参数值</returns>
        public float GetFloatValue(string key)
        {
            float.TryParse(GetStringValue(key), out float value);
            return value;
        }

        /// <summary>
        /// 根据参数名获取参数值
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns>参数值</returns>
        public decimal GetDecimalValue(string key)
        {
            decimal.TryParse(GetStringValue(key), out decimal value);
            return value;
        }

        /// <summary>
        /// 根据参数名获取参数值
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns>参数值</returns>
        public byte GetByteValue(string key)
        {
            byte.TryParse(GetStringValue(key), out byte value);
            return value;
        }

        /// <summary>
        /// 根据参数名获取参数值
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns>参数值</returns>
        public char GetCharValue(string key)
        {
            char.TryParse(GetStringValue(key), out char value);
            return value;
        }

        /// <summary>
        /// 根据参数名获取参数值
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns>参数值</returns>
        public bool GetBoolValue(string key)
        {
            bool.TryParse(GetStringValue(key), out bool value);
            return value;
        }

        /// <summary>
        /// 是否存在指定参数名
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns></returns>
        public bool Exists(string key) => _values.ContainsKey(key);

        /// <summary>
        /// 将网关数据转成Xml格式数据
        /// </summary>
        /// <returns></returns>
        public string ToXml()
        {
            if (_values.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append("<xml>");
            foreach (var item in _values)
            {
                if (item.Value is string)
                {
                    sb.AppendFormat("<{0}><![CDATA[{1}]]></{0}>", item.Key, item.Value);

                }
                else
                {
                    sb.AppendFormat("<{0}>{1}</{0}>", item.Key, item.Value);
                }
            }
            sb.Append("</xml>");

            return sb.ToString();
        }

        /// <summary>
        /// 将Xml格式数据转换为网关数据
        /// </summary>
        /// <param name="xml">Xml数据</param>
        /// <returns></returns>
        public void FromXml(string xml)
        {
            try
            {
                Clear();
                if (!string.IsNullOrEmpty(xml))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(xml);
                    var xmlNode = xmlDoc.FirstChild;
                    var nodes = xmlNode.ChildNodes;
                    foreach (var item in nodes)
                    {
                        var xe = (XmlElement)item;
                        Add(xe.Name, xe.InnerText);
                    }
                }
            }
            catch
            {
                Add(_defaultResult, xml);
            }
        }

        /// <summary>
        /// 将网关数据转换为Url格式数据
        /// </summary>
        /// <param name="key">不需要转换的key</param>
        /// <returns></returns>
        public string ToUrl(params string[] key)
        {
            var sb = new StringBuilder();
            foreach (var item in _values)
            {
                if (!key.Contains(item.Key))
                {
                    sb.AppendFormat("{0}={1}&", item.Key, item.Value);
                }
            }

            return sb.ToString().TrimEnd('&');
        }

        /// <summary>
        /// 将网关数据转换为Url编码格式数据
        /// </summary>
        /// <param name="key">不需要转换的key</param>
        /// <returns></returns>
        public string ToUrlEncode(params string[] key)
        {
            var sb = new StringBuilder();
            foreach (var item in _values)
            {
                if (!key.Contains(item.Key))
                {
                    sb.AppendFormat("{0}={1}&", item.Key, WebUtility.UrlEncode(item.Value.ToString()));
                }
            }

            return sb.ToString().TrimEnd('&');
        }

        /// <summary>
        /// 将Url格式数据转换为网关数据
        /// </summary>
        /// <param name="url">url数据</param>
        /// <returns></returns>
        public void FromUrl(string url)
        {
            try
            {
                Clear();
                if (!string.IsNullOrEmpty(url))
                {
                    int index = url.IndexOf('?');

                    if (index == 0)
                    {
                        url = url.Substring(index + 1);
                    }

                    var regex = new Regex(@"(^|&)?(\w+)=([^&]+)(&|$)?", RegexOptions.Compiled);
                    var mc = regex.Matches(url);

                    foreach (Match item in mc)
                    {
                        Add(item.Result("$2"), WebUtility.UrlDecode(item.Result("$3")));
                    }
                }
            }
            catch
            {
                Add(_defaultResult, url);
            }
        }

        /// <summary>
        /// 将表单数据转换为网关数据
        /// </summary>
        /// <param name="form">表单</param>
        /// <returns></returns>
        public void FromForm(IFormCollection form)
        {
            try
            {
                Clear();
                var allKeys = form.Keys;

                foreach (var item in allKeys)
                {
                    Add(item, WebUtility.UrlDecode(form[item]));
                }
            }
            catch { }
        }

        /// <summary>
        /// 将网关数据转换为表单数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        public string ToForm(string url)
        {
            var html = new StringBuilder();
            html.AppendLine("<body>");
            html.AppendLine($"<form name='gateway' method='post' action ='{url}'>");
            foreach (var item in _values)
            {
                html.AppendLine($"<input type='hidden' name='{item.Key}' value='{item.Value}'>");
            }
            html.AppendLine("</form>");
            html.AppendLine("<script language='javascript' type='text/javascript'>");
            html.AppendLine("document.gateway.submit();");
            html.AppendLine("</script>");
            html.AppendLine("</body>");

            return html.ToString();
        }

        /// <summary>
        /// 将网关数据转成Json格式数据
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(_values);
        }

        /// <summary>
        /// 将Json格式数据转成网关数据
        /// </summary>
        /// <param name="json">json数据</param>
        /// <returns></returns>
        public void FromJson(string json)
        {
            try
            {
                Clear();
                if (!string.IsNullOrEmpty(json))
                {
                    var jObject = JObject.Parse(json);
                    var list = jObject.Children().OfType<JProperty>();
                    foreach (var item in list)
                    {
                        Add(item.Name, item.Value.ToString());
                    }
                }
            }
            catch
            {
                Add(_defaultResult, json);
            }
        }

        /// <summary>
        /// 将网关参数转为类型
        /// </summary>
        public T ToObject<T>()
        {
            var type = typeof(T);
            var obj = Activator.CreateInstance(type);
            var properties = type.GetProperties();

            foreach (var item in properties)
            {
                var renameAttribute = item.GetCustomAttributes(typeof(ReNameAttribute), true);
                var key = renameAttribute.Length > 0 ? ((ReNameAttribute)renameAttribute[0]).Name : item.Name.ToSnakeCase();
                var value = GetValue(key);

                if (value != null)
                {
                    item.SetValue(obj, Convert.ChangeType(value, item.PropertyType));
                }
            }

            return (T)obj;
        }

        /// <summary>
        /// 异步将网关参数转为类型
        /// </summary>
        public async Task<T> ToObjectAsync<T>()
        {
            return await Task.Run(() => ToObject<T>());
        }

        /// <summary>
        /// 清空网关数据
        /// </summary>
        public void Clear()
        {
            _values.Clear();
        }

        /// <summary>
        /// 移除指定参数
        /// </summary>
        /// <param name="key">参数名</param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            return _values.Remove(key);
        }

        /// <summary>
        /// 获取默认结果,当From无法转换时,值才存在
        /// </summary>
        /// <returns></returns>
        public string GetDefaultResult()
        {
            return GetStringValue(_defaultResult);
        }

        #endregion
    }
}