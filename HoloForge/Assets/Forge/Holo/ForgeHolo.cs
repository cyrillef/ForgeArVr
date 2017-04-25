using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Autodesk.Forge {

//public class ForgeHolo : MonoBehaviour {

//	// Use this for initialization
//	void Start () {
//		print ("cyrille") ;
//		//string test ="http://192.168.1.16:8080/cranehook.manifest" ;
//		//WWW text =new WWW (test) ;
//		//while ( !text.isDone ) {}

//		StartCoroutine (DownloadAndCache ()) ;
	
//	}
	
//	// Update is called once per frame
//	void Update () {
	
//	}

//	IEnumerator DownloadAndCache() {
//		while ( !Caching.ready )
//			yield return null ;
//		// example URL of file on PC filesystem (Windows)
//		// string bundleURL = "file:///D:/Unity/AssetBundles/MyAssetBundle.unity3d";
//		// example URL of file on Android device SD-card
//		//string bundleURL = "file:///mnt/sdcard/AndroidCube.unity3d";

//		string bundleURL ="http://192.168.1.16:8080/cranehook" ;
//		using ( WWW www =WWW .LoadFromCacheOrDownload (bundleURL, 2) ) {
//			yield return www ;
			
//			if ( www .error != null )
//				throw new UnityException ("WWW Download had an error: " + www .error) ;

//			// Load and retrieve the AssetBundle
//			AssetBundle bundle =www .assetBundle ;

//			// Load the assembly and get a type (class) from it
//			//var assembly =System.Reflection.Assembly.Load ("txt.bytes") ;
//			//var type =assembly.GetType ("MyClassDerivedFromMonoBehaviour") ;

//			// Instantiate a GameObject and add a component with the loaded class
//			//GameObject go =new GameObject () ;
//			//go.AddComponent (type) ;

//			//Instantiate (bundle.mainAsset) as GameObject ;
//			GameObject go =bundle.LoadAsset ("CraneHook.prefab") as GameObject ;
//			GameObject prefab =Instantiate<GameObject> (go) ;
//			print (prefab) ;
	
//			// Unload the AssetBundles compressed contents to conserve memory
//			bundle.Unload (false) ;
//		}
//	}

//	//public WWW GET (string url) {
//	//	WWW www = new WWW (url);
//	//	StartCoroutine (WaitForRequest (www));
//	//	return (www);
//	//}

//	//public WWW POST (string url, Dictionary<string, string> post) {
//	//	WWWForm form = new WWWForm ();
//	//	foreach ( KeyValuePair<string, string> post_arg in post )
//	//		form.AddField (post_arg.Key, post_arg.Value);
//	//	WWW www = new WWW (url, form);
//	//	StartCoroutine (WaitForRequest (www));
//	//	return (www);
//	//}
//}

 
public class ForgeHolo : MonoBehaviour {
    private List<GameObject> photos =new List<GameObject> () ;
    private int photosCount =5 ;
 
    //private int currentIndex =0 ;
    //private float MARGIN_X =3f ;
    //private float ITEM_W =10f ;
 
    //private float sliderValue =4f ;
    public Slider uiSlider ;
	public WWW www ;
	public AssetBundle bundle;

	public string _name ="CraneHook2" ;

	void Start() {
		//GameObject c1 =GameObject.Find ("Capsule") ;
		//MeshRenderer r =c1.GetComponent<MeshRenderer> () ;
		//r.material.color =new Color (1.0f, 0f, 1f, 1f) ;


     // Download the file from the URL. It will not be saved in the Cache
   //  using (www = new WWW("http://192.168.1.16:8080/cranehook2.unity3d")) {
   //      yield return www;
   //      if (www.error != null) {
			//	r.material.color =new Color (1.0f, 0f, 0f, 1f) ;
   //          throw new System.Exception ("WWW download had an error:" + www.error);
			//}
   //      bundle = www.assetBundle;
			//r.material.color =new Color (0f, 1f, 0f, 1f) ;
			//string AssetName =_name ;
			////Debug.Log (bundle.mainAsset) ;
   //      if (AssetName == "")
   //          Instantiate(bundle.mainAsset);
   //      else {
			//	Object on =bundle.LoadAsset(AssetName) ;
			//	r.material.color =new Color (1.0f, 1f, 0f, .5f) ;
			//	//Debug.Log (on) ;
   //          Instantiate(on);
			//	r.material.color =new Color (1.0f, 0f, 1f, 1f) ;
			//}
   //                // Unload the AssetBundles compressed contents to conserve memory
   //                //bundle.Unload(false);

   //  } // memory is freed from the web stream (www.Dispose() gets called implicitly)

		//Object oo =Resources.Load("Assets/Resources/CraneHook2.prefab");
		//GameObject go = Instantiate(oo) as GameObject;  

   }

    void Start2 () {
       //loadImages();
       //uiSlider.numberOfSteps = photosCount; //这里可设成Steps模式  随个人喜好

		string url ="http://192.168.1.16:8080/cranehook2" ;
		WWW www =GET (url) ;

		while ( !www.isDone ) {  } ;
		AssetBundle bundle =www.assetBundle ;
		//Object[] objs =bundle.LoadAllAssets () ;
		// Load the object asynchronously
		//GameObject obj2 =bundle.LoadAsset<GameObject> (_name) ;
		//GameObject objI =Instantiate (obj2, Vector3.zero, Quaternion.identity) as GameObject ;

		// Unload the AssetBundles compressed contents to conserve memory
		bundle.Unload (false) ;
		// Frees the memory from the web stream
		www.Dispose () ;

		//GameObject root2 =GameObject.Find ("/Root") ;
		//obj2.transform.parent =root2.transform ;

    }

	private IEnumerator WaitForRequest (WWW www) {
		yield return www;
		// check for errors
		if ( www.error == null ) {
			Debug.Log ("WWW Ok!: " + www.text);
		} else {
			Debug.Log ("WWW Error: " + www.error);
		}
	}

	public WWW GET (string url) {
		WWW www =new WWW (url) ;
		StartCoroutine (WaitForRequest (www)) ;
		return (www) ; 
	}
 
    void loadImages () {
        for (int i = 0; i < photosCount; i++)
        {
            GameObject photo = GameObject.CreatePrimitive(PrimitiveType.Plane);
            photos.Add(photo);
            photo.layer = 14; //我的相片全部作为一个单独的层  这样镜面渲染就好办了
            photo.transform.eulerAngles = new Vector3(-90f, 0f, 0f);
            photo.transform.localScale = new Vector3(1.5f, 1f, -1f);   //根据图片设定长宽比，z：－1，使图正向
			MeshRenderer re =photo.GetComponent<MeshRenderer> () ;
            re.material.mainTexture = Resources.Load("photo" + i.ToString(), typeof(Texture2D)) as Texture2D;
            photo.transform.parent = gameObject.transform;
        }
        //moveSlider(photos.Count / 2);
    }
 
    //void moveSlider(int id)
    //{
    //    if (currentIndex == id)
    //        return;
    //    currentIndex = id;
 
    //    for (int i = 0; i < photosCount; i++)
    //    {
    //        float targetX = 0f;
    //        float targetZ = 0f;
    //        float targetRot = 0f;
 
    //        targetX = MARGIN_X * (i - id);
    //        //left slides
    //        if (i < id)
    //        {
    //            targetX -= ITEM_W * 0.6f;
    //            targetZ = ITEM_W * 3f / 4;
    //            targetRot = -60f;
 
    //        }
    //        //right slides
    //        else if (i > id)
    //        {
    //            targetX += ITEM_W * 0.6f;
    //            targetZ = ITEM_W * 3f / 4;
    //            targetRot = 60f;
    //        }
    //        else
    //        {
    //            targetX += 0f;
    //            targetZ = 0f;
    //            targetRot = 0f;
    //        }
 
    //        //GameObject photo = photos;
    //        //float ys = photo.transform.position.y;
    //        //Vector3 ea = photo.transform.eulerAngles;

    //        //iTween.MoveTo(photo, new Vector3(targetX, ys, targetZ), 1f);
    //        //iTween.RotateTo(photo, new Vector3(ea.x, targetRot, targetZ), 1f);
    //    }
    //}

	//public void OnSliderChange(float value)
 //  {
 //      Debug.Log(value);
 //      moveSlider((int)(value * photosCount));
 //  }

}

}