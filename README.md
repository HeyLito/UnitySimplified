# UnitySimplified
A collection of multi-purposed unity C# scripts assembled to fast forward the initial creation process of a game’s foundation. A summary of what is or could include are serialization management, visual programming assisters, and other extensions to the Unity UI.


## Table of Contents
 - [Features](#features)
	- [DataSerializer](#dataserializer)
	- [VisualStatement](#visualstatement)
	- [GamePrefs](#gameprefs)



## Features
### DataSerializer
DataSerializer is a utility-like class that conveys runtime data into serializable containers.

The class uses two functions to serialize and deserialize. The serialization function determines what data gets cached into [Dictionary](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2) pairings of names as keys and [objects](https://docs.microsoft.com/en-us/dotnet/api/system.object) as values. Meanwhile, the deserialization function uses a cached Dictionary to overwrite a target's values.
>Data is serialized into a dictionary by: <br>
>`DataSerializer.SerializeIntoData(object, Dictionary<string, object>, SerialzierFlags)`
>
>and then deserialized from the dictionary by: <br>
>`DataSerializer.DeserializeIntoInstance(object, Dictionary<string, object>, SerializerFlags)`

<br></br>
Since this class only operates to transport information, the functionality of reading and writing runtime data is at the user's discretion. By default, MonoBehaviour and basic UnityEngine scripts have serializers; However, they can be overridden or extended by creating a CustomSerializer.
>Quasi-excerpt, taken from the CustomSerializer for Unity's GameObject class
>```
> [CustomSerializer(typeof(UnityEngine.GameObject))]
> public class GameObjectSerializer : IConvertibleData
> {
>     public Serialize(object, Dictionary<string, object>, SerializerFlags);
>     public Deserialize(object, Dictionary<string, object>, SerializerFlags);
> }
>```



<br></br>
### VisualStatement
VisualStatements are editor-publicized if-statements.

When in the Editor, the field stores information from the target and retrieves its value later in runtime using [System.Reflection](https://docs.microsoft.com/en-us/dotnet/api/system.reflection).
><img src="https://imgur.com/Saiqkw9.gif"/> <br>

<br></br>
Within the interior of a VisualStatement are multiple conditions, operands, and relational operators. 
>The conditional comparison in an if-statement: <br>
>```if (value == value)``` <br>
>Is as follows when in a VisualStatement: <br>
>```if (operand.GetValue() relationalOperator operand.GetValue())``` <br>
>
>If the condition is in the proper structure, then `==` and `!=` relational operators are available to select. <br>
>If proper and both operands are of number type, then `==`, `!=`, `>`, `>=`, `<`, and `<=`, relational operators are available to select.

<br></br>
The user could then return a boolean result of a VisualStatement by:
>```
> if (visualStatement.IsValid())
>	  if (visualStatement.GetResult())
>	  {	
>         do code...
>	  }
>```


<br></br>
### GamePrefs
GamePrefs are the visual alternative to Unity's [PlayerPrefs](https://docs.unity3d.com/ScriptReference/PlayerPrefs.html).
><img src=https://i.imgur.com/bsdzdTW.png/> <br>

<br></br> 
While having the same capabilities of PlayerPrefs, GamePrefs display and modify persistent data within the UnityEditor. 
><img src=https://i.imgur.com/cwKr3tQ.gif width="1920"/> <br>

<br></br> 
And using GamePrefs as publicized fields allows direct reference to predetermined GamePrefs (Dynamic GamePrefs).
><img src=https://i.imgur.com/cY5qyJo.gif/> <br>
