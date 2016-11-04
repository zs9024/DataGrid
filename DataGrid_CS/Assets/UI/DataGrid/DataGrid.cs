//using MogoEngine.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MogoEngine.UISystem;
namespace MogoEngine.UISystem
{
    /// <summary>
    /// 数据列表渲染组件，Item缓存，支持无限循环列表，即用少量的Item实现大量的列表项显示
    /// </summary>
    public class DataGrid : MonoBehaviour
    {
        [HideInInspector]
        public bool useLoopItems = false;           //是否使用无限循环列表，对于列表项中OnDataSet方法执行消耗较大时不宜使用，因为OnDataSet方法会在滚动的时候频繁调用
        [HideInInspector]
        public bool useClickEvent = true;           //列表项是否监听点击事件
        [HideInInspector]
        public bool autoSelectFirst = true;         //创建时是否自动选中第一个对象

        public delegate void OnDataGridItemSelect(object renderData);
        public OnDataGridItemSelect onItemSelected;       //Item点击时的回调函数

        private RectTransform m_content;
        //private Vector2 m_lastContentPos;
        private ToggleGroup m_toggleGroup;
        private object[] m_data;
        private GameObject m_goItemRender;
        private Type m_itemRenderType;
        private readonly List<ItemRender> m_items = new List<ItemRender>();
        private object m_selectedData;
        private LayoutGroup m_LayoutGroup;
        private RectOffset m_oldPadding;
        //private Canvas m_canvas;

        //下面的属性会需要父对象上有ScrollRect组件
        private ScrollRect m_scrollRect;    //父对象上的，不一定存在
        private RectTransform m_tranScrollRect;
        private int m_itemSpace;          //每个Item的空间
        private int m_viewItemCount;        //可视区域内Item的数量（向上取整）
        private bool m_isVertical;          //是否是垂直滚动方式，否则是水平滚动
        private int m_startIndex;           //数据数组渲染的起始下标
        private string m_itemClickSound = "";//AudioConst.btnClick;

        Vector2 Resolution = new Vector2(1242, 2208);

        public float verticalPos
        {
            get { return m_scrollRect.verticalNormalizedPosition; }
            set { m_scrollRect.verticalNormalizedPosition = value; }
        }

        public float horizonPos
        {
            get { return m_scrollRect.horizontalNormalizedPosition; }
            set { m_scrollRect.horizontalNormalizedPosition = value; }
        }

        //内容长度
        private float ContentSpace
        {
            get
            {
                return m_isVertical ? m_content.sizeDelta.y : m_content.sizeDelta.x;
            }
        }
        //可见区域长度
        private float ViewSpace
        {
            get
            {
                return m_isVertical ? (m_tranScrollRect.sizeDelta.y + Resolution.y) : (m_tranScrollRect.sizeDelta.x + Resolution.x);
            }
        }
        //约束常量（固定的行（列）数）
        private int ConstraintCount
        {
            get
            {
                return m_LayoutGroup == null ? 1 : ((m_LayoutGroup is GridLayoutGroup) ? (m_LayoutGroup as GridLayoutGroup).constraintCount : 1);
            }
        }
        //数据量个数
        private int DataCount
        {
            get
            {
                return m_data == null ? 0 : m_data.Length;
            }
        }
        //缓存数量
        private int CacheCount
        {
            get
            {
                return ConstraintCount + DataCount % ConstraintCount;
            }
        }
        //缓存单元的行（列）数
        private int CacheUnitCount
        {
            get
            {
                return m_LayoutGroup == null ? 1 : Mathf.CeilToInt((float)CacheCount / ConstraintCount);
            }
        }
        //数据单元的行（列）数
        private int DataUnitCount
        {
            get
            {
                return m_LayoutGroup == null ? DataCount : Mathf.CeilToInt((float)DataCount / ConstraintCount);
            }
        }



