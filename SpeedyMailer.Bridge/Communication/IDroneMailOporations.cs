using System;

namespace SpeedyMailer.Bridge.Communication
{
    public interface IDroneMailOporations
    {
        Action StopCurrentJob { get; set; }
        void Preform(DroneSideOporationBase poolSideOporation);
    }
}