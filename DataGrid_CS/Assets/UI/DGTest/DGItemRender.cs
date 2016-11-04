using UnityEngine;
using System.Collections;
using MogoEngine.UISystem;
using UnityEngine.UI;

public class DGItemRender : ItemRender
{
    Text label;
    public override void Awake()
    {
        getWidget();
        initEvent();
    }

    private void getWidget()
    {
        label = transform.Find("Text").GetComponent<Text>();
    }

    private void initEvent()
    {

    }

    protected override void OnSetData(object data)
    {
        m_renderData = data;
        string itemData = data as string;

        SetData(itemData);
    }

    private void SetData(string text)
    {
        label.text = text;
    }
	
}
