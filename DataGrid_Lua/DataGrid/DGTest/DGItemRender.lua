--region DGItemRender.lua
--Date 2016.11.7
----循环列表测试--列表显示

local ItemRender = require "View/Common/DataGrid/ItemRender"
local DGItemRender = class("DGItemRender",ItemRender)

function DGItemRender:ctor(go)
	DGItemRender.super.ctor(self,go);

    
end

--重写父类
function DGItemRender:Awake()
    self:InitUI();
end

function DGItemRender:InitUI()
    --log("DGItemRender:InitUI...");
    self.label = self.transform:Find("Text"):GetComponent("Text");
end

--重写父类
function DGItemRender:OnSetData(data)
    --log("DGItemRender:OnSetData...");
    self.label.text = data;
end

return DGItemRender;

--endregion
