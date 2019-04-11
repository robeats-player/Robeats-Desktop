using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robeats_Desktop.Network.Frames
{
    public enum ProtocolRequest
    {
        DeviceDiscovery = 0x1,
        DeviceDiscoveryReply = 0x11,
        RequestSongList = 0x2,
        ReplySongList =0x102,
        SyncConfirm = 0xF
    }

    public class StateProtocol
    {
        public ProtocolRequest ProtocolType { get; set; }
        public string DeviceName { get; set; }
        public byte DeviceId { get; set; }

        public StateProtocol()
        {
            
        }

        public StateProtocol(ProtocolRequest protocolRequest)
        {
            ProtocolType = protocolRequest;
        }

        public StateProtocol(string deviceName)
        {
            DeviceName = deviceName;
        }

        public StateProtocol(byte deviceId)
        {
            DeviceId = deviceId;
        }

        public StateProtocol(ProtocolRequest protocolRequest, string deviceName, byte deviceId)
        {
            ProtocolType = protocolRequest;
            DeviceName = deviceName;
            DeviceId = deviceId;
        }

        public byte[] ToBytes()
        {
            var bytes = new byte[18];
            bytes[0] = (byte) ProtocolType;
            bytes[1] = DeviceId;
            var nameBytes = Encoding.ASCII.GetBytes(DeviceName);
            nameBytes.CopyTo(bytes, 2);
            return bytes;
        }

        public static StateProtocol FromBytes(byte[] bytes)
        {
            var protocolType = (ProtocolRequest) bytes[0]; 
            var deviceId = bytes[1];
            var deviceName = Encoding.ASCII.GetString(bytes, 2,16);
            return new StateProtocol(protocolType, deviceName, deviceId);
        }
    }
}
