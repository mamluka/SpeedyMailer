using System;

namespace SpeedyMailer.Core.Protocol
{
    public interface IDroneMailOporations
    {
        void Preform(DroneSideOporationBase poolSideOporation);
        Action StopCurrentJob { get; set; }
    }
}