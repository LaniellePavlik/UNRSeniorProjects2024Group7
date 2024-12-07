namespace TobyFredson
{

    public struct TobyShaderValuesModel
    {

        public float season;

        public TobyWindType windType;
        public float windStrength;
        public float windSpeed;
    }

    public enum TobyWindType
    { 
        GentleBreeze,
        WindOff,
    }

}