
namespace UniLiveViewer
{
    public class UserProfile
    {
        public int LanguageCode = 0;
        public float InitCharaSize = 1.00f;
        public float CharaShadow = 1.00f;
        public int CharaShadowType = 1;
        public float VMDScale = 0.750f;
        public bool TouchVibration = true;

        public bool scene_crs_particle = true;
        public bool scene_crs_laser = true;
        public bool scene_crs_reflection = true;
        public bool scene_crs_sonic = true;
        public bool scene_crs_manual = true;

        public bool scene_kagura_particle = true;
        public bool scene_kagura_sea = true;
        public bool scene_kagura_reflection = true;

        public bool scene_view_led = true;
    }
}