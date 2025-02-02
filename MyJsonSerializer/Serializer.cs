using System.Text;

namespace MyJsonSerializer
{
    internal class Serializer
    {
        public static string SerializeObject(object obj)
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
                    builder.Append($"\"{property.Name}\": {SerializeObject(property.GetValue(obj))},");
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

        public static string SerializeEnumerable<T>(object obj, Type[] arguments)
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
                        builder.Append($"{SerializeObject(item)},");
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
                    builder.Append($"{SerializeObject(item)},");
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
    }
}
