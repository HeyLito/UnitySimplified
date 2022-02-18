# UnitySimplified
A collection of multi-purposed unity C# scripts assembled to fast forward the initial creation process of a game’s foundation. A summary of what is or could include are serialization management, visual programming assisters, and other extensions to the Unity UI.


## Table of Contents
 - [Features](#features)
	- [Data Management](#data-management)
	- [VisualStatement](#visualstatement)
	- [GamePrefs](#gameprefs)

## Features
### Data Management
There are two main components in UnitySimplified's data management. The class <b>DataSerializer</b> transfers runtime data into serializable containers, while the <b>DataManager</b> class runs the file writing process.

<i><b>DataSerializer</b></i> <br>
The serializer uses two functions to serialize and deserialize. The serialization function determines what data gets cached into [Dictionary](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2) pairings of names as keys and [objects](https://docs.microsoft.com/en-us/dotnet/api/system.object) as values. Meanwhile, the deserialization function uses a cached Dictionary to overwrite a target's values.
>Data is serialized into a dictionary by: <br>
>`DataSerializer.SerializeIntoData(object, Dictionary<string, object>, SerialzierFlags)`
>
>and deserialized from the dictionary by: <br>
>`DataSerializer.DeserializeIntoInstance(object, Dictionary<string, object>, SerializerFlags)`

Primarily, the <b>DataSerializer</b> class exists only to transport information. Meaning, serialization is processed by custom serializers; Quite similarly to Unity’s Custom Editor system. Thus, resulting in the ability for users to expand or override functionality without modifying pre-existing serializer scripts.
>Quasi-excerpt, taken from the custom serializer for Unity's GameObject class
>```
> [CustomSerializer(typeof(UnityEngine.GameObject))]
> public class GameObjectSerializer : IConvertibleData
> {
>    public Serialize(object, Dictionary<string, object>, SerializerFlags);
>	  public Deserialize(object, Dictionary<string, object>, SerializerFlags);
> }
>```

<br/><i><b>DataManager</b></i> <br>
The main functionality of the <b>DataManager</b> class is to write data to a path. However, pathing and file searching is managed within the script. Rather than indexing the individual data path of every saved file, the manager automatically locates pre-existing data files within the manager’s data path.

>Before creating, loading, or saving, the user must invoke either one of the two functions at least once.
>`DataManager.LoadAllFileDatabases();` <br>
>`DataManager.LoadFileDatabase(FileFormat);` <br>
>This loads all directories and saved files within the `DataManager.TargetDataPath` directories.
>
>Therefore the process to create a new file is: <br>
>`DataManager.CreateNewFile<T>(string fileName, string subPath, T instance, FileFormat fileFormat);`
>
>To save an object to a pre-existing file: <br>
>`DataManager.SaveToFile<T>(string fileName, T instance);`
>
> And to overwrite an object from file: <br>
> `DataManager.LoadFromFile<T>(string fileName, T instance);`
> 
> <i>*Note: Invoking `LoadAllFileDatabases` or `LoadFileDatabase` after creating, saving or loading is unnecessary.</i>


<br></br>
### VisualStatement
><img src="https://imgur.com/Saiqkw9.gif"/> <br>
>A display of a VisualStatement's GUI.

VisualStatements are editor-publicized if-statements. A <b>VisualStatement</b> comprises multiple conditions and operands. The value of the statement is retrieved by using `visualStatement.GetResult()`.
>A standard use case of a statement
>```
> if (visualStatement.IsValid())
>	  if (visualStatement.GetResult())
>	  {	
>         do code...
>	  }
>```
>
<i><b>Operand</b></i> <br>
Operands are the foundation within VisualStatements. When in the Editor, operands store information from a field, property, or method found from UnityEngine.Objects. The data is later recalled in runtime using [System.Reflection](https://docs.microsoft.com/en-us/dotnet/api/system.reflection).

<i><b>Condition</b></i> <br>
Conditions compare the values of two operands using a relational operator. Depending on the operand type, up to six relational operators are supported. 
>The conditional comparison in an if-statement: <br>
>```if (value == value)``` <br>
>Is as follows when in a VisualStatement: <br>
>```if (operand.GetValue() relationalOperator operand.GetValue())``` <br>
>
>If the condition is in the proper structure, then `==` and `!=` relational operators are available to select. <br>
>If proper and both operands are of number type, then `==`, `!=`, `>`, `>=`, `<`, and `<=`, relational operators are available to select.


<br></br>
### GamePrefs
GamePrefs are the visual alternative to Unity's [PlayerPrefs](https://docs.unity3d.com/ScriptReference/PlayerPrefs.html).
><img src=https://i.imgur.com/bsdzdTW.png/> <br>

<br></br> 
While having the same capabilities of PlayerPrefs, GamePrefs display and modify persistent data within the UnityEditor. 
><img src=https://i.imgur.com/cwKr3tQ.gif width="1920"/> <br>

<br></br> 
And using GamePrefs as publicized fields allows direct references to predetermined GamePrefs (Dynamic GamePrefs).
><img src=https://i.imgur.com/cY5qyJo.gif/> <br>
