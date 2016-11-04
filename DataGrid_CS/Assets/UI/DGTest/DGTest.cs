using UnityEngine;
using System.Collections;

public class DGTest : MonoBehaviour {

    private DGItemList dgItemList;

    void Awake()
    {
        Init();
    }

	// Use this for initialization
	void Start () {
        dgItemList.Show();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void Init()
    {
        dgItemList = new DGItemList(GameObject.Find("ScrollPanel").GetComponent<RectTransform>());
    }
}
