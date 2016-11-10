--region DataGrid.lua
--Date  2016.11.4
--无限循环列表 --zs

local DataGrid = class("DataGrid");
-------------------------------------------------------------------------------------------------------------------------------
--region 属性
local  m_content;           --RectTransform
local  m_toggleGroup;       --ToggleGroup
local  m_datas = {};         --object[]
local  m_goItemRender;      --GameObject
local  m_items = {};        --List<ItemRender>
local  m_selectedData;      --object
local  m_LayoutGroup;       --LayoutGroup
local  m_oldPadding;        --RectOffset

--下面的属性会需要父对象上有ScrollRect组件
local  m_scrollRect;        --ScrollRect    //父对象上的，不一定存在
local  m_tranScrollRect;    --RectTransform
local  m_itemSpace;         --int           //每个Item的空间
local  m_viewItemCount;     --int           //可视区域内Item的数量（向上取整）
local  m_isVertical;        --bool          //是否是垂直滚动方式，否则是水平滚动
local  m_startIndex = 0;        --int       //数据数组渲染的起始下标
local  m_itemClickSound = "";--string       //AudioConst.btnClick;

local  Resolution = Vector2.New(1242, 2208);
--endregion
-------------------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------------------
--region 构造和初始化
function DataGrid:ctor(go,option)
    self.useLoopItems = option.useLoopItems;               --是否使用无限循环列表，对于列表项中OnDataSet方法执行消耗较大时不宜使用，因为OnDataSet方法会在滚动的时候频繁调用
    self.useClickEvent = option.useClickEvent;              --列表项是否监听点击事件
    self.autoSelectFirst = true;            --创建时是否自动选中第一个对象
    self.layoutGroupMode = option.layoutGroupMode;    --布局方式，垂直，水平或网格

    self.gameObject = go;
    self.transform = go.transform;

    self:Awake();
    self:Start();
end

function DataGrid:Awake()
    UIHelper.AddMissingComponent(self.gameObject,"CanvasRenderer",UnityEngine.CanvasRenderer);
    m_toggleGroup = self.gameObject:GetComponent("ToggleGroup");  
    m_scrollRect = self.gameObject:GetComponent("ScrollRect");

    if self.layoutGroupMode == LayoutGroupMode.Vertical then
        m_LayoutGroup = self.gameObject:GetComponentInChildren(typeof(UnityEngine.UI.VerticalLayoutGroup));
    elseif self.layoutGroupMode == LayoutGroupMode.Horizon then

    else
        
    end
    m_content = m_LayoutGroup.gameObject:GetComponent("RectTransform");

    if m_LayoutGroup ~= nil then
        m_oldPadding = m_LayoutGroup.padding;
    end

    if m_scrollRect == nil or m_LayoutGroup == nil then
        logError("DataGrid:Awake could not find component scrollrect or layoutgroup !");
        return;
    end

    m_scrollRect.decelerationRate = 0.2;
    m_tranScrollRect = m_scrollRect:GetComponent("RectTransform");
    m_isVertical = m_scrollRect.vertical;
end

function DataGrid:Start()
    if m_scrollRect ~= nil then
        if self.useLoopItems then
            m_scrollRect.onValueChanged:AddListener(function (args)
                self:OnScroll(args);
            end);
        end
        if m_toggleGroup ~= nil then
            m_toggleGroup.allowSwitchOff = self.useLoopItems;
        end
    end
end
--endregion
-------------------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------------------
--region 内容和数据量计算

--内容长度
local function ContentSpace()     
    return m_isVertical and m_content.sizeDelta.y or m_content.sizeDelta.x;  
end

--可见区域长度
local function ViewSpace()
    return m_isVertical and (m_tranScrollRect.sizeDelta.y + Resolution.y) or (m_tranScrollRect.sizeDelta.x + Resolution.x);
end

--约束常量（固定的行（列）数）
local function ConstraintCount()
    --return m_LayoutGroup == nil and 1 or ((m_LayoutGroup is GridLayoutGroup) and (m_LayoutGroup as GridLayoutGroup).constraintCount or 1);
    return 1;
end

--数据量个数
local function DataCount()
    return m_datas == nil and 0 or #m_datas;
end

--缓存数量
local function CacheCount()
    return ConstraintCount() + DataCount() % ConstraintCount();
end

--缓存单元的行（列）数
local function CacheUnitCount()
    return m_LayoutGroup == nil and 1 or math.ceil(CacheCount() / ConstraintCount());
end

--数据单元的行（列）数
local function DataUnitCount()
    return m_LayoutGroup == nil and DataCount() or math.ceil(DataCount() / ConstraintCount());
end

--endregion
-------------------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------------------
--region 显示相关

--设置渲染显示项
function DataGrid:SetItemRender(goItemRender,ItemRender)
    m_goItemRender = goItemRender;
    self.ItemRender = ItemRender;
    local layElem = m_goItemRender:GetComponent("LayoutElement");

    if layElem ~= nil and m_LayoutGroup ~= nil then
        m_itemSpace = layElem.preferredHeight + m_LayoutGroup.spacing;
        m_viewItemCount = math.ceil(ViewSpace() / m_itemSpace);
    end
end

--设置数据，显示列表
function DataGrid:SetDatas(datas)
    if datas == nil then
        logError("DataGrid:SetDatas datas is nil !");
        return;
    end

    m_datas = datas;
    self:UpdateView();
end

