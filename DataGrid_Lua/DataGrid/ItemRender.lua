--region ItemRender.lua
--Date 2016.11.5
--列表项显示类

local ItemRender = class("ItemRender");

function ItemRender:ctor(go)
    self.gameObject = go;
    self.transform = go.transform;

    self.m_renderData = nil;
    self.m_owner = nil;
end

function ItemRender:Awake()
    
end

function ItemRender:OnSetData(data)
    
end

function ItemRender:SetData(data)
    self.m_renderData = data;
    self:OnSetData(data);
end


return ItemRender;

--endregion
