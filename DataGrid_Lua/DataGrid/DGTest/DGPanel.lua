--region *.lua
--Date
--此文件由[BabeLua]插件自动生成

local UIEntity = require "View/UIEntity"
local DGPanel = class("DGPanel",UIEntity);

function DGPanel:ctor(parent,callback)
	log("DGPanel:ctor---->>>")
	self.InitFinish = callback;
	DGPanel.super.ctor(self,"ScrollPanel",parent,1,true)
end

function DGPanel:Init()
    logWarn("DGPanel Init---->>>");
	
	self.transform.offsetMin = Vector2.New(0, 0);
	self.transform.offsetMax = Vector2.New(0, 0);
	self.transform.anchorMin = Vector2.New(0, 0);
	self.transform.anchorMax = Vector2.New(1, 1);

	self:InitPanel();
	if self.InitFinish ~= nil then
		self.InitFinish(self);
	end
	
end

function DGPanel:InitPanel()
	log("DGPanel:InitPanel---->>>")
    self.rectTrans = self.transform:GetComponent("RectTransform");
    
end

function DGPanel:Close()
    DGPanel.super.dispose(self)
	Core.ResManager.DestroyAssetBundle("sutest",true);
end

return DGPanel;

--endregion
