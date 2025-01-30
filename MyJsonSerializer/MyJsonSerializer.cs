using System.Collections;
using System.Reflection;
using System.Text;

namespace MyJsonSerializer
{
    public class MyJsonSerializer
    {
        /// <summary>
        /// Serializes an object to a json string
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>string</returns>
        public static string Serialize(object obj)
        {
            var builder = new StringBuilder();
            bool hasMember = false;
            var type = obj.GetType();

            var properties = type.GetProperties();

            if (properties.Length == 0)
            {
                return "{}";
            }

            builder.Append("{");

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string) || property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(Guid))
                {
                    builder.Append($"\"{property.Name}\":\"{property.GetValue(obj)}\",");
                }
                else if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(decimal))
                {
                    builder.Append($"\"{property.Name}\":{property.GetValue(obj)},");
                }
                else if (property.PropertyType.IsArray)
                {
                    builder.Append($"\"{property.Name}\":{SerializeArray(property.GetValue(obj))},");
                }
                else if (property.PropertyType.IsGenericType)
                {
                    var arguments = property.PropertyType.GetGenericArguments();
                    builder.Append($"\"{property.Name}\":{SerializeEnumerable<object>(property.GetValue(obj), arguments)},");

                }
                else
                {
                    builder.Append($"\"{property.Name}\": {Serialize(property.GetValue(obj))},");
                }
                hasMember = true;
            }
            if (hasMember)
            {
                builder.Remove(builder.Length - 1, 1); // remove last comma
            }
            builder.Append("}");
            return builder.ToString();
        }
        /// <summary>
        /// Deserializes a json string to an object generic type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns>T</returns>
        public static T Deserialize<T>(string json) where T : class , new()
        {
            int index = 0;
            return (T)ParseObject(json, ref index, typeof(T));
        }
        /// <summary>
        /// Deserializes a json string to an object of a specific type. Must be casted to the desired type
        /// </summary>
        /// <param name="json"></param>
        /// <param name="type"></param>
        /// <returns>object</returns>
        public static object Deserialize(string json, Type type)
        {
            int index = 0;
            return ParseObject(json, ref index, type);
        }





        #region Deserializer helper methods
        private static object ParseObject(string json, ref int index, Type type)
        {
            SkipWhiteSpace(json, ref index);

            if (json[index] != '{') throw new ArgumentException("Invalid json format");

            index++; // skip the '{' character

            var obj = Activator.CreateInstance(type);
            var properties = type.GetProperties();


            while (index < json.Length)
            {
                if (json[index] == '}') { index++; break; } // end of object reached

                string key = ParseString(json, ref index); // get the key of the property (example: "Name")
                SkipWhiteSpace(json, ref index);

                IsGenericAndGenericArgumentTypeIsNotValueType(properties, key, out bool argIsNotValueType);// We have to check if the generic argument type is not value type

                if (json[index] != ':') throw new ArgumentException("Invalid json format");
                index++; // skip the ':' character

                SkipWhiteSpace(json, ref index);

                object value = ParseValue(json, ref index, properties, key, argIsNotValueType); // get the value of the property 

                foreach (var prop in properties)
                {
                    if (string.Equals(prop.Name, key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (prop.PropertyType == typeof(Guid))
                            value = Guid.Parse(value.ToString());

                        else if (prop.PropertyType == typeof(DateTime))
                            value = DateTime.Parse(value.ToString());

                        else if (prop.PropertyType.IsArray)
                        {
                            var elementType = prop.PropertyType.GetElementType();
                            var array = (Array)value;
                            var newArray = Array.CreateInstance(elementType, array.Length);
                            for (int i = 0; i < array.Length; i++)
                                newArray.SetValue(array.GetValue(i), i);
                            
                            value = newArray;
                        }
                        else if (prop.PropertyType.IsGenericType)
                            value = InitializeGenericList(value, prop);
                        
                        prop.SetValue(obj, value);
                        break;
                    }
                }

                SkipWhiteSpace(json, ref index);

                if (json[index] == ',') index++; // skip the ',' character
                else if (json[index] == '}') { index++; break; }// end of object reached
                else throw new ArgumentException("Invalid json format");

            }
            return obj;
        }
        private static object ParseValue(string json, ref int index, PropertyInfo[] properties, string key, bool isGenericArgTypeIsNotValue)
        {
            SkipWhiteSpace(json, ref index);

            if (json[index] == '"') return ParseString(json, ref index); // Parse string value
            if (char.IsDigit(json[index]) || json[index] == '-') return ParseNumber(json, ref index); // Parse number value
            if (json[index] == '{') // Create a new object
            {
                var propertyType = properties.FirstOrDefault(p => string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase)).PropertyType;

                if (isGenericArgTypeIsNotValue) // when the generic argument type is not value type
                {
                    propertyType = propertyType.GetGenericArguments()[0];
                }

                return ParseObject(json, ref index, propertyType);
            }
            if (json[index] == '[')
                return ParseArray(json, ref index, properties, key);
            
            if (json.Substring(index, 4) == "true")
            {
                index += 4;
                return true;
            }
            if (json.Substring(index, 5) == "false")
            {
                index += 5;
                return false;
            }
            if (json.Substring(index, 4) == "null")
            {
                index += 4;
                return null;
            }

            throw new ArgumentException("Invalid json format");

        }
        private static object ParseArray(string json, ref int index, PropertyInfo[] properties, string key)
        {
            if (json[index] != '[') throw new ArgumentException("Invalid json format");
            index++; // skip the '[' character

            List<object> list = [];

            while (index < json.Length - 1)
            {
                SkipWhiteSpace(json, ref index);
                if (json[index] == ']') { index++; break; } // end of array reached
                list.Add(ParseValue(json, ref index, properties, key, true));

                SkipWhiteSpace(json, ref index);

                if (json[index] == ',') index++; // skip the ',' character
                else if (json[index] == ']') { index++; break; } // end of array reached
                else throw new ArgumentException("Invalid json format");
            }
            return list.ToArray();
        }
        private static object ParseNumber(string json, ref int index)
        {
            int startIndex = index;
            while (index < json.Length && (char.IsDigit(json[index]) || json[index] == '.' || json[index] == '-'))
            {
                index++;
            }

            string numberStr = json.Substring(startIndex, index - startIndex);
            if (numberStr.Contains('.')) return double.Parse(numberStr);
            return int.Parse(numberStr);
        }
        private static string ParseString(string json, ref int index)
        {
            SkipWhiteSpace(json, ref index);
            if (json[index] == ',') index++;
            if (json[index] != '"') throw new ArgumentException("Invalid json format");
            index++; // skip the '"' character
            var builder = new StringBuilder();
            while (json[index] != '"')
            {
                builder.Append(json[index]);
                index++;
            }
            index++; // skip the '"' character
            return builder.ToString();
        }
        private static void SkipWhiteSpace(string json, ref int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
            {
                index++;
            }
        }
        private static void IsGenericAndGenericArgumentTypeIsNotValueType(PropertyInfo[] props, string key, out bool isTrue)
        {
            var prop = props.FirstOrDefault(p => string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));
            if (prop == null)
            {
                isTrue = false;
                return;
            }
            if (prop.PropertyType.IsGenericType)
            {
                var arguments = prop.PropertyType.GetGenericArguments();
                if (!arguments[0].IsValueType)
                {
                    isTrue = true;
                }
            }
            isTrue = false;
        }
        private static object? InitializeGenericList(object value, PropertyInfo prop)
        {
            var arguments = prop.PropertyType.GetGenericArguments();
            var list = typeof(List<>).MakeGenericType(arguments);
            var instance = (IList)Activator.CreateInstance(list);
            IEnumerable listInstance = (IEnumerable)value;

            foreach (var item in listInstance)
            {
                instance.Add(item);
            }

            return instance;
        }

        #endregion


        #region Serialize helper functions
        private static string SerializeEnumerable<T>(object obj, Type[] arguments)
        {
            var builder = new StringBuilder();
            bool hasMember = false;
            builder.Append("[");

            if (arguments.Count() == 1)
            {
                if (arguments[0] == typeof(string) || arguments[0] == typeof(DateTime) || arguments[0] == typeof(Guid))
                {
                    foreach (var item in (IEnumerable<T>)obj)
                    {
                        builder.Append($"\"{item}\",");
                        hasMember = true;
                    }
                }
                else if (arguments[0].IsPrimitive || arguments[0] == typeof(decimal))
                {
                    foreach (var item in (IEnumerable<T>)obj)
                    {
                        builder.Append($"{item},");
                        hasMember = true;
                    }
                }
                else
                {
                    foreach (var item in (IEnumerable<T>)obj)
                    {
                        builder.Append($"{Serialize(item)},");
                        hasMember = true;
                    }
                }
            }

            if (hasMember)
            {
                builder.Remove(builder.Length - 1, 1); // remove last comma
            }

            builder.Append("]");
            return builder.ToString();
        }
        private static string SerializeArray(object obj)
        {
            var builder = new StringBuilder();
            bool hasMember = false;

            builder.Append("[");

            var elementType = obj.GetType().GetElementType();
            if (elementType.IsPrimitive || elementType == typeof(decimal))
            {
                foreach (var item in (Array)obj)
                {
                    builder.Append($"{item},");
                    hasMember = true;
                }
            }
            else if (elementType == typeof(string) || elementType == typeof(DateTime) || elementType == typeof(Guid))
            {
                foreach (var item in (Array)obj)
                {
                    builder.Append($"\"{item}\",");
                    hasMember = true;
                }
            }
            else
            {
                foreach (var item in (Array)obj)
                {
                    builder.Append($"{Serialize(item)},");
                    hasMember = true;
                }
            }

            if (hasMember)
            {
                builder.Remove(builder.Length - 1, 1); // remove last comma
            }
            builder.Append("]");

            return builder.ToString();
        }
        #endregion
    }
}