        void Awake()
        {
            var go = gameObject;
            var trans = transform;
            //go.AddMissingComponent<CanvasRenderer>();
            go.AddComponent<CanvasRenderer>();
            m_toggleGroup = GetComponent<ToggleGroup>();
            m_LayoutGroup = GetComponentInChildren<LayoutGroup>();
            //m_content = m_LayoutGroup.gameObject.GetRectTransform();
            m_content = m_LayoutGroup.gameObject.GetComponent<RectTransform>();
            if (m_LayoutGroup != null)
                m_oldPadding = m_LayoutGroup.padding;

            m_scrollRect = trans.GetComponentInParent<ScrollRect>();
            if (m_scrollRect != null && m_LayoutGroup != null)
            {
//                 if (m_scrollRect.gameObject.layer != GameSetting.LAYER_VALUE_UI)
//                     m_scrollRect.gameObject.ApplyLayer(GameSetting.LAYER_VALUE_UI);
                m_scrollRect.gameObject.layer = LayerMask.NameToLayer("UI");

                m_scrollRect.decelerationRate = 0.2f;
                m_tranScrollRect = m_scrollRect.GetComponent<RectTransform>();
                m_isVertical = m_scrollRect.vertical;
                var layoutgroup = m_LayoutGroup as GridLayoutGroup;
                if (layoutgroup != null)
                {
                    m_itemSpace = (int)(m_isVertical ? (layoutgroup.cellSize.y + layoutgroup.spacing.y) : (layoutgroup.cellSize.x + layoutgroup.spacing.x));
                    m_viewItemCount = Mathf.CeilToInt(ViewSpace / m_itemSpace);
                }
                //LoggerHelper.Error("view: "+ViewSpace+", item: "+m_itemSpace);
            }
            else
            {
                Debug.LogError("scrollRect is null or verticalLayoutGroup is null");
                //if (gameObject.layer != GameSetting.LAYER_VALUE_UI)
                //    gameObject.ApplyLayer(GameSetting.LAYER_VALUE_UI);
                //            m_canvas = gameObject.AddMissingComponent<Canvas>();
                //            m_canvas.overridePixelPerfect = true;
                //            m_canvas.pixelPerfect = true;
                //单独一个Canvas，在滚动的时候不开启像素对齐
                //            gameObject.AddMissingComponent<SortingOrderRenderer>();
                //            SortingOrderRenderer.RebuildAll();
            }
        }

        void Start()
        {
            if (m_scrollRect != null)
            {
                if (useLoopItems)
                    m_scrollRect.onValueChanged.AddListener(OnScroll);
                if (m_toggleGroup != null)
                    m_toggleGroup.allowSwitchOff = useLoopItems;
            }
        }

        /// <summary>
        /// 设置渲染项
        /// </summary>
        /// <param name="goItemRender"></param>
        /// <param name="itemRenderType"></param>
        public void SetItemRender(GameObject goItemRender, Type itemRenderType)
        {
            m_goItemRender = goItemRender;
            m_itemRenderType = itemRenderType.IsSubclassOf(typeof(ItemRender)) ? itemRenderType : null;
            LayoutElement le = goItemRender.GetComponent<LayoutElement>();
            var layoutGroup = m_LayoutGroup as HorizontalOrVerticalLayoutGroup;
            if (le != null && layoutGroup != null)
            {
                if (m_tranScrollRect == null)
                {
                    m_scrollRect = transform.GetComponentInParent<ScrollRect>();
                    m_tranScrollRect = m_scrollRect.GetComponent<RectTransform>();
                }
                m_itemSpace = (int)(le.preferredHeight + (int)layoutGroup.spacing);
                m_viewItemCount = Mathf.CeilToInt(ViewSpace / m_itemSpace);
            }
        }

        public void SetItemClickSound(string sound)
        {
            //m_itemClickSound = sound;
        }

        //有效数据数目(可见、没有设置ignorelayout=true),非及时，需要延时一帧使用
        //public int ValidCount { get { return m_LayoutGroup.ChildrenCount; } }

        /// <summary>
        /// 数据项
        /// </summary>
        public object[] Data
        {
            set
            {
                m_data = value;
                UpdateView();

                if (autoSelectFirst && m_data.Length > 0)
                {
                    if (m_data[0] != m_selectedData)
                        SelectItem(m_data[0]);
                    SetToggle(0, true);
                }
                else if (m_data.Length == 0)
                    SelectItem(null);
            }
            get { return m_data; }
        }

        public List<ItemRender> ItemRenders
        {
            get { return m_items; }
        }

        public void Remove(object item)
        {
            if (item == null || Data == null)
            {
                return;
            }
            List<object> newList = new List<object>(Data);
            if (newList.Contains(item))
            {
                newList.Remove(item);
            }
            Data = newList.ToArray();
        }

        /// <summary>
        /// 当前选择的数据项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T SelectedData<T>()
        {
            return (T)m_selectedData;
        }

