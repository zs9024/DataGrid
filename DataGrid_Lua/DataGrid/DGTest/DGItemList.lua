--region DGItemList.lua
--Date 2016.11.7
--循环列表测试--列表控制

local ItemList = require "View/Common/DataGrid/ItemList"
local DGItemList = class("DGItemList",ItemList)

function DGItemList:ctor(scrollTrans,ItemRender)
	DGItemList.super.ctor(self,scrollTrans,ItemRender);

    
end

--在这里初始化组件，组件位置，名字可能跟父类查找的不一致
function DGItemList:InitUI()
    log("DGItemList:InitUI..."); 
    DGItemList.super.InitUI(self);
end

--在这里改datagrid属性，比如不用循环，加入点击等
function DGItemList:Create()
    DGItemList.super.Create(self);

    --设置点击
    self:SetItemClickEvent(self.OnItemClick);
end

function DGItemList:OnItemClick(args)
    log("DGItemList:OnItemClick...");
end


return DGItemList;
--endregion
