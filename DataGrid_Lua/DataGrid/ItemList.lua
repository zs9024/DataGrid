--region ItemList.lua
--Date 2016.11.7
--列表数据和显示控制基类，创建DataGrid

local DataGrid = require "View/Common/DataGrid/DataGrid"
local ItemList = class("ItemList");

function ItemList:ctor(scrollTrans,ItemRender) 
    if scrollTrans == nil or ItemRender == nil then
        logError("ItemList:ctor args is nil...");
        return;
    end
    self.rectTrans = scrollTrans
    self.gameObject = scrollTrans.gameObject;
    self.ItemRender = ItemRender;

    self:Init();
end

function ItemList:Init()   
    log("ItemList:Init..."); 
    self:InitUI();
    self:Create();
    if self.dataGrid then
        self.dataGrid:Start();
        self.dataGrid:SetItemRender(self.itemGo, self.ItemRender);
    end
end

function ItemList:InitUI()
    log("ItemList:InitUI..."); 
    self.contentTrans = self.rectTrans:Find("Content"):GetComponent("RectTransform");
    self.scrollRect = self.rectTrans:GetComponent("ScrollRect");
    self.itemGo = self.rectTrans:Find("Content/Item").gameObject;
end

--创建列表类，子类可按需重写
function ItemList:Create()
    local option = self:SetOptions(true,true,LayoutGroupMode.Vertical);
    self:CreateDataGrid(option);
end

function ItemList:CreateDataGrid(option)
    self.dataGrid = DataGrid.New(self.gameObject,option);   
end

--设置列表属性，创建之前调用
function ItemList:SetOptions(useLoopItems,useClickEvent,layoutGroupMode)
    local option = {};
    option.useLoopItems = useLoopItems;
    option.useClickEvent = useClickEvent;
    option.layoutGroupMode = layoutGroupMode;
    return option;
end

--设置点击事件，创建之后调用
function ItemList:SetItemClickEvent(callback)
    self.dataGrid:SetItemClickEvent(
        function (gameObject,data)          
            if callback ~= nil then
                callback(gameObject,data);
            end
        end
    );
end

function ItemList:Show(datas)
    if self.gameObject ~= nil and self.gameObject.activeSelf == false then
        self.gameObject:SetActive(true);
    end

    self.dataGrid:SetDatas(datas);
    --self.dataGrid:SetPositionTopOrBottom(false);
end

function ItemList:Clear(args)

end

function ItemList:Destroy(args)

end

return ItemList;
--endregion
