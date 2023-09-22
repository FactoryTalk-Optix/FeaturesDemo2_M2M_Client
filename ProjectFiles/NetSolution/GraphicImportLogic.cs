#region StandardUsing
using System;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.OPCUAClient;
using FTOptix.OPCUAServer;
using FTOptix.Alarm;
#endregion

public class GraphicImportLogic : FTOptix.NetLogic.BaseNetLogic
{
    public override void Start()
    {
        lock (connectionLock)
        {
            var client = Project.Current.Get<OPCUAClient>("OPC-UA/OPCUAClient1");
            client.ConnectionStatusVariable.VariableChange += ConnectionStatusVariableChange;
            connectionStatusObserverRegistered = true;

            if (client.ConnectionStatus == ConnectionStatus.Connected)
            {
                client.ConnectionStatusVariable.VariableChange -= ConnectionStatusVariableChange;
                connectionStatusObserverRegistered = false;
                LoadPanel();
            }
        }
    }

    public override void Stop()
    {
        lock (connectionLock)
        {
            RemoveConnectionStatusObserver();
        }
    }

    private void ConnectionStatusVariableChange(object sender, VariableChangeEventArgs e)
    {
        lock (connectionLock)
        {
            if ((ConnectionStatus)e.Variable.Value.Value == ConnectionStatus.Connected)
            {
                RemoveConnectionStatusObserver();
                LoadPanel();
            } else {
                var panelLoader = LogicObject.Owner.Get<PanelLoader>("Background/PanelLoaderHMI1");
                panelLoader.ChangePanel(NodeId.Empty, NodeId.Empty);
                panelLoaded = false;
            }
        }
    }

    private void RemoveConnectionStatusObserver()
    {
        if (!connectionStatusObserverRegistered)
            return;

        var client = Project.Current.Get<OPCUAClient>("OPC-UA/OpcUaClient1");
        if (client != null)
            client.ConnectionStatusVariable.VariableChange -= ConnectionStatusVariableChange;

        connectionStatusObserverRegistered = false;
    }

    private void LoadPanel()
    {
        if (panelLoaded)
            return;
        var panelToLoad = Project.Current.Get("UI/MachineToMachine/Main");
        var panelLoader = LogicObject.Owner.Get<PanelLoader>("Background/PanelLoaderHMI1");
        panelLoader.ChangePanel(panelToLoad.NodeId, NodeId.Empty);
        panelLoaded = true;
    }

    private readonly object connectionLock = new object();
    private bool connectionStatusObserverRegistered = false;
    private bool panelLoaded = false;
}
