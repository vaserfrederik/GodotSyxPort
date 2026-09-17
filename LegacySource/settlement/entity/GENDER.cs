using System;

namespace Settlement.Entity
{
    public enum GENDER
    {
        MALE,
        FEMALE
    }

    public static class GenderExtensions
    {
        private const string MALE_NAME = "male";
        private const string FEMALE_NAME = "female";
        private const float MALE_WEIGHT_REDUCTION = 1f;
        private const float FEMALE_WEIGHT_REDUCTION = 0.6f;

        public static string Name(this GENDER gender)
        {
            switch (gender)
            {
                case GENDER.MALE:
                    return MALE_NAME;
                case GENDER.FEMALE:
                    return FEMALE_NAME;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
            }
        }

        public static float WeightReduction(this GENDER gender)
        {
            switch (gender)
            {
                case GENDER.MALE:
                    return MALE_WEIGHT_REDUCTION;
                case GENDER.FEMALE:
                    return FEMALE_WEIGHT_REDUCTION;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gender), gender, null);
            }
        }

        public static GENDER GetRND()
        {
            Random rnd = new Random();
            return (GENDER)rnd.Next(2);
        }
    }
}