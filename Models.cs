using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonTesting;

[AttributeUsage(AttributeTargets.Property)]
public class PrimaryKey : Attribute;


public abstract class DynamicObject
{
    [JsonIgnore] public JObject ServerObject { get; set; }
}

public abstract class DynamicArrayItem
{
    public int JsonArrayIndex { get; set; }
}

public class UserModel : DynamicObject
{
    public string Login { get; set; }
    public string Password { get; set; }
    public int[] TestArray { get; set; }
    public CarModel SuperCar { get; set; }
    public List<CarModel> Cars { get; set; } = [];
}

public class CarModel : DynamicArrayItem
{
    [PrimaryKey]
    public int Id { get; set; }
    public string Name { get; set; }
}