--更新列表视图
function DataGrid:UpdateView()
    if m_goItemRender == nil or m_content == nil then
        return;
    end

    if self.useLoopItems then
        m_startIndex = math.max(0, math.min(m_startIndex / ConstraintCount(), DataUnitCount() - m_viewItemCount - CacheUnitCount())) * ConstraintCount();
        local frontSpace = m_startIndex / ConstraintCount() * m_itemSpace;
        local behindSpace = math.max(0, m_itemSpace * (DataUnitCount() - CacheUnitCount()) - frontSpace - (m_itemSpace * m_viewItemCount));
        if m_isVertical then
            m_LayoutGroup.padding = UnityEngine.RectOffset.New(m_oldPadding.left, m_oldPadding.right, frontSpace, behindSpace);
        else
            m_LayoutGroup.padding = UnityEngine.RectOffset.New(frontSpace, behindSpace, m_oldPadding.top, m_oldPadding.bottom);
        end
    else
        m_startIndex = 0;
    end

    local itemLength = self.useLoopItems and m_viewItemCount * ConstraintCount() + CacheCount() or #m_datas;
    itemLength = math.min(itemLength, #m_datas);

    for i = itemLength + 1, #m_items do
        Util.Destroy(m_items[i].gameObject);
        m_items[i] = nil;
        table.remove(m_items,i);
    end

    for i = 1,  itemLength do 
        while true do
            local index = m_startIndex + i;
            if index > #m_datas or index < 0 then
                --continue;     lua 没有continue
                break;
            end
            if i < #m_items + 1 then        
                m_items[i]:SetData(m_datas[index]);     
            else
                local go = GameObject.Instantiate(m_goItemRender);   
                go.name = m_goItemRender.name;
                go.transform:SetParent(m_content, false);
                go:SetActive(true); 
                
                local itemRender = self.ItemRender.New(go);    
                itemRender:Awake();
                itemRender:SetData(m_datas[index]);
                itemRender.m_owner = self;  

                if self.useClickEvent then
                    AddObjClick(go,function (gameObject,args)
                        self:OnItemClick(gameObject,args);
                    end,itemRender);
                end

                --m_items[i] = itemRender;
                table.insert(m_items,itemRender);
            end
           
            break; 
        end
    end
end

--响应滑动事件
function DataGrid:OnScroll(data)
    --log("DataGrid:OnScroll...");

    local value = (ContentSpace() - ViewSpace()) * (m_isVertical and data.y or 1 - data.x);
    local start = ContentSpace() - value - ViewSpace();
    local startIndex = math.floor(start / m_itemSpace) * ConstraintCount();
    startIndex = math.max(0, startIndex);

    if startIndex ~= m_startIndex then
        m_startIndex = startIndex;
        self:UpdateView();
    end
end

--endregion
-------------------------------------------------------------------------------------------------------------------------------
-------------------------------------------------------------------------------------------------------------------------------
--region 辅助方法

function DataGrid:GetItemRenders()
    return m_items;
end

function DataGrid:RemoveItem(go)
    if go == nil then
        logError("DataGrid:RemoveItem go is nil...");
        return;
    end

    local idx = 0;
    for k,v in pairs(m_items) do
        if v ~= nil and v.gameObject == go then
            idx = k;
            break;
        end
    end
    if idx ~= 0 then
        table.remove(m_items,idx);
    else
        logWarn("DataGrid:RemoveItem does not exsit this item in m_items...");
    end
end

--重置滚动位置，如果同时还要赋值新的datas，请在赋值之前调用本方法
--初始创建时不需要重置,如果在SetDatas以后马上调用，不起效果（不知道为什么reset以后会响应onscroll，多刷新一遍，延迟一帧调用即可）
function DataGrid:ResetScrollPosition(index)
    if m_datas == nil then
        return;
    end

    local unitIndex = self:math_clamp(index / ConstraintCount(), 0, DataUnitCount() - m_viewItemCount > 0 and DataUnitCount() - m_viewItemCount or 0);
    local value = (unitIndex * m_itemSpace) / (math.max(ViewSpace(), ContentSpace() - ViewSpace()));
    value = self:math_clamp(value,0,1);

    --特殊处理无法使指定条目置顶的情况——拉到最后
    if unitIndex ~= index / ConstraintCount() then
        value = 1;
    end

    if m_isVertical then
        m_scrollRect.verticalNormalizedPosition = 1 - value;
    else
        m_scrollRect.horizontalNormalizedPosition = value;
    end

    m_startIndex = unitIndex * ConstraintCount();
    self:UpdateView();
end

function DataGrid:SetPositionTopOrBottom(top)
    if m_datas == nil then
        return;
    end

    local index = top and 0 or #m_datas;
    self:ResetScrollPosition(index);
end

function DataGrid:SetItemClickEvent(callback)
    self.itemClickEvent = callback;
end

function DataGrid:OnItemClick(gameObject,itemRender)
    if itemRender == nil then
        logError("DataGrid:OnItemClick the param itemRender is nil...");
        return;
    end
    
    if self.useClickEvent then
        if self.itemClickEvent ~= nil then
            self.itemClickEvent(gameObject,itemRender.m_renderData);
        end
    end
end

--lua数学库没有clamp？
function DataGrid:math_clamp( _n, _min, _max )
    if type(_n) ~= "number" or type(_min) ~= "number" or type(_max) ~= "number" then
        logError("math_clamp args type is error...");
        return;
    end

    if _n < _min then
        return _min;
    elseif _n > _max then
        return _max;
    else
        return _n;
    end
end
--endregion
-------------------------------------------------------------------------------------------------------------------------------
return DataGrid;

--endregion
