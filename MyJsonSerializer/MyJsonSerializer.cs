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
            return Serializer.SerializeObject(obj);
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
            return (T)Deserializer.ParseObject(json, ref index, typeof(T));
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
            return Deserializer.ParseObject(json, ref index, type);
        }
    }
}
