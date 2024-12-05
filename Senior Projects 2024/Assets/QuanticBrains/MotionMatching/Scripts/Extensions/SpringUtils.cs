using Unity.Burst;
using Unity.Mathematics;

namespace QuanticBrains.MotionMatching.Scripts.Extensions
{
    public static class SpringUtils
    {
        private const float InertHalfLife = 0.05f;
        ///Spring method for rotations
        public static void SpringRotationPrediction(ref quaternion rot, ref float3 angular, quaternion goalRot,
            float halfLife, float deltaTime)
        {
            //Half life approximation and parameters
            float w = HalfLifeToDamping(halfLife) * 0.5f;
            float3 rotIncrement = MathUtils.ToScaledAngleAxis(MathUtils.Abs(math.mul(rot, math.inverse(goalRot))));
            float3 damp = angular + rotIncrement * w;

            //Fast exp
            float exp = FastExp(w * deltaTime);

            //Solve with spring formula
            rot = math.mul(MathUtils.FromScaledAngleAxis(exp * (rotIncrement + damp * deltaTime)), goalRot);
            angular = exp * (angular - damp * w * deltaTime);
        }

        /// Spring method for positions
        public static void SpringPositionPrediction(ref float3 pos, ref float3 speed, ref float3 accel, float3 goalSpeed,
            float halfLife, float deltaTime)
        {
            //Half life approximation and parameters
            float w = HalfLifeToDamping(halfLife) / 2.0f;
            float3 speedIncrement = speed - goalSpeed;
            float3 damp = accel + w * speedIncrement;

            //Fast exp
            float exp = FastExp(w * deltaTime);

            //Integrate position based on speed formula
            pos = exp * (((-damp) / (w * w))
                         + ((-speedIncrement - damp * deltaTime) / w))
                  + (damp / (w * w)) + speedIncrement / w
                  + goalSpeed * deltaTime + pos;

            //Solve speed and accel with spring formula
            speed = exp * (speedIncrement + damp * deltaTime) + goalSpeed;
            accel = exp * (accel - damp * w * deltaTime);
        }

        /// <summary>
        /// Special decay spring method for rotation offsets in inertialization
        /// </summary>
        [BurstCompile]
        public static (quaternion, float3) DecaySpringRotation(quaternion rot, float3 angular, float deltaTime, float halfLife = InertHalfLife)
        {
            DecaySpringRotation(ref rot, ref angular, deltaTime, halfLife);
            return (rot, angular);
        }


        /// <summary>
        /// Special decay spring method for rotation offsets in inertialization
        /// </summary>
        [BurstCompile]
        public static void DecaySpringRotation(ref quaternion rot, ref float3 angular, float deltaTime, float halfLife = InertHalfLife)
        {
            //Half life approximation
            float w = HalfLifeToDamping(halfLife) / 2.0f;

            //On this case, goalRot is identity, so decay equals original rot offset
            float3 rotDecay = MathUtils.ToScaledAngleAxis(rot);
            float3 damp = angular + rotDecay * w;
            float exp = FastExp(w * deltaTime);

            //Rot computing based on critical damper spring formula
            rot = MathUtils.FromScaledAngleAxis(exp * (rotDecay + damp * deltaTime));
            angular = exp * (angular - damp * w * deltaTime);
        }
    
        /// <summary>
        /// Special decay spring method for position offsets in inertialization
        /// </summary>
        [BurstCompile]
        public static (float3, float3) DecaySpringPosition(float3 pos, float3 velocity, float deltaTime, float halfLife = InertHalfLife)
        {
            DecaySpringPosition(ref pos, ref velocity, deltaTime, halfLife);
            return (pos, velocity);
        }
        
        /// <summary>
        /// Special decay spring method for position offsets in inertialization
        /// </summary>
        public static void DecaySpringPosition(ref float3 pos, ref float3 velocity, float deltaTime, float halfLife = InertHalfLife)
        {
            //Half life approximation
            float w = HalfLifeToDamping(halfLife) / 2.0f;
            float3 damp = velocity + pos * w;
            float exp = FastExp(w * deltaTime);

            //Solve new pos and velocity offset with spring formula
            //There is no need to integrate position based on velocity, since it is a decay spring
            pos = exp * (pos + damp * deltaTime);
            velocity = exp * (velocity - damp * w * deltaTime);
        }
    
        //Half life approximation
        private static float HalfLifeToDamping(float halfLife, float eps = 1e-5f)
        {
            return (4.0f * math.LN2) / (halfLife + eps);
        }

        //Faster approximation of 1/e^x
        private static float FastExp(float x)
        {
            return 1.0f / (1.0f + x + 0.48f * x * x + 0.235f * x * x * x);
        }
    }
}
