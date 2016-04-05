using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Runtime.InteropServices; //Only needed for the cursor positioning


public class PoolManager : MonoBehaviour
{
    public List<GameObject> poolGameObjects = new List<GameObject>();


    void OnEnable()
    {
        GameManager.playersDied += GameManager_playersDied; 
    }

    void OnDisable()
    {
        GameManager.playersDied -= GameManager_playersDied; 
    }

    //Event called when both players are dead
    void GameManager_playersDied()
    {
        DespawnPoolObjects();
    }

    public static PoolManager instance
    {
        //By using 'get', we make this variable READ ONLY, if we want to write we have to add `set`
        get
        {
            //the first time the instance variable is requested, _instance is null
            if (_instance == null)
            {
                //we search through the scene to get the object with the GameManager component
                _instance = FindObjectOfType<PoolManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject();
                    _instance = go.AddComponent <PoolManager>();
                }
                DontDestroyOnLoad(_instance);
            }
            //the _instance variable is returned
            return _instance;
        }
        
        
    }

    private static PoolManager _instance;
    private Dictionary<string, Stack<GameObject>> poolObjects;

    //Only needed for the Cursor positioning
    //[DllImport("user32.dll")]
    //public static extern bool SetCursorPos(int X, int Y);


    public void DespawnPoolObjects()
    {
        foreach (GameObject gO in poolGameObjects) //Go through all pool game objects and despawn them
        {
            if (gO != null)
                Despawn(gO);
        }
    }

    void Awake()
    {
        //Making sure that there is only one PoolManager on the scene
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        poolObjects = new Dictionary<string, Stack<GameObject>>();
        GameObject[] objects = Resources.LoadAll<GameObject>("PoolObjects");

        foreach (GameObject obj in objects)
        {
            //create a new entry in the Dictionary and place an empty stack in there
            poolObjects.Add(obj.name, new Stack<GameObject>());
            //We push our obj (the prefabe from the Resources folder) as the first element in the stack)
            poolObjects[obj.name].Push(obj);
        }
            
        //Creating some pool objects that are heavy on the beggining of the game (Main Menu) to avoid spikes later on
        GameObject floatingDamage = Spawn("FloatingDamage");
        floatingDamage.transform.position = new Vector3(100, 0, 100);
        for (int i = 0; i < 15; i++)
        {
            GameObject light = Spawn("ElectricTouchBolt");
            light.transform.position = new Vector3(100, 0, 100);
            GameObject lamiaGlyphF = Spawn("LamiaGlyphFormation");
            lamiaGlyphF.transform.position = new Vector3(100, 0, 100);
            GameObject lamiaGlyphE = Spawn("LamiaGlyphExplosion");
            lamiaGlyphE.transform.position = new Vector3(100, 0, 100);
        }

        //Making the cursor invisible
       // Cursor.visible = false;
        //Puting the cursor on the corner of the game so it don`t bug the UI
       // SetCursorPos(-3000, -3000);
    }

    public GameObject Spawn(string objName)
    {
        Stack<GameObject> objectStack = poolObjects[objName];
        if (objectStack.Count == 1)
        {
            //get the reference of the GameObject that is on the top of the stack and instantiate it
            GameObject newObject = Instantiate(objectStack.Peek());
            //we set the name of the newObject, because we will use that name in Despawn
            DontDestroyOnLoad(newObject);
            newObject.name = objName;
            //adding this game object on the pool objects list
            poolGameObjects.Add(newObject);
            return newObject;   
        }
        //get the top object on the stack
        GameObject objFromStack = objectStack.Pop();
        //set the object to activated
        objFromStack.SetActive(true);
        return objFromStack;
    }

  /*
    void Update()
    {
        //Making the cursor invisible
       // Cursor.visible = false;
        //Puting the cursor on the corner of the game so it don`t bug the UI
      //  SetCursorPos(-3000, -3000);
    }
  */
    public void Despawn(GameObject obj)
    {
        //safety to only despawn objects that are activated
        if (obj.activeSelf)
        {
            //set the parent object to null so if it had another GameObject as a parent it will cause to not break if this GameObject was destroyed.
            obj.transform.parent = null;
            //set the pool object to false
            obj.SetActive(false);
            //put the poolbject again on the stack
            poolObjects[obj.name].Push(obj);
        }
    }

}