        /// <summary>
        /// 下一帧把指定项显示在最顶端并选中，这个比ResetScrollPosition保险，否则有些在UI一初始化完就执行的操作会不生效
        /// </summary>
        /// <param name="index"></param>
        public void ShowItemOnTop(int index)
        {
            //TimerManager.SetTimeOut(0.01f, () =>
            //{
            //    if (m_data.Length > index)
            //        SelectItem(m_data[index]);
            //    ResetScrollPosition(index);
            //});
        }

        public void ShowItemOnTop(object data)
        {
            if (m_data == null || m_data.Length == 0)
            {
                return;
            }
            var target_index = -1;
            for (int i = 0; i < m_data.Length; i++)
            {
                if (m_data[i] == data)
                {
                    target_index = i;
                    break;
                }
            }
            if (target_index != -1)
            {
                //TimerManager.SetTimeOut(0.01f, () =>
                //{
                //    SelectItem(m_data[target_index]);
                //    ResetScrollPosition(target_index);
                //});
            }
        }

        /// <summary>
        /// 重置滚动位置，
        /// </summary>
        /// <param name="top">true则跳转到顶部，false则跳转到底部</param>
        public void ResetScrollPosition(bool top = true)
        {
            if (m_data == null)
                return;
            int index = top ? 0 : m_data.Length;
            // LoggerHelper.Error("len: "+index);
            ResetScrollPosition(index);
        }

        /// <summary>
        /// 重置滚动位置，如果同时还要赋值新的Data，请在赋值之前调用本方法
        /// </summary>
        public void ResetScrollPosition(int index)
        {
            if (m_data == null)
                return;
            var unitIndex = Mathf.Clamp(index / ConstraintCount, 0, DataUnitCount - m_viewItemCount > 0 ? DataUnitCount - m_viewItemCount : 0);
            var value = (unitIndex * m_itemSpace) / (Mathf.Max(ViewSpace, ContentSpace - ViewSpace));
            value = Mathf.Clamp01(value);

            //特殊处理无法使指定条目置顶的情况——拉到最后
            if (unitIndex != index / ConstraintCount)
                value = 1;

            if (m_scrollRect)
            {
                if (m_isVertical)
                    m_scrollRect.verticalNormalizedPosition = 1 - value;
                else
                    m_scrollRect.horizontalNormalizedPosition = value;
            }

            m_startIndex = unitIndex * ConstraintCount;
            UpdateView();
        }

        //    private void Update()
        //    {
        //        //只在可滚动的情况下执行
        //        if (m_canvas != null)
        //        {
        //            if (m_content.anchoredPosition == m_lastContentPos)
        //            {
        //                if (!m_canvas.pixelPerfect)
        //                    m_canvas.pixelPerfect = true;
        //            }
        //            else
        //                m_lastContentPos = m_content.anchoredPosition;
        //        }
        //    }

        /// <summary>
        /// 更新视图
        /// </summary>
        public void UpdateView()
        {
            if (useLoopItems)
            {
                if (m_data != null)
                    m_startIndex = Mathf.Max(0, Mathf.Min(m_startIndex / ConstraintCount, DataUnitCount - m_viewItemCount - CacheUnitCount)) * ConstraintCount;
                var frontSpace = m_startIndex / ConstraintCount * m_itemSpace;
                var behindSpace = Mathf.Max(0, m_itemSpace * (DataUnitCount - CacheUnitCount) - frontSpace - (m_itemSpace * m_viewItemCount));
                if (m_isVertical)
                    m_LayoutGroup.padding = new RectOffset(m_oldPadding.left, m_oldPadding.right, frontSpace, behindSpace);
                else
                    m_LayoutGroup.padding = new RectOffset(frontSpace, behindSpace, m_oldPadding.top, m_oldPadding.bottom);
            }
            else
                m_startIndex = 0;

            if (m_goItemRender == null || m_itemRenderType == null || m_data == null || m_content == null)
                return;

            int itemLength = useLoopItems ? m_viewItemCount * ConstraintCount + CacheCount : m_data.Length;
            itemLength = Mathf.Min(itemLength, m_data.Length);
            //LoggerHelper.Error("len: "+itemLength);
            for (int i = itemLength; i < m_items.Count; i++)
            {
                Destroy(m_items[i].gameObject);
                m_items[i] = null;
            }
            for (int i = m_items.Count - 1; i >= 0; i--)
            {
                if (m_items[i] == null)
                    m_items.RemoveAt(i);
            }

            for (int i = 0; i < itemLength; i++)
            {
                var index = m_startIndex + i;
                if (index >= m_data.Length || index < 0)
                    continue;
                if (i < m_items.Count)
                {
                    m_items[i].SetData(m_data[index]);

                    if (useClickEvent || autoSelectFirst)
                        SetToggle(i, m_selectedData == m_data[index]);
                }
                else
                {
                    var go = Instantiate(m_goItemRender) as GameObject;
                    go.name = m_goItemRender.name;
                    go.transform.SetParent(m_content, false);
                    go.SetActive(true);
                    var script = go.AddComponent(m_itemRenderType) as ItemRender;
                    if (!go.activeInHierarchy)
                        script.Awake();
                    script.SetData(m_data[index]);
                    script.m_owner = this;
                    if (useClickEvent)
                        UGUIClickHandler.Get(go, m_itemClickSound).onPointerClick += OnItemClick;
                    if (m_toggleGroup != null)
                    {
                        var toggle = go.GetComponent<Toggle>();
                        if (toggle != null)
                        {
                            toggle.group = m_toggleGroup;

                            //使用循环模式的话不能用渐变效果，否则视觉上会出现破绽
                            if (useLoopItems)
                                toggle.toggleTransition = Toggle.ToggleTransition.None;
                        }
                    }
                    m_items.Add(script);
                }
            }
        }

