using System;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class PhotonClient : LoadBalancingClient
{
    //SendMessagequeue var in spacehub would send acks only when less of the network was required, ask photon about this behaviour and how to do it now 
    //that the used var has been deprecated.
    public Action<WebRpcResponse> onWebRpcResponse;
    
    public override void OnOperationResponse(OperationResponse operationResponse)
    {
        base.OnOperationResponse(operationResponse);

        switch (operationResponse.OperationCode)
        {
            case (byte)OperationCode.WebRpc:
                if (operationResponse.ReturnCode == 0)
                {
                    onWebRpcResponse?.Invoke(new WebRpcResponse(operationResponse));
                }
                break;
        }
    }
}
