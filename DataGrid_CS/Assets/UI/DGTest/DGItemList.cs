using UnityEngine;
using System.Collections;
using MogoEngine.UISystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class DGItemList 
{
    private RectTransform _trans;
    private GameObject _go;

    private RectTransform transContent;
    private ScrollRect scrollRect;
    private GameObject item;

    private DataGrid dataGrid;

    public DGItemList(RectTransform trans)
    {
        _trans = trans;
        _go = _trans.gameObject;

        Init();
    }
	
    private void Init()
    {
        transContent = _trans.Find("Content").GetComponent<RectTransform>();
        scrollRect = _trans.GetComponent<ScrollRect>();
        item = _trans.Find("Content/Item").gameObject;

        dataGrid = _go.AddComponent<DataGrid>();
        dataGrid.SetItemRender(item, typeof(DGItemRender));
        dataGrid.useLoopItems = true;
    }


    public void Show()
    {
        if (_go != null)
        {
            _go.SetActive(true);
        }

        //发请求。。。

        List<string> datas = new List<string>();
        for (int i = 0; i < 100;i++ )
        {
            datas.Add("DataGrid data -->" + i);
        }

        dataGrid.ResetScrollPosition();
        dataGrid.Data = datas.ToArray();
    }
}

