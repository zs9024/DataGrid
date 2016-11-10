--region DGTest.lua
--Date 2016.11.7
--循环列表测试--controller，操作itemlist,itemrender
local DGItemList = require "View/Common/DataGrid/DGTest/DGItemList"
local DGItemRender = require "View/Common/DataGrid/DGTest/DGItemRender"
local DGPanel = require "View/Common/DataGrid/DGTest/DGPanel"

DGTest = {};
local this = DGTest;

function DGTest.New(args)
    return this;
end

function DGTest.Awake(args)
    DGPanel.New(panelMgr.Parent,this.OnCreate);
end

function DGTest.OnCreate(args)
    if args == nil then
        logError("LoginCtrl.OnCreate args is nil !!!");
        return;
    end

    this.dgPanel = args;

    this.Init();
    this.Show();
end

function DGTest.Init()
    this.dgItemList = DGItemList.New(this.dgPanel.rectTrans,DGItemRender);
end

function DGTest.Show()
    local datas = {};
    for i = 1,100 do 
        datas[i] = "DataGrid data -->"..i;
    end
    this.dgItemList:Show(datas);
end


return DGTest;

--endregion
