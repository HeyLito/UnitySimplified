using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDataSerializerCallback
{
    void OnBeforeSerialization();
    void OnAfterSerialization();
    void OnBeforeDeserialization();
    void OnAfterDeserialization();
}
