using System.Collections;
using UnityEngine;

public static class HelpfulUtility
{

    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if (stringToCheck == "")
        {
            Debug.Log(fieldName + " is empty" + thisObject.name.ToString());
            return true;
        }
        return false;
    }
    public static bool ValidateCheckEnumerableValues(Object thisobject, string fieldName, IEnumerable enumerableObjectToCheck)
    {
        bool error = false;
        int count = 0;

        if (enumerableObjectToCheck == null)
        {
            Debug.Log(fieldName + "is null in object" + thisobject.name.ToString());
            return true;
        }

        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null)
            {
                Debug.Log(fieldName + " has null values in object " + thisobject.name.ToString());
                error = true;
            }
            else
            {
                count++;
            }
        }


        if (count == 0)
        {
            Debug.Log(fieldName + " has no values in object " + thisobject.name.ToString());
            error = true;
        }
        return error;
    }
    public static bool ValidateCheckNullValue(Object thisObject, string fieldName, UnityEngine.Object objectToCheck)
    {
        if (objectToCheck == null)
        {
            Debug.Log(fieldName + " is null and must contain a value in object " + thisObject.name.ToString());
            return true;
        }
        return false;
    }
    public static bool ValidateCheckPositiveVlaue(Object thisObject, string fieldName, int valueToCheck, bool isZeroAllowed)
    {
        bool error = false;

        if (isZeroAllowed)
        {
            if (valueToCheck < 0)
            {
                Debug.Log(fieldName + " must contain  a postitive valuie or zero inb object " + thisObject.name.ToString());
                error = true;
            }

        }
        else
        {
            if (valueToCheck <= 0)
            {
                Debug.Log(fieldName + " must contain  a postitive valuie in object " + thisObject.name.ToString());
                error = true;
            }

        }

        return error;
    }
    public static Vector3 GetMousePosition()
    {
        Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vec.z = 0f;
        return vec;
    }

    private static Vector3 GetMouseWorldPositionWithZ(Vector3 mousePosition, Camera camera)
    {
        Vector3 worldPosition = camera.ScreenToWorldPoint(mousePosition);
        return worldPosition;
    }

}
