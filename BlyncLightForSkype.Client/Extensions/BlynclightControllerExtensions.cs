using Blynclight;
using BlyncLightForSkype.Client.Models;

namespace BlyncLightForSkype.Client.Extensions
{
    public static class BlynclightControllerExtensions
    {
        public static void ResetAll(this BlynclightController controller)
        {
            for (var i = 0; i < controller.aoDevInfo.Length; i++)
            {
                controller.ResetLight(i);
            }           
        }

        public static void SetStatusCallInProgress(this BlynclightController controller)
        {
            SetCallStatus(controller, FlashSpeed.Slow);
        }

        public static void SetStatusRinging(this BlynclightController controller)
        {
            SetCallStatus(controller, FlashSpeed.Medium);
        }

        public static void SetStatusCallMissed(this BlynclightController controller)
        {
            for (var i = 0; i < controller.aoDevInfo.Length; i++)
            {
                controller.TurnOnOrangeLight(i);
                SetFlashspeed(controller, i, FlashSpeed.Slow);
            }
        }

        public static void SetStatusAway(this BlynclightController controller)
        {
            for (var i = 0; i < controller.aoDevInfo.Length; i++)
            {
                controller.TurnOnYellowLight(i);
                SetFlashspeed(controller, i, FlashSpeed.None);
                controller.SetLightDim(i);
            }
        }

        public static void SetStatusOnline(this BlynclightController controller)
        {
            for (var i = 0; i < controller.aoDevInfo.Length; i++)
            {
                controller.TurnOnGreenLight(i);
                SetFlashspeed(controller, i, FlashSpeed.None);
                controller.SetLightDim(i);
            }
        }

        public static void SetStatusBusy(this BlynclightController controller)
        {
            for (var i = 0; i < controller.aoDevInfo.Length; i++)
            {
                controller.TurnOnRedLight(i);
                SetFlashspeed(controller, i, FlashSpeed.None);
                controller.SetLightDim(i);
            }
        }

        public static void SetStatusOffline(this BlynclightController controller)
        {
            for (var i = 0; i < controller.aoDevInfo.Length; i++)
            {
                controller.TurnOnMagentaLight(i);
                SetFlashspeed(controller, i, FlashSpeed.Slow);
                controller.ClearLightDim(i);
            }
        }

        private static void SetCallStatus(BlynclightController controller, FlashSpeed flashSpeed)
        {
            for (var i = 0; i < controller.aoDevInfo.Length; i++)
            {
                controller.TurnOnRedLight(i);
                controller.ClearLightDim(i);
                SetFlashspeed(controller, i, flashSpeed);
            }
        }

        private static void SetFlashspeed(BlynclightController controller, int deviceIndex, FlashSpeed flashSpeed)
        {
            if (flashSpeed == FlashSpeed.None)
            {
                controller.StopLightFlash(deviceIndex);
            }
            else
            {
                controller.SelectLightFlashSpeed(deviceIndex, (byte)(flashSpeed + 1));
                controller.StartLightFlash(deviceIndex);
            }
        }
    }
}
