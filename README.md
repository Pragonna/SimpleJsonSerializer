# MyJsonSerializer

MyJsonSerializer is a simple C# library designed to convert objects to JSON format and parse JSON back into objects.

## Features
- Serialize objects into JSON format
- Deserialize JSON strings into objects
- Supports primitive types, `string`, `DateTime`, `Guid`, `Array`, and `List<>`
- Handles nested objects
- Supports generic collections

## Installation
To use this library in your project, simply include the `MyJsonSerializer.cs` file in your project.

## Usage

### Serialization (Convert an object to JSON format)
```csharp
using MyJsonSerializer;

var person = new {
    Name = "John Doe",
    Age = 30,
    BirthDate = new DateTime(1993, 5, 15),
    Hobbies = new string[] { "Football", "Reading" }
};

string json = MyJsonSerializer.Serialize(person);
Console.WriteLine(json);
```

**Output:**
```json
{"Name":"John Doe","Age":30,"BirthDate":"1993-05-15T00:00:00","Hobbies":["Football","Reading"]}
```

### Deserialization (Convert JSON into an object)
```csharp
using MyJsonSerializer;

string json = "{"Name":"John Doe","Age":30}";
var person = MyJsonSerializer.Deserialize<Person>(json);
Console.WriteLine(person.Name);
```

**Output:**
```
John Doe
```

## Supported Data Types
- `string`
- `int`, `double`, `decimal`, `bool`
- `DateTime`, `Guid`
- `Array` and `List<T>`
- Nested objects

## License
This project is licensed under the MIT License.
