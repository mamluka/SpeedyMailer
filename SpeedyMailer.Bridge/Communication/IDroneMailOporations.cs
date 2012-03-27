using System;

namespace SpeedyMailer.Bridge.Communication
{
    public interface IDroneMailOporations
    {
        void Preform(DroneSideOporationBase poolSideOporation);
        Action StopCurrentJob { get; set; }
    }
}