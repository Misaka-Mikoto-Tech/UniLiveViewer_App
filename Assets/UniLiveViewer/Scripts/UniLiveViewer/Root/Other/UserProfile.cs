
namespace UniLiveViewer
{
    public class UserProfile
    {
        public int LanguageCode = 0;
        public string LastSceneName = "LiveScene";
        public float InitCharaSize = 1.15f;
        public float CharaShadow = 1.25f;
        public int CharaShadowType = 1;
        public bool IsSmoothVMD = false;
        public bool IsVRM10 = true;
        public float VMDScale = 0.750f;
        public bool TouchVibration = true;
        public bool StepSE = true;

        public bool scene_crs_particle = true;
        public bool scene_crs_laser = true;
        public bool scene_crs_reflection = true;
        public bool scene_crs_sonic = true;
        public bool scene_crs_manual = true;

        public bool scene_kagura_particle = true;
        public bool scene_kagura_sea = true;
        public bool scene_kagura_reflection = true;

        public bool scene_view_led = true;

        public bool scene_gym_whitelight = true;
    }
}