        private void OnScroll(Vector2 data)
        {
            //if (m_canvas != null && m_canvas.pixelPerfect)
            //    m_canvas.pixelPerfect = false;
            var value = (ContentSpace - ViewSpace) * (m_isVertical ? data.y : 1 - data.x);
            var start = ContentSpace - value - ViewSpace;
            var startIndex = Mathf.FloorToInt(start / m_itemSpace) * ConstraintCount;
            startIndex = Mathf.Max(0, startIndex);

            if (startIndex != m_startIndex)
            {
                m_startIndex = startIndex;
                UpdateView();
            }
        }

        private void SelectItem(object renderData)
        {
            m_selectedData = renderData;
            if (onItemSelected != null)
                onItemSelected(m_selectedData);
        }

        private void OnItemClick(GameObject target, BaseEventData baseEventData)
        {
            var renderData = target.GetComponent<ItemRender>().m_renderData;
            if (useLoopItems && renderData == m_selectedData)
            {
                var toggle = target.GetComponent<Toggle>();
                if (toggle)
                    toggle.isOn = true;
            }
            SelectItem(renderData);
        }

        private void SetToggle(int index, bool value)
        {
            if (index < m_items.Count)
            {
                var toggle = m_items[index].GetComponent<Toggle>();
                if (toggle)
                    toggle.isOn = value;
            }
        }

        void Destroy()
        {
            onItemSelected = null;
            m_items.Clear();
        }

        /// <summary>
        /// 选择指定项
        /// </summary>
        /// <param name="index"></param>
        public void Select(int index)
        {
            if (index >= m_data.Length)
                return;

            if (m_data[index] != m_selectedData)
                SelectItem(m_data[index]);

            UpdateView();
        }

        /// <summary>
        /// 开启或关闭某一项的响应
        /// </summary>
        /// <param name="index"></param>
        public void Enable(int index, bool isEnable = true)
        {
            if (index < m_items.Count)
            {
                var toggle = m_items[index].GetComponent<Toggle>();
                if (toggle)
                {
                    toggle.isOn = isEnable;
                    toggle.enabled = isEnable;
                    if (isEnable)
                    {
                        UGUIClickHandler.Get(toggle.gameObject, m_itemClickSound).onPointerClick += OnItemClick;
                    }
                    else
                    {
                        UGUIClickHandler.Get(toggle.gameObject, m_itemClickSound).RemoveAllHandler();
                    }
                }
            }
        }

        /// <summary>
        /// 选择指定项
        /// </summary>
        /// <param name="renderData"></param>
        public void Select(object renderData)
        {
            if (renderData == null)
            {
                SelectItem(null);
                UpdateView();
                return;
            }
            for (int i = 0; i < m_data.Length; i++)
            {
                if (m_data[i] == renderData)
                {
                    SelectItem(m_data[i]);
                    UpdateView();
                    break;
                }
            }
        }
    }
}